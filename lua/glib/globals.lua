-- This is wrong but should work for now.
Msg = print

function AddCSLuaFile()
    -- No-op for now.
end

function AccessorFunc()
    -- TODO
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
