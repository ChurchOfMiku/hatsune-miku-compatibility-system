#nullable enable

using System;

namespace Miku.Lua
{
	class Function
	{
		public ProtoFunction Prototype;
		public Table Env;
		public PrimitiveMetaTables PrimitiveMT;
		public Executor.UpValueBox[] UpValues;

		public Function( ProtoFunction prototype, Table env, PrimitiveMetaTables prim_meta, Executor.UpValueBox[] upvals = null! )
		{
			if (upvals == null)
			{
				upvals = new Executor.UpValueBox[0];
			}
			Prototype = prototype;
			Env = env;
			PrimitiveMT = prim_meta;
			UpValues = upvals;
		}

		public ValueSlot[] Call(LuaMachine machine, ValueSlot[]? args = null)
		{
			var exec = new Executor( this, args ?? new ValueSlot[0] );
			exec.Machine = machine;
			exec.Run();
			if ( exec.Results == null )
			{
				throw new Exception( "Executor did not return?" );
			}
			return exec.Results;
		}
	}
}
