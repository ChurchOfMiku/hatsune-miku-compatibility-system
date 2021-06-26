﻿using System;
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
		public static GmodMachineClient Client;
		public static GmodMachineServer Server;

		public static GModMachineBase GetMachine()
		{
			if ( Client != null)
			{
				return Client;
			}

			return Server;
		}

		[Event( "hotloaded" )]
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

			//Client?.RunFile( "test/free_darkrp_hud.lua" );
			Client?.RunFile( "test/patterns.lua" );

			//GModServer?.LoadSWEP( "weapon_base", "test/Best of Toybox/lua/weapons/weapon_base/init.lua" );
			//GModClient?.LoadSWEP( "weapon_base", "test/Best of Toybox/lua/weapons/weapon_base/cl_init.lua" );
		}
	}
}
