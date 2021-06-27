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
end

function AddConsoleCommand()

end

function AddCSLuaFile()

end

function CreateConVar()

end

sql = {}
function sql.Query(...)
    print("sql query",...)
end
