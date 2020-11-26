-- This module contains functions to calculate SHA2 digest.
--    Supported hashes: SHA-224, SHA-256, SHA-384, SHA-512, SHA-512/224, SHA-512/256
--    This is a pure-Lua module, compatible with Lua 5.1
--    It works on Lua 5.1/5.2/5.3/5.4/LuaJIT, but it doesn't use benefits of Lua versions 5.2+

--    Input data may must be provided either as a whole string or as a sequence of substrings (chunk-by-chunk).
--    Result (SHA2 digest) is a string of lowercase hex digits.
--
--    Simplest usage example:
--       local your_hash = require("sha2for51").sha512("your string")

--    See file "sha2for51_test.lua" for more examples.



local unpack, table_concat, byte, char, string_rep, sub, string_format, floor, ceil, min, max =
   table.unpack or unpack, table.concat, string.byte, string.char, string.rep, string.sub, string.format, math.floor, math.ceil, math.min, math.max

--------------------------------------------------------------------------------
-- BASIC BITWISE FUNCTIONS
--------------------------------------------------------------------------------

-- 32-bit bitwise functions
local AND, OR, XOR, SHL, SHR, ROL, ROR, HEX
-- Only low 32 bits of function arguments matter, high bits are ignored
-- The result of all functions (except HEX) is an integer (pair of integers) inside range 0..(2^32-1)

function SHL(x, n)
   return (x * 2^n) % 4294967296
end

function SHR(x, n)
   x = x % 4294967296 / 2^n
   return x - x % 1
end

function ROL(x, n)
   x = x % 4294967296 * 2^n
   local r = x % 4294967296
   return r + (x - r) / 4294967296
end

function ROR(x, n)
   x = x % 4294967296 / 2^n
   local r = x % 1
   return r * 4294967296 + (x - r)
end

local AND_of_two_bytes = {}  -- look-up table (256*256 entries)
for idx = 0, 65535 do
   local x = idx % 256
   local y = (idx - x) / 256
   local res = 0
   local w = 1
   while x * y ~= 0 do
      local rx = x % 2
      local ry = y % 2
      res = res + rx * ry * w
      x = (x - rx) / 2
      y = (y - ry) / 2
      w = w * 2
   end
   AND_of_two_bytes[idx] = res
end

local function and_or_xor(x, y, operation)
   -- operation: nil = AND, 1 = OR, 2 = XOR
   local x0 = x % 4294967296
   local y0 = y % 4294967296
   local rx = x0 % 256
   local ry = y0 % 256
   local res = AND_of_two_bytes[rx + ry * 256]
   x = x0 - rx
   y = (y0 - ry) / 256
   rx = x % 65536
   ry = y % 256
   res = res + AND_of_two_bytes[rx + ry] * 256
   x = (x - rx) / 256
   y = (y - ry) / 256
   rx = x % 65536 + y % 256
   res = res + AND_of_two_bytes[rx] * 65536
   res = res + AND_of_two_bytes[(x + y - rx) / 256] * 16777216
   if operation then
      res = x0 + y0 - operation * res
   end
   return res
end

function AND(x, y)
   return and_or_xor(x, y)
end

function OR(x, y)
   return and_or_xor(x, y, 1)
end

function XOR(x, y, z)          -- 2 or 3 arguments
   if z then
      y = and_or_xor(y, z, 2)
   end
   return and_or_xor(x, y, 2)
end

function HEX(x)
   return string_format("%08x", x % 4294967296)
end

-- Arrays of SHA2 "magic numbers"
local sha2_K_lo, sha2_K_hi, sha2_H_lo, sha2_H_hi = {}, {}, {}, {}
local sha2_H_ext256 = {[224] = {}, [256] = sha2_H_hi}
local sha2_H_ext512_lo, sha2_H_ext512_hi = {[384] = {}, [512] = sha2_H_lo}, {[384] = {}, [512] = sha2_H_hi}

local common_W = {}  -- a temporary table shared between all calculations

local function sha256_feed_64(H, K, str, W, offs, size)
   -- offs >= 0, size >= 0, size is multiple of 64
   for pos = offs, size + offs - 1, 64 do
      for j = 1, 16 do
         pos = pos + 4
         local a, b, c, d = byte(str, pos - 3, pos)
         W[j] = ((a * 256 + b) * 256 + c) * 256 + d
      end
      for j = 17, 64 do
         local a, b = W[j-15], W[j-2]
         W[j] = XOR(ROR(a, 7), ROL(a, 14), SHR(a, 3)) + XOR(ROL(b, 15), ROL(b, 13), SHR(b, 10)) + W[j-7] + W[j-16]
      end
      local a, b, c, d, e, f, g, h, z = H[1], H[2], H[3], H[4], H[5], H[6], H[7], H[8]
      for j = 1, 64 do
         z = XOR(ROR(e, 6), ROR(e, 11), ROL(e, 7)) + AND(e, f) + AND(-1-e, g) + h + K[j] + W[j]
         h = g
         g = f
         f = e
         e = z + d
         d = c
         c = b
         b = a
         a = z + AND(d, c) + AND(a, XOR(d, c)) + XOR(ROR(a, 2), ROR(a, 13), ROL(a, 10))
      end
      H[1], H[2], H[3], H[4] = (a + H[1]) % 4294967296, (b + H[2]) % 4294967296, (c + H[3]) % 4294967296, (d + H[4]) % 4294967296
      H[5], H[6], H[7], H[8] = (e + H[5]) % 4294967296, (f + H[6]) % 4294967296, (g + H[7]) % 4294967296, (h + H[8]) % 4294967296
   end
end

local function sha512_feed_128(H_lo, H_hi, K_lo, K_hi, str, W, offs, size)
   -- offs >= 0, size >= 0, size is multiple of 128
   -- W1_hi, W1_lo, W2_hi, W2_lo, ...   Wk_hi = W[2*k-1], Wk_lo = W[2*k]
   for pos = offs, size + offs - 1, 128 do
      for j = 1, 32 do
         pos = pos + 4
         local a, b, c, d = byte(str, pos - 3, pos)
         W[j] = ((a * 256 + b) * 256 + c) * 256 + d
      end
      local tmp1, tmp2
      for jj = 17 * 2, 80 * 2, 2 do
         local a_lo, a_hi, b_lo, b_hi = W[jj-30], W[jj-31], W[jj-4], W[jj-5]
         tmp1 = XOR(SHR(a_lo, 1) + SHL(a_hi, 31), SHR(a_lo, 8) + SHL(a_hi, 24), SHR(a_lo, 7) + SHL(a_hi, 25)) + XOR(SHR(b_lo, 19) + SHL(b_hi, 13), SHL(b_lo, 3) + SHR(b_hi, 29), SHR(b_lo, 6) + SHL(b_hi, 26)) + W[jj-14] + W[jj-32]
         tmp2 = tmp1 % 4294967296
         W[jj-1] = XOR(SHR(a_hi, 1) + SHL(a_lo, 31), SHR(a_hi, 8) + SHL(a_lo, 24), SHR(a_hi, 7)) + XOR(SHR(b_hi, 19) + SHL(b_lo, 13), SHL(b_hi, 3) + SHR(b_lo, 29), SHR(b_hi, 6)) + W[jj-15] + W[jj-33] + (tmp1 - tmp2) / 4294967296
         W[jj] = tmp2
      end
      local a_lo, b_lo, c_lo, d_lo, e_lo, f_lo, g_lo, h_lo, z_lo = H_lo[1], H_lo[2], H_lo[3], H_lo[4], H_lo[5], H_lo[6], H_lo[7], H_lo[8]
      local a_hi, b_hi, c_hi, d_hi, e_hi, f_hi, g_hi, h_hi, z_hi = H_hi[1], H_hi[2], H_hi[3], H_hi[4], H_hi[5], H_hi[6], H_hi[7], H_hi[8]
      for j = 1, 80 do
         local jj = 2 * j
         tmp1 = XOR(SHR(e_lo, 14) + SHL(e_hi, 18), SHR(e_lo, 18) + SHL(e_hi, 14), SHL(e_lo, 23) + SHR(e_hi, 9)) + AND(e_lo, f_lo) + AND(-1-e_lo, g_lo) + h_lo + K_lo[j] + W[jj]
         z_lo = tmp1 % 4294967296
         z_hi = XOR(SHR(e_hi, 14) + SHL(e_lo, 18), SHR(e_hi, 18) + SHL(e_lo, 14), SHL(e_hi, 23) + SHR(e_lo, 9)) + AND(e_hi, f_hi) + AND(-1-e_hi, g_hi) + h_hi + K_hi[j] + W[jj-1] + (tmp1 - z_lo) / 4294967296
         h_lo = g_lo
         h_hi = g_hi
         g_lo = f_lo
         g_hi = f_hi
         f_lo = e_lo
         f_hi = e_hi
         tmp1 = z_lo + d_lo
         e_lo = tmp1 % 4294967296
         e_hi = z_hi + d_hi + (tmp1 - e_lo) / 4294967296
         d_lo = c_lo
         d_hi = c_hi
         c_lo = b_lo
         c_hi = b_hi
         b_lo = a_lo
         b_hi = a_hi
         tmp1 = z_lo + AND(d_lo, c_lo) + AND(b_lo, XOR(d_lo, c_lo)) + XOR(SHR(b_lo, 28) + SHL(b_hi, 4), SHL(b_lo, 30) + SHR(b_hi, 2), SHL(b_lo, 25) + SHR(b_hi, 7))
         a_lo = tmp1 % 4294967296
         a_hi = z_hi + (AND(d_hi, c_hi) + AND(b_hi, XOR(d_hi, c_hi))) + XOR(SHR(b_hi, 28) + SHL(b_lo, 4), SHL(b_hi, 30) + SHR(b_lo, 2), SHL(b_hi, 25) + SHR(b_lo, 7)) + (tmp1 - a_lo) / 4294967296
      end
      tmp1 = H_lo[1] + a_lo
      tmp2 = tmp1 % 4294967296
      H_lo[1], H_hi[1] = tmp2, (H_hi[1] + a_hi + (tmp1 - tmp2) / 4294967296) % 4294967296
      tmp1 = H_lo[2] + b_lo
      tmp2 = tmp1 % 4294967296
      H_lo[2], H_hi[2] = tmp2, (H_hi[2] + b_hi + (tmp1 - tmp2) / 4294967296) % 4294967296
      tmp1 = H_lo[3] + c_lo
      tmp2 = tmp1 % 4294967296
      H_lo[3], H_hi[3] = tmp2, (H_hi[3] + c_hi + (tmp1 - tmp2) / 4294967296) % 4294967296
      tmp1 = H_lo[4] + d_lo
      tmp2 = tmp1 % 4294967296
      H_lo[4], H_hi[4] = tmp2, (H_hi[4] + d_hi + (tmp1 - tmp2) / 4294967296) % 4294967296
      tmp1 = H_lo[5] + e_lo
      tmp2 = tmp1 % 4294967296
      H_lo[5], H_hi[5] = tmp2, (H_hi[5] + e_hi + (tmp1 - tmp2) / 4294967296) % 4294967296
      tmp1 = H_lo[6] + f_lo
      tmp2 = tmp1 % 4294967296
      H_lo[6], H_hi[6] = tmp2, (H_hi[6] + f_hi + (tmp1 - tmp2) / 4294967296) % 4294967296
      tmp1 = H_lo[7] + g_lo
      tmp2 = tmp1 % 4294967296
      H_lo[7], H_hi[7] = tmp2, (H_hi[7] + g_hi + (tmp1 - tmp2) / 4294967296) % 4294967296
      tmp1 = H_lo[8] + h_lo
      tmp2 = tmp1 % 4294967296
      H_lo[8], H_hi[8] = tmp2, (H_hi[8] + h_hi + (tmp1 - tmp2) / 4294967296) % 4294967296
   end
end

--------------------------------------------------------------------------------
-- CALCULATING THE MAGIC NUMBERS (roots of primes)
--------------------------------------------------------------------------------

do
   local function mul(src1, src2, factor, result_length)
      -- Long arithmetic multiplication: src1 * src2 * factor
      -- src1, src2 - long integers (arrays of digits in base 2^24)
      -- factor - short integer
      local result = {}
      local carry = 0
      local value = 0.0
      local weight = 1.0
      for j = 1, result_length do
         local prod = 0
         for k = max(1, j + 1 - #src2), min(j, #src1) do
            prod = prod + src1[k] * src2[j + 1 - k]
         end
         carry = carry + prod * factor
         local digit = carry % 16777216
         result[j] = digit
         carry = floor(carry / 16777216)
         value = value + digit * weight
         weight = weight * 2^24
      end
      return
         result,    -- long integer
         value      -- and its floating point approximation
   end

   local idx, step, p, one  = 0, {4, 1, 2, -2, 2}, 4, {1}
   local sqrt_hi, sqrt_lo, idx_disp = sha2_H_hi, sha2_H_lo, 0
   repeat
      p = p + step[p % 6]
      local d = 1
      repeat
         d = d + step[d % 6]
         if d * d > p then
            idx = idx + 1
            local root = p^(1/3)
            local R = mul({floor(root * 2^40)}, one, 1, 2)
            local _, delta = mul(R, mul(R, R, 1, 4), -1, 4)
            local hi = R[2] % 65536 * 65536 + floor(R[1] / 256)
            local lo = R[1] % 256 * 16777216 + floor(delta * (2^-56 / 3) * root / p)
            sha2_K_hi[idx], sha2_K_lo[idx] = hi, lo
            if idx < 17 then
               root = p^(1/2)
               R = mul({floor(root * 2^40)}, one, 1, 2)
               _, delta = mul(R, R, -1, 2)
               hi = R[2] % 65536 * 65536 + floor(R[1] / 256)
               lo = R[1] % 256 * 16777216 + floor(delta * 2^-17 / root)
               sha2_H_ext256[224][idx + idx_disp] = lo
               sqrt_hi[idx + idx_disp], sqrt_lo[idx + idx_disp] = hi, lo
               if idx == 8 then
                  sqrt_hi, sqrt_lo, idx_disp = sha2_H_ext512_hi[384], sha2_H_ext512_lo[384], -8
               end
            end
            break
         end
      until p % d == 0
   until idx > 79
end

-- Calculating IV for SHA512/224 and SHA512/256
for width = 224, 256, 32 do
   local H_lo, H_hi = {}, {}
   for j = 1, 8 do
      H_lo[j] = XOR(sha2_H_lo[j], 0xa5a5a5a5)
      H_hi[j] = XOR(sha2_H_hi[j], 0xa5a5a5a5)
   end
   sha512_feed_128(H_lo, H_hi, sha2_K_lo, sha2_K_hi, "SHA-512/"..tonumber(width).."\128"..string_rep("\0", 115).."\88", common_W, 0, 128)
   sha2_H_ext512_lo[width] = H_lo
   sha2_H_ext512_hi[width] = H_hi
end


--------------------------------------------------------------------------------
-- FINAL FUNCTIONS
--------------------------------------------------------------------------------

local function sha256ext(width, text)

   -- Create an instance (private objects for current calculation)
   local H, length, tail = {unpack(sha2_H_ext256[width])}, 0, ""

   local function partial(text_part)
      if text_part then
         if tail then
            length = length + #text_part
            local offs = 0
            if tail ~= "" and #tail + #text_part >= 64 then
               offs = 64 - #tail
               sha256_feed_64(H, sha2_K_hi, tail..sub(text_part, 1, offs), common_W, 0, 64)
               tail = ""
            end
            local size = #text_part - offs
            local size_tail = size % 64
            sha256_feed_64(H, sha2_K_hi, text_part, common_W, offs, size - size_tail)
            tail = tail..sub(text_part, #text_part + 1 - size_tail)
            return partial
         else
            error("Adding more chunks is not allowed after asking for final result", 2)
         end
      else
         if tail then
            local final_blocks = {tail, "\128", string_rep("\0", (-9 - length) % 64 + 1)}
            tail = nil
            -- Assuming user data length is shorter than 2^53 bytes
            -- Anyway, it looks very unrealistic that one would spend enough time to process a 2^53 bytes of data by using this Lua script :-)
            -- 2^53 bytes = 2^56 bits, so "bit-counter" fits in 7 bytes
            length = length * (8 / 256^7)  -- convert "byte-counter" to "bit-counter" and move floating point to the left
            for j = 4, 10 do
               length = length % 1 * 256
               final_blocks[j] = char(floor(length))
            end
            final_blocks = table_concat(final_blocks)
            sha256_feed_64(H, sha2_K_hi, final_blocks, common_W, 0, #final_blocks)
            local max_reg = width / 32
            for j = 1, max_reg do
               H[j] = HEX(H[j])
            end
            H = table_concat(H, "", 1, max_reg)
         end
         return H
      end
   end

   if text then
      -- Actually perform calculations and return the SHA256 digest of a message
      return partial(text)()
   else
      -- Return function for partial chunk loading
      -- User should feed every chunks of input data as single argument to this function and receive SHA256 digest by invoking this function without an argument
      return partial
   end

end


local function sha512ext(width, text)

   -- Create an instance (private objects for current calculation)
   local length, tail, H_lo, H_hi = 0, "", {unpack(sha2_H_ext512_lo[width])}, {unpack(sha2_H_ext512_hi[width])}

   local function partial(text_part)
      if text_part then
         if tail then
            length = length + #text_part
            local offs = 0
            if tail ~= "" and #tail + #text_part >= 128 then
               offs = 128 - #tail
               sha512_feed_128(H_lo, H_hi, sha2_K_lo, sha2_K_hi, tail..sub(text_part, 1, offs), common_W, 0, 128)
               tail = ""
            end
            local size = #text_part - offs
            local size_tail = size % 128
            sha512_feed_128(H_lo, H_hi, sha2_K_lo, sha2_K_hi, text_part, common_W, offs, size - size_tail)
            tail = tail..sub(text_part, #text_part + 1 - size_tail)
            return partial
         else
            error("Adding more chunks is not allowed after asking for final result", 2)
         end
      else
         if tail then
            local final_blocks = {tail, "\128", string_rep("\0", (-17-length) % 128 + 9)}
            tail = nil
            -- Assuming user data length is shorter than 2^53 bytes
            -- 2^53 bytes = 2^56 bits, so "bit-counter" fits in 7 bytes
            length = length * (8 / 256^7)  -- convert "byte-counter" to "bit-counter" and move floating point to the left
            for j = 4, 10 do
               length = length % 1 * 256
               final_blocks[j] = char(floor(length))
            end
            final_blocks = table_concat(final_blocks)
            sha512_feed_128(H_lo, H_hi, sha2_K_lo, sha2_K_hi, final_blocks, common_W, 0, #final_blocks)
            local max_reg = ceil(width / 64)
            for j = 1, max_reg do
               H_lo[j] = HEX(H_hi[j])..HEX(H_lo[j])
            end
            H_hi = nil
            H_lo = table_concat(H_lo, "", 1, max_reg):sub(1, width / 4)
         end
         return H_lo
      end
   end

   if text then
      -- Actually perform calculations and return the SHA256 digest of a message
      return partial(text)()
   else
      -- Return function for partial chunk loading
      -- User should feed every chunks of input data as single argument to this function and receive SHA256 digest by invoking this function without an argument
      return partial
   end

end

local sha2for51 = {
   sha224     = function (text) return sha256ext(224, text) end,  -- SHA-224
   sha256     = function (text) return sha256ext(256, text) end,  -- SHA-256
   sha384     = function (text) return sha512ext(384, text) end,  -- SHA-384
   sha512     = function (text) return sha512ext(512, text) end,  -- SHA-512
   sha512_224 = function (text) return sha512ext(224, text) end,  -- SHA-512/224
   sha512_256 = function (text) return sha512ext(256, text) end,  -- SHA-512/256
}

local sha2 = sha2for51

local function test_sha256()

    local sha256 = sha2.sha256
 
    -- some test strings
    assert(sha256("The quick brown fox jumps over the lazy dog") == "d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592")
    assert(sha256("The quick brown fox jumps over the lazy cog") == "e4c4d8f3bf76b692de791a173e05321150f7a345b46484fe427f6acc7ecc81be")
    assert(sha256("abc") == "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad")
    assert(sha256("123456") == "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92")
    assert(sha256("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq") == "248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1")
    assert(sha256("abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu") == "cf5b16a778af8380036ce59e7b0492370b249b11e8f07a51afac45037afee9d1")
 
    -- chunk-by-chunk loading:   sha256("string") == sha256()("st")("ri")("ng")()
    local append_next_chunk = sha256() -- create a private closure for calculating digest of single string
    append_next_chunk("The quick brown fox")
    append_next_chunk(" jumps ")
    append_next_chunk("")              -- chunk may be empty string
    append_next_chunk("over the lazy dog")
    assert(append_next_chunk() == "d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592")  -- asking for final result (invocation without an argument)
    assert(append_next_chunk() == "d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592")  -- you can ask the same result multiple times if needed
    -- append_next_chunk("more text") will fail here: no more chunks are allowed after receiving the result, the closure is useless now, let it be GC-ed
    assert(not pcall(append_next_chunk, "more text"))
 
    -- one-liner is possible due to "append_next_chunk(chunk)" returns the function "append_next_chunk"
    assert(sha256()("The quick brown fox")(" jumps ")("")("over the lazy dog")() == "d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592")
 
    -- empty string
    assert(sha256("") == "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")
    assert(sha256()() == "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")
 
    -- computations of different strings don't interfere with each other
    local chunk_for_digits = sha256()
    chunk_for_digits("123")
    local chunk_for_fox = sha256()
    chunk_for_fox("The quick brown fox jumps ")
    chunk_for_digits("45")
    chunk_for_fox("over the lazy dog")
    chunk_for_digits("6")
    assert(chunk_for_digits() == "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92")
    assert(chunk_for_fox() == "d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592")
 
    -- "00...0\n"
    for i, dgst in pairs{  -- from 50 to 70 zeroes
       [50] = "9660acb8046abf46cf27280e61abd174ebac98ad6855e093772b78df85523129",
       [51] = "31e1c552b357ace9bcb924691799a3c0d3aa10d8b428d9de28a278e3c79ecb7b",
       [52] = "0be5c4bcb6f47e30c13515594dbef4faa3a6485af67c177179fee8b33cd4f2a0",
       [53] = "d368c7f6038c1743bdbfe6a9c3a72d4e6916aa219ed8d559766c9e8f9845f3b8",
       [54] = "7080a4aa6ff030ae152fe610a62ee29464f92afeb176474551a69d35aab154a0",
       [55] = "149c1cda81fa9359c0c2a5e405ca972986f1d53e05f6282871dd1581046b3f44",
       [56] = "eb2d4d41948ce546c8adff07ee97342070c5b89789f616a33efe52c7d3ec73d4",
       [57] = "c831db596ccbbf248023461b1c05d3ae084bcc79bcb2626c5ec179fb34371f2a",
       [58] = "1345b8a930737b1069bbf9b891ce095850f6cdba6e25874ea526a2ccb611fe46",
       [59] = "380ad21e466885fae080ceeada75ac04944687e626e161c0b24e91af3eec2def",
       [60] = "b9ab06fa30ef8531c5eee11651aa86f8279a245e0a3c29bf6228c59475cc610a",
       [61] = "bcc187de6605d9e11a0cc6edf02b67fb651fe1779ec59438788093d8e376c07c",
       [62] = "ae0b3681157b83b34de8591d2453915e40c3105ae79434e241d82d4035218e01",
       [63] = "68a27b4735f6806fb5983c1805a23797aa93ea06e0ebcb6daada2ea1ab5a05af",
       [64] = "827d096d92f3deeaa0e8070d79f45beb176768e57a958a1cd325f5f4b754b048",
       [65] = "6c7bd8ec0fe9b4e05a2d27dd5e41a8687a9716a2e8926bdfa141266b12942ec1",
       [66] = "2f4b4c41017a2ddd1cc8cd75478a82e9452e445d4242f09782535376d6f4ba50",
       [67] = "b777b86e005807a446ead00986fcbf3bdd6c022524deabf017eeb3f0c30b6eed",
       [68] = "777da331f60c793f582e4ca33223778218ddfd241981f15be5886171fb8301b5",
       [69] = "06ed0c4cbf7d2b38de5f01eab2d2cd552d9cb87f97b714b96bb7a9d1b6117c6d",
       [70] = "e82223344d5f3c024514cfbe6d478b5df98bb878f34d7a07e7b064fa7fa91946"
    } do
       assert(sha256(("0"):rep(i).."\n") == dgst)
    end
 
    -- "aa...a"
    assert(sha256(("a"):rep(55)) == "9f4390f8d30c2dd92ec9f095b65e2b9ae9b0a925a5258e241c9f1e910f734318")
    assert(sha256(("a"):rep(56)) == "b35439a4ac6f0948b6d6f9e3c6af0f5f590ce20f1bde7090ef7970686ec6738a")
 
    -- "aa...a\n" in chunk-by-chunk mode
    local next_chunk = sha256()
    for i = 1, 65 do
       next_chunk("a")
    end
    next_chunk("\n")
    assert(next_chunk() == "574883a9977284a46845620eaa55c3fa8209eaa3ebffe44774b6eb2dba2cb325")
 
    local function split_and_calculate_sha256(s, len) -- split string s in chunks of length len
       local next_chunk = sha256()
       for idx = 1, #s, len do
          next_chunk(s:sub(idx, idx + len - 1))
       end
       return next_chunk()
    end
    -- "00...0\n00...0\n...00...0\n" (80 lines of 80 zeroes each) in chunk-by-chunk mode with different chunk lengths
    local s = (("0"):rep(80).."\n"):rep(80)
    assert(split_and_calculate_sha256(s, 1)  == "736c7a8b17e2cfd44a3267a844db1a8a3e8988d739e3e95b8dd32678fb599139")
    assert(split_and_calculate_sha256(s, 2)  == "736c7a8b17e2cfd44a3267a844db1a8a3e8988d739e3e95b8dd32678fb599139")
    assert(split_and_calculate_sha256(s, 7)  == "736c7a8b17e2cfd44a3267a844db1a8a3e8988d739e3e95b8dd32678fb599139")
    assert(split_and_calculate_sha256(s, 70) == "736c7a8b17e2cfd44a3267a844db1a8a3e8988d739e3e95b8dd32678fb599139")
 
 end
 
 
 local function test_sha512()
 
    local sha512 = sha2.sha512
 
    assert(sha512("abc") == "ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f")
 
    assert(sha512("abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu") ==
       "8e959b75dae313da8cf4f72814fc143f8f7779c6eb9f7fa17299aeadb6889018501d289e4900f7e4331b99dec4b5433ac7d329eeb6dd26545e96e55b874be909")
 
    -- "aa...a"
    for i, dgst in pairs{  -- from 109 to 116 letters "a"
       [109] = "0cda6b04d9466bb7f3995c16732e1347f29c23a64fe0b085fadba0995644cc5aa71587423c274c10e09518310c5f866cfaceb229fabb574219f12182eb114182",
       [110] = "c825949632e509824543f7eaf159fb6041722fce3c1cdcbb613b3d37ff107c519417baac32f8e74fe29d7f4823bf6886956603dca5354a6ed6e4a542e06b7d28",
       [111] = "fa9121c7b32b9e01733d034cfc78cbf67f926c7ed83e82200ef86818196921760b4beff48404df811b953828274461673c68d04e297b0eb7b2b4d60fc6b566a2",
       [112] = "c01d080efd492776a1c43bd23dd99d0a2e626d481e16782e75d54c2503b5dc32bd05f0f1ba33e568b88fd2d970929b719ecbb152f58f130a407c8830604b70ca",
       [113] = "55ddd8ac210a6e18ba1ee055af84c966e0dbff091c43580ae1be703bdb85da31acf6948cf5bd90c55a20e5450f22fb89bd8d0085e39f85a86cc46abbca75e24d",
       [114] = "5e9eb0e4b270d086e77eeaf3ce8b1cfc615031b8c463dc34f5c139786f274f22accb4d89e8f40d1a0c2acc84c4dc0f2bab390a9d9495493bd617ed004271bb64",
       [115] = "eaa30f93760743ac7d0a6cb8ed5ef3b30c59097bc44d0ec337344301deba9fb92b20c488d55de415f6aaed0df4925b42894b81d2e1cde89d91ec7f6cc67262b4",
       [116] = "a8bff469314a1ce0c990bb3fd539d92accb6249cc674b559bc9d3898b7a126fee597197fa42c971443470053c7d7f54b09371a59b0f7af87b1917c5347e8f8e0",
    } do
       assert(sha512(("a"):rep(i)) == dgst)
    end
 
 end
 
 
 local function all_tests_sha2()
 
    test_sha256()
 
    assert(sha2.sha224"abc" == "23097d223405d8228642a477bda255b32aadbce4bda0b3f7e36c9da7")
    assert(sha2.sha224"abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq" == "75388b16512776cc5dba5da1fd890150b0c6455cb4f58b1952522525")
 
    test_sha512()
 
    assert(sha2.sha384"abc" == "cb00753f45a35e8bb5a03d699ac65007272c32ab0eded1631a8b605a43ff5bed8086072ba1e7cc2358baeca134c825a7")
    assert(sha2.sha384"abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu" == "09330c33f71147e83d192fc782cd1b4753111b173b3b05d22fa08086e3b0f712fcc7c71a557e2db966c3e9fa91746039")
 
    assert(sha2.sha512_224"abc" == "4634270f707b6a54daae7530460842e20e37ed265ceee9a43e8924aa")
    assert(sha2.sha512_224"abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu" == "23fec5bb94d60b23308192640b0c453335d664734fe40e7268674af9")
 
    assert(sha2.sha512_256"abc" == "53048e2681941ef99b2e29b76b4c7dabe4c2d0c634fc6d46e0e2f13107e7af23")
    assert(sha2.sha512_256"abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu" == "3928e184fb8690f840da3988121d31be65cb9d3ef83ee6146feac861e19b563a")
 
    print"All tests passed"
 
 end
 
 all_tests_sha2()
 
 
 
 local function benchmark()
 
    print("Benchmarking (calculating SHA512 of 1KByte string of letters 'a')...")
 
    local time_intervals = {}
 
    local length = 2^10
    local part = ("a"):rep(2^4)
    local N = length/#part
    local result
 
    local k = 2
    for j = 1, 2*k-1 do
       local clk0 = os.clock()
       local x = sha2.sha512()
       for j = 1, N do
          x(part)
       end
       result = x()
       time_intervals[j] = os.clock() - clk0
    end
 
    --print("Result = "..result)
 
    -- get median time
    table.sort(time_intervals)
    print('CPU seconds:', time_intervals[k])
 
 end
 
 benchmark() 