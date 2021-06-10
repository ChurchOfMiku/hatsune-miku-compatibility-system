hook = {}

local hook_registry = {}

function hook.Add(event,id,func)
    if type(id) ~= "string" then
        error("non-string hook ids not supported "..id)
    end
    hook_registry[event] = func
end

function hook.Run(event,...)
    local f = hook_registry[event]
    if f then f(...) end
end
