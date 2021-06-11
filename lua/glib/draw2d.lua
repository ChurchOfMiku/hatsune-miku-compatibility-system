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
