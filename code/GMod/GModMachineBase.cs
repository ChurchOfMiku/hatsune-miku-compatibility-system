
using Miku.Lua;
using Miku.GMod.Entities;
using Sandbox;

namespace Miku.GMod
{
	class GModMachineBase : Lua.LuaMachine
	{
		Function RunHookFunction;
		EntityMapper Ents = new EntityMapper();

		public GModMachineBase()
		{
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

	class GmodMachineClient : GModMachineBase
	{
		public GmodMachineClient()
		{
			new Lib.Draw2D( this );
		}

		public void Frame()
		{
			Host.AssertClient();
			Local.Hud.DeleteChildren( true );

			RunHook( "HUDPaint", new ValueSlot[0] );
		}
	}
}
