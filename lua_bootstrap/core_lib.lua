bit = {}
bit.band = function(a,b,c) error("bit.band",a,b,c) end

string = {}
string.sub = function() error("string.sub") end
string.byte = function() error("string.byte") end
string.char = function() error("string.char") end

local ffi = {}
ffi.typeof = function(x) return "typeof["..x.."]" end

require = function(name)
    if name == "ffi" then
        return ffi
    end
    return _MIKU_BOOTSTRAP_REQUIRE(name)
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
