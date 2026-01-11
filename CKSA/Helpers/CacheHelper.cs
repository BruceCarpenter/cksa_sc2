using Microsoft.Extensions.Caching.Memory;

namespace CKSA.Helpers
{
	public class CacheHelper
	{
		private readonly IMemoryCache _cache;
		public CacheHelper(IMemoryCache cache)
		{
			_cache = cache;
		}

		// Save an object to cache
		public void Save(string key, object value, int days = 30)
		{
			_cache.Set(key, value, TimeSpan.FromDays(days));
		}

		// Get an object from cache (returns null if not found)
		public T? Get<T>(string key) where T : class
		{
			if (_cache.TryGetValue(key, out T? result))
			{
				return result;
			}
			return null;
		}

		public static void Save<T>(IMemoryCache cache, string key, T value, int minutes = 1440)
		{
			cache.Set(key, value, TimeSpan.FromMinutes(minutes));
		}
		public static T? Get<T>(IMemoryCache cache, string key)
		{
			cache.TryGetValue(key, out T? value);
			return value;
		}
	}
}