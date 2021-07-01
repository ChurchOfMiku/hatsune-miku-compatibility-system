
function isstring(x)
    return type(x) == "string"
end

function isfunction(x)
    return type(x) == "function"
end

function FindMetaTable(name)
    local result = _R[name]
    -- temp: create the table if not exist
    if result == nil then
        result = {}
        _R[name] = result
        _R.miku_debug_lib(result,"[class "..name.."]")
    end
    return result
end
