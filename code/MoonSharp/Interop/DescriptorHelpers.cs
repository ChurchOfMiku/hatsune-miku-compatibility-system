using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Compatibility;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Helper extension methods used to simplify some parts of userdata descriptor implementations
	/// </summary>
	public static class DescriptorHelpers
	{

		/// <summary>
		/// Gets the name of a conversion method to be exposed to Lua scripts
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static string GetConversionMethodName(this Type type)
		{
			StringBuilder sb = new StringBuilder(type.Name);

			for (int i = 0; i < sb.Length; i++)
				if (!char.IsLetterOrDigit(sb[i])) sb[i] = '_';

			return "__to" + sb.ToString();
		}

		/// <summary>
		/// Determines whether the string is a valid simple identifier (starts with letter or underscore
		/// and contains only letters, digits and underscores).
		/// </summary>
		public static bool IsValidSimpleIdentifier(string str)
		{
			if (string.IsNullOrEmpty(str))
				return false;

			if (str[0] != '_' && !char.IsLetter(str[0]))
				return false;

			for (int i = 1; i < str.Length; i++)
				if (str[i] != '_' && !char.IsLetterOrDigit(str[i]))
					return false;

			return true;
		}

		/// <summary>
		/// Converts the string to a valid simple identifier (starts with letter or underscore
		/// and contains only letters, digits and underscores).
		/// </summary>
		public static string ToValidSimpleIdentifier(string str)
		{
			if (string.IsNullOrEmpty(str))
				return "_";

			if (str[0] != '_' && !char.IsLetter(str[0]))
				str = "_" + str;

			StringBuilder sb = new StringBuilder(str);

			for (int i = 0; i < sb.Length; i++)
				if (sb[i] != '_' && !char.IsLetterOrDigit(sb[i]))
					sb[i] = '_';

			return sb.ToString();
		}

		/// <summary>
		/// Converts the specified name from underscore_case to camelCase.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public static string Camelify(string name)
		{
			StringBuilder sb = new StringBuilder(name.Length);

			bool lastWasUnderscore = false;
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] == '_' && i != 0)
				{
					lastWasUnderscore = true;
				}
				else
				{
					if (lastWasUnderscore)
						sb.Append(char.ToUpperInvariant(name[i]));
					else
						sb.Append(name[i]);

					lastWasUnderscore = false;
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Converts the specified name to one with an uppercase first letter (something to Something).
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public static string UpperFirstLetter(string name)
		{
			if (!string.IsNullOrEmpty(name))
				return char.ToUpperInvariant(name[0]) + name.Substring(1);

			return name;
		}
	}
}
