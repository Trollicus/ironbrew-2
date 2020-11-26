local l1 = function()
    print"a1"
    return 1, "a1ret"
end

local l2 = function()
    print"a2"
    return 2, "a2ret"
end

local l3 = function()
    print"a3"
    return 3, "a3ret"
end

print(l1())
print(l2())
print(l3())