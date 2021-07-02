#nullable enable

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
	class Surface
	{
		const string DEFAULT_FONT = "sans-serif";
		const float DEFAULT_FONT_SIZE = 16;

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

		private void ApplyFont(Font? font, PanelStyle style)
		{
			if ( font != null )
			{
				style.FontFamily = font.Family;
				style.FontSize = Length.Pixels( (float)font.Size );
				style.FontWeight = (int)(font.Weight * 1.0);
			} else
			{
				style.FontSize = Length.Pixels( DEFAULT_FONT_SIZE );
			}
		}

		private Dictionary<string, Font> FontRegistry = new Dictionary<string, Font>();

		private Color CurrentColor = Color.White;
		private string CurrentMaterial = "";
		private Font? CurrentFont = null;
		private (int, int) CurrentTextPos = (0, 0);
		private Color CurrentTextColor = Color.Black;

		public Surface(GmodMachineClient machine)
		{
			var env = machine.Env;

			// These have deprecated aliases in the surface library. Makes as much sense to define them here as anywhere else.
			// The console still breaks these but they're mostly accurate now.
			var screen_width = env.DefineFunc( "ScrW", ( Executor ex ) => {
				return ValueSlot.Number( Screen.Width );
			} );

			var screen_height = env.DefineFunc( "ScrH", ( Executor ex ) => {
				return ValueSlot.Number( Screen.Height );
			} );

			var surface_lib = env.DefineLib("surface");

			surface_lib.Set( "ScreenWidth", screen_width );
			surface_lib.Set( "ScreenHeight", screen_height );

			surface_lib.DefineFunc( "SetFont", ( Executor ex ) => {
				CurrentFont = FontRegistry.GetValueOrDefault( ex.GetArg(0).CheckString(), null! );
				return null;
			} );

			surface_lib.DefineFunc( "SetTextPos", ( Executor ex ) => {
				int x = (int)ex.GetArg( 0 ).CheckNumber();
				int y = (int)ex.GetArg( 1 ).CheckNumber();
				CurrentTextPos = (x, y);
				return null;
			} );

			surface_lib.DefineFunc( "SetTextColor", ( Executor ex ) => {
				int r = (int)ex.GetArg( 0 ).CheckNumber();
				int g = (int)ex.GetArg( 1 ).CheckNumber();
				int b = (int)ex.GetArg( 2 ).CheckNumber();
				int a = (int)(ex.GetArg( 3 ).TryGetNumber() ?? 255);
				CurrentTextColor = Color.FromBytes(r,g,b,a);
				return null;
			} );

			surface_lib.DefineFunc( "GetTextSize", ( Executor ex ) =>
			{
				// TODO there seems to be some potential here but I couldn't get it to work.

				//var check_label = new Label();
				string text = ex.GetArg( 0 ).CheckString();

				/*var cc = new LayoutCascade();
				cc.SelectorChanged = true;
				cc.FontFamily = "sans-serif";
				cc.FontSize = Length.Pixels(10);
				cc.Scale = Vector2.One;

				check_label.PreLayout(cc);
				check_label.FinalLayout();*/
				//Log.Info(">> "+check_label.Box.Rect+" "+check_label.ScrollSize);

				var font = CurrentFont;

				int width = 5;
				int height = 5;

				if (CurrentFont != null)
				{
					width = (int)(CurrentFont.Size * (double)text.Length);
					height = (int)CurrentFont.Size;
				}

				ex.Return(ValueSlot.Number(width));
				return ValueSlot.Number(height);
			} );

			surface_lib.DefineFunc( "DrawText", ( Executor ex ) => {
				// forceAdditive is not implemented
				string text = ex.GetArg( 0 ).CheckString();

				var label = Local.Hud.AddChild<Sandbox.UI.Label>();
				label.Style.Position = PositionMode.Absolute;

				label.Text = text;

				label.Style.Left = Length.Pixels( CurrentTextPos.Item1 );
				label.Style.Top = Length.Pixels( CurrentTextPos.Item2 );
				label.Style.FontColor = CurrentTextColor;

				ApplyFont( CurrentFont, label.Style );

				return null;
			} );

			surface_lib.DefineFunc( "SetDrawColor", ( Executor ex ) =>
			{
				CurrentColor = ColorFromTable( ex.GetArg( 0 ).CheckTable() );
				return null;
			} );

			surface_lib.DefineFunc( "SetMaterial", ( Executor ex ) =>
			{
				CurrentMaterial = ex.GetArg(0).CheckString();
				return null;
			} );

			surface_lib.DefineFunc( "DrawRect", ( Executor ex ) =>
			{
				double x = ex.GetArg( 0 ).CheckNumber();
				double y = ex.GetArg( 1 ).CheckNumber();
				double w = ex.GetArg( 2 ).CheckNumber();
				double h = ex.GetArg( 3 ).CheckNumber();

				var panel = Local.Hud.AddChild<Sandbox.UI.Panel>();
				panel.Style.Position = PositionMode.Absolute;

				panel.Style.Left = Length.Pixels( (float)x );
				panel.Style.Top = Length.Pixels( (float)y );
				panel.Style.Width = Length.Pixels( (float)w );
				panel.Style.Height = Length.Pixels( (float)h );
				panel.Style.BackgroundColor = CurrentColor;

				return null;
			} );

			surface_lib.DefineFunc( "DrawTexturedRect", ( Executor ex ) =>
			{
				double x = ex.GetArg( 0 ).CheckNumber();
				double y = ex.GetArg( 1 ).CheckNumber();
				double w = ex.GetArg( 2 ).CheckNumber();
				double h = ex.GetArg( 3 ).CheckNumber();

				var panel = Local.Hud.AddChild<Sandbox.UI.Image>();
				panel.Style.Position = PositionMode.Absolute;

				panel.Style.Left = Length.Pixels( (float)x );
				panel.Style.Top = Length.Pixels( (float)y );
				panel.Style.Width = Length.Pixels( (float)w );
				panel.Style.Height = Length.Pixels( (float)h );
				panel.SetTexture("/img/"+CurrentMaterial);

				return null;
			} );

			surface_lib.DefineFunc( "DrawOutlinedRect", ( Executor ex ) =>
			{
				double x = ex.GetArg( 0 ).CheckNumber();
				double y = ex.GetArg( 1 ).CheckNumber();
				double w = ex.GetArg( 2 ).CheckNumber();
				double h = ex.GetArg( 3 ).CheckNumber();
				double border_w = ex.GetArgCount() > 4 ? ex.GetArg( 4 ).CheckNumber() : 1;

				var panel = Local.Hud.AddChild<Sandbox.UI.Panel>();
				panel.Style.Position = PositionMode.Absolute;

				panel.Style.Left = Length.Pixels( (float)x );
				panel.Style.Top = Length.Pixels( (float)y );
				panel.Style.Width = Length.Pixels( (float)w );
				panel.Style.Height = Length.Pixels( (float)h );
				panel.Style.BorderColor = CurrentColor;
				panel.Style.BorderWidth = Length.Pixels( (float)border_w );

				return null;
			} );

			surface_lib.DefineFunc( "CreateFont", ( Executor ex ) => {

				if ( ex.GetArg( 1 ).Kind == ValueKind.Table)
				{
					var name = ex.GetArg( 0 ).CheckString();
					var settings = ex.GetArg( 1 ).CheckTable();

					FontRegistry[name] = new Font()
					{
						Family = settings.Get("font").TryGetString() ?? DEFAULT_FONT,
						Size = settings.Get("size").TryGetNumber() ?? DEFAULT_FONT_SIZE,
						Weight = settings.Get("weight").TryGetNumber() ?? 0
					};
				} else
				{
					var name = ex.GetArg( 5 ).CheckString();

					FontRegistry[name] = new Font() {
						Family = ex.GetArg( 0 ).CheckString(),
						Size = ex.GetArg( 1 ).CheckNumber(),
						Weight = ex.GetArg( 2 ).CheckNumber()
					};
				}

				return null;
			} );
		}
	}
}
