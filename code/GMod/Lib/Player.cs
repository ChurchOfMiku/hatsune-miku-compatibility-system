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

			class_player.Set( "Health", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var ply = args[0].CheckTable().UserData as Sandbox.Player;
				return new ValueSlot[] { ValueSlot.Number( ply.Health ) };
			} ));

			class_player.Set( "Nick", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
				var ply = args[0].CheckTable().UserData as Sandbox.Player;
				var client = ply.GetClientOwner();
				return new ValueSlot[] { ValueSlot.String( client.Name ) };
			} ) );

			if (machine.IsClient)
			{
				machine.Env.Set( "LocalPlayer", ValueSlot.UserFunction( ( ValueSlot[] args, Table env ) => {
					var result = machine.Ents.Get( Local.Pawn );
					return new ValueSlot[] { ValueSlot.Table( result ) };
				}));
			}
		}
	}
}
