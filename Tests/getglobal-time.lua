local T = {}

local Start = os.clock()
for i=1, 100000 do
    T[i] = pcall
end
print("time: " .. os.clock() - Start .. "s")