#nullable enable

using System;

namespace Miku.Lua
{
	using UserFunction = Func<Executor, ValueSlot?>;

	class Function
	{
		public ProtoFunction Prototype;
		public Table Env;
		public Executor.UpValueBox[] UpValues;

		public Function( ProtoFunction prototype, Table env, Executor.UpValueBox[] upvals = null! )
		{
			if (upvals == null)
			{
				upvals = new Executor.UpValueBox[0];
			}
			Prototype = prototype;
			Env = env;
			UpValues = upvals;
		}

		public Executor Call(LuaMachine machine, ValueSlot[]? args = null)
		{
			var exec = new Executor( this, args ?? new ValueSlot[0], machine );
			exec.Run();
			return exec;
		}
	}

	class ProtoFunction
	{
		public byte flags;
		public byte numArgs;
		public byte numSlots;

		// All this is either filled in or irrelevant because we are a user function.
		public uint[] code = null!;
		public ushort[] UpValues = null!;
		public ValueSlot[] constGC = null!;
		public double[] constNum = null!;

		public string DebugName = "? (UNNAMED)";

		/// <summary>
		/// If set, this object actually just acts as a reference to the UserFunction in question.
		/// </summary>
		public UserFunction? UserFunc = null;

		public ValueSlot GetConstGC( int index )
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
