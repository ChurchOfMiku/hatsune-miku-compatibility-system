namespace Miku.Lua
{
	class ProtoFunction {
		public byte flags;
		public byte numArgs;
		public byte numSlots;

		public uint[] code;
		public ushort[] upVars;
		public ValueSlot[] constGC;
		public double[] constNum;
	}
}
