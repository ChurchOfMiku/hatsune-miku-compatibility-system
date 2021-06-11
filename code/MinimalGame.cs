
using Sandbox;
using System;
//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace Miku
{

	/// <summary>
	/// This is your game class. This is an entity that is created serverside when
	/// the game starts, and is replicated to the client. 
	/// 
	/// You can use this to create things like HUDs and declare which player class
	/// to use for spawned players.
	/// 
	/// Your game needs to be registered (using [Library] here) with the same name 
	/// as your game addon. If it isn't then we won't be able to find it.
	/// </summary>
	[Library( "miku" )]
	public partial class MinimalGame : Sandbox.Game
	{
		public MinimalGame()
		{
			GMod.GModGlobal.Init();
		}

		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );
			GMod.GModGlobal.GModClient.Frame();
		}

		/// <summary>
		/// A client has joined the server. Make them a pawn to play with
		/// </summary>
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new MinimalPlayer();
			client.Pawn = player;

			player.Respawn();
			player.Inventory.Add(new GMod.Entities.GmodWeapon(), true);
		}
	}
}
