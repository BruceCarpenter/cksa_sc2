
using System.Globalization;
using System.Text.Json;


/// <summary>
/// This will cache an object to the cache table.
/// The caller is respobsible for making the key unique.
/// </summary>
/// 

namespace ckLib
{
	public class PageCacher<T>
	{
		// These need to be statics or constants
		public string ShopKey { get { return "Shop"; } }
		public string CatKey { get { return "Cat{0}"; } }
		public string SubCatKey { get { return "SubCat{0}"; } }
		public string MiniFilterKey { get { return "MF{0}"; } }
		public string IdeaDefaultKey { get { return "ID{0}"; } }
		public string IdeaTypeKey { get { return "IT{0}"; } }
		public string IdeaMiniKey { get { return "IM{0}"; } }

		/// <summary>
		/// Easy way to turn off the caching.
		/// </summary>
		public bool Disable { get; set; }

		public PageCacher()
		{
			//Disable = System.Diagnostics.Debugger.IsAttached;
			Disable = false;
		}

		public void Store(string key, T store)
		{
			Store(store, key);
		}

		/// <summary>
		/// Convert the passed in object to a storable object.
		/// </summary>
		public void Store(T store, string key)
		{
			if (Disable)
				return;

			try
			{
				var storeMe = JsonSerializer.Serialize(store);

				using var conn = DbDriver.OpenConnection();
				using (var command = conn.CreateCommand())
				{
					command.CommandType = System.Data.CommandType.Text;
					command.CommandText = "insert into BodyCache (Id,Body) values (@c0,@c1)";
					command.Parameters.AddWithValue("@c0", key);
					command.Parameters.AddWithValue("@c1", storeMe);
					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "PageCacher/PageCacher:Store", key);
			}
		}

		public void StoreTemp(string storeMe, string key)
		{
			if (Disable)
				return;

			try
			{
				Clear(key);
				using var conn = DbDriver.OpenConnection();
				using (var command = conn.CreateCommand())
				{
					command.CommandType = System.Data.CommandType.Text;
					command.CommandText = "insert into BodyCache (Id,Body) values (@c0,@c1)";
					command.Parameters.AddWithValue("@c0", key);
					command.Parameters.AddWithValue("@c1", storeMe);
					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "PageCacher/PageCacher:Store", key);
			}
		}
		public string RetrieveTemp(string key)
		{
			var data = string.Empty;

			if (Disable)
				return data;

			try
			{
				using var conn = DbDriver.OpenConnection();
				using (var command = conn.CreateCommand())
				{
					command.CommandType = System.Data.CommandType.Text;
					command.CommandText = "select Body from BodyCache where Id = @c0";
					command.Parameters.AddWithValue("@c0", key);
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							data = reader.GetString(0);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "PageCacher/PageCacher:Store", key);
			}

			return data;
		}

		public T Retrieve(string key)
		{
			if (Disable)
				return default(T);

			var data = string.Empty;
			try
			{
				using var conn = DbDriver.OpenConnection();
				using (var command = conn.CreateCommand())
				{
					command.CommandType = System.Data.CommandType.Text;
					command.CommandText = "select Body from BodyCache where Id = @c0";
					command.Parameters.AddWithValue("@c0", key);
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							data = reader.GetString(0);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "PageCacher/Retrieve", key);
			}

			if (typeof(T) == typeof(DateTime) && string.IsNullOrEmpty(data))
			{
				data = JsonSerializer.Serialize(DateTime.MinValue);
			}

			if (string.IsNullOrEmpty(data))
				return default(T);

			return JsonSerializer.Deserialize<T>(data)!;
		}

		public void Clear(string key)
		{
			try
			{
				using var conn = DbDriver.OpenConnection();
				using (var command = conn.CreateCommand())
				{
					command.CommandType = System.Data.CommandType.Text;
					command.CommandText = "delete from BodyCache where Id = @c0";
					command.Parameters.AddWithValue("@c0", key);
					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "PageCacher/Clear", key);
			}

		}

		/// <summary>
		/// Clear the cache table.
		/// </summary>
		public static void Clear()
		{
			try
			{
				using var conn = DbDriver.OpenConnection();
				using (var command = conn.CreateCommand())
				{
					command.CommandType = System.Data.CommandType.Text;
					command.CommandText = "delete from BodyCache";
					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "PageCacher:Clear");
			}
		}

	}
}