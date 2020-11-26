local getMT = getrawmetatable or getmetatable
local dec = decompile or nil

local function tFunc()
    local Kek = 'THISISNORMAL'
    return Kek
end

local DecS, Ret = pcall(dec, tFunc)
local dumpf = dump_function or dec

local a = loadstring(Ret)()
print(a)