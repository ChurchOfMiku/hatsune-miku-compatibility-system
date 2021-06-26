--[[for i=1,100 do
    if i%2 == 0 then continue end
    print(i)
    do continue end
end]]

--[[local i = 0
while i < 100 do
    i = i + 1
    if i % 10 > 5 then continue end
    print(i)
    continue
end]]

--[[local i = 0
repeat
    i = i + 1
    if i % 10 ~= 5 then continue end
    print(i)
    continue
until i > 100
]]

--[[local stuff = {5,23,5,2,4,2,1,5,2,6,3,1,2,5,6,4,2,1,2}
for k,v in pairs(stuff) do
    if k % 2 == 0 then continue end
    print(k,v)
    continue
end]]
