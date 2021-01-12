using System;
using Kaguya.Internal.Enums;

namespace Kaguya.Internal.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal class ExampleAttribute : Attribute
    {
        public string Examples { get; }
        public ExampleStringFormat Format { get; }

        /// <param name="examples">
        /// A collection of examples, separated by \n (new line) characters.
        /// This string may not be empty or only comprised of white-space characters.
        /// </param>
        /// <param name="format">How to format the <see cref="Examples"/> inside of the $help documentation.</param>
        public ExampleAttribute(string examples, ExampleStringFormat format = ExampleStringFormat.CodeblockSingleLine)
        {
            // We allow empty strings deliberately to showcase that the command can be used by itself 
            // without any additional input from the user. This is only typically used with complex commands.
            Examples = examples ?? throw new ArgumentNullException(nameof(examples));
            Format = format;
        }
    }
}