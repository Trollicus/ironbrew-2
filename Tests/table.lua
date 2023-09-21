local l = {} 
l.t = {} 
l.t.a = {1, 2, 3, 4, ['asd'] = 5} 

pcall(function() 
    table.insert(l.t.a, 1) 
end) 

print(l.t.a[1], l.t.a[2], l.t.a.asd)