using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

using Miku.Lua;

using Sandbox.UI;

namespace Miku.GMod
{
	class RootPanelNoScaling : RootPanel
	{
		protected override void UpdateScale( Rect screenSize )
		{
			Scale = 1.0f;
		}
	}

	class GModGlobal
	{
		static bool DUMP_STATUS = true;

		public static GmodMachineClient Client;
		public static GmodMachineServer Server;

		public static GModMachine GetMachine()
		{
			if ( Client != null)
			{
				return Client;
			}

			return Server;
		}

		// Don't hotload for now, re-initializing everything takes too damn long.
		//[Event( "hotloaded" )]
		public static void Init()
		{
			if ( Host.IsServer )
			{
				Server = new GmodMachineServer();
			}
			if ( Host.IsClient )
			{
				// TODO better HUD management
				if ( Local.Hud != null )
				{
					Local.Hud.Delete();
				}
				Local.Hud = new RootPanelNoScaling();

				Client = new GmodMachineClient();
			}

			if (DUMP_STATUS)
			{
				Client?.Env.Dump( "client.json" );
				Server?.Env.Dump( "server.json" );
			}

			Client?.RunString( "team.SetUp(1,'some team',Color(255,0,0))", "blah" );
			Client?.RunFile( "scripts/free_darkrp_hud.lua" );

			Client?.LoadWeapon( "glib_official/garrysmod/gamemodes/base/entities/weapons/weapon_base" );
			Server?.LoadWeapon( "glib_official/garrysmod/gamemodes/base/entities/weapons/weapon_base" );

			//GetMachine().RunFile("scripts/H0L-D4/lua/autorun/holohud.lua");

			GetMachine().RunHook( "Initialize", new ValueSlot[0] );

			//Client?.RunFile( "test/patterns.lua" );

			//GModServer?.LoadSWEP( "weapon_base", "test/Best of Toybox/lua/weapons/weapon_base/init.lua" );
			//GModClient?.LoadSWEP( "weapon_base", "test/Best of Toybox/lua/weapons/weapon_base/cl_init.lua" );
		}
	}
}
