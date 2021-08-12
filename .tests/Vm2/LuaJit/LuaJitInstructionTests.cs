using System.Linq;
using Miku.Lua;
using Miku.Lua.Vm2;
using Miku.Tests.TestData;
using Xunit;
using Xunit.Abstractions;

namespace Miku.Tests.Vm2.LuaJit
{
	public class LuaJitInstructionTests
	{
		private readonly ITestOutputHelper _testOutputHelper;

		public LuaJitInstructionTests( ITestOutputHelper testOutputHelper )
		{
			_testOutputHelper = testOutputHelper;
		}

		[Theory]
		[MemberData( nameof( GetDecodeData ) )]
		internal void Decode_DecodesProperly( string disasm, uint raw, OpCode opcode, byte a, ushort d )
		{
			LuaJitInstruction instr = LuaJitInstruction.Decode( raw );
			_testOutputHelper.WriteLine( $"Decoded instruction {disasm} ({raw:X8}): {instr}" );

			Assert.Equal( opcode, instr.OpCode );
			Assert.Equal( a, instr.A );
			Assert.Equal( d, instr.D );
			Assert.Equal( (byte)(d >> 8), instr.B );
			Assert.Equal( (byte)(d >> 0), instr.C );
		}

		[Theory]
		[MemberData( nameof( GetEncodeData ) )]
		internal void Encode_EncodesProperly( OpCode opCode, byte a, ushort d, uint expected )
		{
			LuaJitInstruction instr = new( opCode, a, d );

			uint encoded = instr.Encode();
			_testOutputHelper.WriteLine( $"Encoded instruction {instr}: {encoded:X8} (expected {expected})" );

			Assert.Equal( expected, encoded );
		}

		[Theory]
		[MemberData( nameof( GetEncodeAndDecodeData ) )]
		internal void DecodeAndEncode_RoundTripsProperly( uint raw )
		{
			LuaJitInstruction instr = LuaJitInstruction.Decode( raw );

			uint encoded = instr.Encode();

			Assert.Equal( raw, encoded );
		}

		private static TheoryData<string, uint, OpCode, byte, ushort> GetDecodeData()
		{
			TheoryData<string, uint, OpCode, byte, ushort> data = new();
			foreach ( Instruction instruction in LuaJitFunctions.All.SelectMany( snippet => snippet.Instructions ) )
			{
				data.Add( instruction.Repr, instruction.Raw, instruction.OpCode, instruction.A, instruction.D );
			}
			return data;
		}

		private static TheoryData<OpCode, byte, ushort, uint> GetEncodeData()
		{
			TheoryData<OpCode, byte, ushort, uint> data = new();
			foreach ( Instruction instruction in LuaJitFunctions.All.SelectMany( snippet => snippet.Instructions ) )
			{
				data.Add( instruction.OpCode, instruction.A, instruction.D, instruction.Raw );
			}
			return data;
		}

		private static TheoryData<uint> GetEncodeAndDecodeData()
		{
			TheoryData<uint> data = new();
			foreach ( Instruction instruction in LuaJitFunctions.All.SelectMany( snippet => snippet.Instructions ) )
			{
				data.Add( instruction.Raw );
			}
			return data;
		}
	}
}
