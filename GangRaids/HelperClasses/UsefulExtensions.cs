using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace GangRaids.HelperClasses
{
    public static class UsefulExtensions
    {
        private static Random rng = new Random();

        public static T RandomElement<T>(this T[] array)
        {
            return (T)array.GetValue(rng.Next(array.Length));
        }

        public static T RandomElement<T>(this IList<T> list)
        {
            return list[rng.Next(list.Count)];
        }

        public static T RandomElement<T>(this IEnumerable<T> myEnum)
        {
            T[] values = (T[])Enum.GetValues(typeof(T));
            return values.RandomElement();
        }

        public static KeyValuePair<TKey, TValue> RandomElement<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            return dict.ElementAt(rng.Next(dict.Count));
        }

        public static Pos4 GetPos4(this Entity entity)
        {
            return new Pos4(entity.Position, entity.Heading);
        }

        public static void SetPos4(this Entity entity, Pos4 pos4)
        {
            entity.Position = pos4.Position;
            entity.Heading = pos4.Heading;
            return;
        }

        public static Pos4 ToPos4(this string[] stringarray)
        {
            return new Pos4(stringarray);
        }

        public static Vector3 Copy(this Vector3 oldVec3)
        {
            var newVec3 = new Vector3(oldVec3.X, oldVec3.Y, oldVec3.Z);
            return newVec3;
        }

        public static bool Decide(int probability)
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
    }
}
