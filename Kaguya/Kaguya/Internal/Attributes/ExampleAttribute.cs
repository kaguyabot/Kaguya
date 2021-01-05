using System;

namespace Kaguya.Internal.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal class ExampleAttribute : Attribute
    {
        public string Examples { get; }

        /// <param name="examples">
        /// A collection of examples, separated by \n (new line) characters.
        /// This string may not be empty or only comprised of white-space characters.
        /// </param>
        public ExampleAttribute(string examples)
        {
            // We allow empty strings deliberately to showcase that the command can be used by itself 
            // without any additional input from the user. This is only typically used with complex commands.
            Examples = examples ?? throw new ArgumentNullException(nameof(examples));
        }
    }
}