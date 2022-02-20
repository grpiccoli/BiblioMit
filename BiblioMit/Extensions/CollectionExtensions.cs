﻿using BiblioMit.Models.Entities.Digest;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace BiblioMit.Extensions
{
    public static class CustomCollectionExtensions
    {
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            if (list is null) throw new ArgumentNullException(nameof(list));
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do randomNumberGenerator.GetBytes(box);
                while (!(box[0] < n * (byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
        public static void AddRangeOverride<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd) =>
            dicToAdd.ForEach(x => dic[x.Key] = x.Value);
        public static void AddRangeOverride<TKey>(this IList<TKey> list, ICollection<TKey> listToAdd)
        {
            list.Clear();
            listToAdd.ForEach(x => list.Add(x));
        }
        public static void AddOrSum(this ICollection<DeclarationDate> dates, DeclarationDate date)
        {
            if (date == null) return;
            if (dates == null) dates = new List<DeclarationDate>();
            if (dates.Any(d => d.Date == date.Date))
            {
                dates.First(d => d.Date == date.Date).Weight += date.Weight;
            }
            else
            {
                dates.Add(date);
            }
        }
        public static async void AddOrChange(this DbSet<DeclarationDate> dates, DeclarationDate date)
        {
            if (date == null || dates == null) return;
            if (dates.Any(d => d.SernapescaDeclarationId == date.SernapescaDeclarationId && d.Date == date.Date))
            {
                var old = await dates
                    .FirstAsync(d => d.SernapescaDeclarationId == date.SernapescaDeclarationId && d.Date == date.Date)
                    .ConfigureAwait(false);
                old.Weight = date.Weight;
                dates.Update(old);
            }
            dates.Add(date);
        }
        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            if (list == null) return;
            T item = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, item);
        }
        public static IEnumerable<T> MoveFirst<T>(this List<T> list, Func<T, bool> func)
        {
            T item = list.First(func);
            list.Remove(item);
            return list.Prepend(item);
        }
        public static IEnumerable<T> MoveLast<T>(this List<T> list, Func<T, bool> func)
        {
            T item = list.First(func);
            list.Remove(item);
            return list.Append(item);
        }
        public static IEnumerable<TKey> ExceptNull<TKey>(this IEnumerable<TKey> list, IEnumerable<TKey> listExcept) =>
            list.Except(listExcept);
        public static void AddRangeOverride<TKey>(this IList<TKey> list, IList<TKey> listToAdd)
        {
            list.Clear();
            listToAdd.ForEach(x => list.Add(x));
        }
        public static void AddRangeOverride<TKey>(this IList<TKey> list, IEnumerable<TKey> listToAdd)
        {
            list.Clear();
            listToAdd.ForEach(x => list.Add(x));
        }
        public static void AddRangeOverride<TKey>(this ICollection<TKey> list, IList<TKey> listToAdd)
        {
            list.Clear();
            listToAdd.ForEach(x => list.Add(x));
        }
        public static void AddRangeOverride<TKey>(this ICollection<TKey> list, ICollection<TKey> listToAdd)
        {
            list.Clear();
            listToAdd.ForEach(x => list.Add(x));
        }
        public static void AddRangeNewOnly<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd) =>
            dicToAdd.ForEach(x => { if (!dic.ContainsKey(x.Key)) dic.Add(x.Key, x.Value); });

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd) =>
            dicToAdd.ForEach(x => dic.Add(x.Key, x.Value));
        public static bool ContainsKeys<TKey, TValue>(this IDictionary<TKey, TValue> dic, IEnumerable<TKey> keys)
        {
            bool result = false;
            keys.ForEachOrBreak((x) => { result = dic.ContainsKey(x); return result; });
            return result;
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source != null && action != null) foreach (var item in source) action(item);
        }
        public static void ForEachOrBreak<T>(this IEnumerable<T> source, Func<T, bool> func)
        {
            if (source != null && func != null)
                foreach (var item in source)
                {
                    bool result = func(item);
                    if (result) break;
                }
        }
        public static string GetValue(this Dictionary<(int, int), string> matrix, int column, int row)
        {
            if (matrix.ContainsKey((column, row)))
                return matrix[(column, row)];
            return string.Empty;
        }
    }
}
