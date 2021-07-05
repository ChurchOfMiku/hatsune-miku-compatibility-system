using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox;
using Miku.Lua;

namespace Miku.GMod.Lib
{
	class Entity
	{
		public Entity( GModMachineBase machine )
		{
			var class_entity = machine.DefineClass( "Entity" );
			class_entity.DefineFunc( "GetTable", ( Executor ex ) =>
			{
				var ent_info = ex.GetArg( 0 ).CheckUserData().CheckEntity();
				return ent_info.LuaTable;
			} );

			class_entity.DefineFunc( "GetOwner", ( Executor ex ) =>
			{
				var ent_info = ex.GetArg( 0 ).CheckUserData().CheckEntity();
				var owner_info = machine.Ents.Get( ent_info.Entity.Owner );
				return owner_info.LuaValue;
			} );

			class_entity.DefineFunc( "FireBullets", ( Executor ex ) => {
				// TODO the spread isn't consistentent between CL and SV.
				// TODO a bunch of settings aren't supported
				var ent = ex.GetArg( 0 ).CheckUserData().CheckEntity().Entity;
				var bullet_info = ex.GetArg( 1 ).CheckTable();
				bullet_info.Log();
				if ( ex.GetArg( 2 ).IsTruthy() )
				{
					throw new Exception( "supress host events?" );
				}

				var src = machine.VectorFromValue( bullet_info.Get( "Src" ) );
				var base_dir = machine.VectorFromValue( bullet_info.Get( "Dir" ) );
				var spread = machine.VectorFromValue( bullet_info.Get( "Spread" ) );

				var damage = (float)bullet_info.Get( "Damage" ).CheckNumber();
				var force = (float)bullet_info.Get( "Force" ).CheckNumber();
				var num = (int)bullet_info.Get( "Num" ).CheckNumber();
				var radius = 2.0f;
				bool InWater = Physics.TestPointContents( src, CollisionLayer.Water );

				var dir_rot = Rotation.LookAt( base_dir );
				var SPREAD_MUL = 50f;

				for (int i=0;i<num;i++ )
				{
					var rand = Vector3.Random * SPREAD_MUL;
					var spread_rot = Rotation.From( rand.x * spread.x, rand.y * spread.y, 0 );

					var dir = (dir_rot * spread_rot).Forward;

					var stop = src + dir * 10000;

					// Copied from BaseWeapon.
					var tr = Trace.Ray( src, stop )
							.UseHitboxes()
							.HitLayer( CollisionLayer.Water, !InWater )
							.Ignore( ent )
							.Size( radius )
							.Run();

					// Show effects
					if (ent.IsClient)
					{
						DebugOverlay.Line( tr.StartPos, tr.EndPos, ent.IsServer ? new Color(0,1,1,0.5f) : new Color( 1, 1, 0, 0.5f ), 0.5f );
						tr.Surface.DoBulletImpact( tr );
					}
					if (ent.IsServer)
					{
						if ( tr.Entity != null )
						{
							var damage_info = new DamageInfo();
							damage_info.Damage = damage;
							tr.Entity.TakeDamage( damage_info );

						}
						if ( tr.Body != null )
						{
							float FORCE_MUL = 30000;
							tr.Body.ApplyImpulse( dir * force * FORCE_MUL );
						}
					}
				}

				return null;
			} );
		}
	}
}
