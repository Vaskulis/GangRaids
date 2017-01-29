using System;
using System.Collections.Generic;
using System.Linq;
using Rage;

namespace GangsOfSouthLS.HelperClasses.CommonUtilities
{
    internal static class UsefulExtensions
    {
        internal static Random rng = new Random();

        internal static T RandomElement<T>(this T[] array)
        {
            return (T)array.GetValue(rng.Next(array.Length));
        }

        internal static T RandomElement<T>(this IList<T> list)
        {
            return list[rng.Next(list.Count)];
        }

        internal static T RandomElement<T>(this IEnumerable<T> myEnum)
        {
            T[] values = (T[])Enum.GetValues(typeof(T));
            return values.RandomElement();
        }

        internal static KeyValuePair<TKey, TValue> RandomElement<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            return dict.ElementAt(rng.Next(dict.Count));
        }

        internal static Pos4 GetPos4(this Entity entity)
        {
            return new Pos4(entity.Position, entity.Heading);
        }

        internal static void SetPos4(this Entity entity, Pos4 pos4)
        {
            entity.Position = pos4.Position;
            entity.Heading = pos4.Heading;
            return;
        }

        internal static Pos4 ToPos4(this string[] stringarray)
        {
            return new Pos4(stringarray);
        }

        internal static Vector3 Copy(this Vector3 oldVec3)
        {
            var newVec3 = new Vector3(oldVec3.X, oldVec3.Y, oldVec3.Z);
            return newVec3;
        }

        internal static bool Decide(int probability)
        {
            if (rng.Next(101) <= probability)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
