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
		public static GmodMachineClient GModClient;

		[Event( "hotloaded" )]
		public static void Init()
		{
			if (Host.IsClient)
			{
				// TODO better HUD management
				if (Local.Hud != null)
				{
					Local.Hud.Delete();
				}
				Local.Hud = new RootPanelNoScaling();

				GModClient = new GmodMachineClient();
				GModClient.LoadSWEP( "test/Best of Toybox/lua/weapons/weapon_base/cl_init.lua" );
			}
		}
	}
}
