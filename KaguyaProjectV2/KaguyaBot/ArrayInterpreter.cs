using System;

namespace KaguyaProjectV2.KaguyaBot
{
    public static class ArrayInterpreter
    {
        /// <summary>
        /// Takes in a period character separated list from a string[] and returns an array of arguments.
        /// </summary>
        /// <param name="args">Period separated list of arguments. Ex: "Smelly Pandas.Smoothies.SomeLongRole.Some Separated Role"</param>
        public static string[] ReturnParams(string args)
        {
            string[] newArgs = args.Split('.');
            return newArgs;
        }
    }
}