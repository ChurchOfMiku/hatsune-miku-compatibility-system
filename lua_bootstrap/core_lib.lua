bit = {}
bit.band = function(a,b,c) error("bit.band",a,b,c) end

string = {}
string.sub = function() error("string.sub") end
string.byte = function() error("string.byte") end
string.char = function() error("string.char") end

local ffi = {}
ffi.typeof = function() error("ffi.typeof") end

require = function(name)
    if name == "ffi" then
        return ffi
    end
    _MIKU_BOOTSTRAP_REQUIRE(name)
end
