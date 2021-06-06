namespace Miku.Lua
{
	enum ValueKind
	{
		Nil,
		True,
		False,
		Number,
		String,
		Table,
		ProtoFunction
	}

	struct ValueSlot
	{
		ValueKind kind;
		object reference;
		double number;

		public static ValueSlot Nil()
		{
			return new ValueSlot() { kind = ValueKind.Nil };
		}

		public static ValueSlot String(string x)
		{
			return new ValueSlot() { kind = ValueKind.String, reference = x };
		}

		public static ValueSlot Number( double x )
		{
			return new ValueSlot() { kind = ValueKind.Number, number = x };
		}

		public static ValueSlot Table( Table x )
		{
			return new ValueSlot() { kind = ValueKind.Table, reference = x };
		}

		public static ValueSlot ProtoFunction( ProtoFunction x )
		{
			return new ValueSlot() { kind = ValueKind.ProtoFunction, reference = x };
		}
	}
}
