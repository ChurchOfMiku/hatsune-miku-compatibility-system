net = net or {}

function Material(name)
    return name
end

function Angle()

end

timer = timer or {}
function timer.Create()

end

util = util or {}
if SERVER then
    function util.AddNetworkString(str)
        
    end
end

if CLIENT then
    function LoadPresets()
        return {}
    end

    vgui = vgui or {}
    _R.miku_debug_lib(vgui,"vgui")

    function surface.GetTextureID( name )
        return 0
    end

    render = render or {}
    _R.miku_debug_lib(vgui,"render")

    function render.GetScreenEffectTexture()
        return "NO_TEXTURE"
    end

    derma = {}
    function derma.DefineControl(name,desc,tab,base)
        print("register control",name)
        return tab
    end
end

function AddCSLuaFile() end

function AddConsoleCommand()

end

local function stub_convar(name,value)
    local value = tostring(value or "")
    if name == "cl_drawhud" then
        value = "1"
    end

    return {
        GetInt = function() return math.floor(tonumber(value) or 0) end
    }
end

function GetConVar_Internal(name)
    return stub_convar(name)
end

function CreateConVar(name,value)
    return stub_convar(name,value)
end

sql = {}
_R.miku_debug_lib(sql,"sql")

function sql.Query(...)
    print("sql query",...)
end

file = {}
_R.miku_debug_lib(file,"file")

function file.Open()
    return nil
end

function file.Find()
    return {}, {}
end

function file.Exists()
    return false
end

ents = {}
_R.miku_debug_lib(ents,"ents")

player = {}
_R.miku_debug_lib(player,"player")

team = {}
_R.miku_debug_lib(team,"team")

game = {}
_R.miku_debug_lib(game,"game")

function game.SinglePlayer()
    -- Does sbox even have the notion of singleplayer yet?
    return false
end

function game.GetAmmoID()
    -- https://wiki.facepunch.com/gmod/Default_Ammo_Types
    -- we can probably just implement ammo types in lua
    return 1
end

if CLIENT then
    input = input or {}
    _R.miku_debug_lib(input,"input")
    
    function input.SelectWeapon()
    
    end
end

motionsensor = motionsensor or {}
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

    function Player:KeyDown() return false end
    function Player:InVehicle() return false end
    function Player:Ping() return 69 end


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
end

-- Entity
do
    local Entity = FindMetaTable("Entity")
    function Entity:IsNPC() return false end

    function Entity:MuzzleFlash() end

    function Entity:SetAnimation() end
end

-- DARKRP
DarkRP = DarkRP or {}
_R.miku_debug_lib(DarkRP,"DarkRP")

function DarkRP.formatMoney(x)
    return "$"..x
end
