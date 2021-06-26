local ffi = {}
ffi.typeof = function(x) return "typeof("..x..")" end

-- DEBUG LIB: Probably going to be very annoying to actually implement.
local REGISTRY = _R
debug = {}
_R.miku_debug_lib(debug,"debug")
function debug.getregistry()
    return REGISTRY
end

-- OS LIB: Only used for timing. Unstub at some point please.
os = {}
_R.miku_debug_lib(os,"os")

function os.time()
    return 1234
end

-- MATH LIB:
function math.randomseed()
    -- STUB STUB
end

function math.min(x,y)
    if x < y then
        return x
    else
        return y
    end
end

function math.max(x,y)
    if x > y then
        return x
    else
        return y
    end
end

local module_cache = {}
module_cache.ffi = ffi
module_cache.bit = bit
module_cache.jit = {version_num = 20100}

function assert(v,msg,...)
    if not v then
        error(msg or "assertion failed!")
    end
    return v, msg, ...
end

function pairs(tab)
    return next, tab
end

package = {}
_R.miku_debug_lib(package,"package")

function package.seeall(mod)
    setmetatable(mod,{__index=_G})
end

function module(name,...)
    local tab = {}
    _R.miku_debug_lib(tab,"[module "..name.."]")
    _G[name] = tab
    setfenv(2,tab)

    local loaders = {...}
    for i =1,#loaders do
        loaders[i](tab)
    end
end

function require(name)
    -- Check cache
    if module_cache[name] then
        return module_cache[name]
    end

    -- Load.
    local mod = _R.miku_require(name)

    -- Write to cache.
    module_cache[name] = mod
    return mod
end
