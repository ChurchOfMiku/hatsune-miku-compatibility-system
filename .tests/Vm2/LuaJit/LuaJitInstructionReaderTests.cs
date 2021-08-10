using System;
using System.Collections.Immutable;
using System.Linq;
using Miku.Lua.Vm2;
using Miku.Tests.TestData;
using Xunit;

namespace Miku.Tests.Vm2.LuaJit
{
	public class LuaJitInstructionReaderTests
	{
		private ImmutableArray<Instruction> Instructions => LuaJitFunctions.FizzBuzz.Instructions;
		private Instruction Instruction1 => Instructions[0];
		private Instruction Instruction2 => Instructions[1];

		[Fact]
		public void LuaJitInstructionReader_TryPeek_ReturnsFalseWhenEmpty()
		{
			LuaJitInstructionReader reader = new( ReadOnlyMemory<uint>.Empty );

			Assert.False( reader.TryPeek( out _ ) );
		}

		[Fact]
		public void LuaJitInstructionReader_TryPeek_ReturnsFalseWhenAtTheEnd()
		{
			LuaJitInstructionReader reader = new( new[] { Instruction1.Raw }, 1 );

			Assert.False( reader.TryPeek( out _ ) );
		}

		[Fact]
		public void LuaJitInstructionReader_TryRead_ReturnsFalseWhenEmpty()
		{
			LuaJitInstructionReader reader = new( ReadOnlyMemory<uint>.Empty );

			Assert.False( reader.TryRead( out _ ) );
		}

		[Fact]
		public void LuaJitInstructionReader_TryRead_ReturnsFalseWhenAtTheEnd()
		{
			LuaJitInstructionReader reader = new( new[] { Instruction1.Raw }, 1 );

			Assert.False( reader.TryRead( out _ ) );
		}

		[Fact]
		public void LuaJitInstructionReader_TryPeek_DoesNotAdvanceInTheStream()
		{
			LuaJitInstructionReader reader = new( new[] { Instruction1.Raw } );

			Assert.True( reader.TryPeek( out LuaJitInstruction instruction ) );
			Assert.Equal( 0, reader.Position );
			Assert.Equal( Instruction1.OpCode, instruction.OpCode );
			Assert.Equal( Instruction1.A, instruction.A );
			Assert.Equal( Instruction1.B, instruction.B );
			Assert.Equal( Instruction1.C, instruction.C );
			Assert.Equal( Instruction1.D, instruction.D );

			Assert.True( reader.TryPeek( out instruction ) );
			Assert.Equal( 0, reader.Position );
			Assert.Equal( Instruction1.OpCode, instruction.OpCode );
			Assert.Equal( Instruction1.A, instruction.A );
			Assert.Equal( Instruction1.B, instruction.B );
			Assert.Equal( Instruction1.C, instruction.C );
			Assert.Equal( Instruction1.D, instruction.D );
		}

		[Fact]
		public void LuaJitInstructionReader_TryRead_AdvancesInTheStream()
		{
			LuaJitFunction snippet = LuaJitFunctions.FizzBuzz;
			LuaJitInstructionReader reader = new( snippet.Instructions.Select( i => i.Raw ).ToArray() );

			for ( int idx = 0; idx < snippet.Instructions.Length; idx++ )
			{
				Instruction expected = snippet.Instructions[idx];

				Assert.Equal( idx, reader.Position );
				Assert.True( reader.TryRead( out LuaJitInstruction actual ) );
				Assert.Equal( idx + 1, reader.Position );
				Assert.Equal( expected.OpCode, actual.OpCode );
				Assert.Equal( expected.A, actual.A );
				Assert.Equal( expected.B, actual.B );
				Assert.Equal( expected.C, actual.C );
				Assert.Equal( expected.D, actual.D );
			}
		}

		[Fact]
		public void LuaJitInstructionReader_Position_ErrorsIfValueIsNegative()
		{
			LuaJitInstructionReader reader = new( ReadOnlyMemory<uint>.Empty );

			Assert.Throws<ArgumentOutOfRangeException>( () => reader.Position = -1 );
			Assert.Throws<ArgumentOutOfRangeException>( () => reader.Position = -256 );
		}

		[Fact]
		public void LuaJitInstructionReader_Position_ErrorsIfValueIsGreaterThanLength()
		{
			LuaJitInstructionReader reader = new( new[] { Instruction1.Raw } );

			Assert.Throws<ArgumentOutOfRangeException>( () => reader.Position = 2 );
			Assert.Throws<ArgumentOutOfRangeException>( () => reader.Position = 256 );
		}

		[Fact]
		public void LuaJitInstructionReader_Position_DoesNotErrorIfPositionIsLessThanOrEqualToLength()
		{
			LuaJitInstructionReader reader = new( new[] { Instruction1.Raw, Instruction2.Raw } );

			reader.Position = 1;
			Assert.True( reader.TryPeek( out _ ) );

			reader.Position = 2;
			Assert.False( reader.TryPeek( out _ ) );

			reader.Position = 0;
			Assert.True( reader.TryPeek( out _ ) );
		}
	}
}
