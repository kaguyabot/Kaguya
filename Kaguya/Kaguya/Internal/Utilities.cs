using System;

namespace Kaguya.Internal
{
	public static class Utilities
	{
		/// <summary>
		///  Returns a T[] containing all members belonging to
		///  a specific Enum type. This is generally used for "iterating through an enum",
		///  for lack of a better term.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>All members belonging to a specific Enum (<see cref="T" />)</returns>
		public static T[] GetValues<T>() where T : Enum { return (T[]) Enum.GetValues(typeof(T)); }
	}
}