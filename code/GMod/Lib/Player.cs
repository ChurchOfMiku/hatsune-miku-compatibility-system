using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Miku.Lua;
using Sandbox;

namespace Miku.GMod.Lib
{
	class Player
	{

		public Player(GModMachineBase machine)
		{
			machine.RunFile( "glib/player.lua" );
			var class_player = machine.Env.Get( "_CLASS_PLAYER" ).CheckTable();
			machine.Ents.ClassPlayer = class_player;

			class_player.Set( "Health", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var ply = args[0].CheckTable().UserData as Sandbox.Player;
				return new ValueSlot[] { ValueSlot.Number( ply.Health ) };
			} ));

			class_player.Set( "Nick", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var ply = args[0].CheckTable().UserData as Sandbox.Player;
				var client = ply.GetClientOwner();
				return new ValueSlot[] { ValueSlot.String( client.Name ) };
			} ) );

			class_player.Set( "GetShootPos", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var ply = args[0].CheckTable().UserData as Sandbox.Player;
				var pos = ply.EyePos; // PROBABLY EXTREMELY WRONG!
				return new ValueSlot[] { machine.Vector(pos) };
			} ) );

			class_player.Set( "GetAimVector", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var ply = args[0].CheckTable().UserData as Sandbox.Player;
				var pos = ply.EyeRot.Forward; // PROBABLY EXTREMELY WRONG!
				return new ValueSlot[] { machine.Vector( pos ) };
			} ) );

			class_player.Set( "FireBullets", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
				var ply = args[0].CheckTable().UserData as Sandbox.Player;
				Log.Info( "FireBullets" );
				return null;
			} ) );

			if (machine.IsClient)
			{
				machine.Env.Set( "LocalPlayer", ValueSlot.UserFunction( ( ValueSlot[] args, Executor ex ) => {
					var result = machine.Ents.Get( Local.Pawn );
					return new ValueSlot[] { ValueSlot.Table( result ) };
				}));
			}
		}
	}
}
