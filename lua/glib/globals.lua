
local msg_saved = ""

local function msg_inner(str)
    local split = string.find(str,"\n")
    if split == nil then
        msg_saved = str
        return
    end
    local a = string.sub(str,1,split-1)
    local b = string.sub(str,split+1)
    print(a)
    msg_inner(b)
    --[[if b == "" then
    end]]
end

function Msg(...)
    local parts = {...}
    local str = ""
    for i = 1,#parts do
        str = str..parts[i]
    end
    msg_inner(msg_saved..str)
end

local Msg = Msg
function MsgN(...)
    Msg(...,"\n")
end
