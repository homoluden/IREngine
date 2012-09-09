using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IRGame.Common
{
    public struct Vector2
    {
        public double X;
        public double Y;
    }

    public static class Vector2Ex {
        public static Vector2 Normalize(this Vector2 vector)
        {
            double len = Math.Sqrt(vector.X*vector.X + vector.Y*vector.Y);
            var result = new Vector2 { X = vector.X/len, Y = vector.Y/len};

            return result;
        }

        public static double Angle(this Vector2 vector)
        {
            var normd = vector.Normalize();
            double angle = Math.Acos(normd.X);
            return angle;
        }
    }
}
