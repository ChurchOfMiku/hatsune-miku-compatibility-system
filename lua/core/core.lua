-- MATH
do
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
end

-- FFI
ffi.typeof = function(x) return "typeof("..x..")" end

-- JIT
jit.version_num = 20100

-- DEBUG
local REGISTRY = _R
function debug.getregistry()
    return REGISTRY
end

-- PACKAGE
function package.seeall(mod)
    setmetatable(mod,{__index=_G})
end

-- GLOBALS
function assert(v,msg,...)
    if not v then
        error(msg or "assertion failed!")
    end
    return v, msg, ...
end

function pairs(tab)
    return next, tab
end

function module(name,...)
    local tab = {}
    _R.miku_debug_lib(tab,name)
    _G[name] = tab
    setfenv(2,tab)

    local loaders = {...}
    for i =1,#loaders do
        loaders[i](tab)
    end
end

local MODULE_CACHE = {}
MODULE_CACHE.ffi = ffi
MODULE_CACHE.bit = bit
MODULE_CACHE.jit = jit

function require(name)
    -- Check cache
    if MODULE_CACHE[name] then
        return MODULE_CACHE[name]
    end

    -- Load.
    local mod = _R.miku_require(name)

    -- Write to cache.
    MODULE_CACHE[name] = mod
    return mod
end
