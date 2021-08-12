----------------------------------------------------------------------------
-- HMCS LuaJIT snippet generator module. Based on the LuaJIT bytecode listing module.
--
-- Copyright (C) 2021 Church of Miku. All rights reserved.
-- Released under the MIT license.
--
-- Original Copyright:
-- Copyright (C) 2005-2017 Mike Pall. All rights reserved.
-- Released under the MIT license. See Copyright Notice in luajit.h
----------------------------------------------------------------------------
--
-- This module produces C# code for all functions contained within a given file.
-- It can only be loaded through -l.
--
-- Example usage:
--
--   luajit -jhmcssnippetgen=FizzBuzz fizzbuzz.lua
--   luajit -jhmcssnippetgen=Factorial factorial.lua
--
-- Output will always go to stderr.
-- If a name is not provided through '=', __NAME__ is used as a placeholder.
--
----------------------------------------------------------------------------

local jutil = require "jit.util"
local jbc = require "jit.bc"
local vmdef = require "jit.vmdef"
local bit = require("bit")
local sub, gsub, format = string.sub, string.gsub, string.format
local byte, band, shr = string.byte, bit.band, bit.rshift
local bcline, bctargets = jbc.line, jbc.targets
local funcinfo, funcbc, funck = jutil.funcinfo, jutil.funcbc, jutil.funck
local bcnames = vmdef.bcnames
local stdout, stderr = io.stdout, io.stderr

local function csstrescape(str)
  str = gsub(str, "\"", "\\\"")
  str = gsub(str, "([^%w%p ])", function(c)
    if c == "\n" then
      return "\\n"
    elseif c == "\r" then
      return "\\r"
    else
      return format("\\x%02X", byte(c))
    end
  end)
  return str
end

local function bline(func, pc, prefix)
  local line = bcline(func, pc, prefix)
  line = sub(line, 1, -2)
  return line
end

local function iline(func, pc, prefix)
  local ins, m = funcbc(func, pc)
  if not ins then return end
  local opcode = band(ins, 0xff)
  local oidx = 6*opcode
  local opname = sub(bcnames, oidx+1, oidx+6)
  local a, d = band(shr(ins, 8), 0xff), band(shr(ins, 16), 0xffff)
  return format("new Instruction( \"%s\", 0x%04X%02X%02Xu, OpCode.%s, %d, %d )",
    csstrescape(bline(func, pc, prefix)), d, a, opcode, opname, a, d)
end

local function const(cons, indent)
  indent = indent or "        "
  if type(cons) == "string" then
    return format("%snew Constant( \"%s\" )",
      indent, csstrescape(cons))
  elseif type(cons) == "number" then
    return format("%snew Constant( %g )", indent, cons)
  elseif type(cons) == "proto" then
    local pi = funcinfo(cons)
    return format("%snew Constant( %s__%d_%d )",
      indent, originalname, pi.linedefined, pi.lastlinedefined)
  elseif type(cons) == "table" then
    local r = format("%snew Constant( new Dictionary<Constant, Constant>\n", indent)
    r = r .. format("%s{\n", indent)
    for k, v in pairs(cons) do
      r = r .. format("%s    [%s] = %s,\n",
        indent, const(k, ""), const(v, ""))
    end
    r = r .. format("%s} )", indent)
    return r
  else
    error(format("Type %s is not being handled by the constants loop", type(k)))
  end
end

-- Dump bytecode instructions of a function.
local function sdump(name, func, out)
  if not out then out = stdout end
  local fi = funcinfo(func)

  if fi.upvalues and fi.upvalues > 0 then
    error "Functions with upvalues are not supported."
  end

  local originalname = name
  name = format("%s__%d_%d", name, fi.linedefined, fi.lastlinedefined)
  out:write(format("// %s~%d\n", fi.loc, fi.lastlinedefined))
  out:write(format("public static LuaJitFunction %s { get; }\n", name))
  out:write(format("%s = new(\n", name))
  out:write(format("    nameof( %s ),\n", name))

  if fi.nconsts and fi.nconsts > 0 then
    out:write("    ImmutableArray.Create(\n")
    for n = 1, fi.nconsts do
      local k = funck(func, n-1)
      if type(k) ~= "number" then error("Non-numeric constant in constants?") end
      out:write(format("        %gd", k))
      if n < fi.nconsts then out:write(",\n") end
    end
    out:write(" ),\n")
  else
    out:write("    ImmutableArray<double>.Empty,\n")
  end

  if fi.gcconsts and fi.gcconsts > 0 then
    out:write("    ImmutableArray.Create(\n")
    for n = 1, fi.gcconsts do
      local k = funck(func, -n)
      if not k then error("Missing constant") end
      out:write(const(k, "        "))
      if n < fi.gcconsts then out:write(",\n") end
    end
    out:write(" ),\n")
  else
    out:write("    ImmutableArray<Constant>.Empty,\n")
  end

  -- Print out instructions.
  out:write("    ImmutableArray.Create(\n");
  local target = bctargets(func)
  for pc = 1, fi.bytecodes - 1 do
    local s = iline(func, pc, target[pc] and "=>")
    if not s then error("Less instructions than expected?") end
    out:write("        ")
    out:write(s)
    if pc < fi.bytecodes - 1 then out:write(",\n") end
  end
  out:write(" ) );\n")
  out:flush()
end

------------------------------------------------------------------------------

-- Active flag and output file handle.
local name, active, out

-- List handler.
local function h_list(func)
  return sdump(name, func, out)
end

-- Detach list handler.
local function slistoff()
  if active then
    active = false
    jit.attach(h_list)
    if out and out ~= stdout and out ~= stderr then out:close() end
    out = nil
  end
end

-- Open the output file and attach list handler.
local function sliston(pname)
  if active then slistoff() end
  out = stderr
  name = pname or "__NAME__"
  jit.attach(h_list, "bc")
  active = true
end

return {
  start = sliston
}
