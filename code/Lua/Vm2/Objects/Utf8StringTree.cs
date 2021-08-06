using System;
using System.Threading;

#nullable enable

namespace Miku.Lua.Vm2
{
	/// <summary>
	/// A tree that stores deduplicated <see cref="Utf8String"/>.
	/// </summary>
	public sealed class Utf8StringTree
	{
		private sealed class Node
		{
			public WeakReference<Utf8String>? Value { get; set; }
			public Node?[] Children { get; } = new Node[byte.MaxValue];
		}

		// The root is the empty string node
		private readonly Node _root = new()
		{
			Value = new WeakReference<Utf8String>( Utf8String.Empty )
		};
		// We start from 1 because 0 is always the empty string.
		private long _counter = 1;

		/// <summary>
		/// Initializes a new string tree.
		/// </summary>
		public Utf8StringTree()
		{
		}

		/// <summary>
		/// Gets a deduplicated string from the tree.
		/// </summary>
		/// <param name="bytes">The bytes that compose the string.</param>
		/// <returns></returns>
		public Utf8String? GetString( ReadOnlySpan<byte> bytes )
		{
			Node? node = _root;
			int idx = 0;
			while ( idx < bytes.Length && node?.Children[idx] is not null )
			{
				node = node.Children[idx++];
			}

			if ( node?.Value?.TryGetTarget( out Utf8String? value ) is true )
			{
				return value;
			}

			return null;
		}

		/// <summary>
		/// Walk down the tree to get the node we want.
		/// Creating the nodes along the way if needed.
		/// </summary>
		/// <param name="bytes">The bytes of the string to get the node of.</param>
		/// <returns></returns>
		private Node GetOrCreateNode( ReadOnlySpan<byte> bytes )
		{
			Node node = _root;
			int idx = 0;
			while ( idx < bytes.Length )
			{
				byte value = bytes[idx++];
				if ( node.Children[value] is null )
				{
					lock ( node )
					{
						if ( node.Children[value] is null )
						{
							node.Children[value] = new Node();
						}
					}
				}

				node = node.Children[value]!;
			}
			return node;
		}

		/// <summary>
		/// Adds a string to the tree if it's not already in it.
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public Utf8String Intern( ReadOnlySpan<byte> bytes )
		{
			Node node = GetOrCreateNode( bytes );

			// Then check if the WeakRef has been created and create it if
			// it hasn't,
			Utf8String? str = null;
			if ( node.Value is null )
			{
				// We only lock if we know that we might need to modify it.
				lock ( node )
				{
					if ( node.Value is null )
					{
						str = new Utf8String(
						  Interlocked.Increment( ref _counter ),
						  (int)Hash.GetXXHash32HashCode( bytes ),
						  bytes.ToImmutableArray() );
						node.Value = new WeakReference<Utf8String>( str );
					}
				}
			}

			// If we 
			if ( str is null && !node.Value.TryGetTarget( out str ) )
			{
				lock ( node )
				{
					if ( !node.Value.TryGetTarget( out str ) )
					{
						str = new Utf8String(
							Interlocked.Increment( ref _counter ),
							(int)Hash.GetXXHash32HashCode( bytes ),
							bytes.ToImmutableArray() );
						node.Value.SetTarget( str );
					}
				}
			}

			return str;
		}

		public void Intern( Utf8String str )
		{
			Node node = GetOrCreateNode( str._buffer.AsSpan() );

			// Then check if the WeakRef has been created and create it if
			// it hasn't,
			if ( node.Value is null )
			{
				// We only lock if we know that we might need to modify it.
				lock ( node )
				{
					if ( node.Value is null )
					{
						node.Value = new WeakReference<Utf8String>( str );
					}
				}
			}

			// If we 
			if ( !node.Value.TryGetTarget( out _ ) )
			{
				lock ( node )
				{
					if ( !node.Value.TryGetTarget( out _ ) )
					{
						node.Value.SetTarget( str );
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
		public Utf8String Substring( Utf8String str, int start, int length )
		{
			if ( str is null )
				throw new ArgumentNullException( nameof( str ) );

			return Intern( str._buffer.AsSpan().Slice( start, length ) );
		}
	}
}