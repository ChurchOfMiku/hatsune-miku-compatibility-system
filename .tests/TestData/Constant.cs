namespace Miku.Tests.TestData
{
	internal sealed class Constant : IEquatable<Constant?>
	{
		public ConstantType Type { get; }
		public object? Value { get; }

		private Constant( ConstantType type, object value )
		{
			Type = type;
			Value = value;
		}

		public Constant()
		{
			Type = ConstantType.Nop;
		}

		public Constant( string str ) : this( ConstantType.String, str )
		{
		}

		public Constant( double num ) : this( ConstantType.Number, num )
		{
		}

		public Constant( LuaJitFunction function ) : this( ConstantType.Function, function )
		{
		}

		public Constant( Dictionary<Constant, Constant> table ) : this( ConstantType.Table, table )
		{
		}

		public override bool Equals( object? obj ) => Equals( obj as Constant );
		public bool Equals( Constant? other ) => other != null && Type == other.Type && EqualityComparer<object?>.Default.Equals( Value, other.Value );
		public override int GetHashCode() => HashCode.Combine( Type, Value );

		public static bool operator ==( Constant? left, Constant? right ) => EqualityComparer<Constant>.Default.Equals( left, right );
		public static bool operator !=( Constant? left, Constant? right ) => !(left == right);
	}
}
