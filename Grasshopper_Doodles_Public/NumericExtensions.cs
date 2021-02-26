using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grasshopper_Doodles_Public
{
    public static class NumericExtensions
    {

        /// <summary>
        /// sets all members of the array to value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="value"></param>
        /// <returns>returns the array</returns>
        public static T[] Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }

            return arr;
        }

        public static double RoundTo(this double x, double nearest)
        {
            return x % nearest >= nearest / 2.0 ? x + nearest - x % nearest : x - x % nearest;
        }

    }
}
