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

    function Player:ViewPunch( ang ) end

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

-- Weapon stubs
do
    local Weapon = FindMetaTable("Weapon")
    function Weapon:Clip1() return 10 end

    function Weapon:SetClip1(x) end

    function Weapon:Clip2() return 10 end

    function Weapon:SetClip2(x) end

    function Weapon:ShootEffects() end
end

-- Entity
do
    local Entity = FindMetaTable("Entity")
    function Entity:IsNPC() return false end
end

-- DARKRP
DarkRP = {}
_R.miku_debug_lib(DarkRP,"DarkRP")

function DarkRP.formatMoney(x)
    return "$"..x
end
