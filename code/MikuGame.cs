

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
	[Library( "hatsune-miku-compatibility-system" )]
	public partial class MikuGame : Sandbox.Game
	{
		public MikuGame()
		{
			GMod.Assets.SoundRegistry.ParseSoundScript( "lua/glib_official/garrysmod/scripts/sounds/hl2_game_sounds_weapons.txt" );
			GMod.GModGlobal.Init();
		}

		public override void FrameSimulate( Client cl )
		{
			base.FrameSimulate( cl );
			GMod.GModGlobal.Client.Frame();
		}

		int N = 0;

		[Event.Tick]
		void Tick()
		{
			if (IsServer)
			{
				if (N == 0)
				{
					var crate = new ModelEntity();
					crate.SetModel( "models/weapons/w_pistol.vmdl" );
					crate.Position = new Vector3( 500, 500, 500 );
					crate.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
					crate.Health = 1000;
					//ragdoll.PhysicsGroup.Velocity = EyeRot.Forward * 1000;
				}
				N = (N + 1) % 100;
			}
		}

		/// <summary>
		/// A client has joined the server. Make them a pawn to play with
		/// </summary>
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new MikuPlayer();
			client.Pawn = player;

			player.Respawn();
		}
	}
}
