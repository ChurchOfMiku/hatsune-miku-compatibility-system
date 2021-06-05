using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter.Compatibility;
using MoonSharp.Interpreter.Interop.BasicDescriptors;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Standard descriptor for userdata types.
	/// </summary>
	public class StandardUserDataDescriptor : DispatchingUserDataDescriptor, IWireableDescriptor
	{
		/// <summary>
		/// Gets the interop access mode this descriptor uses for members access
		/// </summary>
		public InteropAccessMode AccessMode { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardUserDataDescriptor"/> class.
		/// </summary>
		/// <param name="type">The type this descriptor refers to.</param>
		/// <param name="accessMode">The interop access mode this descriptor uses for members access</param>
		/// <param name="friendlyName">A human readable friendly name of the descriptor.</param>
		public StandardUserDataDescriptor(Type type, InteropAccessMode accessMode, string friendlyName = null)
			: base(type, friendlyName)
		{
			if (accessMode == InteropAccessMode.NoReflectionAllowed)
				throw new ArgumentException("Can't create a StandardUserDataDescriptor under a NoReflectionAllowed access mode");

			if (Script.GlobalOptions.Platform.IsRunningOnAOT())
				accessMode = InteropAccessMode.Reflection;

			if (accessMode == InteropAccessMode.Default)
				accessMode = UserData.DefaultAccessMode;

			AccessMode = accessMode;

			FillMemberList();
		}

		/// <summary>
		/// Fills the member list.
		/// </summary>
		private void FillMemberList()
		{
			// miku: removed
		}

		public void PrepareForWiring(Table t)
		{
			if (AccessMode == InteropAccessMode.HideMembers || Framework.Do.GetAssembly(Type) == Framework.Do.GetAssembly(this.GetType()))
			{
				t.Set("skip", DynValue.NewBoolean(true));
			}
			else
			{
				t.Set("visibility", DynValue.NewString(this.Type.GetClrVisibility()));

				t.Set("class", DynValue.NewString(this.GetType().FullName));
				DynValue tm = DynValue.NewPrimeTable();
				t.Set("members", tm);
				DynValue tmm = DynValue.NewPrimeTable();
				t.Set("metamembers", tmm);

				Serialize(tm.Table, Members);
				Serialize(tmm.Table, MetaMembers);
			}
		}

		private void Serialize(Table t, IEnumerable<KeyValuePair<string, IMemberDescriptor>> members)
		{
			foreach (var pair in members)
			{
				IWireableDescriptor sd = pair.Value as IWireableDescriptor;

				if (sd != null)
				{
					DynValue mt = DynValue.NewPrimeTable();
					t.Set(pair.Key, mt);
					sd.PrepareForWiring(mt.Table);
				}
				else
				{
					t.Set(pair.Key, DynValue.NewString("unsupported member type : " + pair.Value.GetType().FullName));
				}
			}
		}
	}
}
