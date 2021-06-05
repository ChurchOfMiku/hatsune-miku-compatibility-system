using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MoonSharp.Interpreter.Compatibility;
using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.Interop.BasicDescriptors;

namespace MoonSharp.Interpreter.Interop.UserDataRegistries
{
	/// <summary>
	/// Registry of all extension methods. Use UserData statics to access these.
	/// </summary>
	internal class ExtensionMethodsRegistry
	{
		private static MultiDictionary<string, IOverloadableMemberDescriptor> s_Registry = new MultiDictionary<string, IOverloadableMemberDescriptor>();
		private static MultiDictionary<string, UnresolvedGenericMethod> s_UnresolvedGenericsRegistry = new MultiDictionary<string, UnresolvedGenericMethod>();
		private static int s_ExtensionMethodChangeVersion = 0;

		private class UnresolvedGenericMethod
		{
			/*public readonly MethodInfo Method;
			public readonly InteropAccessMode AccessMode;
			public readonly HashSet<Type> AlreadyAddedTypes = new HashSet<Type>();

			public UnresolvedGenericMethod(MethodInfo mi, InteropAccessMode mode)
			{
				AccessMode = mode;
				Method = mi;
			}*/
		}

		private static object FrameworkGetMethods()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets all the extension methods which can match a given name
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public static IEnumerable<IOverloadableMemberDescriptor> GetExtensionMethodsByName(string name)
		{
			return new List<IOverloadableMemberDescriptor>(s_Registry.Find(name));
		}

		/// <summary>
		/// Gets a number which gets incremented everytime the extension methods registry changes.
		/// Use this to invalidate caches based on extension methods
		/// </summary>
		/// <returns></returns>
		public static int GetExtensionMethodsChangeVersion()
		{
			return s_ExtensionMethodChangeVersion;
		}


		/// <summary>
		/// Gets all the extension methods which can match a given name and extending a given Type
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="extendedType">The extended type.</param>
		/// <returns></returns>
		public static List<IOverloadableMemberDescriptor> GetExtensionMethodsByNameAndType(string name, Type extendedType)
		{
			// miku: this seems to be used by a couple of things
			throw new NotImplementedException();
			/*List<UnresolvedGenericMethod> unresolvedGenerics = null;

			lock (s_Lock)
			{
				unresolvedGenerics = s_UnresolvedGenericsRegistry.Find(name).ToList();
			}

			foreach (UnresolvedGenericMethod ugm in unresolvedGenerics)
			{
				ParameterInfo[] args = ugm.Method.GetParameters();
				if (args.Length == 0) continue;
				Type extensionType = args[0].ParameterType;

				Type genericType = GetGenericMatch(extensionType, extendedType);

				if (ugm.AlreadyAddedTypes.Add(genericType))
				{
					if (genericType != null)
					{
						MethodInfo mi = InstantiateMethodInfo(ugm.Method, extensionType, genericType, extendedType);
						if (mi != null)
						{
							if (!MethodMemberDescriptor.CheckMethodIsCompatible(mi, false))
								continue;

							var desc = new MethodMemberDescriptor(mi, ugm.AccessMode);

							s_Registry.Add(ugm.Method.Name, desc);
							++s_ExtensionMethodChangeVersion;
						}
					}
				}
			}

			return s_Registry.Find(name)
				.Where(d => d.ExtensionMethodType != null && Framework.Do.IsAssignableFrom(d.ExtensionMethodType, extendedType))
				.ToList();*/
		}

		/*private static MethodInfo InstantiateMethodInfo(MethodInfo mi, Type extensionType, Type genericType, Type extendedType)
		{
			Type[] defs = mi.GetGenericArguments();
			Type[] tdefs = Framework.Do.GetGenericArguments(genericType);

			if (tdefs.Length == defs.Length)
			{
				return mi.MakeGenericMethod(tdefs);
			}

			return null;
		}*/
	}
}
