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
				Local.Hud = new RootPanel();

				GModClient = new GmodMachineClient();
				GModClient.RunFile( "test/gmodbluehud.lua" );
			}
		}
	}
}
