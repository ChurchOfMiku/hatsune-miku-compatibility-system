
using Miku.Lua;
using Miku.GMod.Entities;
using Sandbox;

namespace Miku.GMod
{
	abstract class GModMachine : Lua.LuaMachine
	{
		Function RunHookFunction;
		Function RegisterWeaponFunction;
		Function MakeWeaponFunction;

		Function MakeVectorFunction;
		public EntityRegistry Ents = new EntityRegistry();

		public abstract bool IsClient { get; }
		public bool IsServer { get => !IsClient; }

		public GModMachine()
		{
			DoBaseClassReplacement = true;

			// Setup glua-specific constants.
			// SEE https://wiki.facepunch.com/gmod/Global_Variables
			{
				Env.Set( "CLIENT", IsClient );
				Env.Set( "SERVER", IsServer );

				Env.Set( "CLIENT_DLL", IsClient ? ValueSlot.TRUE : ValueSlot.NIL );
				Env.Set( "GAME_DLL",   IsServer ? ValueSlot.TRUE : ValueSlot.NIL );
			}

			// Load Enum Dumps
			RunFile( "glib/enums_shared.lua" );
			if ( IsClient )
			{
				RunFile( "glib/enums_client.lua" );
			}
			else
			{
				RunFile( "glib/enums_server.lua" );
			}

			new Lib.Entity( this );
			new Lib.Player( this );
			new Lib.Weapon( this );
			SetupRealmInternalLibs();

			RunFile( "glib/types.lua" );
			RunFile( "glib/stubs.lua" );
			RunFile( "glib/globals.lua" );
			RunFile( "glib/gamemode.lua" );

			RunFile( "glib_official/garrysmod/lua/includes/init.lua" );
			RunHookFunction = Env.Get( "hook" ).CheckTable().Get( "Run" ).CheckFunction();
			RegisterWeaponFunction = Env.Get( "weapons" ).CheckTable().Get( "Register" ).CheckFunction();
			MakeWeaponFunction = Env.Get( "weapons" ).CheckTable().Get( "Get" ).CheckFunction();

			// STUPID:
			MakeVectorFunction = Env.Get( "Vector" ).CheckFunction();
		}

		protected abstract void SetupRealmInternalLibs();

		public ValueSlot Vector(Vector3 vec)
		{
			return MakeVectorFunction.Call( this, new ValueSlot[] {
				vec.x,
				vec.y,
				vec.z } ).GetResult( 0 );
		}

		public Vector3 VectorFromValue( ValueSlot v )
		{
			var t = v.CheckTable();
			var x = (float)t.Get( "x" ).CheckNumber();
			var y = (float)t.Get( "y" ).CheckNumber();
			var z = (float)t.Get( "z" ).CheckNumber();

			return new Vector3( x, y, z );
		}

		public Color ColorFromValue( ValueSlot v )
		{
			var t = v.CheckTable();
			int R = (int)t.Get( "r" ).CheckNumber();
			int G = (int)t.Get( "g" ).CheckNumber();
			int B = (int)t.Get( "b" ).CheckNumber();
			int A = (int)t.Get( "a" ).CheckNumber();

			return Color.FromBytes( R, G, B, A );
		}

		private string GetEntityName(string path)
		{
			var parts = path.Split( '/' );
			int i = parts.Length - 1;
			while (parts[i] == "")
			{
				i--;
			}
			return parts[i];
		}

		public void LoadWeapon(string path)
		{
			var name = GetEntityName( path );

			string init_path = path + (IsServer ? "/init.lua" : "/cl_init.lua");
			if (!CheckFile(init_path))
			{
				init_path = path + "/shared.lua";
			}
			
			var swep_table = new Table();
			Env.Set( "SWEP", swep_table );
			swep_table.Set( "Primary", new Table() );
			swep_table.Set( "Secondary", new Table() );

			RunFile( init_path );

			Env.Set( "SWEP", ValueSlot.NIL );

			Log.Info( "Register weapon: " + name );
			RegisterWeaponFunction.Call( this, new ValueSlot[] { swep_table, name } );
		}

		public Table MakeTableWeapon(string name)
		{
			var res = MakeWeaponFunction.Call( this, new ValueSlot[] { name } );
			return res.GetResult( 0 ).CheckTable();
		}

		public Executor RunHook(string name, ValueSlot[] args)
		{
			var full_args = new ValueSlot[args.Length + 1];
			full_args[0] = name;
			for (int i=0;i<args.Length;i++ )
			{
				full_args[i + 1] = args[i];
			}

			return RunHookFunction.Call( this, full_args );
		}
	}

	class GmodMachineClient : GModMachine
	{
		public override bool IsClient => true;

		public GmodMachineClient()
		{
			//RunFile( "glib_official/garrysmod/lua/includes/extensions/client/globals.lua" );
		}

		protected override void SetupRealmInternalLibs()
		{
			new Lib.Surface( this );
		}

		public void Frame()
		{
			Host.AssertClient();
			Local.Hud.DeleteChildren( true );

			RunHook( "HUDPaint", new ValueSlot[0] );
		}
	}

	class GmodMachineServer : GModMachine
	{
		public override bool IsClient => false;

		protected override void SetupRealmInternalLibs()
		{

		}

		public GmodMachineServer()
		{

		}
	}
}
