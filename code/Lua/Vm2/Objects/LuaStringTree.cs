using System;
#if MIKU_CONSOLE
using System.Diagnostics;
#endif
using System.Threading;

#nullable enable

namespace Miku.Lua.Vm2
{
	/// <summary>
	/// A tree that stores deduplicated <see cref="LuaString"/>.
	/// </summary>
	public sealed class LuaStringTree : IDisposable
	{
		/// <summary>
		/// A node of the tree.
		/// NEVER USE THE PARAMETERLESS CONSTRUCTOR FOR IT.
		/// ALWAYS USE ONE OF THE Create METHODS.
		/// </summary>
		private readonly struct Node
		{
			public readonly object NodeLock;
			public readonly WeakReference<LuaString> Value;
			public readonly int[] ChildrenIndexes;

			// Need this dummy value because structs can't have
			// parameterless constructors.
			private Node( int _dummy )
			{
				NodeLock = new object();
				// The constructor accepts null but isn't properly annotated for it.
				Value = new WeakReference<LuaString>( null! );
				ChildrenIndexes = new int[byte.MaxValue];
			}

			private Node( LuaString str ) : this( 0 )
			{
				Value = new WeakReference<LuaString>( str );
			}

			public static Node Create() => new( 0 );
			public static Node Create( LuaString str ) => new( str );
		}

		private readonly ReaderWriterLockSlim _nodesLock = new( LockRecursionPolicy.SupportsRecursion );
		private Node[] _nodes = new Node[1]
		{
			Node.Create( LuaString.Empty )
		};
		private bool disposedValue;

		/// <summary>
		/// Initializes a new string tree.
		/// </summary>
		public LuaStringTree()
		{
		}

		~LuaStringTree()
		{
			_nodesLock?.Dispose();
		}

		public int Count { get; private set; } = 1;
		public int Capacity => _nodes.Length;

		/// <summary>
		/// Gets a string by its id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public LuaString? GetById( int id )
		{
			if ( id < 0 )
				throw new ArgumentOutOfRangeException( nameof( id ), "Id must be positive." );

			_nodesLock.EnterReadLock();
			try
			{
				if ( id >= Count || !_nodes[id].Value.TryGetTarget( out var str ) )
					return null;
				return str;
			}
			finally
			{
				_nodesLock.ExitReadLock();
			}
		}

		/// <summary>
		/// Gets a deduplicated string from the tree.
		/// </summary>
		/// <param name="bytes">The bytes that compose the string.</param>
		/// <returns></returns>
		public LuaString? GetInterned( ReadOnlySpan<byte> bytes )
		{
			_nodesLock.EnterReadLock();

			Node node;
			try
			{
				node = _nodes[0];
				int idx = 0;
				while ( idx < bytes.Length )
				{
					var childIdx = node.ChildrenIndexes[idx++];
					// Strings can't reference the empty string so we use it
					// as an invalid child index value.
					if ( childIdx == 0 )
						return null;
					node = _nodes[childIdx];
				}
			}
			finally
			{
				_nodesLock.ExitReadLock();
			}

			return node.Value.TryGetTarget( out LuaString? value ) ? value : null;
		}

		private int Insert( in Node node )
		{
			_nodesLock.EnterWriteLock();
			try
			{
				if ( Count + 1 >= Capacity )
				{
					Array.Resize( ref _nodes, Capacity << 1 );
				}

				var idx = Count++;
				_nodes[idx] = node;
				return idx;
			}
			finally
			{
				_nodesLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Walk down the tree to get the index of node we want.
		/// Creating the nodes along the way if needed.
		/// </summary>
		/// <param name="bytes">The bytes of the string to get the node of.</param>
		/// <returns>The index at which the node we want is at.</returns>
		/// NOTE: Returning an index here lets us avoid recursive locking and also
		///       any issues with the array being reallocated between the call to
		///       this method and the access of the node in a child method.
		///       It *should* also let the JIT access the offset of the weak ref
		///       directly instead of going through some indirect ways because we
		///       get a ref to the node first.
		private int GetOrCreateNode( ReadOnlySpan<byte> bytes )
		{
			_nodesLock.EnterUpgradeableReadLock();
			try
			{
				var nodeIdx = 0;
				int bytesIdx = 0;
				while ( bytesIdx < bytes.Length )
				{
					byte value = bytes[bytesIdx++];
					// Obtain a reference to the array field where the
					// index of the child node is/will be stored in.
					// Every time we read it we'll actually be dereferencing
					// it so we can check it multiple times.
					ref var childIdx = ref _nodes[nodeIdx].ChildrenIndexes[value];
					if ( childIdx == 0 )
					{
						_nodesLock.EnterWriteLock();
						try
						{
							if ( childIdx == 0 )
							{
								childIdx = Insert( Node.Create() );
							}
						}
						finally
						{
							_nodesLock.ExitWriteLock();
						}
					}

#if MIKU_CONSOLE
					Debug.Assert( childIdx != 0 );
#endif
					nodeIdx = childIdx;
				}

				return nodeIdx;
			}
			finally
			{
				_nodesLock.ExitUpgradeableReadLock();
			}
		}

		/// <summary>
		/// Adds a string to the tree if it's not already in it.
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public LuaString Intern( ReadOnlySpan<byte> bytes )
		{
			var index = GetOrCreateNode( bytes );

			WeakReference<LuaString> value;
			object nodeLock;
			_nodesLock.EnterReadLock();
			try
			{
				value = _nodes[index].Value;
				nodeLock = _nodes[index].NodeLock;
			}
			finally
			{
				_nodesLock.ExitReadLock();
			}

			if ( !value.TryGetTarget( out var str ) )
			{
				lock ( nodeLock )
				{
					if ( !value.TryGetTarget( out str ) )
					{
						str = new LuaString(
							index,
							(int)Hash.GetXXHash32HashCode( bytes ),
							bytes.ToImmutableArray() );
						value.SetTarget( str );
					}
				}
			}

			return str;
		}

		/// <summary>
		/// Adds the provided string to the tree if it's not already in it.
		/// This does not recreate the string so it doesn't return it.
		/// </summary>
		/// <param name="str"></param>
		public void Intern( LuaString str )
		{
			var nodeIdx = GetOrCreateNode( str._buffer.AsSpan() );

			WeakReference<LuaString> value;
			object nodeLock;
			_nodesLock.EnterReadLock();
			try
			{
				value = _nodes[nodeIdx].Value;
				nodeLock = _nodes[nodeIdx].NodeLock;
			}
			finally
			{
				_nodesLock.ExitReadLock();
			}

			if ( !value.TryGetTarget( out _ ) )
			{
				lock ( nodeLock )
				{
					if ( !value.TryGetTarget( out _ ) )
					{
						value.SetTarget( str );
					}
				}
			}
		}

		/// <summary>
		/// Creates and interns a part of a string.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="start"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public LuaString Substring( LuaString str, int start, int length )
		{
			if ( str is null )
			{
				throw new ArgumentNullException( nameof( str ) );
			}

			return Intern( str._buffer.AsSpan().Slice( start, length ) );
		}

		#region IDisposable

		private void Dispose( bool disposing )
		{
			if ( !disposedValue )
			{
				if ( disposing )
				{
					// dispose managed state (managed objects)
					_nodesLock.Dispose();
				}

				// free unmanaged resources (unmanaged objects) and override finalizer
				// set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~LuaStringTree()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose( disposing: true );
			GC.SuppressFinalize( this );
		}

		#endregion IDisposable
	}
}