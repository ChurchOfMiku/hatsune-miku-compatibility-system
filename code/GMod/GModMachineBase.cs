
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
			// Setup glua-specific constants.
			// SEE https://wiki.facepunch.com/gmod/Global_Variables
			{
				Env.Set( "CLIENT", ValueSlot.Bool( IsClient ) );
				Env.Set( "SERVER", ValueSlot.Bool( IsServer ) );

				Env.Set( "CLIENT_DLL", IsClient ? ValueSlot.TRUE : ValueSlot.NIL );
				Env.Set( "GAME_DLL",   IsServer ? ValueSlot.TRUE : ValueSlot.NIL );
			}

			// Setup string metatable.
			PrimitiveMeta.MetaString = new Table();
			PrimitiveMeta.MetaString.Set( "__index", Env.Get( "string" ) );

			RunFile( "glib/types.lua" );
			RunFile( "glib/stubs_sh.lua" );
			RunFile( "glib/gamemode.lua" );

			new Lib.Player( this );

			//RunFile( "glib_official/garrysmod/lua/includes/util.lua" ); // also includes util/color.lua
			//RunFile( "glib_official/garrysmod/lua/includes/modules/hook.lua" );
			RunFile( "glib_official/garrysmod/lua/includes/init.lua" );


			//RunFile( "glib/globals.lua" );
			//RunFile( "glib/enums_sh.lua" );
			//RunFile( "glib/hook.lua" );
			//new Lib.Weapon( this );

			RunHookFunction = Env.Get( "hook" ).CheckTable().Get( "Run" ).CheckFunction();
			//MakeVectorFunction = Env.Get( "Vector" ).CheckFunction();
		}

		public ValueSlot Vector(Vector3 vec)
		{
			var res = MakeVectorFunction.Call( this, new ValueSlot[] {
				ValueSlot.Number(vec.x),
				ValueSlot.Number(vec.y),
				ValueSlot.Number(vec.z) } );
			return res[0];
		}

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
			RunFile( "glib_official/garrysmod/lua/includes/extensions/client/globals.lua" );

			new Lib.Draw2D( this );
			RunFile( "glib/draw2d.lua" ); // stub file ATM
		}

		public void Frame()
		{
			Host.AssertClient();
			Local.Hud.DeleteChildren( true );

			RunHook( "HUDPaint", new ValueSlot[0] );
		}
	}

	class GmodMachineServer : GModMachineBase
	{
		public override bool IsClient => false;

		public GmodMachineServer()
		{

		}
	}
}
