using System;
using System.IO;
using System.Text.Json;

#nullable enable

namespace Sandbox
{
    internal class FileSystem
    {
        public static readonly FileSystem Data = new( "./data" );
        public static readonly FileSystem Mounted = new( "." );

        private readonly string _basePath;
        private FileSystem( string basePath )
        {
            _basePath = basePath;
        }

        private string Resolve( string path ) => Path.Join( _basePath, path );
        public bool DirectoryExists( string path ) => Directory.Exists( Resolve( path ) );
        public void CreateDirectory( string path ) => Directory.CreateDirectory( Resolve( path ) );
        public bool FileExists( string path ) => File.Exists( Resolve( path ) );
        public ReadOnlySpan<byte> ReadAllBytes( string path ) => File.ReadAllBytes( Resolve( path ) );
        public string ReadAllText( string path ) => File.ReadAllText( Resolve( path ) );
        public Stream OpenWrite( string path ) => File.Create( Resolve( path ) );
        public void WriteJson<T>( string path, T value )
        {
            using FileStream? stream = File.OpenWrite( Resolve( path ) );
            JsonSerializer.SerializeAsync( stream, value );
        }
    }
}
