using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoonSharp.Compatibility
{
	public class SboxWhitelistFix
	{
		// TODO: These "CultureInvariant" methods are not actually culture-invariant yet.
		// Might just wait for the whitelist to be properly updated, IDK.
		public static bool TryParseFloatCultureInvariant( string txt, out double res )
		{
			return double.TryParse( txt, out res );
		}

		public static bool TryParseHexCultureInvariant( string txt, out ulong res )
		{
			// NYI
			throw new Exception( "parsing" );
			res = 0;
			return false;
		}

		public static int ParseIntCultureInvariant( string txt )
		{
			return int.Parse( txt );
		}

		public static int ParseHexCultureInvariant( string txt )
		{
			// NYI
			throw new Exception( "parsing" );
			return 0;
		}

		public static string FloatToStringCultureInvariant( double n )
		{
			return n.ToString();
		}
	}
}
