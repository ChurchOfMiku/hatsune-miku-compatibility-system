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

		public Player(GModMachine machine)
		{
			var class_player = machine.DefineClass( "Player" );
			machine.Ents.ClassPlayer = class_player;

			class_player.DefineFunc( "Health", ( Executor ex ) => {
				var ply = (Sandbox.Player)ex.GetArg( 0 ).CheckUserData().CheckEntity().Entity;
				return ply.Health;
			} );

			class_player.DefineFunc( "Alive", ( Executor ex ) => {
				var ply = (Sandbox.Player)ex.GetArg( 0 ).CheckUserData().CheckEntity().Entity;
				return ply.Health > 0;
			} );

			class_player.DefineFunc( "Nick", ( Executor ex ) => {
				var ply = (Sandbox.Player)ex.GetArg( 0 ).CheckUserData().CheckEntity().Entity;
				var client = ply.GetClientOwner();
				return client.Name;
			} );

			class_player.DefineFunc( "GetShootPos", ( Executor ex ) => {
				var ply = (Sandbox.Player)ex.GetArg( 0 ).CheckUserData().CheckEntity().Entity;
				var pos = ply.EyePos; // PROBABLY EXTREMELY WRONG!
				return machine.Vector(pos);
			} );

			class_player.DefineFunc( "GetAimVector", ( Executor ex ) => {
				var ply = (Sandbox.Player)ex.GetArg( 0 ).CheckUserData().CheckEntity().Entity;
				var pos = ply.EyeRot.Forward; // PROBABLY EXTREMELY WRONG!
				return machine.Vector( pos );
			} );

			class_player.DefineFunc( "GetActiveWeapon", ( Executor ex ) => {
				var ply = (Sandbox.Player)ex.GetArg( 0 ).CheckUserData().CheckEntity().Entity;
				return null; // TODO the following probably doesn't work on the client.
				//var weapon = ply.Inventory.Active;
				//return machine.Ents.Get(weapon).LuaValue;
			} );

			if (machine.IsClient)
			{
				machine.Env.DefineFunc( "LocalPlayer", ( Executor ex ) => {
					var result = machine.Ents.Get( Local.Pawn );
					return result.LuaValue;
				} );
			}
		}
	}
}
