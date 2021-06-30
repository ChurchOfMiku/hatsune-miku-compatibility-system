#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Sandbox;

namespace Miku.Lua
{
    class Profiler
    {
		static bool ENABLE = true;
		static Stopwatch SW = new Stopwatch();
		static OpCode? CurrentOp;
		static string? CurrentFunc;

		static Dictionary<OpCode, double> TableOps = new Dictionary<OpCode, double>();
		static Dictionary<string, double> TableFuncs = new Dictionary<string, double>();

		public static void Update(OpCode op, string func)
		{
			Cycle();
			CurrentOp = op;
			CurrentFunc = func;
		}

		public static void UpdateUserFunc(string func)
		{
			Cycle();
			CurrentOp = null;
			CurrentFunc = func;
		}

		public static void Stop()
		{
			Cycle();
			CurrentOp = null;
			CurrentFunc = null;
		}

		private static void Cycle()
		{
			if (!ENABLE)
			{
				return;
			}
			var t = SW.Elapsed.TotalMilliseconds;
			if ( CurrentOp != null )
			{
				double x = TableOps.GetValueOrDefault(CurrentOp.Value);
				x += t;
				TableOps[CurrentOp.Value] = x;
			}
			if (CurrentFunc != null)
			{
				double x = TableFuncs.GetValueOrDefault( CurrentFunc );
				x += t;
				TableFuncs[CurrentFunc] = x;
			}
			SW.Restart();
		}

		[ClientCmd("lua_stats")]
		private static void Dump()
		{
			DumpTable( TableFuncs );
			Log.Info( "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~" );
			DumpTable( TableOps );
		}

		private static void DumpTable<K>( Dictionary<K, double> table )
		{
			double total = table.Sum( ( a ) => a.Value );
			var sorted = table.ToList();
			sorted.Sort( ( a, b ) => {
				return a.Value.CompareTo( b.Value );
			} );
			int i = 1;
			foreach ( var pair in sorted )
			{
				Log.Info( $"{sorted.Count-i+1}. {pair} ( {(pair.Value / total * 100):F1}% )" );
				i++;
			}
		}
    }
}
