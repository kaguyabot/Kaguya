using System;
using Kaguya.Discord.Attributes.Enums;

namespace Kaguya.Discord.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class ModuleAttribute : Attribute
    {
        public CommandModule Module { get; }
        public ModuleAttribute(CommandModule module) { Module = module; }
    }
}