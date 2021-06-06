﻿using Sandbox;
using System;
using System.Linq;

using Miku.Lua;

namespace Miku
{
	partial class MinimalPlayer : Player
	{
		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			//
			// Use WalkController for movement (you can make your own PlayerController for 100% control)
			//
			Controller = new WalkController();

			//
			// Use StandardPlayerAnimator  (you can make your own PlayerAnimator for 100% control)
			//
			Animator = new StandardPlayerAnimator();

			//
			// Use ThirdPersonCamera (you can make your own Camera for 100% control)
			//
			Camera = new ThirdPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			base.Respawn();
		}

		/// <summary>
		/// Called every tick, clientside and serverside.
		/// </summary>
		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			//
			// If you have active children (like a weapon etc) you should call this to 
			// simulate those too.
			//
			SimulateActiveChild( cl, ActiveChild );

			//
			// If we're running serverside and Attack1 was just pressed, spawn a ragdoll
			//
			if ( IsServer && Input.Pressed( InputButton.Attack1 ) )
			{
				var func = Dump.Read( new byte[] { 27, 76, 74, 2, 0, 9, 64, 116, 101, 115, 116, 46, 108, 117, 97, 200, 2, 0, 2, 11, 0, 5, 2, 45, 108, 1, 16, 53, 2, 0, 0, 52, 3, 3, 0, 62, 1, 1, 3,
					21, 4, 3, 0, 56, 4, 4, 3, 3, 4, 0, 0, 88, 4, 16, 128, 85, 4, 15, 128, 54, 4, 1, 0, 57, 4, 2, 4, 18, 5, 2, 0, 21, 6, 2, 0, 56, 6, 6, 2,
					24, 6, 0, 6, 66, 4, 3, 1, 54, 4, 1, 0, 57, 4, 2, 4, 18, 5, 3, 0, 21, 6, 2, 0, 56, 6, 6, 2, 34, 6, 1, 6, 66, 4, 3, 1, 88, 4, 236, 127,
					41, 4, 0, 0, 41, 5, 0, 0, 21, 6, 2, 0, 23, 6, 1, 6, 41, 7, 1, 0, 41, 8, 255, 255, 77, 6, 9, 128, 56, 10, 9, 3, 32, 10, 10, 5, 3, 10, 0, 0,
					88, 10, 4, 128, 56, 10, 9, 3, 32, 5, 10, 5, 56, 10, 9, 2, 32, 4, 10, 4, 79, 6, 247, 127, 18, 6, 4, 0, 54, 7, 3, 0, 57, 7, 4, 7, 33, 8, 0, 5,
					66, 7, 2, 0, 73, 6, 1, 0, 8, 97, 98, 115, 9, 109, 97, 116, 104, 11, 105, 110, 115, 101, 114, 116, 10, 116, 97, 98, 108, 101, 1, 2, 0, 0, 3, 1, 4, 2, 1, 1,
					1, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 6, 6, 8, 8, 8, 8, 8, 9, 9, 9, 9, 10, 10, 11, 11, 8, 15, 15, 15,
					15, 15, 15, 100, 105, 118, 105, 100, 101, 110, 100, 0, 0, 46, 100, 105, 118, 105, 115, 111, 114, 0, 0, 46, 112, 119, 114, 115, 0, 4, 42, 100, 98, 108, 115, 0, 0, 42, 97, 110,
					115, 0, 22, 20, 97, 99, 99, 117, 109, 0, 0, 20, 1, 4, 10, 2, 0, 10, 3, 0, 10, 105, 0, 1, 8, 0, 201, 1, 3, 0, 12, 0, 6, 0, 19, 36, 0, 22, 51, 0,
					0, 0, 55, 0, 1, 0, 41, 0, 68, 2, 41, 1, 34, 0, 54, 2, 1, 0, 18, 3, 0, 0, 18, 4, 1, 0, 66, 2, 3, 3, 54, 4, 2, 0, 18, 5, 0, 0, 39, 6,
					3, 0, 18, 7, 1, 0, 39, 8, 4, 0, 18, 9, 2, 0, 39, 10, 5, 0, 18, 11, 3, 0, 38, 5, 11, 5, 66, 4, 2, 1, 75, 0, 1, 0, 16, 32, 114, 101, 109, 97,
					105, 110, 100, 101, 114, 32, 35, 32, 117, 115, 105, 110, 103, 32, 116, 104, 101, 32, 69, 103, 121, 112, 116, 105, 97, 110, 32, 109, 101, 116, 104, 111, 100, 32, 105, 115, 32, 17, 32, 100,
					105, 118, 105, 100, 101, 100, 32, 98, 121, 32, 10, 112, 114, 105, 110, 116, 20, 101, 103, 121, 112, 116, 105, 97, 110, 95, 100, 105, 118, 109, 111, 100, 0, 17, 1, 19, 19, 20, 20, 20,
					20, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 105, 0, 5, 15, 106, 0, 0, 15, 100, 0, 4, 11, 109, 0, 0, 11, 0, 0} );
				Log.Info( "~> " + func );
			}
		}

		public override void OnKilled()
		{
			base.OnKilled();

			EnableDrawing = false;
		}
	}
}
