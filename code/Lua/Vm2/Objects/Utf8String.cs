using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

#nullable enable

namespace Miku.Lua.Vm2
{
	/// <summary>
	/// A byte sequence that represents a string.
	/// Does not validate its contents.
	/// </summary>
	public sealed class Utf8String : IEnumerable<byte>
	{
		public static readonly Utf8String Empty = new( 0, 0, ImmutableArray<byte>.Empty );

		/// <summary>
		/// DO NOT USE THIS. THIS IS ONLY INTERNAL BECAUSE OF THE TREE.
		/// FOR ANYTHING ELSE ADD METHODS TO THIS CLASS AND THE TREE.
		/// </summary>
		internal readonly ImmutableArray<byte> _buffer;

		/// <summary>
		/// The pre-computed xxHash hash code of the string.
		/// </summary>
		private readonly int _hashCode;

		/// <summary>
		/// Don't construct these directly. Always use <see cref="Utf8StringTree"/>.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="hashCode"></param>
		/// <param name="buffer"></param>
		internal Utf8String( long id, int hashCode, ImmutableArray<byte> buffer )
		{
			Id = id;
			_hashCode = hashCode;
			_buffer = buffer;
		}

		/// <summary>
		/// The unique ID of this string in the 
		/// </summary>
		public long Id { get; }

		public byte this[int index] => _buffer[index];

		#region IEnumerable<byte>

		ImmutableArray<byte>.Enumerator GetEnumerator()
		{
			return _buffer.GetEnumerator();
		}

		IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
		{
			return ((IEnumerable<byte>)_buffer).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_buffer).GetEnumerator();
		}

		#endregion IEnumerable<byte>

		/// <summary>
		/// Creates a slice of this string and interns it into the provided tree.
		/// </summary>
		/// <param name="tree">The tree to add the interned slice to.</param>
		/// <param name="start"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public Utf8String Substring( Utf8StringTree tree, int start, int length ) =>
			tree.Substring( this, start, length );

		public override int GetHashCode()
		{
			return _hashCode;
		}
	}
}