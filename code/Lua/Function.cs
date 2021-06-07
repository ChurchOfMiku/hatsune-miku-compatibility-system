namespace Miku.Lua
{
	class Function
	{
		// TODO upvalues
		public ProtoFunction prototype;
		public Table env;

		public Function( Table env, ProtoFunction prototype )
		{
			this.env = env;
			this.prototype = prototype;
		}
	}
}
