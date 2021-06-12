-- Stub functions for players.
_CLASS_WEAPON = {}
_CLASS_WEAPON.__index = _CLASS_WEAPON
_MIKU_DEBUG_LIB(_CLASS_WEAPON,"Weapon")

function _CLASS_WEAPON.Clip1()
    return 0
end

function _CLASS_WEAPON.GetPrimaryAmmoType()
    return "a"
end

function _CLASS_WEAPON.GetSecondaryAmmoType()
    return "b"
end

function _CLASS_WEAPON.GetPrintName()
    return "A Weapon"
end

local LOCAL_WEAPON = setmetatable({},_CLASS_WEAPON)

---------------------------------------------------------

_CLASS_PLAYER = {}
_CLASS_PLAYER.__index = _CLASS_PLAYER
_MIKU_DEBUG_LIB(_CLASS_PLAYER,"Player")

function _CLASS_PLAYER.Armor()
    -- No Armor
    return 0
end

function _CLASS_PLAYER.GetAmmoCount()
    return 0
end

function _CLASS_PLAYER.GetActiveWeapon()
    return LOCAL_WEAPON
end

function _CLASS_PLAYER.Team()
    return 0
end

-- DARKRP LOL
function _CLASS_PLAYER.isWanted()
    return false
end

function _CLASS_PLAYER.getDarkRPVar()
    return false
end

DarkRP = {}

function DarkRP.formatMoney()
    return "$100"
end

------------------------------------------------------------

team = {}

function team.GetName(team_id)
    return "Some Team"
end
