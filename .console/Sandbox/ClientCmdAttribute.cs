using System;

#nullable enable

namespace Sandbox
{
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false, Inherited = false )]
    internal class ClientCmdAttribute : Attribute
    {
        public string CommandName { get; }

        public ClientCmdAttribute( string commandName )
        {
            CommandName = commandName;
        }
    }
}
