namespace PCL.Neo.Core.Utils;

public static class ICollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> target)
    {
        foreach (var item in target)
        {
            collection.Add(item);
        }
    }
}