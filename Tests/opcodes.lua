print("some string")

local l1 = function()
	print'a1'
end

local l2 = function()
	print'a2'
end

local l3 = function()
	print'a3'
end

l1()
l2()
l3()

local LOADK = 1
local MOVE = LOADK
local LOADBOOL = true
local LOADNIL = nil
local GETGLOBAL = table
local GETTABLE = table.insert
local NEWTABLE = {}

print(LOADBOOL, LOADNIL)

function NEWTABLE:CLOSURE(...)
	print'aaa'
	local SETLIST_VARARG = {...}
	local GETUPVAL = GETGLOBAL
	LOADNIL = SETUPVAL
	return SETLIST_VARARG
end

print(LOADK)

local CALL = GETTABLE(NEWTABLE, not (-LOADK + LOADK - LOADK * LOADK / LOADK ^ LOADK % LOADK))
local LEN_CONCAT = "" .. #NEWTABLE
local EQ, LT, LE = LOADK == 1, LOADK < 1, LOADK <= 1

print (EQ, LT, LE)

if LOADBOOL then
	local TESTSET = LOADK and LOADK
end

function SETGLOBAL_TAILCALL()
	print'bbb'
	return NEWTABLE:CLOSURE(1, 2)
end

for i, v in next, SETGLOBAL_TAILCALL() do
	for j = 1, 5 do
		print(j)
	end
end

do
	local p,q
	r = function() return p,q end
end

print("DONE")	