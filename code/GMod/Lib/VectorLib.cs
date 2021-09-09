using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Miku.Lua;
using Miku.Lua.Objects;
using Sandbox;

namespace Miku.GMod.Lib
{
	class VectorLib
	{
		private Table ClassVector;

		public ValueSlot MakeVector(Vector3 val)
		{
			if ( ClassVector == null)
			{
				throw new Exception( "vector metatable not ready!" );
			}
			return new UserData( TypeID.Vector, val, ClassVector );
		}

		public VectorLib( GModMachine machine )
		{
			ClassVector = machine.DefineClass( "Vector" );

			ClassVector.DefineFunc( "__add", ( Executor ex ) =>
			{
				var lhs = ex.GetArg( 0 );
				var rhs = ex.GetArg( 1 );
				return MakeVector( lhs.CheckUserData().CheckVector() + rhs.CheckUserData().CheckVector() );
			} );

			ClassVector.DefineFunc( "__mul", ( Executor ex ) =>
			{
				var lhs = ex.GetArg( 0 );
				var rhs = ex.GetArg( 1 );
				if (lhs.Kind == ValueKind.Number)
				{
					return MakeVector( rhs.CheckUserData().CheckVector() * (float)lhs.CheckNumber() );
				} else
				{
					if (rhs.Kind == ValueKind.Number)
					{
						return MakeVector( lhs.CheckUserData().CheckVector() * (float)rhs.CheckNumber() );
					} else
					{
						throw new Exception($"Vector.__mul: {lhs} {rhs}");
					}
				}
			} );

			machine.Env.DefineFunc( "Vector", ( Executor ex ) =>
			{
				var arg_count = ex.GetArgCount();
				var arg0 = ex.GetArg( 0 );
				if ( arg0.Kind == ValueKind.UserData )
				{
					throw new Exception( "todo copy vector" );
				} else if ( arg0.Kind == ValueKind.String )
				{
					throw new Exception( "todo parse string to vector" );
				} else
				{
					var x = (float)(arg0.TryGetNumber() ?? 0);
					var y = (float)(ex.GetArg(1).TryGetNumber() ?? 0);
					var z = (float)(ex.GetArg(2).TryGetNumber() ?? 0);
					return MakeVector(new Vector3(x,y,z));
				}
			} );
		}
	}
}
