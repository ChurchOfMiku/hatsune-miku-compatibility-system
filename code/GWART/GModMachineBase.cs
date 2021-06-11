
// GLua whatever abstraction runtime

using Miku.Lua;

namespace Miku.GWART
{
	class GModMachineBase : Lua.LuaMachine
	{
		Function RunHookFunction;

		public GModMachineBase()
		{
			Lib.Draw2D.Init( Env );
			RunFile( "glib/draw2d.lua" );

			RunFile( "glib/hook.lua" );
			RunFile( "glib/player.lua" );

			RunHookFunction = Env.Get( "hook" ).CheckTable().Get( "Run" ).CheckFunction();
		}

		public ValueSlot[] RunHook(string name, ValueSlot[] args)
		{
			var full_args = new ValueSlot[args.Length + 1];
			full_args[0] = ValueSlot.String( name );
			for (int i=0;i<args.Length;i++ )
			{
				full_args[i + 1] = args[i];
			}

			return RunHookFunction.Call( full_args );
		}
	}
}
