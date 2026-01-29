using MySqlConnector;
using System.Data.Common;
using System.Diagnostics;

namespace ckLib
{
	public class DbDriver
	{
		private static string _connectionString = @"server=localhost;user=CKSA;database=pricelist;port=3306;password=TunaFish71;Charset=utf8mb4;";

		static public MySqlConnection? OpenConnection()
		{
			try
			{
				var mySql = new MySqlConnection(_connectionString);
				mySql.Open();

				Debug.WriteLine("Opening DB");

				return mySql;
			}
			catch(Exception ex)
			{
				// Need to do something here like block the person if connection string issue.
				throw;
			}
		}

	}
	public static class ReaderExtension
	{
		public static T? GetValueOrNull<T>(this DbDataReader reader, int column) where T : struct
		{
			if (reader.IsDBNull(column))
			{
				return (T?)null;
			}

			return reader.GetFieldValue<T>(column);
		}

		// This is the extension method.
		// The first parameter takes the "this" modifier
		// and specifies the type for which the method is defined.
		public static string ReadString(this DbDataReader reader, int column)
		{
			var value = string.Empty;

			if (!reader.IsDBNull(column))
			{
				value = reader.GetString(column);
			}

			return value;
		}

		public static string ReadString(this DbDataReader reader, string columnName)
		{
			int column = reader.GetOrdinal(columnName);
			return ReadString(reader, column);
		}

		public static int ReadInt32(this DbDataReader reader, int column)
		{
			var value = int.MinValue;

			if (!reader.IsDBNull(column))
			{
				value = reader.GetInt32(column);
			}

			return value;
		}

		public static int ReadInt32(this DbDataReader reader, string columnName)
		{
			int column = reader.GetOrdinal(columnName);
			return ReadInt32(reader, column);
		}

		public static DateTime ReadDateTime(this DbDataReader reader, int column)
		{
			var value = DateTime.Now.Date;

			if (!reader.IsDBNull(column))
			{
				value = reader.GetDateTime(column);
			}

			return value;
		}
		public static double ReadDouble(this DbDataReader reader, int column)
		{
			var value = double.MinValue;

			if (!reader.IsDBNull(column))
			{
				value = reader.GetDouble(column);
			}

			return value;
		}

		public static Decimal ReadDecimal(this DbDataReader reader, int column)
		{
			var value = Decimal.MinValue;

			if (!reader.IsDBNull(column))
			{
				value = reader.GetDecimal(column);
			}

			return value;
		}
	}
}
