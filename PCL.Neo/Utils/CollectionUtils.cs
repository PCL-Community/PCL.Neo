using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PCL.Neo.Utils;

public static class CollectionUtils
{
    public delegate bool CompareThreadStart<T>(T left, T right);

    private class MyEqual<T>(CompareThreadStart<T> method) : IEqualityComparer<T>
    {
        public bool Equals(T? x, T? y)
        {
            return method(x, y);
        }

        public int GetHashCode([DisallowNull] T obj)
        {
            return obj.GetHashCode();
        }
    }

    /// <summary>
    /// 将元素与 List 的混合体拆分为元素组。
    /// </summary>
    public static IList<T> GetFullList<T>(IList<T> data)
    {
        //for (int i = 0; i <= data.Count - 1; i++)
        //{
        //    if (data[i] is ICollection)
        //    {
        //        result.AddRange(data[i]);
        //    }
        //    else
        //    {
        //        result.Add(data[i]);
        //    }
        //}

        //foreach (var item in data)
        //{
        //    if (item is ICollection)
        //    {
        //        result.AddRange(item); // TODO: fix this bug "can not ensure Type"
        //    }
        //    else
        //    {
        //        result.Add(item);
        //    }
        //}

        return data.ToList(); // temp solution： flat once
    }
}
