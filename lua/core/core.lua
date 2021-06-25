local ffi = {}
ffi.typeof = function(x) return "typeof("..x..")" end

string.char = function() error("string.char") end

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

function math.max(x,y)
    if x > y then
        return x
    else
        return y
    end
end

function module(name,...)
    local tab = {}
    _R.miku_debug_lib(tab,"[module "..name.."]")
    _G[name] = tab
    setfenv(2,tab)
end

function require(name)
    -- Check cache
    if module_cache[name] then
        return module_cache[name]
    end

    -- Load.
    local mod = _R.miku_bootstrap_require(name)

    -- Write to cache.
    module_cache[name] = mod
    return mod
end

--[[
function bunt()
    local meme = 1

    function test1(a)
        meme = meme + a + 10
    end

    function test2(a)
        meme = meme * 2 + a
    end

    function show()
        print(meme)
    end

    test1(5)
    test2(10)
    show()
end

bunt()
test1(9)
test2(44)
show()
]]
