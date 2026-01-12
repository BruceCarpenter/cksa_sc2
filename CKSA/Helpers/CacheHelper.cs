using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

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

		public List<T> LoadJsonCache<T>(string cacheName, string fileName)
		{
			// Try cache first
			var items = _cache.Get<List<T>>(cacheName);
			if (items != null)
				return items;

			// Load from JSON if cache miss
			var path = Path.Combine(AppContext.BaseDirectory, "json", fileName);

			if (File.Exists(path))
			{
				var json = File.ReadAllText(path);

				items = JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});
			}

			// Ensure non-null
			items ??= new List<T>();

			// Store in cache
			Save(cacheName, items);
			return items;
		}
	}
}