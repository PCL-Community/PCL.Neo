using System;
using System.Collections.Generic;
using System.Linq;

namespace PCL.Neo.Utils;

public static class RandomUtils
{
    private static readonly Random _random = new();

    /// <summary>
    /// 随机获取集合中的一个元素。
    /// </summary>
    public static T RandomOne<T>(ICollection<T> objects)
    {
        return objects.ElementAt(_random.Next(0, objects.Count - 1));
    }

    /// <summary>
    /// 将数组随机打乱。
    /// </summary>
    public static IList<T> Shuffle<T>(IList<T> array)
    {
        var result = new List<T>();
        do
        {
            var i = _random.Next(0, array.Count - 1);
            result.Add(array[i]);
            array.RemoveAt(i);
        } while (array.Count != 0);

        return result;
    }
}
