#nullable enable

using System;

namespace Miku.Lua
{
	class Function
	{
		public ProtoFunction Prototype;
		public Table Env;
		public Executor.UpValueBox[] UpValues;

		public Function( Table env, ProtoFunction prototype, Executor.UpValueBox[] upvals)
		{
			Env = env;
			Prototype = prototype;
			UpValues = upvals;
		}

		public ValueSlot[] Call(ValueSlot[]? args = null, bool debug = false)
		{
			var exec = new Executor( this, args ?? new ValueSlot[0] );
			exec.Debug = debug;
			exec.Run();
			if ( exec.Results == null )
			{
				throw new Exception( "Executor did not return?" );
			}
			return exec.Results;
		}
	}
}
