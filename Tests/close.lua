local x, y, z, a, b, c = 1, 2, 3, 4, 1, 2

do
    x = 5
    local j, k, l = 4, 5, 6
    r = function()
        print(j, k, l)
    end
end

r()
print(x, y, z, a, b, c)