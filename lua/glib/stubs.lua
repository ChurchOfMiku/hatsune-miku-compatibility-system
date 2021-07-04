local function quick_sort(tbl, start, stop, sorter)
    if stop <= start then return end
    local pivot_i = math.floor((start+stop)/2)
    if pivot_i < start or pivot_i > stop then error("pivot bad") end
    local pivot = tbl[pivot_i]
    
    i = start - 1
    j = stop + 1
    while true do
        repeat
            i = i+1
        until not sorter(tbl[i],pivot)
        repeat
            j = j-1
        until not sorter(pivot,tbl[j])
        if i >= j then break end
        local tmp = tbl[i]
        tbl[i] = tbl[j]
        tbl[j] = tmp
    end

    quick_sort(tbl,start,j-1,sorter)
    quick_sort(tbl,j+1,stop,sorter)
end

function table.sort(tbl, sorter)
    quick_sort(tbl,1,#tbl,sorter or function(a,b) return a<b end)
end

--[[local t = {6,3,2,567,2,1}
table.sort(t)
for i=1,#t do
    print(i,t[i])
end
error("x")]]

net = {}

function Material(name)
    return name
end

function Vector(x,y,z)
    return  {
        x=x or 0,
        y=y or 0,
        z=z or 0
    }
end

function Angle()

end

util = {}
if SERVER then
    function util.AddNetworkString(str)
        
    end
end

if CLIENT then
    function LoadPresets()
        return {}
    end

    vgui = {}
    _R.miku_debug_lib(vgui,"vgui")

    function surface.GetTextureID( name )
        return 0
    end

    render = {}
    _R.miku_debug_lib(vgui,"render")

    function render.GetScreenEffectTexture()
        return "NO_TEXTURE"
    end
end

function AddConsoleCommand()

end

function AddCSLuaFile()

end

function CreateConVar()

end

sql = {}
_R.miku_debug_lib(sql,"sql")

function sql.Query(...)
    print("sql query",...)
end

file = {}
_R.miku_debug_lib(file,"file")

--local fake_file = {}
--_R.miku_debug_lib(fake_file,"[class File]")

function file.Open()
    return nil
end

ents = {}
_R.miku_debug_lib(ents,"ents")

player = {}
_R.miku_debug_lib(player,"player")

team = {}
_R.miku_debug_lib(team,"team")

game = {}
_R.miku_debug_lib(game,"game")

motionsensor = {}
_R.miku_debug_lib(motionsensor,"motionsensor")

-- Player stubs
do
    local Player = FindMetaTable("Player")
    function Player:Team()
        return 1
    end

    function Player:Armor()
        return 0
    end

    -- DARKRP
    function Player:isWanted()
        return false
    end

    function Player:getDarkRPVar(var)
        if var == "HasGunlicense" then return false end
        if var == "money" then return 10 end
        print("var",var)
    end
end

-- DARKRP
DarkRP = {}
_R.miku_debug_lib(DarkRP,"DarkRP")

function DarkRP.formatMoney(x)
    return "$"..x
end
