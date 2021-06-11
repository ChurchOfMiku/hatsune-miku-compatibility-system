using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Miku.Lua;
using Sandbox;
using Sandbox.UI;

// Contains:
// - surface.*
// - draw.*
// - ScrW / ScrH

namespace Miku.GMod.Lib
{
	class Draw2D
	{
		private record Font
		{
			public string Family;
			public double Size;
			public double Weight;
		}

		private static Color ColorFromTable( Table t )
		{
			int R = (int)t.Get( "r" ).CheckNumber();
			int G = (int)t.Get( "g" ).CheckNumber();
			int B = (int)t.Get( "b" ).CheckNumber();
			int A = (int)t.Get( "a" ).CheckNumber();

			return Color.FromBytes( R, G, B, A );
		}

		private void ApplyFont(string name, PanelStyle style)
		{
			if (FontRegistry.TryGetValue(name, out Font font ) )
			{
				style.FontSize = Length.Pixels( (float)font.Size );
				//style.FontFamily = font.Family;
				style.FontWeight = (int)(font.Weight * 1.0);
			} else
			{
				float font_size = 12;
				style.FontSize = Length.Pixels( font_size );
				//Log.Warning( "Can't find font: " + name );
				//style.BackgroundColor = Color.Red;
			}
		}

		private Dictionary<string, Font> FontRegistry = new Dictionary<string, Font>();

		public Draw2D(GmodMachineClient machine)
		{
			var env = machine.Env;
			FontRegistry.Clear();

			// Console still breaks these but they're mostly accurate now.
			env.Set( "ScrW", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				return new ValueSlot[] { ValueSlot.Number( Screen.Width ) };
			} ) );

			env.Set( "ScrH", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				return new ValueSlot[] { ValueSlot.Number( Screen.Height ) };
			} ) );

			var draw_lib = new Table();
			draw_lib.DebugLibName = "draw";
			env.Set( "draw", ValueSlot.Table( draw_lib ) );

			draw_lib.Set( "RoundedBox", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) =>
			{
				double radius = args[0].CheckNumber();
				double x = args[1].CheckNumber();
				double y = args[2].CheckNumber();
				double w = args[3].CheckNumber();
				double h = args[4].CheckNumber();
				Color color = ColorFromTable( args[5].CheckTable() );

				var panel = Local.Hud.AddChild<Sandbox.UI.Panel>();
				panel.Style.Position = PositionMode.Absolute;
				//panel.Style.Padding = Length.Pixels( 0 );
				//panel.Style.Margin = Length.Pixels( 0 );

				panel.Style.Left = Length.Pixels( (float)x );
				panel.Style.Top = Length.Pixels( (float)y );
				panel.Style.Width = Length.Pixels( (float)w );
				panel.Style.Height = Length.Pixels( (float)h );
				var rad = Length.Pixels( (float)radius );
				panel.Style.BorderBottomLeftRadius = rad;
				panel.Style.BorderBottomRightRadius = rad;
				panel.Style.BorderTopLeftRadius = rad;
				panel.Style.BorderTopRightRadius = rad;
				panel.Style.BackgroundColor = color;

				return null;
			} ) );

			draw_lib.Set( "SimpleText", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				string text = args[0].ToString();
				string font_name = args[1].CheckString();
				double x = args[2].CheckNumber();
				double y = args[3].CheckNumber();
				Color color = ColorFromTable( args[4].CheckTable() );

				var label = Local.Hud.AddChild<Sandbox.UI.Label>();
				label.Style.Position = PositionMode.Absolute;

				label.Text = text;
				label.Style.Left = Length.Pixels( (float)x );
				label.Style.Top = Length.Pixels( (float)y );
				label.Style.FontColor = color;
				ApplyFont( font_name, label.Style );

				return null;
			}));

			var surface_lib = new Table();
			surface_lib.DebugLibName = "surface";
			env.Set( "surface", ValueSlot.Table( surface_lib ) );

			surface_lib.Set( "CreateFont", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var name = args[5].CheckString();

				FontRegistry[name] = new Font() {
					Family = args[0].CheckString(),
					Size = args[1].CheckNumber(),
					Weight = args[2].CheckNumber()
				};

				return null;
			} ) );

			machine.RunFile( "glib/draw2d.lua" );
		}
	}
}
