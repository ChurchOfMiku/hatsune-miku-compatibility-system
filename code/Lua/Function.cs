#nullable enable

using System;

namespace Miku.Lua
{
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
}
