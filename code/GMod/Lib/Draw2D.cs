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
		const string DEFAULT_FONT = "sans-serif";
		const float DEFAULT_SIZE = 16;

		const int TEXT_ALIGN_LEFT = 0;
		const int TEXT_ALIGN_CENTER = 1;
		const int TEXT_ALIGN_RIGHT = 2;
		const int TEXT_ALIGN_TOP = 3;
		const int TEXT_ALIGN_BOTTOM = 4;

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
				style.FontSize = Length.Pixels( DEFAULT_SIZE );
				//Log.Warning( "Can't find font: " + name );
				//style.BackgroundColor = Color.Red;
			}
		}

		private Dictionary<string, Font> FontRegistry = new Dictionary<string, Font>();
		private Color CurrentSurfaceColor = Color.Green;
		private string CurrentSurfaceMaterial = "";

		public Draw2D(GmodMachineClient machine)
		{
			var env = machine.Env;

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
				int align_x = args.Length >= 6 ? (int)args[5].CheckNumber() : TEXT_ALIGN_LEFT;
				int align_y = args.Length >= 7 ? (int)args[6].CheckNumber() : TEXT_ALIGN_TOP;

				var label = Local.Hud.AddChild<Sandbox.UI.Label>();
				label.Style.Position = PositionMode.Absolute;

				label.Text = text;
				
				label.Style.Top = Length.Pixels( (float)y );
				label.Style.Left = Length.Pixels( (float)x );
				label.Style.FontColor = color;

				// I could not for the life of me figure out how to do alignment.
				/*
				 * const int ALIGN_HACK_WIDTH = 1000;
				 * PanelTransform? xform = null;
				switch (align_x)
				{
					case TEXT_ALIGN_LEFT:
						break;
					case TEXT_ALIGN_CENTER:
						label.Style.Width = Length.Pixels( ALIGN_HACK_WIDTH );
						xform = new PanelTransform();
						xform.Value.AddTranslateX( Length.Percent( 1 ) );
						label.Style.TextAlign = TextAlign.Center;
						break;
					case TEXT_ALIGN_RIGHT:
						//label.Style.Width = Length.Pixels( ALIGN_HACK_WIDTH );
						//label.Style.Left = Length.Pixels( (float)x - ALIGN_HACK_WIDTH );
						//label.Style.TextAlign = TextAlign.Right;
						break;
				}
				if (align_y != TEXT_ALIGN_TOP )
				{
					Log.Warning( "TODO VERTICAL ALIGN" );
				}
				label.Style.Transform = xform;*/
				ApplyFont( font_name, label.Style );

				return null;
			}));

			var surface_lib = new Table();
			surface_lib.DebugLibName = "surface";
			env.Set( "surface", ValueSlot.Table( surface_lib ) );

			surface_lib.Set( "SetDrawColor", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) =>
			{
				CurrentSurfaceColor = ColorFromTable( args[0].CheckTable() );
				return null;
			} ) );

			surface_lib.Set( "SetMaterial", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) =>
			{
				CurrentSurfaceMaterial = args[0].CheckString();
				return null;
			} ) );

			surface_lib.Set( "DrawRect", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) =>
			{
				double x = args[0].CheckNumber();
				double y = args[1].CheckNumber();
				double w = args[2].CheckNumber();
				double h = args[3].CheckNumber();

				var panel = Local.Hud.AddChild<Sandbox.UI.Panel>();
				panel.Style.Position = PositionMode.Absolute;

				panel.Style.Left = Length.Pixels( (float)x );
				panel.Style.Top = Length.Pixels( (float)y );
				panel.Style.Width = Length.Pixels( (float)w );
				panel.Style.Height = Length.Pixels( (float)h );
				panel.Style.BackgroundColor = CurrentSurfaceColor;

				return null;
			} ) );

			surface_lib.Set( "DrawTexturedRect", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) =>
			{
				double x = args[0].CheckNumber();
				double y = args[1].CheckNumber();
				double w = args[2].CheckNumber();
				double h = args[3].CheckNumber();

				var panel = Local.Hud.AddChild<Sandbox.UI.Image>();
				panel.Style.Position = PositionMode.Absolute;

				panel.Style.Left = Length.Pixels( (float)x );
				panel.Style.Top = Length.Pixels( (float)y );
				panel.Style.Width = Length.Pixels( (float)w );
				panel.Style.Height = Length.Pixels( (float)h );
				panel.SetTexture("/img/"+CurrentSurfaceMaterial);

				return null;
			} ) );

			surface_lib.Set( "DrawOutlinedRect", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) =>
			{
				double x = args[0].CheckNumber();
				double y = args[1].CheckNumber();
				double w = args[2].CheckNumber();
				double h = args[3].CheckNumber();
				double border_w = args.Length > 4 ? args[4].CheckNumber() : 1;

				var panel = Local.Hud.AddChild<Sandbox.UI.Panel>();
				panel.Style.Position = PositionMode.Absolute;

				panel.Style.Left = Length.Pixels( (float)x );
				panel.Style.Top = Length.Pixels( (float)y );
				panel.Style.Width = Length.Pixels( (float)w );
				panel.Style.Height = Length.Pixels( (float)h );
				panel.Style.BorderColor = CurrentSurfaceColor;
				panel.Style.BorderWidth = Length.Pixels( (float)border_w );

				return null;
			} ) );

			surface_lib.Set( "CreateFont", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {

				if (args[1].Kind == ValueKind.Table)
				{
					var name = args[0].CheckString();
					var settings = args[1].CheckTable();

					FontRegistry[name] = new Font()
					{
						Family = settings.Get("font").TryGetString() ?? DEFAULT_FONT,
						Size = settings.Get("size").TryGetNumber() ?? DEFAULT_SIZE,
						Weight = settings.Get("weight").TryGetNumber() ?? 0
					};
				} else
				{
					var name = args[5].CheckString();

					FontRegistry[name] = new Font() {
						Family = args[0].CheckString(),
						Size = args[1].CheckNumber(),
						Weight = args[2].CheckNumber()
					};
				}


				return null;
			} ) );

			machine.RunFile( "glib/draw2d.lua" );
		}
	}
}
