using System;
using System.Reflection;

namespace MoonSharp.Interpreter.Compatibility.Frameworks
{
	class FrameworkStubs : FrameworkBase
	{
		public override MethodInfo GetAddMethod( EventInfo ei )
		{
			throw new NotImplementedException();
		}

		public override Assembly GetAssembly( Type t )
		{
			throw new NotImplementedException();
		}

		public override Type[] GetAssemblyTypes( Assembly asm )
		{
			throw new NotImplementedException();
		}

		public override Type GetBaseType( Type t )
		{
			throw new NotImplementedException();
		}

		public override ConstructorInfo[] GetConstructors( Type type )
		{
			throw new NotImplementedException();
		}

		public override Attribute[] GetCustomAttributes( Type t, bool inherit )
		{
			throw new NotImplementedException();
		}

		public override Attribute[] GetCustomAttributes( Type t, Type at, bool inherit )
		{
			throw new NotImplementedException();
		}

		public override EventInfo[] GetEvents( Type type )
		{
			throw new NotImplementedException();
		}

		public override FieldInfo[] GetFields( Type t )
		{
			throw new NotImplementedException();
		}

		public override Type[] GetGenericArguments( Type t )
		{
			throw new NotImplementedException();
		}

		public override MethodInfo GetGetMethod( PropertyInfo pi )
		{
			throw new NotImplementedException();
		}

		public override Type GetInterface( Type type, string name )
		{
			throw new NotImplementedException();
		}

		public override Type[] GetInterfaces( Type t )
		{
			throw new NotImplementedException();
		}

		public override MethodInfo GetMethod( Type type, string name )
		{
			throw new NotImplementedException();
		}

		public override MethodInfo GetMethod( Type resourcesType, string v, Type[] type )
		{
			throw new NotImplementedException();
		}

		public override MethodInfo[] GetMethods( Type type )
		{
			throw new NotImplementedException();
		}

		public override Type[] GetNestedTypes( Type type )
		{
			throw new NotImplementedException();
		}

		public override PropertyInfo[] GetProperties( Type type )
		{
			throw new NotImplementedException();
		}

		public override PropertyInfo GetProperty( Type type, string name )
		{
			throw new NotImplementedException();
		}

		public override MethodInfo GetRemoveMethod( EventInfo ei )
		{
			throw new NotImplementedException();
		}

		public override MethodInfo GetSetMethod( PropertyInfo pi )
		{
			throw new NotImplementedException();
		}

		public override bool IsAbstract( Type t )
		{
			throw new NotImplementedException();
		}

		public override bool IsAssignableFrom( Type current, Type toCompare )
		{
			throw new NotImplementedException();
		}

		public override bool IsDbNull( object o )
		{
			throw new NotImplementedException();
		}

		public override bool IsEnum( Type t )
		{
			throw new NotImplementedException();
		}

		public override bool IsGenericType( Type t )
		{
			throw new NotImplementedException();
		}

		public override bool IsGenericTypeDefinition( Type t )
		{
			throw new NotImplementedException();
		}

		public override bool IsInstanceOfType( Type t, object o )
		{
			throw new NotImplementedException();
		}

		public override bool IsInterface( Type t )
		{
			throw new NotImplementedException();
		}

		public override bool IsNestedPublic( Type t )
		{
			throw new NotImplementedException();
		}

		public override bool IsValueType( Type t )
		{
			throw new NotImplementedException();
		}

		public override bool StringContainsChar( string str, char chr )
		{
			throw new NotImplementedException();
		}
	}
}
