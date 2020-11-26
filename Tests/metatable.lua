local mt = getrawmetatable(game)
local namecall = mt.__namecall
local setreadonly = setreadonly or make_writeable

setreadonly(mt,false)

mt.__namecall = function(...)
    local args = {...}
    if args[#args] == "FireServer" or args[#args] == "InvokeServer" then
        if args[1] == game.ReplicatedStorage.Interaction.Ban or args[1] == game.ReplicatedStorage.Transactions.AddLog or args[1] == game.ReplicatedStorage.Interaction.DamageHumanoid then
            return
        end
    end
    if args[#args] == "Kick" then
        return
    end
    return namecall(...)
end

print'aaaa'