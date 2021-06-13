
using Miku.Lua;
using Miku.GMod.Entities;
using Sandbox;

namespace Miku.GMod
{
	abstract class GModMachineBase : Lua.LuaMachine
	{
		Function RunHookFunction;
		public EntityMapper Ents = new EntityMapper();

		public abstract bool IsClient { get; }
		public bool IsServer { get => !IsClient; }

		public GModMachineBase()
		{
			RunFile( "glib/globals.lua" );
			RunFile( "glib/hook.lua" );
			new Lib.Player( this );

			RunHookFunction = Env.Get( "hook" ).CheckTable().Get( "Run" ).CheckFunction();
		}

		public void LoadSWEP(string filename)
		{
			// TODO use a dirname instead!
			var swep_table = new Table();
			Env.Set( "SWEP", ValueSlot.Table(swep_table) );
			swep_table.Set( "Primary", ValueSlot.Table( new Table() ) );
			swep_table.Set( "Secondary", ValueSlot.Table( new Table() ) );
			RunFile( filename );
		}

		public ValueSlot[] RunHook(string name, ValueSlot[] args)
		{
			var full_args = new ValueSlot[args.Length + 1];
			full_args[0] = ValueSlot.String( name );
			for (int i=0;i<args.Length;i++ )
			{
				full_args[i + 1] = args[i];
			}

			return RunHookFunction.Call( this, full_args );
		}
	}

	class GmodMachineClient : GModMachineBase
	{
		public override bool IsClient => true;

		public GmodMachineClient()
		{
			Env.Set( "CLIENT", ValueSlot.TRUE );
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
