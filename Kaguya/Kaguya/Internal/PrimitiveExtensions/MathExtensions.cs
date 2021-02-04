using System;

namespace Kaguya.Internal.PrimitiveExtensions
{
    public static class MathExtensions
    {
        public static int ToFloor(this double d) => (int) Math.Floor(d);
    }
}