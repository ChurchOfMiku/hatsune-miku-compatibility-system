
-- STUB FILE: Move the enums and delete it.

TEXT_ALIGN_LEFT	= 0
TEXT_ALIGN_CENTER = 1
TEXT_ALIGN_RIGHT = 2
TEXT_ALIGN_TOP = 3
TEXT_ALIGN_BOTTOM = 4

function surface.GetTextureID( name )
    return 0
end

render = render or {}

function render.GetScreenEffectTexture()
    return "NO_TEXTURE"
end
