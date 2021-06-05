//Garry's Mod HUD
//Version 1.0
//Made by [MAP]Alberto

//---LANGUAGE!! (EDITABLE)---\\
local hptxt = "HEALTH"
local aptxt = "SUIT"
local ammotxt = "AMMO"
//---------------------------\\

--BUFFS
local hpbuff = 0
local hptime = 0
local hplast = 100
local apbuff = 0
local aptime = 0
local aplast = 0
local hptime2 = 0
local am1buff = 0
local am1time = 0
local am1last = 0
local am2buff = 0
local am2time = 0
local am2last = 0

function gmodbluehud()
if LocalPlayer():Health() < 1 then return false end
local client = LocalPlayer()
local hp = LocalPlayer():Health()
local ap = LocalPlayer():Armor()
local mag = LocalPlayer():GetActiveWeapon():Clip1()
local mag_extra = client:GetAmmoCount(client:GetActiveWeapon():GetPrimaryAmmoType())
local secondary_ammo = client:GetAmmoCount(client:GetActiveWeapon():GetSecondaryAmmoType())

surface.CreateFont ("coolvetica", ScreenScale(17), 400, true, true, "GmodBluHUDSm")
surface.CreateFont ("coolvetica", ScreenScale(35), 400, true, true, "GmodBluHUD")

//---HEALTH---\\
    if hp > 19 then
        draw.RoundedBox( 6, ScrW()/38, ScrH()/1.112, ScrH()/4.7, ScrW()/17, Color( 0, 0, 0, 100 ) )
        draw.SimpleText(hptxt,"HudHintTextLarge",ScrW()/26, ScrH()/1.064,Color(0,190,200,255))
        draw.SimpleText(hp,"GmodBluHUD",ScrW()/8.5, ScrH()/1.1,Color(0,190,200,255))
    elseif hp < 20 then
        draw.RoundedBox( 6, ScrW()/38, ScrH()/1.112, ScrH()/4.7, ScrW()/17, Color( 0, 0, 0, 100 ) )
        draw.SimpleText(hptxt,"HudHintTextLarge",ScrW()/26, ScrH()/1.064,Color(200,0,0,255))
        draw.SimpleText(hp,"GmodBluHUD",ScrW()/8.5, ScrH()/1.1,Color(200,0,0,255))
    end
    
-- Health Buff
local hp = client:Health()
if hplast ~= hp then
hpbuff = hpbuff + ( hp - hplast )
hptime = CurTime() + 1
hptime2 = CurTime() + 2   
hplast = hp
end

if hptime > CurTime() then
    if hpbuff < 0 then
        draw.SimpleText(hp,"GmodBluHUD",ScrW()/8.5, ScrH()/1.1, Color( 0, 255, 255, math.Clamp( ( hptime - CurTime() ) * 130, 0, 200 ) ))
        draw.SimpleText(hptxt,"HudHintTextLarge",ScrW()/26, ScrH()/1.064, Color( 0, 255, 255, math.Clamp( ( hptime - CurTime() ) * 130, 0, 200 ) ))
    end
if hpbuff > 0 then

        draw.SimpleText(hp,"GmodBluHUD",ScrW()/8.5, ScrH()/1.1, Color( 0, 255, 255, math.Clamp( ( hptime - CurTime() ) * 130, 0, 200 ) ))
        draw.SimpleText(hptxt,"HudHintTextLarge",ScrW()/26, ScrH()/1.064, Color( 0, 255, 255, math.Clamp( ( hptime - CurTime() ) * 130, 0, 200 ) ))
    end
else
    hpbuff = 0
end
--CRIT--
if hpbuff == 0 then
        if client:Health() < 19 then
              if hptime2 > CurTime() then
                    draw.RoundedBox( 6, ScrW()/38, ScrH()/1.112, ScrH()/4.7, ScrW()/17, Color( 255, 0, 0, math.Clamp( ( hptime2 - CurTime() ) * 130, 0, 10 ) ))
                    draw.SimpleText(hp,"GmodBluHUD",ScrW()/8.5, ScrH()/1.1, Color( 255, 0, 0, math.Clamp( ( hptime2 - CurTime() ) * 130, 0, 255 ) ))
                else
                    hptime2 = CurTime() + 1
                end
            else
        end
    else
end

//--ARMOR--\\

    if ap > 0 then
        draw.RoundedBox( 6, ScrW()/4.3, ScrH()/1.112, ScrH()/4.7, ScrW()/17, Color( 0, 0, 0, 100 ) )
        draw.SimpleText(aptxt,"HudHintTextLarge",ScrW()/4.07, ScrH()/1.064,Color(0,190,200,255))
        draw.SimpleText(ap,"GmodBluHUD",ScrW()/3.1, ScrH()/1.1,Color(0,190,200,255))
    else
    end
    
local ap = client:Armor()
if aplast ~= ap then
apbuff = apbuff + ( ap - aplast )
aptime = CurTime() + 1
aptime2 = CurTime() + 2   
aplast = ap
end

if aptime > CurTime() then
    if apbuff < 0 then
        draw.SimpleText(ap,"GmodBluHUD",ScrW()/3.1, ScrH()/1.1, Color( 0, 255, 255, math.Clamp( ( aptime - CurTime() ) * 130, 0, 200 ) ))
        draw.SimpleText(aptxt,"HudHintTextLarge",ScrW()/4.07, ScrH()/1.064, Color( 0, 255, 255, math.Clamp( ( aptime - CurTime() ) * 130, 0, 200 ) ))
    end
if apbuff > 0 then

        draw.SimpleText(ap,"GmodBluHUD",ScrW()/3.1, ScrH()/1.1, Color( 0, 255, 255, math.Clamp( ( aptime - CurTime() ) * 130, 0, 200 ) ))
        draw.SimpleText(aptxt,"HudHintTextLarge",ScrW()/4.07, ScrH()/1.064, Color( 0, 255, 255, math.Clamp( ( aptime - CurTime() ) * 130, 0, 200 ) ))
    end
else
    apbuff = 0
end

//--AMMUNITION--\\
    if LocalPlayer():GetActiveWeapon():GetPrintName() != "#HL2_GravityGun" then
        if secondary_ammo == 0 then
                    if mag > 0 then
                        draw.RoundedBox( 6, ScrW()/1.334, ScrH()/1.112, ScrH()/3.64, ScrW()/17, Color( 0, 0, 0, 100 ) )
                        draw.SimpleText(ammotxt,"HudHintTextLarge",ScrW()/1.31, ScrH()/1.064,Color(0,190,200,255))
                        draw.SimpleText(mag,"GmodBluHUD",ScrW()/1.215, ScrH()/1.1,Color(0,190,200,255))
                        draw.SimpleText(mag_extra,"GmodBluHUDSm",ScrW()/1.095, ScrH()/1.07,Color(0,190,200,255))
                    elseif mag == 0 then
                        draw.RoundedBox( 6, ScrW()/1.334, ScrH()/1.112, ScrH()/3.64, ScrW()/17, Color( 0, 0, 0, 100 ) )
                        draw.SimpleText(ammotxt,"HudHintTextLarge",ScrW()/1.31, ScrH()/1.064,Color(255,0,0,255))
                        draw.SimpleText(mag,"GmodBluHUD",ScrW()/1.215, ScrH()/1.1,Color(255,0,0,255))
                        draw.SimpleText(mag_extra,"GmodBluHUDSm",ScrW()/1.095, ScrH()/1.07,Color(255,9,9,255))
                    else
                end
            else
                    if mag > 0 then
                        draw.RoundedBox( 6, ScrW()/1.59, ScrH()/1.112, ScrH()/3.64, ScrW()/17, Color( 0, 0, 0, 100 ) )
                        draw.SimpleText(ammotxt,"HudHintTextLarge",ScrW()/1.555, ScrH()/1.064,Color(0,190,200,255))
                        draw.SimpleText(mag,"GmodBluHUD",ScrW()/1.42, ScrH()/1.1,Color(0,190,200,255))
                        draw.SimpleText(mag_extra,"GmodBluHUDSm",ScrW()/1.25, ScrH()/1.07,Color(0,190,200,255))
                        --ALT
                        draw.RoundedBox( 6, ScrW()/1.145, ScrH()/1.112, ScrH()/8, ScrW()/17, Color( 0, 0, 0, 100 ) )
                        draw.SimpleText("ALT","HudHintTextLarge",ScrW()/1.128, ScrH()/1.06,Color(0,190,200,255))
                        draw.SimpleText(secondary_ammo,"GmodBluHUD",ScrW()/1.073, ScrH()/1.1,Color(0,190,200,255))
                    elseif mag == 0 then
                        draw.RoundedBox( 6, ScrW()/1.59, ScrH()/1.112, ScrH()/3.64, ScrW()/17, Color( 0, 0, 0, 100 ) )
                        draw.SimpleText(ammotxt,"HudHintTextLarge",ScrW()/1.555, ScrH()/1.064,Color(255,0,0,255))
                        draw.SimpleText(mag,"GmodBluHUD",ScrW()/1.42, ScrH()/1.1,Color(255,0,0,255))
                        draw.SimpleText(mag_extra,"GmodBluHUDSm",ScrW()/1.25, ScrH()/1.07,Color(255,0,0,255))
                        --ALT
                        draw.RoundedBox( 6, ScrW()/1.145, ScrH()/1.112, ScrH()/8, ScrW()/17, Color( 0, 0, 0, 100 ) )
                        draw.SimpleText("ALT","HudHintTextLarge",ScrW()/1.128, ScrH()/1.06,Color(0,190,200,255))
                        draw.SimpleText(secondary_ammo,"GmodBluHUD",ScrW()/1.073, ScrH()/1.1,Color(0,190,200,255))
                end
            end
        else
        am1buff = 0
    end
    
if am1last ~= mag then
am1buff = am1buff + ( mag - am1last )
am1time = CurTime() + 1
am1last = mag
end

if mag > -1 then
if secondary_ammo == 0 then
if am1time > CurTime() then
    if am1buff < 0 then
        draw.SimpleText(mag,"GmodBluHUD",ScrW()/1.215, ScrH()/1.1, Color( 0, 255, 255, math.Clamp( ( am1time - CurTime() ) * 130, 0, 200 ) ))
    end
if am1buff > 0 then

        draw.SimpleText(mag,"GmodBluHUD",ScrW()/1.215, ScrH()/1.1, Color( 0, 255, 255, math.Clamp( ( am1time - CurTime() ) * 130, 0, 200 ) ))
    end
else
    am1buff = 0
end
else
if am1time > CurTime() then
    if am1buff < 0 then
        draw.SimpleText(mag,"GmodBluHUD",ScrW()/1.42, ScrH()/1.1, Color( 0, 255, 255, math.Clamp( ( am1time - CurTime() ) * 130, 0, 200 ) ))
    end
if am1buff > 0 then

        draw.SimpleText(mag,"GmodBluHUD",ScrW()/1.42, ScrH()/1.1, Color( 0, 255, 255, math.Clamp( ( am1time - CurTime() ) * 130, 0, 200 ) ))
    end
else
    am1buff = 0
end
end
else
end

--SECONDARY--
if am2last ~= secondary_ammo then
am2buff = am2buff + ( secondary_ammo - am2last )
am2time = CurTime() + 1
am2last = secondary_ammo
end

if secondary_ammo != 0 then
if am2time > CurTime() then
    if am2buff < 0 then
        draw.SimpleText(secondary_ammo,"GmodBluHUD",ScrW()/1.073, ScrH()/1.1, Color( 0, 255, 255, math.Clamp( ( am2time - CurTime() ) * 130, 0, 200 ) ))
    end
if am2buff > 0 then
        draw.SimpleText(secondary_ammo,"GmodBluHUD",ScrW()/1.073, ScrH()/1.1, Color( 0, 255, 255, math.Clamp( ( am2time - CurTime() ) * 130, 0, 200 ) ))
    end
else
    am2buff = 0
end
else
am2buff = 0
end
--GRENADES AND RPGS--
    if mag == -1 then
        if mag_extra > 0 then
            draw.RoundedBox( 6, ScrW()/1.245, ScrH()/1.112, ScrH()/4.8, ScrW()/17, Color( 0, 0, 0, 100 ) )
            draw.SimpleText(ammotxt,"HudHintTextLarge",ScrW()/1.225, ScrH()/1.064,Color(0,190,200,255))
            draw.SimpleText(mag_extra,"GmodBluHUD",ScrW()/1.14, ScrH()/1.1,Color(0,190,200,255))
        else
        end
    else
    end       
---
end
hook.Add("HUDPaint", "gmodbluehud", gmodbluehud)

local tohide = { -- This is a table where the keys are the HUD items to hide
["CHudHealth"] = true,
["CHudBattery"] = true,
["CHudAmmo"] = true,
["CHudSecondaryAmmo"] = true
}
local function HUDShouldDraw(name) -- This is a local function because all functions should be local unless another file needs to run it
if (tohide[name]) then     -- If the HUD name is a key in the table
return false;      -- Return false.
end
end
hook.Add("HUDShouldDraw", "gmodbluehudHudShouldDraw", HUDShouldDraw)
