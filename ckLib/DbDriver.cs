using MySqlConnector;
using System.Data.Common;
using System.Diagnostics;

namespace ckLib
{
	public class DbDriver
	{
		private static string _connectionString = @"server=localhost;user=CKSA;database=pricelist;port=3306;password=TunaFish71;Charset=utf8mb4;";
		public static int OpenCounter = 0;

		static public MySqlConnection? OpenConnection()
		{
			try
			{
				var mySql = new MySqlConnection(_connectionString);
				mySql.Open();

				OpenCounter++;

				Debug.WriteLine($"Opening DB: {OpenCounter}");

				return mySql;
			}
			catch (Exception ex)
			{
				// Need to do something here like block the person if connection string issue.
				throw;
			}
		}
		public static MySqlDataReader ExecuteReader(string sql)
		{
			// Remove the 'using' from the connection here!
			var connection = DbDriver.OpenConnection();
			try
			{
				using (var mySqlCommand = connection.CreateCommand())
				{
					mySqlCommand.CommandText = sql;

					// This magic flag tells the reader: 
					// "When the user closes the reader, kill the connection too."
					return mySqlCommand.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
				}
			}
			catch (Exception ex)
			{
				// If an error happens before the reader is even created, 
				// we must close the connection manually or we get a 'Leak'.
				connection.Close();
				ErrorHandler.Handle(new ckExceptionData(ex, "MySQLHealer::ExecuteReader(sql)", sql, ""));
				return null;
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

		public static int ReadInt16(this DbDataReader reader, int column)
		{
			var value = int.MinValue;

			if (!reader.IsDBNull(column))
			{
				value = reader.GetInt16(column);
			}

			return value;
		}

		public static int ReadInt16(this DbDataReader reader, string columnName)
		{
			int column = reader.GetOrdinal(columnName);
			return ReadInt16(reader, column);
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
			var value = DateTime.MaxValue;

			if (!reader.IsDBNull(column))
			{
				value = reader.GetDateTime(column);
			}

			return value;
		}

		public static DateTime ReadDateTime(this DbDataReader reader, string columnName)
		{
			int column = reader.GetOrdinal(columnName);
			return reader.ReadDateTime(column);
		}

		public static DateTime ReadDateTimeAsString(this DbDataReader reader, int column)
		{
			DateTime result = DateTime.MaxValue;

			if (!reader.IsDBNull(column))
			{
				var str = reader.GetString(column);
				if (string.IsNullOrEmpty(str) == false)
				{
					DateTime.TryParse(str, out result);
				}
			}

			return result;
		}

		public static DateTime ReadDateTimeAsString(this DbDataReader reader, string columnName)
		{
			int column = reader.GetOrdinal(columnName);
			return reader.ReadDateTimeAsString(column);
		}


		public static double ReadDouble(this DbDataReader reader, string columnName)
		{
			int column = reader.GetOrdinal(columnName);
			return reader.ReadDouble(column);
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
		static public bool ReadBool(this DbDataReader reader, int column)
		{
			if (reader.IsDBNull(column) == false)
			{
				var str = reader.GetByte(column);
				return (str == 1);
			}

			return false;
		}

		static public bool ReadBool(this DbDataReader reader, string columnName)
		{
			int index = reader.GetOrdinal(columnName);
			return reader.ReadBool(index);
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

		static public Decimal ReadDecimal(this DbDataReader reader, string columnName)
		{
			int index = reader.GetOrdinal(columnName);
			return reader.ReadDecimal(index);
		}
	}
}
