//#define ENABLE_PROFILER
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
		static Stopwatch SW = new Stopwatch();
		static OpCode? CurrentOp;
		static string? CurrentFunc;

		static Dictionary<OpCode, double> TableOps = new Dictionary<OpCode, double>();
		static Dictionary<OpCode, int> TableOpCounts = new Dictionary<OpCode, int>();

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
			CurrentFunc = "[CSHARP] "+func;
		}

		public static void Stop()
		{
			Cycle();
			CurrentOp = null;
			CurrentFunc = null;
		}

		[Conditional("ENABLE_PROFILER")]
		private static void Cycle()
		{
			var t = SW.Elapsed.TotalMilliseconds;
			if ( CurrentOp != null )
			{
				double x = TableOps.GetValueOrDefault(CurrentOp.Value);
				x += t;
				TableOps[CurrentOp.Value] = x;

				int y = TableOpCounts.GetValueOrDefault(CurrentOp.Value);
				y += 1;
				TableOpCounts[CurrentOp.Value] = y;
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
			DumpTable( TableOps, TableOpCounts );
		}

		private static void DumpTable<K>( Dictionary<K, double> table, Dictionary<K, int>? table_counts = null )
		{
			double total = table.Sum( ( a ) => a.Value );
			var sorted = table.ToList();
			sorted.Sort( ( a, b ) => {
				return a.Value.CompareTo( b.Value );
			} );
			int i = 1;
			foreach ( var pair in sorted )
			{
				if ( table_counts != null )
				{
					int count = table_counts[pair.Key];
					Log.Info( $"{sorted.Count - i + 1}. {pair} ( {(pair.Value / total * 100):F1}% ) #{count:n0} AVG={pair.Value / count * 1_000_000:n0}ns" );
				} else
				{
					Log.Info( $"{sorted.Count-i+1}. {pair} ( {(pair.Value / total * 100):F1}% )" );
				}
				i++;
			}
		}
    }
}
