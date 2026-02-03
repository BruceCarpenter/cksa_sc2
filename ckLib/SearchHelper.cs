using MySqlConnector;

/// <summary>
/// Summary description for SearchHelper
/// </summary>
namespace ckLib
{
	public class SearchHelper
	{
		PorterStemmerAlgorithm.PorterStemmer ps = new PorterStemmerAlgorithm.PorterStemmer();

		public bool BoolSearch { get; set; }
		public string[] SearchWords { get; set; }
		public string StemmedWord { get; set; }
		public int NumRecordsTotal { get; set; }
		public string SearchAgainst { get; set; }
		public string SelectSql { get; set; }
		public bool WildCard { get; set; }
		public Dictionary<int, int> SearchCount { get; set; }
		public Filter.OrderProduct OrderBy { get; set; }


		/// <summary>
		/// If this is true then only return the item ids and not all the other product info.
		/// This is a big time saver.
		/// </summary>
		public bool OnlyItemIds { get; set; }

		public SearchHelper()
		{
			OnlyItemIds = false;
			BoolSearch = false;
			WildCard = true;
			NumRecordsTotal = 0;
			OrderBy = Filter.OrderProduct.Popular;
		}

		public void Do()
		{
			string select = string.Empty;
			string booleanText = "IN BOOLEAN MODE";

			SearchWords = CleanSearch(SearchAgainst.Split(' '));
			StemmedWord = CreateStemmedWord(SearchWords);

			select = "SELECT ItemId";

			//if (ShouldDoBool())
			//{
			//    // If results are returned with booleon mode on than use it, if not don't.
			//    BoolSearch = true;
			//    booleanText = "IN BOOLEAN MODE";
			//}

			//
			// SearchType - Used as a way to separate the way the item was found
			// 1 - indicates it was found via boolean stemmed search
			// 2 - indicates it was found via non boolean stemmed search
			// 3 - indicates it was found via wildcard search

			if (OnlyItemIds)
			{
				// This gets called when getting the filter values for the search.
				SelectSql = select + string.Format(@" FROM `item numbers, descrip, page` WHERE MATCH(stemmed) AGAINST(@c IN BOOLEAN MODE)");
			}
			else
			{
				// If boolean search then we have some exact search matches
				// If user does search "blue non-pareils" then the - is removing the IN BOOLEAN MODE vlaue     
				/*if (DoesSearchContainBooleanTerm())
                {
                    booleanText = string.Empty;
                }*/

				select = "SELECT ItemId, `Item Number`, Description,Units,Price AS PriceA,SalePrice,ItemTempOOS,CKPSpecial,DateExpCKPS,SaleEndCKPS,QuantityCKPS,FriendlyUrl";
				SelectSql = select + string.Format(@" ,1 as SearchType, Popularity,MasterItemNumber,WholesalePriceA, ImageUrl FROM `item numbers, descrip, page` WHERE MATCH(stemmed) AGAINST(@c {0}) ", booleanText);

				if (WildCard)
				{
					SelectSql += "Union " + select + string.Format(@" ,2 as SearchType, Popularity, MasterItemNumber,WholesalePriceA, ImageUrl FROM `item numbers, descrip, page` WHERE MATCH(stemmed) AGAINST(@c) ");
					SelectSql = @"SELECT distinct A.ItemId, `Item Number`, Description,Units,PriceA,SalePrice,ItemTempOOS,CKPSpecial,DateExpCKPS,SaleEndCKPS,QuantityCKPS,FriendlyUrl, SearchType, Popularity, MasterItemNumber,WholesalePriceA, ImageUrl,q.Quantity,q.Price AS QuantityPrice from (" +
						SelectSql;

					SelectSql += " Union " + select + " ,3 as SearchType, Popularity, MasterItemNumber,WholesalePriceA, ImageUrl FROM `item numbers, descrip, page` WHERE {0}";

					//SelectSql += @") as A group by ItemId order by SearchType  limit {0},{1}";
					SelectSql += string.Format(@") as A LEFT JOIN QuantityDiscount q ON A.ItemId = q.ItemId group by ItemId order by SearchType, {0}", GetSortByString(OrderBy));
				}
			}
		}

		public string GetSortByString(Filter.OrderProduct orderBy)
		{
			var sortBy = string.Format(" Popularity");

			switch (orderBy)
			{
				case Filter.OrderProduct.ItemNumber:
					sortBy = string.Format(" `Item Number` asc");
					break;
				case Filter.OrderProduct.Newest:
					sortBy = string.Format(" ItemId desc");
					break;
				case Filter.OrderProduct.Popular:
					sortBy = string.Format(" Popularity asc");
					break;
				case Filter.OrderProduct.PriceAsc:
					sortBy = string.Format(" Price asc");
					break;
				case Filter.OrderProduct.PriceDesc:
					sortBy = string.Format(" Price desc");
					break;
			}

			return sortBy;
		}

		/// <summary>
		/// Determine if the search term contains special search IN BOOLEAN MODE
		/// characters.
		/// </summary>
		protected bool DoesSearchContainBooleanTerm()
		{
			bool doesContain = false;

			foreach (var s in SearchWords)
			{
				if (s.Contains("-"))
				{
					return true;
				}
			}

			return doesContain;
		}

		private bool ShouldDoBool()
		{
			bool doBoolean = false;
			var sql = "select count(ItemId) FROM `item numbers, descrip, page` WHERE MATCH(stemmed) AGAINST(@c IN BOOLEAN MODE)";

			//
			// Might be able to share this with count, might be able to remove count?
			//
			using (var mySql = DbDriver.OpenConnection())
			using (var command = mySql.CreateCommand())
			{
				command.CommandText = sql;
				command.CommandType = System.Data.CommandType.Text;
				command.Parameters.AddWithValue("@c", StemmedWord);
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						var count = reader.GetInt16(0);
						doBoolean = (count > 0) ? true : false;
					}
				}
			}

			return doBoolean;
		}

		public string GetDescriptionSearch(MySqlCommand command)
		{
			var description = string.Empty;
			var searchTerms = SearchAgainst.Split(' ');
			int counter = 0;

			foreach (var term in searchTerms)
			{
				if (string.IsNullOrEmpty(term) == false)
				{
					var searchAgainst = string.Format("%{0}%", term);
					var parm = string.Format("@c{0}", counter);
					if (string.IsNullOrEmpty(description) == false)
					{
						description += " or ";
					}
					description += string.Format(" description like {0}", parm);
					command.Parameters.AddWithValue(parm, searchAgainst);
					counter++;
				}
			}

			return description;
		}

		static public string[] CleanSearch(string[] searchWords)
		{
			var cleaned = new System.Collections.Generic.List<string>();

			foreach (var word in searchWords)
			{
				if (string.IsNullOrEmpty(word) == false)
				{
					cleaned.Add(word);
				}
			}

			return cleaned.ToArray();
		}

		private string CreateStemmedWord(string[] searchWords)
		{
			var stemmedWord = string.Empty;

			foreach (var word in searchWords)
			{
				if (stemmedWord.Length > 0)
					stemmedWord += " ";

				stemmedWord += "+" + "\"" + ps.stemTerm(word.ToLower()) + "\"";
			}

			return stemmedWord;
		}

		public void TotalRecordCount(MySqlCommand preBuiltCommand)
		{
			SearchCount = new Dictionary<int, int>();

			try
			{
				using (var conn = DbDriver.OpenConnection())
				using (var command = conn.CreateCommand())
				{
					var sqlCount = preBuiltCommand.CommandText;

					command.CommandText = @" select count( B.SearchType ), B.SearchType from (" + sqlCount + ") as B group by SearchType";

					foreach (var p in preBuiltCommand.Parameters)
					{
						command.Parameters.Add(p);
					}

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var count = reader.GetInt32(0);
							var searchType = reader.GetInt32(1);
							SearchCount[searchType] = count;
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "SearchResults::TotalRecordCount-1", preBuiltCommand.CommandText));
			}
		}

		private string RemoveLimitFromSearch(string p)
		{
			int limitPos = p.LastIndexOf("limit");
			var s = p.Substring(0, limitPos);

			return s;
		}

		/// <summary>
		/// Determine if this is a boolean search or regular.
		/// </summary>
		/// <param name="searchWords"></param>
		/// <returns></returns>
		private bool IsBooleanSearch(string[] searchWords)
		{
			foreach (var word in searchWords)
			{
				if (IsSpecialSearchMarker(word[0]))
					return true;
				else if (word.Length > 0 && IsSpecialSearchMarker(word[word.Length - 1]))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Check if this word is using a special search marker.
		/// </summary>
		/// <param name="mark"></param>
		/// <returns></returns>
		private bool IsSpecialSearchMarker(char mark)
		{
			return (mark == '-' || mark == '+' || mark == '*');
		}

	}
}