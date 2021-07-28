#nullable enable

using System;

namespace Miku.Lua.Objects
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
		public byte Flags;
		public byte NumArgs;
		public byte NumSlots; // this is ignored by the VM

		// All this is either filled in or irrelevant because we are a user function.
		public uint[] Code = null!;
		public ushort[] UpValues = null!;
		public ValueSlot[] ConstGC = null!;
		public double[] ConstNum = null!;
		public string ChunkName = null!;
		public uint[] LineInfo = null!;

		/// <summary>
		/// Used to tag User-Functions and for our crappy profiler.
		/// Where possible, use ChunkName + LineInfo instead.
		/// </summary>
		public string DebugName = "? (UNNAMED)";

		/// <summary>
		/// If set, this object actually just acts as a reference to the UserFunction in question.
		/// </summary>
		public UserFunction? UserFunc = null;

		public ValueSlot GetConstGC( int index )
		{
			return this.ConstGC[this.ConstGC.Length - 1 - index];
		}

		public double GetConstNum( int index )
		{
			return this.ConstNum[index];
		}

		public bool IsVarArg()
		{
			return (Flags & 2) != 0;
		}

		public string GetDebugLine(int pc)
		{
			if (LineInfo != null)
			{
				return ChunkName+":"+LineInfo[pc];
			} else
			{
				return DebugName;
			}
		}
	}
}
