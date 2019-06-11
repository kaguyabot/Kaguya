using System;
using Discord.Commands;

namespace Kaguya.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    class KaguyaModuleAttribute : GroupAttribute
    {
        public KaguyaModuleAttribute(string moduleName) : base(moduleName)
        {
        }
    }
}
