using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

using Miku.Lua;

using Sandbox.UI;

namespace Miku.GWART
{

	class GModGlobal
	{
		static GModMachineBase GModClient;

		[Event( "hotloaded" )]
		public static void Init()
		{
			if (Host.IsClient)
			{
				if (Local.Hud != null)
				{
					Local.Hud.Delete();
				}
				Local.Hud = new RootPanel();
				RenderBroke = false;
				GModClient = new GModMachineBase();
				GModClient.RunFile( "gmodbluehud.lua" );
			}
		}

		public static bool RenderBroke = false;

		public static void Frame()
		{
			if (RenderBroke)
			{
				return;
			}
			Host.AssertClient();
			Local.Hud.DeleteChildren(true);

			try
			{
				GModClient.RunHook( "HUDPaint", new ValueSlot[0] );
			} catch (Exception e)
			{
				Log.Warning( "render broke" );
				RenderBroke = true;
				throw;
			}
		}
	}
}
