using System;

namespace Miku.Lua
{
	class Function
	{
		// TODO upvalues
		public ProtoFunction prototype;
		public Table env;
		public Executor.UpValueBox[] UpValues;

		public Function( Table env, ProtoFunction prototype, Executor.UpValueBox[] upvals)
		{
			this.env = env;
			this.prototype = prototype;
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
