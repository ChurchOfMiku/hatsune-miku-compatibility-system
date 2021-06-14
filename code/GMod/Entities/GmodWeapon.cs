using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using Miku.Lua;

namespace Miku.GMod.Entities
{
	class GmodWeapon : BaseWeapon
	{
		private Table LuaTable;
		public GmodWeapon(Table class_table)
		{
			LuaTable = new Table();
			LuaTable.Set( "Weapon", ValueSlot.Table(LuaTable) );
			LuaTable.MetaTable = class_table;
			Log.Info( "weapon created " + LuaTable );
		}

		// SBox needs a parameter-less ctor to init clientside.
		public GmodWeapon()
		{
			Log.Info( "TODO client setup for sweps" );
		}

		public override Entity Owner {
			get => base.Owner;
			set {
				// TODO use an __index function for this.
				var owner_tab = GModGlobal.GetMachine().Ents.Get( value );
				LuaTable.Set( "Owner", ValueSlot.Table( owner_tab ) );
				base.Owner = value;
			}
		}

		public override void AttackPrimary()
		{
			if (LuaTable != null)
			{
				var method = ValueOperations.Get( ValueSlot.Table( LuaTable ), ValueSlot.String( "PrimaryAttack" ) );
				method.CheckFunction().Call( GMod.GModGlobal.GModServer, new ValueSlot[] { ValueSlot.Table( LuaTable ) } );
			}
			//var method = LuaTable.Get( "PrimaryAttack" ).CheckFunction();
			//base.AttackPrimary();
			//var dmg = new DamageInfo();
			//dmg.Damage = 10;
			//Owner.TakeDamage( dmg );
		}
	}
}
