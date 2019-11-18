using System;
using System.Collections.Generic;

namespace IronBrew2.Extensions
{
	public static class IEnumerableExtensions
	{
		private static Random _rnd = new Random();
		
		public static void Shuffle<T>(this IList<T> list)
		{
			for(var i=0; i < list.Count; i++)
				list.Swap(i, _rnd.Next(i, list.Count));
		}

		public static void Swap<T>(this IList<T> list, int i, int j)
		{
			var temp = list[i];
			list[i] = list[j];
			list[j] = temp;
		}

		public static T Random<T>(this IList<T> list) =>
			list[_rnd.Next(0, list.Count)];
	}
}