
using Miku.Lua;
using Miku.GMod.Entities;
using Sandbox;

namespace Miku.GMod
{
	abstract class GModMachineBase : Lua.LuaMachine
	{
		Function RunHookFunction;
		Function MakeVectorFunction;
		public EntityRegistry Ents = new EntityRegistry();

		public abstract bool IsClient { get; }
		public bool IsServer { get => !IsClient; }

		public GModMachineBase()
		{
			RunFile( "glib/globals.lua" );
			RunFile( "glib/enums_sh.lua" );
			RunFile( "glib/hook.lua" );
			new Lib.Player( this );
			new Lib.Weapon( this );

			RunHookFunction = Env.Get( "hook" ).CheckTable().Get( "Run" ).CheckFunction();
			MakeVectorFunction = Env.Get( "Vector" ).CheckFunction();
		}

		/*public ValueSlot Vector(Vector3 vec)
		{
			var res = MakeVectorFunction.Call( this, new ValueSlot[] {
				ValueSlot.Number(vec.x),
				ValueSlot.Number(vec.y),
				ValueSlot.Number(vec.z) } );
			return res[0];
		}*/

		public void LoadSWEP(string class_name, string filename)
		{
			// TODO use a dirname instead!
			
			/*var swep_table = new Table();
			Env.Set( "SWEP", ValueSlot.Table(swep_table) );

			swep_table.Set( "Primary", ValueSlot.Table( new Table() ) );
			swep_table.Set( "Secondary", ValueSlot.Table( new Table() ) );

			RunFile( filename );

			Env.Set( "SWEP", ValueSlot.NIL );

			Ents.RegisterClass( class_name, ScriptedEntityKind.Weapon, swep_table );*/
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
			//RunFile( "glib/enums_cl.lua" );
			//new Lib.Draw2D( this );
		}

		public void Frame()
		{
			//Host.AssertClient();
			//Local.Hud.DeleteChildren( true );

			//RunHook( "HUDPaint", new ValueSlot[0] );
		}
	}

	class GmodMachineServer : GModMachineBase
	{
		public override bool IsClient => false;

		public GmodMachineServer()
		{
			Env.Set( "SERVER", ValueSlot.TRUE );
		}
	}
}
