using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace Miku.Lua.Vm2
{
	readonly struct LuaJitInstruction
	{
		public OpCode Op { get; }
		public byte A { get; }
		public byte B => (byte)(D >> 8);
		public byte C => (byte)D;
		public ushort D { get; }

		public LuaJitInstruction( OpCode op, byte a, ushort d )
		{
			Op = op;
			A = a;
			D = d;
		}

		public static LuaJitInstruction Decode( uint raw ) =>
			new( (OpCode)(byte)raw, (byte)(raw >> 8), (ushort)(raw >> 16) );

		public override string ToString() => $"{{ Op = {Op}, A = 0x{A:X2}, B = 0x{B:X2}, C = 0x{C:X2}, D = 0x{D:X4} }}";
	}
}
