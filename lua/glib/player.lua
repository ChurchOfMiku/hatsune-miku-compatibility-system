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

function _CLASS_PLAYER.Team()
    return 0
end

function _CLASS_PLAYER:MuzzleFlash()

end

function _CLASS_PLAYER:SetAnimation()

end

function _CLASS_PLAYER:ViewPunch()

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
