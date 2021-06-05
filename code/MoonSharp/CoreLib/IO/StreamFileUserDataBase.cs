using System.IO;

namespace MoonSharp.Interpreter.CoreLib.IO
{
	/// <summary>
	/// Abstract class implementing a file Lua userdata. Methods are meant to be called by Lua code.
	/// </summary>
	internal abstract class StreamFileUserDataBase : FileUserDataBase
	{
		protected bool m_Closed = false;

		protected void Initialize(Stream stream, StreamReader reader, StreamWriter writer)
		{

		}


		private void CheckFileIsNotClosed()
		{
			if (m_Closed)
				throw new ScriptRuntimeException("attempt to use a closed file");
		}


		protected override bool Eof()
		{
			CheckFileIsNotClosed();
			throw new ScriptRuntimeException( "nyi" );
		}

		protected override string ReadLine()
		{
			CheckFileIsNotClosed();
			throw new ScriptRuntimeException( "nyi" );
		}

		protected override string ReadToEnd()
		{
			CheckFileIsNotClosed();
			throw new ScriptRuntimeException( "nyi" );
		}

		protected override string ReadBuffer(int p)
		{
			CheckFileIsNotClosed();
			throw new ScriptRuntimeException( "nyi" );
		}

		protected override char Peek()
		{
			CheckFileIsNotClosed();
			throw new ScriptRuntimeException( "nyi" );
		}

		protected override void Write(string value)
		{
			CheckFileIsNotClosed();
			throw new ScriptRuntimeException( "nyi" );
		}

		protected override string Close()
		{
			CheckFileIsNotClosed();
			throw new ScriptRuntimeException( "nyi" );
		}

		public override bool flush()
		{
			CheckFileIsNotClosed();
			throw new ScriptRuntimeException( "nyi" );
		}

		public override long seek(string whence, long offset = 0)
		{
			CheckFileIsNotClosed();
			throw new ScriptRuntimeException( "nyi" );
		}

		public override bool setvbuf(string mode)
		{
			CheckFileIsNotClosed();
			throw new ScriptRuntimeException( "nyi" );
		}

		protected internal override bool isopen()
		{
			return !m_Closed;
		}

	}
}
