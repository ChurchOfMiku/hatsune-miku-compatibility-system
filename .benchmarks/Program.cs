using BenchmarkDotNet.Running;

namespace Miku.Benchmarks
{
	static class Program
	{
		public static void Main( string[] args ) =>
			BenchmarkSwitcher.FromAssembly( typeof( Program ).Assembly )
							 .Run( args );
	}
}