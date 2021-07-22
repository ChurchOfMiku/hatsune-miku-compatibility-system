using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Miku.GMod.Assets;

using Miku.Lua;

namespace Miku.GMod.Entities
{
	// Not sure if we actually need a separate viewmodel class.
	class GmodViewModel : BaseViewModel
	{
		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			Rotation = camSetup.Rotation;
			Position = camSetup.Position;

			camSetup.ViewModel.FieldOfView = FieldOfView;
		}
	}

	partial class GmodWeapon : BaseWeapon
	{
		// View Models
		public override string ViewModelPath {
			get {
				return AssetUtil.FixModelName( GetTable().Get( "ViewModel" ).CheckString() );
			}
		}

		public override void CreateViewModel()
		{
			Host.AssertClient();

			string view_model = ViewModelPath;

			if ( string.IsNullOrEmpty( view_model ) )
				return;

			ViewModelEntity = new GmodViewModel();
			ViewModelEntity.Position = Position;
			ViewModelEntity.Owner = Owner;
			ViewModelEntity.EnableViewmodelRendering = true;
			ViewModelEntity.UseAnimGraph = false;
			ViewModelEntity.SetModel( view_model );
			ViewModelEntity.FieldOfView = (float)GetTable().Get( "ViewModelFOV" ).CheckNumber();
		}

		[Net, OnChangedCallback]
		private string LuaClassName { get; set; }
		public GmodWeapon(string class_name)
		{
			LuaClassName = class_name;
			SetupLua();
		}

		// SBox needs a parameter-less ctor to init clientside.
		public GmodWeapon()
		{
			//Log.Info( "CL INIT ");
		}

		// DO NOT DELETE: Used for clientside setup.
		private void OnLuaClassNameChanged()
		{
			SetupLua();
		}

		private void SetupLua()
		{
			Log.Info( "class = " + LuaClassName );
			var machine = GModGlobal.GetMachine();
			var tab = machine.MakeTableWeapon(LuaClassName);
			var ent_data = machine.Ents.Get(this, tab);
			Log.Info( "setup done!" );
		}

		public override bool CanPrimaryAttack()
		{
			return CanAttack(InputButton.Attack1, "Primary");
		}

		public override bool CanSecondaryAttack()
		{
			return CanAttack( InputButton.Attack2, "Secondary" );
		}

		private Table GetTable()
		{
			var machine = GModGlobal.GetMachine();
			var ent_info = machine.Ents.Get( this );
			return ent_info.LuaTable;
		}

		private bool CanAttack(InputButton button, string name)
		{
			if ( !Owner.IsValid() ) return false;
			if ( !Input.Down( button ) )
			{
				return false;
			}
			// Semi-automatics only fire if the fire button has just been pressed
			var is_automatic = GetTable().Get( name ).CheckTable().Get( "Automatic" ).IsTruthy();
			if ( !is_automatic && !Input.Pressed( button ) )
			{
				return false;
			}

			// The rest of this is handled by Lua in PrimaryAttack.
			return true;
		}

		public override void AttackPrimary()
		{
			var machine = GModGlobal.GetMachine();
			var ent_info = machine.Ents.Get( this );
			var func = ent_info.LuaTable.Get( "PrimaryAttack" ).CheckFunction();
			func.Call(machine, new ValueSlot[] { ent_info.LuaValue } );
		}

		public override void AttackSecondary()
		{
			var machine = GModGlobal.GetMachine();
			var ent_info = machine.Ents.Get( this );
			var func = ent_info.LuaTable.Get( "SecondaryAttack" ).CheckFunction();
			func.Call( machine, new ValueSlot[] { ent_info.LuaValue } );
		}

		public override void Reload()
		{
			var machine = GModGlobal.GetMachine();
			var ent_info = machine.Ents.Get( this );
			var func = ent_info.LuaTable.Get( "Reload" ).CheckFunction();
			func.Call( machine, new ValueSlot[] { ent_info.LuaValue } );
		}
	}
}
