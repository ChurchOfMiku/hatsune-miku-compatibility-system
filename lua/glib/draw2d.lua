
TEXT_ALIGN_LEFT	= 0
TEXT_ALIGN_CENTER = 1
TEXT_ALIGN_RIGHT = 2
TEXT_ALIGN_TOP = 3
TEXT_ALIGN_BOTTOM = 4

function ScreenScale( size )
	return size * ( ScrW() / 640.0 )	
end

function Color(r,g,b,a)
    a = a or 255
    return {r=r,g=g,b=b,a=a}
end

function CurTime()
    return 4
end

function Material(name)
    return name
end
