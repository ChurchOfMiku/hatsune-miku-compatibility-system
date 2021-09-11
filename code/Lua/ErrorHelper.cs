#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox;
using Miku.Lua.Objects;

namespace Miku.Lua
{
	/// <summary>
	/// The purpose of this class is to try and create useful error messages from an executor's state.
	/// </summary>
    static class ErrorHelper
	{
		public static string Check(Function func, int pc)
		{
			if (func.Prototype.UserFunc != null)
			{
				return "Error occured in C# code.";
			}
			uint instr = func.Prototype.Code[pc];
			var OP = (OpCode)(instr & 0xFF);
			int A = (int)((instr >> 8) & 0xFF);
			int B = (int)((instr >> 24) & 0xFF);
			int C = (int)((instr >> 16) & 0xFF);
			int D = (int)((instr >> 16) & 0xFFFF);

			switch(OP)
			{
				case OpCode.CALL:
				case OpCode.CALLM:
					return $"Attempt to call `{FindName( func, pc, A )}`.";

				default: return $"No specialized error message for opcode `{OP}`.";
			}
		}

		private static string FindName( Function func, int pc, int slot )
		{
			string suffix = "";
			Top:
			var name = FindNameSimple(func,pc,slot);
			if (name != null)
			{
				return name + suffix;
			}

			pc--;
			for ( ;pc >= 0; pc--)
			{
				uint instr = func.Prototype.Code[pc];
				var OP = (OpCode)(instr & 0xFF);
				int A = (int)((instr >> 8) & 0xFF);
				int B = (int)((instr >> 24) & 0xFF);
				int C = (int)((instr >> 16) & 0xFF);
				int D = (int)((instr >> 16) & 0xFFFF);

				switch ( OP )
				{
					case OpCode.MOV:
						if (A == slot)
						{
							slot = D;
							goto Top;
						}
						break;
					case OpCode.GGET:
						if (A == slot)
						{
							var global = func.Prototype.GetConstGC( D );
							return global + suffix;
						}
						break;
					case OpCode.TGETS:
						if (A == slot)
						{
							var field = func.Prototype.GetConstGC(C);
							suffix = "." + field.CheckString() + suffix;
							slot = B;
							goto Top;
						}
						break;
					default:
						Log.Info( "?? " + OP );
						break;
				}
			}
			return "[?]" + suffix;
		}

		// Adapted from https://github.com/LuaJIT/LuaJIT/blob/4deb5a1588ed53c0c578a343519b5ede59f3d928/src/lj_debug.c#L147
		private static string? FindNameSimple( Function func, int pc, int slot )
		{
			//Log.Info( $"=> {pc} {slot}" );
			/*for ( int i = 0; i < func.Prototype.LocalInfo.Count; i++ )
			{
				var entry = func.Prototype.LocalInfo[i];
				//Log.Info( $"{entry.Name} {entry.Start} {entry.End}" );
			}*/
			for ( int i = 0; i < func.Prototype.LocalInfo.Count; i++ )
			{
				var entry = func.Prototype.LocalInfo[i];
				if ( pc < entry.Start ) break;
				if ( pc < entry.End )
				{
					if ( slot == 0 )
					{
						return entry.Name;
					}
					slot--;
				}
			}
			return null;
		}
	}
}
