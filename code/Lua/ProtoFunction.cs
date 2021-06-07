namespace Miku.Lua
{
	class ProtoFunction {
		public byte flags;
		public byte numArgs;
		public byte numSlots;

		public uint[] code;
		public ushort[] UpValues;
		public ValueSlot[] constGC;
		public double[] constNum;

		public ValueSlot GetConstGC(uint index)
		{
			return this.constGC[this.constGC.Length - 1 - index];
		}

		public double GetConstNum( uint index )
		{
			return this.constNum[index];
		}
	}
}
