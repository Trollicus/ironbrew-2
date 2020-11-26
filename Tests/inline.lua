local String = "85 152 67 95 71 149 72 143"
local S = '';

for SNew in String:gmatch'%S+' do
    print(SNew)
    local Bool = math.random(1, 2) == 1;
    local Offset = math.random(50);
        
    local P1, P2 = SNew:find'%+' or SNew:find'%-';
    if P1 or P2 then
        S = S .. string.char(tonumber(SNew:sub(1, (P1 or P2) - 1)));
    else
        print(string.char(tonumber(SNew) + Offset))
        S = S .. (Bool and (string.char(tonumber(SNew) - Offset)) or (string.char(tonumber(SNew) + Offset)));
    end;
end;

print(S)