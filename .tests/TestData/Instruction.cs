using Miku.Lua;

namespace Miku.Tests.TestData
{
	internal sealed record Instruction( string Repr, uint Raw, OpCode OpCode, byte A, ushort D )
	{
		public byte B => (byte)(D >> 8);
		public byte C => (byte)D;
	}
}
