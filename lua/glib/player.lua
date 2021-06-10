-- Stub functions for players.

local WEAPON_CLASS = {__index = {}}
_MIKU_DEBUG_LIB(WEAPON_CLASS.__index,"Weapon")

function WEAPON_CLASS.__index.Clip1()
    return 2
end

function WEAPON_CLASS.__index.GetPrimaryAmmoType()
    return "dicks"
end

local LOCAL_WEAPON = setmetatable({},WEAPON_CLASS)

---------------------------------------------------------

local PLAYER_CLASS = {__index = {}}
_MIKU_DEBUG_LIB(PLAYER_CLASS.__index,"Player")

function PLAYER_CLASS.__index.Health()
    return 69
end

function PLAYER_CLASS.__index.Armor()
    return 420
end

function PLAYER_CLASS.__index.GetAmmoCount()
    return 7
end

function PLAYER_CLASS.__index.GetActiveWeapon()
    return LOCAL_WEAPON
end

-- TODO only create this for clients
local LOCAL_PLAYER = setmetatable({},PLAYER_CLASS)

function LocalPlayer()
    return LOCAL_PLAYER
end
