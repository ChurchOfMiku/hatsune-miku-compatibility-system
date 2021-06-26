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

		public string DebugName;

		public ValueSlot GetConstGC(int index)
		{
			return this.constGC[this.constGC.Length - 1 - index];
		}

		public double GetConstNum( int index )
		{
			return this.constNum[index];
		}

		public bool IsVarArg()
		{
			return (flags & 2) != 0;
		}
	}
}
