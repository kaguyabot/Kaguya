using System;
using Kaguya.Internal.Enums;

namespace Kaguya.Internal.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class ModuleAttribute : Attribute
    {
        public CommandModule Module { get; }
        public ModuleAttribute(CommandModule module) { Module = module; }
    }
}