local function gay1(a, b, c, ...)
    print(a, b, c)
    return {...}, ...
end
print(gay1(1, nil, 3, 4, nil, 6))
print(#{1, nil, 3, 4, nil, 5})