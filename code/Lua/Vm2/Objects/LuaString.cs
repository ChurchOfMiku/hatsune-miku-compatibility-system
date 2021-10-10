using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

#nullable enable

namespace Miku.Lua.Vm2
{
	/// <summary>
	/// A byte sequence that represents a string.
	/// Does not validate its contents.
	/// </summary>
	public sealed class LuaString : IComparable, IComparable<LuaString>, IEquatable<LuaString?>, IEnumerable<byte>
	{
		/// <summary>
		/// DO NOT USE THIS. THIS IS ONLY INTERNAL BECAUSE OF THE TREE.
		/// FOR ANYTHING ELSE ADD METHODS TO THIS CLASS AND THE TREE.
		/// </summary>
		private readonly ImmutableArray<byte> _buffer;

		/// <summary>
		/// Don't construct these directly. Always use <see cref="Utf8StringTree"/>.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="hashCode"></param>
		/// <param name="buffer"></param>
		internal LuaString( LuaStringTree parent, int id, int hashCode, ImmutableArray<byte> buffer )
		{
			Parent = parent;
			Id = id;
			HashCode = hashCode;
			_buffer = buffer;
		}

		/// <summary>
		/// The tree that contains this string.
		/// </summary>
		public LuaStringTree Parent { get; }

		/// <summary>
		/// The unique ID of this string in the 
		/// </summary>
		public int Id { get; }

		/// <summary>
		/// The pre-computed hash code of the string.
		/// </summary>
		public int HashCode { get; }

		#region IEnumerable<byte>

		public ImmutableArray<byte>.Enumerator GetEnumerator() => _buffer.GetEnumerator();

		IEnumerator<byte> IEnumerable<byte>.GetEnumerator() => ((IEnumerable<byte>)_buffer).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_buffer).GetEnumerator();

		#endregion IEnumerable<byte>

		#region String Instance Methods

		/// <summary>
		/// Creates a slice of this string and interns it into the provided tree.
		/// </summary>
		/// <param name="tree">The tree to add the interned slice to.</param>
		/// <param name="start">The index at which the substring begins.</param>
		/// <param name="length">The desired length of the substring.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="start"/> is less than zero or greater than <see cref="LuaString.Length"/>.
		/// </exception>
		public LuaString Substring( int start, int length )
		{
			if ( length == 0 )
			{
				return Parent.Empty;
			}

			return Parent.Intern( _buffer.AsSpan().Slice( start, length ) );
		}

		public byte this[int index] => _buffer[index];
		public int Length => _buffer.Length;
		public ReadOnlySpan<byte> AsSpan() => _buffer.AsSpan();
		public override string ToString()
		{
			ImmutableArray<byte> buffer = _buffer;
			if ( buffer.Length < 256 )
			{
				Span<char> charBuffer = stackalloc char[buffer.Length];
				for ( int idx = 0; idx < buffer.Length; idx++ )
				{
					charBuffer[idx] = (char)buffer[idx];
				}

				return new string( charBuffer );
			}
			return string.Concat( buffer.Select( c => (char)c ) );
		}

		#endregion String Instance Methods

		/// <summary>
		/// Clones this instance but with a different parent.
		/// </summary>
		/// <param name="parent"></param>
		/// <returns>
		/// The same instance if the parent hasn't changed or a clone of this string
		/// with its parent replaced by the provided one.
		/// </returns>
		public LuaString WithParent( LuaStringTree parent ) =>
			parent == Parent ? this : new( parent, Id, HashCode, _buffer );

		/// <inheritdoc/>
		public override int GetHashCode() => HashCode;
		/// <inheritdoc/>
		public override bool Equals( object? obj ) => Equals( obj as LuaString );
		/// <inheritdoc/>
		public bool Equals( LuaString? other ) => other != null && Parent == other.Parent && Id == other.Id;
		/// <inheritdoc/>
		public int CompareTo( object? other )
		{
			if ( other is null )
			{
				return 1;
			}

			if ( other is not LuaString str )
			{
				throw new ArgumentException( "Object to compare the string with must be a string.", nameof( other ) );
			}

			return CompareTo( str );
		}
		public int CompareTo( LuaString? otherStr ) => Compare( this, otherStr );

		/// <summary>
		/// Compares the two strings without taking into account any type of encoding.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static int Compare( LuaString? left, LuaString? right )
		{
			if ( ReferenceEquals( left, right ) )
			{
				return 0;
			}

			if ( left is null )
			{
				return -1;
			}

			if ( right is null )
			{
				return 1;
			}

			ReadOnlySpan<byte> leftSpan = left._buffer.AsSpan();
			ReadOnlySpan<byte> rightSpan = right._buffer.AsSpan();
			return leftSpan.SequenceCompareTo( rightSpan );
		}

		/// <summary>
		/// Concatenates two strings.
		/// Both strings must belong to the same tree.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static LuaString? Concat( LuaString? left, LuaString? right )
		{
			if ( left is null )
			{
				return right;
			}

			if ( right is null )
			{
				return left;
			}

			if ( left.Parent != right.Parent )
			{
				throw new InvalidOperationException( "Can't concatenate two strings that don't belong to the same tree." );
			}

			if ( (uint)(left.Length + right.Length) < 256 )
			{
				Span<byte> concatBuffer = stackalloc byte[left.Length + right.Length];
				left._buffer.AsSpan().CopyTo( concatBuffer );
				right._buffer.AsSpan().CopyTo( concatBuffer[left.Length..] );
				return left.Parent.Intern( concatBuffer );
			}
			else
			{
				byte[]? concatBuffer = ArrayPool<byte>.Shared.Rent( left.Length + right.Length );
				left._buffer.CopyTo( concatBuffer );
				right._buffer.CopyTo( concatBuffer, left.Length );
				return left.Parent.Intern( concatBuffer );
			}
		}

		/// <summary>
		/// Checks whether this string is equal to another.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==( LuaString? left, LuaString? right )
		{
			if ( right is null )
			{
				return left is null;
			}

			return right.Equals( left );
		}

		public static bool operator !=( LuaString? left, LuaString? right ) => !(left == right);

		public static implicit operator ReadOnlySpan<byte>( LuaString str ) => str.AsSpan();
	}
}