-- Type your code here or try samples.
-- Hot loops are JIT-compiled.
local t = {}
for i = 1, 100 do
  t[i] = i % 2 == 0 and "fizz" or "buzz"
end
