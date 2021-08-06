using Miku.Lua;
using Miku.Lua.Vm2;
using Xunit;
using Xunit.Abstractions;

namespace Miku.Tests.Vm2.LuaJit
{
	// NOTE: Jumps are stored with an offset of 0x8000
	// NOTE: Jumps are also relative to current instruction number.
	public class LuaJitInstructionTests
	{
		private readonly ITestOutputHelper _testOutputHelper;

		public LuaJitInstructionTests( ITestOutputHelper testOutputHelper )
		{
			_testOutputHelper = testOutputHelper;
		}

		[Theory]
		[InlineData( "0002   TNEW     0   0    ", 0x00000034u, OpCode.TNEW, (byte)0, (ushort)0 )]
		[InlineData( "0003   KSHORT   1   1    ", 0x00010129u, OpCode.KSHORT, (byte)1, (ushort)1 )]
		[InlineData( "0004   KSHORT   2 100    ", 0x00640229u, OpCode.KSHORT, (byte)2, (ushort)100 )]
		[InlineData( "0005   KSHORT   3   1    ", 0x00010329u, OpCode.KSHORT, (byte)3, (ushort)1 )]
		[InlineData( "0006   FORI     1 => 0014", 0x8008014du, OpCode.FORI, (byte)1, (ushort)(0x8000 + 14 - 6) )]
		[InlineData( "0007   MODVN    5   4   0", 0x0400051au, OpCode.MODVN, (byte)5, (ushort)(/*B*/(4 << 8) | /*C*/0) )]
		[InlineData( "0008   ISNEN    5   1    ", 0x00010509u, OpCode.ISNEN, (byte)5, (ushort)1 )]
		[InlineData( "0009   JMP      5 => 0011", 0x80020558u, OpCode.JMP, (byte)5, (ushort)(0x8000 + 11 - 9) )]
		[InlineData( "0010   KSTR     5   0    ", 0x00000527u, OpCode.KSTR, (byte)5, (ushort)0 )]
		[InlineData( "0011   JMP      6 => 0012", 0x80010658u, OpCode.JMP, (byte)6, (ushort)(0x8000 + 12 - 11) )]
		[InlineData( "0012   KSTR     5   1    ", 0x00010527u, OpCode.KSTR, (byte)5, (ushort)1 )]
		[InlineData( "0013   TSETV    5   0   4", 0x0004053cu, OpCode.TSETV, (byte)5, (ushort)(/*B*/(0 << 8) | /*C*/4) )]
		[InlineData( "0014   FORL     1 => 0006", 0x7ff8014fu, OpCode.FORL, (byte)1, (ushort)(0x8000 + 6 - 14) )]
		[InlineData( "0015   RET0     0   1    ", 0x0001004bu, OpCode.RET0, (byte)0, (ushort)1 )]
		internal void Decode_DecodesProperly( string disasm, uint raw, OpCode opcode, byte a, ushort d )
		{
			LuaJitInstruction instr = LuaJitInstruction.Decode( raw );

			_testOutputHelper.WriteLine( $"Decoded instruction {disasm} ({raw:X8}): {instr}" );
			Assert.Equal( opcode, instr.Op );
			Assert.Equal( a, instr.A );
			Assert.Equal( d, instr.D );
			Assert.Equal( (byte)(d >> 8), instr.B );
			Assert.Equal( (byte)(d >> 0), instr.C );
		}
	}
}
