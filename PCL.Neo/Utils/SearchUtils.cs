using System;
using System.Collections.Generic;
using System.Linq;

namespace PCL.Neo.Utils;

public static class SearchUtils
{
    /// <summary>
    /// 用于搜索的项目。
    /// </summary>
    public class SearchEntry<T>
    {
        /// <summary>
        /// 该项目对应的源数据。
        /// </summary>
        public T Item;

        /// <summary>
        /// 该项目用于搜索的源。
        /// </summary>
        public List<KeyValuePair<string, double>> SearchSource;

        /// <summary>
        /// 相似度。
        /// </summary>
        public double Similarity;

        /// <summary>
        /// 是否完全匹配。
        /// </summary>
        public bool AbsoluteRight;
    }

    /// <summary>
    /// 获取搜索文本的相似度。
    /// </summary>
    /// <param name="source">被搜索的长内容。</param>
    /// <param name="query">用户输入的搜索文本。</param>
    private static double SearchSimilarity(string source, string query)
    {
        var qp = 0;
        double lenSum = 0;
        source = source.ToLower().Replace(" ", "");
        query = query.ToLower().Replace(" ", "");
        var sourceLength = source.Length;
        var queryLength = query.Length; // 用于计算最后因数的长度缓存
        while (qp < queryLength)
        {
            // 对 qp 作为开始位置计算
            int sp = 0, lenMax = 0, spMax = 0;
            // 查找以 qp 为头的最大子串
            while (sp < sourceLength)
            {
                // 对每个 sp 作为开始位置计算最大子串
                var len = 0;
                while (qp + len < queryLength
                       && sp + len < sourceLength
                       && source[sp + len] == query[qp + len])
                    len += 1;

                // 存储 len
                if (len > lenMax)
                {
                    lenMax = len;
                    spMax = sp;
                }

                // 根据结果增加 sp
                sp += Math.Max(1, len);
            }

            if (lenMax > 0)
            {
                source = source[..spMax] +
                         (sourceLength > spMax + lenMax ? source[(spMax + lenMax)..] : string.Empty);
                // 存储 lenSum
                var incWeight = Math.Pow(1.4, 3 + lenMax) - 3.6; // 根据长度加成
                incWeight *= 1 + 0.3 * Math.Max(0, 3 - Math.Abs(qp - spMax)); // 根据位置加成
                lenSum += incWeight;
            }

            // 根据结果增加 qp
            qp += Math.Max(1, lenMax);
        }

        return lenSum / queryLength * (3 / Math.Pow(sourceLength + 15, 0.5)) *
               (queryLength <= 2 ? 3 - queryLength : 1);
    }

    /// <summary>
    /// 获取多段文本加权后的相似度。
    /// </summary>
    private static double SearchSimilarityWeighted(List<KeyValuePair<string, double>> source, string query)
    {
        double totalWeight = 0;
        double sum = 0;
        foreach (var pair in source)
        {
            sum += SearchSimilarity(pair.Key, query) * pair.Value;
            totalWeight += pair.Value;
        }

        return sum / totalWeight;
    }

    public static List<SearchEntry<T>> Search<T>(List<SearchEntry<T>> entries, string query, int maxBlurCount = 5,
        double minBlurSimilarity = 0.1)
    {
        // 初始化
        var resultList = new List<SearchEntry<T>>();
        if (entries.Count == 0) return resultList;
        // 进行搜索，获取相似信息
        foreach (var entry in entries)
        {
            entry.Similarity = SearchSimilarityWeighted(entry.SearchSource, query);
            entry.AbsoluteRight = query.Split(" ").All(queryPart => entry.SearchSource.Any(source =>
                source.Key.Replace(" ", "").Contains(queryPart, StringComparison.OrdinalIgnoreCase)));
        }

        // 按照相似度进行排序
        entries.Sort((left, right) =>
        {
            if (left.AbsoluteRight ^ right.AbsoluteRight)
            {
                return left.AbsoluteRight ? -1 : 1;
            }

            return left.Similarity > right.Similarity ? -1 : 1;
        });
        // 返回结果
        var blurCount = 0;
        foreach (var entry in entries)
        {
            if (entry.AbsoluteRight)
            {
                resultList.Add(entry);
            }
            else
            {
                if (entry.Similarity < minBlurSimilarity || blurCount == maxBlurCount) break;
                resultList.Add(entry);
                blurCount++;
            }
        }

        return resultList;
    }
}
