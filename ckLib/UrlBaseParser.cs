using MySqlConnector;
using System.Data;
using System.Text;

namespace ckLib
{
	public class UrlBaseParser
	{

		public UrlBaseParser()
		{
		}

		protected bool IsValidId(string id)
		{
			Int64 temp;
			return Int64.TryParse(id, out temp);
		}

		protected string CheckIfDigit(string shopId)
		{
			if (IsValidId(shopId) == false)
			{
				return string.Empty;
			}

			return shopId;
		}

		/// <summary>
		/// Same format for this. param1 is emtpy and param 2 is not empty and is a digit and not 0.
		/// </summary>
		/// <param name="param1">value that needs to be obtained if empty</param>
		/// <param name="intParamId">used to get param1 must be int</param>
		protected bool GetFirstParamValue(string param1, string intParamId)
		{
			bool getParam = false;
			Int64 forTryParse;

			getParam = string.IsNullOrEmpty(param1) &&
			   string.IsNullOrEmpty(intParamId) == false &&
				Int64.TryParse(intParamId, out forTryParse) &&
				forTryParse != 0;

			return getParam;
		}

		protected bool GetFirstParamValue(int param1, int paramId)
		{
			return param1 != 0 && paramId != 0;
		}

		protected bool GetFirstParamValue(string param1, int paramId)
		{
			return !string.IsNullOrEmpty(param1) && paramId != 0;
		}

		protected int GetValueInt(string sql, int whereParam, string field)
		{
			var convertToInt = GetValue(sql, whereParam, field);
			if (int.TryParse(convertToInt, out int result))
			{
				return result;
			}

			return 0;
		}

		protected string GetValue(string sql, int whereParam, string field)
		{
			var id = string.Empty;

			try
			{
				using var conn = DbDriver.OpenConnection();
				using (var command = conn.CreateCommand())
				{
					command.CommandType = CommandType.Text;
					command.CommandText = sql;
					command.Parameters.AddWithValue("@c0", whereParam);

					using (var read = command.ExecuteReader())
					{
						if (read.Read())
						{
							var columnIndex = read.GetOrdinal(field);
							if (!read.IsDBNull(columnIndex))
							{
								id = read.GetString(columnIndex);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "GetValue", sql);
				throw;
			}

			return id;
		}

		public static string MakeUrlFriendly(string url)
		{
			if (string.IsNullOrWhiteSpace(url)) return "";

			// Max length is 40, so we initialize the buffer to that size
			var sb = new StringBuilder(40);
			bool lastWasSeparator = false;
			string input = url.ToLowerInvariant();

			// We'll skip the word "and" by checking for it manually or using a quick replace
			// For extreme performance, we handle characters one by one
			for (int i = 0; i < input.Length; i++)
			{
				char c = input[i];

				// 1. Logic for "and" removal: Check if this char starts "and"
				if (c == 'a' && i + 2 < input.Length && input[i + 1] == 'n' && input[i + 2] == 'd')
				{
					// Ensure it's the whole word "and" by checking boundaries
					bool start = (i == 0 || !char.IsLetterOrDigit(input[i - 1]));
					bool end = (i + 3 == input.Length || !char.IsLetterOrDigit(input[i + 3]));

					if (start && end)
					{
						i += 2; // skip 'a', 'n', 'd'
						continue;
					}
				}

				// 2. Filter allowed characters
				if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
				{
					sb.Append(c);
					lastWasSeparator = false;
				}
				else if (c == ' ' || c == '-')
				{
					// 3. Handle multiple spaces/hyphens (convert to single hyphen)
					if (!lastWasSeparator && sb.Length > 0)
					{
						sb.Append('-');
						lastWasSeparator = true;
					}
				}

				// 4. Hard stop at 40 characters
				if (sb.Length >= 40) break;
			}

			string result = sb.ToString().Trim('-');

			// 5. Ensure we don't end with a trailing hyphen from the substring logic
			if (result.Length > 0 && result.Contains('-'))
			{
				int lastDash = result.LastIndexOf('-');
				// If we hit the 40 char limit, trim back to the last full word
				if (sb.Length >= 40 && lastDash > 0)
					return result.Substring(0, lastDash);
			}

			return result;
		}

	}
}
