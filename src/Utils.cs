using System.Threading.Tasks;
using System.Collections.Generic;

namespace Mediatek
{
	public class Utils
	{
		public static async Task<List<T>> ToArray<T>(IAsyncEnumerable<T> iter)
		{
			List<T> list = new List<T>();
			await foreach (T e in iter) list.Add(e);
			return list;
		}
	}
}