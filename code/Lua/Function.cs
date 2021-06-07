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
	}
}
