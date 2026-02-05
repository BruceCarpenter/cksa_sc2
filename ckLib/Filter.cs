using Microsoft.AspNetCore.Http;
using MySqlConnector;
using System.Text;


/*
 * misc sql *
 * items with same filter multiple times
 * select fId,ItemNumber, count(*) as cnt from FilterProduct as B group by fId, ItemNumber having cnt > 1
*/
namespace ckLib
{
	public class Filter
	{
		public class FilterValue
		{
			public string FilterName { get; set; }
			public int FilterId { get; set; }
			public string FilterSet { get; set; }
			public string IsFilterUiSet { get; set; }
			public int Count { get; set; }
			/// <summary>
			/// Only used if sort has to be manually set.
			/// </summary>
			public int SortValue { get; set; }
		}

		public class FilterList
		{
			public string FilterGroupName { get; set; }
			public int FilterGroupId { get; set; }
			public List<FilterValue> Filters { get; set; }
			public int SortValue { get; set; }
		}

		public class FilterProduct : Product
		{
			public int FilterCount { get; set; }

			/// <summary>
			///  Used for filter.
			/// </summary>
			public int HowFound { get; set; }
		}


		public enum FilterMode
		{
			Search,
			Mini,
			Promotion
		}

		public enum OrderProduct
		{
			ItemNumber, // Name
			Newest,
			Popular,
			PriceDesc,
			PriceAsc
		}

		public FilterMode Mode { get; set; }
		public OrderProduct OrderBy { get; set; }
		public string SearchText { get; set; }
		public List<FilterList> TheFilters { get; set; }
		public List<FilterProduct> Products { get; set; }
		public const int MiniPerPage = 42;

		// Help get only the number of results that are needed.
		public int NumberResults { get; set; }
		public int PageToGet { get; set; }

		// Keep track of the number of matches for each search type.
		public Dictionary<int, int> SearchCount { get; set; }

		/// <summary>
		/// List of products used for finding filters based off of products.
		/// </summary>
		protected StringBuilder ProductInSql { get; set; }

		static public int GetAllItems = -1;

		static public Dictionary<string, int> AvailableFilters;

		/// <summary>
		/// Cache information
		/// </summary>
		private bool CanCache { get; set; }

		private int WholeSaleCustomer { get; set; }


		public Filter(int wholesale)
		{
			TheFilters = new List<FilterList>();
			PageToGet = GetAllItems;
			OrderBy = OrderProduct.ItemNumber;
			CanCache = true;
			WholeSaleCustomer = wholesale;
		}

		/// <summary>
		/// Set the order by based off of a numerical value passed in.
		/// </summary>
		public void SetOrderBy(string orderBy)
		{
			if (string.IsNullOrEmpty(orderBy) == false)
			{
				var index = 0;
				if (int.TryParse(orderBy, out index))
				{
					OrderBy = (OrderProduct)Enum.GetValues(typeof(OrderProduct)).GetValue(index);
				}
			}
		}

		/// <summary>
		/// Get the filter list organized for the UI.
		/// </summary>
		public void ReadyFiltersForUi()
		{
			SortPrice();
			SortCandyBox();
			SortBoxConstruction();

			// Sort the filter by their SortValue
			TheFilters = TheFilters.OrderBy(a => a.SortValue).ToList();
		}

		protected void SortBoxConstruction()
		{
			var filterList = FindFilterList(36);
			var sortWeights = new Dictionary<int, int>()
			{
				{466,1},
				{467,2},
				{468,3}
			};

			if (filterList != null)
			{
				foreach (var f in filterList.Filters)
				{
					f.SortValue = sortWeights[f.FilterId];
				}
				filterList.Filters = filterList.Filters.OrderBy(a => a.SortValue).ToList();
			}
		}

		protected void SortCandyBox()
		{
			var filterList = FindFilterList(35);
			var sortWeights = new Dictionary<int, int>()
			{
				{457,1},
				{458,2},
				{459,3},
				{460,5},
				{461,6},
				{462,7},
				{463,9},
				{464,10},
				{465,12},
				{535,8},
				{536,11},
				{537,4},
				{594,13},
				{597, 14 }// TODO: Not sure if this is correct but needed to stop crashing.
            };

			if (filterList != null)
			{
				foreach (var f in filterList.Filters)
				{
					try
					{
						f.SortValue = sortWeights[f.FilterId];
					}
					catch (Exception ex)
					{
						ErrorHandler.Handle(new ckExceptionData(ex, "SearchResults::SortCandyBox", "Missing a sort box"));
					}
				}
				filterList.Filters = filterList.Filters.OrderBy(a => a.SortValue).ToList();
			}
		}

		protected void SortPrice()
		{
			// Get the Price list organized properly.
			// Since prices are sorted as text need to get those items inserted properly.
			// Price is fType of 17.
			var priceFilter = FindFilterList(17);
			if (priceFilter != null)
			{
				if (priceFilter.Filters.Count > 1)
				{
					// sort by FilterId.
					priceFilter.Filters = priceFilter.Filters.OrderBy(a => a.FilterId).ToList();
				}
			}
		}

		/// <summary>
		/// Get the products based on the filter mode: either from search or subcategory.
		/// Use the passed in filters to filter out the results. Filters are passed in as 
		/// an array of strings: ["1,2"],["5,8"]. This ors across each string and then
		/// ands across the array indexes. Item can be in 1 or 2 and then in either 5 or 8.
		/// 
		/// If filters are set for each time the item is found with a valid filter it will be
		/// found in the select. So if 2 filters are set then the product should be found 
		/// twice. If it is only found once then it did not matcht the and part of the filter.
		/// </summary>
		public void GetProducts(List<string> filters)
		{
			var sql = string.Empty;
			var sqlGenerated = string.Empty;

			//if (!Regex.IsMatch(SearchText, @"^\d+$"))
			//{
			//	EMailHelper.SendMeDebugInfo(string.Format("GetProducts starting: {0}", SearchText));
			//}
			foreach (var filterType in filters)
			{
				CanCache = false;

				if (string.IsNullOrEmpty(sql) == false)
				{
					sql += " union all ";
				}
				sql += Filter.CreateFilterSelect(filterType);
			}

			using (var mySql = DbDriver.OpenConnection())
			{
				if (string.IsNullOrEmpty(SearchText) == false)
				{
					if (Mode == FilterMode.Mini)
					{
						// do mini page search, SearchText is Cat3ID
						sqlGenerated = CreateMiniFilterSearchSql(SearchText, sql, false);
						if (string.IsNullOrEmpty(sql))
						{
							var miniSearchSql = CreateMiniFilterSearchSql(SearchText, sql, true);
							using (var countCommand = mySql.CreateCommand())
							{
								countCommand.CommandText = miniSearchSql;
								GetCount(countCommand);
							}
						}
					}
					else if (Mode == FilterMode.Promotion)
					{
						sqlGenerated = CreatePromotionSearchSql(SearchText, sql, false);
						if (string.IsNullOrEmpty(sql))
						{
							var countCommand = CreatePromotionSearchSql(SearchText, sql, true);
							using (var countCommand2 = mySql.CreateCommand())
							{
								countCommand2.Connection = mySql;
								GetCount(countCommand2);
							}
						}
					}
					else
					{
						// do stem search.
						var command = CreateStemmedFilterSearchSql(SearchText, sql);
					}
				}
				else
				{
					// no search term or mini id. Going to assume getting products that satisfy certain filter criteria.
					if (string.IsNullOrEmpty(sql) == false)
					{
						string sqlPart2 = @"inner join ( select  ItemId, `Item Number`, Description,Units,Price,SalePrice,ItemTempOOS,CKPSpecial,DateExpCKPS,SaleEndCKPS,QuantityCKPS, FriendlyUrl, Popularity, MasterItemNumber
                                FROM `item numbers, descrip, page` as A
                                ) as B 
                                on T.ProductId = B.ItemId order by B.Popularity asc";
						sql = string.Format(@"select T.FilterCount, T.ProductId, T.ItemNumber, B.Description, B.Units,B.Price,B.SalePrice, B.ItemTempOOS,B.CKPSpecial,B.DateExpCKPS,B.SaleEndCKPS,B.QuantityCKPS, B.FriendlyUrl, T.MasterItemNumber 
                        from (
                            {0}
                            ) as T {1}", sql, sqlPart2);
					}
				}

				Products = new List<FilterProduct>();

				var dictionary = new Dictionary<string, FilterProduct>();

				using (MySqlCommand command = new MySqlCommand())
				{
					command.Connection = mySql;
					command.CommandText = sqlGenerated;

					// No filter set and nothing to search against.
					if (string.IsNullOrEmpty(command.CommandText))
						return;

					// Index counter to consolidate the dataReader index code.
					var columnCounter = 0;
					if (filters.Count != 0)
					{
						ProductInSql = new StringBuilder();
						columnCounter = 1;
					}

					using (var dataReader = command.ExecuteReader())
					{
						if (dataReader != null)
						{
							while (dataReader.Read())
							{
								var p = new FilterProduct();
								try
								{
									p.ItemId = dataReader.GetInt32(columnCounter);
									p.MasterItemNumber = dataReader.GetString("MasterItemNumber");
									p.ItemNumber = dataReader.GetString(columnCounter + 1);
									p.Description = dataReader.GetString(columnCounter + 2);
									p.Units = dataReader.GetString(columnCounter + 3);
									p.Price = dataReader.GetDecimal(columnCounter + 4);
									p.SalePrice = dataReader.ReadDecimal(columnCounter + 5);
									p.ImageName = dataReader.ReadString("ImageUrl").ToLower();
									p.ImageUrl = CkDefines.ImageUrl(p.MasterItemNumber, p.ImageName).ToLower();

									if (CkDefines.ProductValidWholesale(p.ItemNumber))
									{
										p.WholeSalePriceA = dataReader.ReadDecimal("WholeSalePriceA");
										p.Price = GetWholesalePriceA(p.Price, p.WholeSalePriceA);
									}

									try
									{
										// Check for multi discount pricing
										// LEFT join for search can cause some of this to be NULL.
										if (dataReader.FieldCount < 17)
										{
											throw new Exception("Search needs updated.");
										}

										if (/*Mode != FilterMode.Search && */!dataReader.IsDBNull(16) && !dataReader.IsDBNull(17) && dataReader.FieldCount >= 19)
										{
											// Only do this if the discount price is cheaper. If WholesalePrice is cheaper then no need.											
											var discountPrice = dataReader.GetDecimal(18);
											if (discountPrice < p.Price)
											{
												p.Discounts = new List<Product.Discount>();
												p.Discounts.Add(new Product.Discount { Quantity = 1, Price = p.Price });
												p.Discounts.Add(new Product.Discount { Quantity = dataReader.GetInt32(17), Price = discountPrice });
											}
										}
									}
									catch (Exception ex)
									{

									}

									//
									// This started in June 23, 2016??????
									if (dataReader.IsDBNull(columnCounter + 6) == false)
									{
										p.TempOutOfStock = dataReader.GetInt32(columnCounter + 6);
									}
									else
									{
										p.TempOutOfStock = 0;
									}
									p.CKPSpecial = dataReader.ReadInt32(columnCounter + 7);
									if (p.CKPSpecial > 0)
									{
										p.DateEndCKPS = dataReader.ReadDateTime(columnCounter + 8);
										if (dataReader.IsDBNull(columnCounter + 10))
										{
											p.QuanitityCKPS = Int32.MaxValue - 1;
										}
										else
										{
											p.QuanitityCKPS = dataReader.GetInt16(columnCounter + 10);
										}
										//
										// If date has ended then no special should be run.
										if (p.DateEndCKPS == DateTime.MaxValue)
										{
											p.CKPSpecial = 0;
										}
									}

									if (dataReader.IsDBNull(columnCounter + 9) == false)
									{
										p.SaleEndCKPS = dataReader.ReadDateTimeAsString(columnCounter + 9);
									}

									p.FriendlyUrlLink = dataReader.GetString(columnCounter + 11);

									if (filters.Count == 0)
									{
										// no filter set, get all values
										if (Mode == FilterMode.Search)
										{
											p.HowFound = dataReader.ReadInt16(12);
										}

										var masterProduct = Products.Find(x => x.ItemId == p.ItemId);
										if (masterProduct == null)
										{
											Products.Add(p);
											p.SetAllUnitsOOS(p.TempOutOfStock == 1);
										}
										else
										{
											// Need to get sale price here as well.

											// If the master is on sale then set its price to its
											// sale price. If not then the regular price will show if that is higher.
											var salePrice = masterProduct.SalePrice;
											if (salePrice != decimal.MinValue && salePrice < masterProduct.Price)
											{
												masterProduct.Price = salePrice;
											}
											masterProduct.AddPrice(p.Price, p.SalePrice);
											masterProduct.SetAllUnitsOOS(p.TempOutOfStock == 1);
										}
									}
									else
									{
										// Only add to dictionary once but keep track of times found.
										try
										{
											var key = p.ItemNumber;
											if (dictionary.ContainsKey(key))
											{
												dictionary[key].FilterCount++;
											}
											else
											{
												p.FilterCount = 1;
												dictionary.Add(key, p);
											}
										}
										catch (Exception ex)
										{
											// problem loading an item, inform but keep going.
										}
									}
								}
								catch (Exception ex)
								{
								}
							}
						}
					}


					// Special case of putting some stuff to the top
					if (SearchText == "1013" && OrderBy == OrderProduct.ItemNumber)
					{
						// Put our icing at the top.
						Products = Products.OrderBy(p => p.ItemNumber.StartsWith("CK-BC") ? 0 : 1).ToList();
					}

					// move valid finds from dictionary to final list.
					// Only add items to products list if they match the filter criteria.
					if (columnCounter == 1)
					{
						foreach (var d in dictionary)
						{
							if (d.Value.FilterCount == filters.Count)
							{
								Products.Add(d.Value);
								ProductInSql.AppendFormat("{0},", d.Value.ItemId);
							}
						}
						if (ProductInSql.Length > 0)
						{
							ProductInSql.Remove(ProductInSql.Length - 1, 1);
						}
					}

				}


				if (NumberResults == 0)
				{
					NumberResults = Products.Count;
				}
			}
		}

		/// <summary>
		/// TODO: This is in product.cs eventually remove one of them.
		/// </summary>
		public decimal GetWholesalePriceA(decimal price, decimal wholesalePriceA)
		{
			if (WholeSaleCustomer != 0)
			{
				if (wholesalePriceA > 0)
				{
					return Math.Min(wholesalePriceA, price * .9m);
				}
				else
				{
					return price * .9m;
				}
			}

			return price;
		}

		protected void GetCount(MySqlCommand countCommand)
		{
			using (var reader = countCommand.ExecuteReader())
			{
				if (reader.Read())
				{
					NumberResults = reader.GetInt32(0);
				}
			}
		}

		private void GetFiltersForASpecificGroup(string groupName)
		{
			try
			{
				using (var mySql = DbDriver.OpenConnection())
				{
					using (var command = GetFilterOptions())
					{
						command.CommandType = System.Data.CommandType.Text;
						command.Connection = mySql;

						//
						// This should give the count of each filter.
						//
						var sql = string.Format(@"select L.*, count(*) from
								(
								select X.fValue, X.fTypeId, X.fId, GroupName from (
													select Q.ProductId, V.fValue, V.fTypeId, V.fId, R.fTypeName as GroupName from
																(
																select F.fId, F.ProductId from FilterProduct as F where F.ProductId in 
																(
																	{0}
																) 
																) as Q

																inner join FilterValue as V on Q.fId = V.fId
																inner join FilterType as R on V.fTypeId = R.fTypeId

														) as X where GroupName=@c0 group by x.ProductId, x.fId
								) as L
									group by L.fId", command.CommandText);

						command.Parameters.AddWithValue("@c0", groupName);

						command.CommandText = sql;

						using (var reader = command.ExecuteReader())
						{
							//
							// In order to keep these items alphabatized clear the list
							// and recreate it here.
							var filterList = FindFilterList(groupName);
							if (filterList != null)
							{
								filterList.Filters.Clear();

								while (reader.Read())
								{
									var filtervalue = new FilterValue();

									filtervalue.FilterId = reader.GetInt16(2);
									filtervalue.FilterName = reader.GetString(0);
									filtervalue.FilterSet = reader.GetString(3);
									var filterGroupId = reader.GetInt16(1);
									filtervalue.Count = reader.GetInt16(4);

									AddFilterValue(filtervalue, filterGroupId, true);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "Filter::GetFiltersForASpecificGroup"));
			}
		}

		/// <summary>
		/// Based on the products available only return valid filters values. Don't return Black is Black is not
		/// available in the results.
		/// </summary>
		public void BuildUiFromSearch(string singleGroupSelected = null)
		{
			try
			{
				if (string.IsNullOrEmpty(SearchText) == false)
				{
					//
					// Setting this to null should allow all filters to come back....
					//ProductInSql = null;
					var cacher = new PageCacher<List<FilterList>>();
					var key = string.Format(CacheKeys.MiniFilterKey, SearchText);

					if (CanCache && Mode == FilterMode.Mini)
					{
						TheFilters = cacher.Retrieve(key);
					}
					else
					{
						cacher = null;
						TheFilters = null;
					}

					if (TheFilters == null)
					{
						TheFilters = new List<FilterList>();
						using (var mySql = DbDriver.OpenConnection())
						{
							using (var command = GetFilterOptions())
							{
								// Only get if boolean search is true.
								if (string.IsNullOrEmpty(command.CommandText) == false)
								{
									command.CommandType = System.Data.CommandType.Text;
									command.Connection = mySql;

									//
									// This should give the count of each filter.
									// This has been giving numbers to itesm that are no longer in the system but I think
									// are still related to filters.
									// Updating sql to use what looks like a better query.
									var sql = string.Format(@"select L.*, count(*) from
                            (
                                select X.fValue, X.fTypeId, X.fId, GroupName from (
                                    select Q.ProductId, V.fValue, V.fTypeId, V.fId, R.fTypeName as GroupName from
                                    (
                                        select F.fId, F.ProductId from FilterProduct as F where F.ProductId in 
                                            (
                                            {0}
                                            ) 
                                    ) as Q

                                    inner join FilterValue as V on Q.fId = V.fId
                                    inner join FilterType as R on V.fTypeId = R.fTypeId

                                ) as X  group by x.ProductId, x.fId
                            ) as L
                            group by L.fId order by L.fValue", command.CommandText);



									command.CommandText = sql;

									using (var reader = command.ExecuteReader())
									{
										while (reader.Read())
										{
											var filtervalue = new FilterValue();

											filtervalue.FilterId = reader.GetInt16(2);
											filtervalue.FilterName = reader.GetString(0);
											filtervalue.FilterSet = reader.GetString(3);
											var filterGroupId = reader.GetInt16(1);
											filtervalue.Count = reader.GetInt16(4);

											AddFilterValue(filtervalue, filterGroupId);
										}
									}
								} // if
							} // using
							if (cacher != null)
							{
								cacher.Store(TheFilters, key);
							}
						}
					}
				}

				//
				// Get all the filters in this one group.
				//

				//
				// If this is not cleared then this group of items is used to return the valid groups and most have been filtered out
				//   so clear this to get all groups in original filter type.
				//
				ProductInSql = null;
				if (string.IsNullOrEmpty(singleGroupSelected) == false)
				{
					GetFiltersForASpecificGroup(singleGroupSelected);
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "Filter::BuildUiFromSearch"));
			}
		}

		static public string GetSortByString(string tableName, OrderProduct orderBy)
		{
			var putOosAtEnd = string.Format("{0}.ItemTempOOS,", tableName);
			var sortBy = string.Format(" order by {0} {1}.`Item Number` asc", putOosAtEnd, tableName);

			switch (orderBy)
			{
				case OrderProduct.ItemNumber:
					// default
					break;
				case OrderProduct.Newest:
					sortBy = string.Format(" order by {0} {1}.ItemId desc", putOosAtEnd, tableName);
					break;
				case OrderProduct.Popular:
					sortBy = string.Format(" order by {0} {1}.Popularity asc", putOosAtEnd, tableName);
					break;
				case OrderProduct.PriceAsc:
					sortBy = string.Format(" order by {0} {1}.Price asc", putOosAtEnd, tableName);
					break;
				case OrderProduct.PriceDesc:
					sortBy = string.Format(" order by {0} {1}.Price desc", putOosAtEnd, tableName);
					break;
			}

			return sortBy;
		}

		#region Private

		private MySqlCommand GetFilterOptions()
		{
			if (ProductInSql != null)
			{
				MySqlCommand command = new MySqlCommand();
				command.CommandText = ProductInSql.ToString();
				return command;
			}

			if (Mode == FilterMode.Mini)
			{
				return GetFilterOptionsFromMiniSql(SearchText);
			}
			else if (Mode == FilterMode.Promotion)
			{
				return GetFilterOptionsFromPromotionSql(SearchText);
			}

			return GetFilterOptionsFromSearchSql(SearchText);
		}

		private MySqlCommand GetFilterOptionsFromPromotionSql(string searchAgainst)
		{
			MySqlCommand command = new MySqlCommand();

			try
			{
				if (string.IsNullOrEmpty(searchAgainst) == false)
				{
					var dateFormat = "%m/%d/%Y";
					var selectSql = string.Format(@"Select ItemId from `item numbers, descrip, page` where 
                            CKPSpecial = {0} and STR_TO_DATE(SaleEndCKPS,'{1}') > STR_TO_DATE('{2}','%m/%d/%Y')",
							searchAgainst, dateFormat, DateTime.Now.ToShortDateString());
					command.CommandText = $"select T.ItemId from ({selectSql}) as T";
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "SearchResults::GetFilterOptionsFromPromotionSql", command.CommandText));
			}

			return command;
		}

		private MySqlCommand GetFilterOptionsFromMiniSql(string searchAgainst)
		{
			MySqlCommand command = new MySqlCommand();

			try
			{
				if (string.IsNullOrEmpty(searchAgainst) == false)
				{
					command.CommandText = $"select T.ItemId from (Select ItemId from `category 3 and items` where Cat3ID={searchAgainst}) as T";
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "SearchResults::GetFilterOptionsFromMiniSql", command.CommandText));
			}

			return command;
		}

		private MySqlCommand GetFilterOptionsFromSearchSql(string searchAgainst)
		{
			MySqlCommand command = new MySqlCommand();

			try
			{
				if (string.IsNullOrEmpty(searchAgainst) == false)
				{
					var sh = new SearchHelper();
					sh.OnlyItemIds = true;
					sh.SearchAgainst = searchAgainst;
					sh.Do();

					//
					// Only get filter values for boolean search results.
					//if (sh.BoolSearch)
					{
						command.Parameters.AddWithValue("@c", sh.StemmedWord);
						command.CommandText = sh.SelectSql;
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "SearchResults::GetFilterOptionsFromSearchSql"));
			}

			return command;
		}

		private MySqlCommand CreateStemmedFilterSearchSql(string searchAgainst, string filterSql)
		{
			var SQL = string.Empty;
			var command = new MySqlCommand();

			try
			{
				var sh = new SearchHelper();

				sh.WildCard = filterSql == string.Empty;
				sh.SearchAgainst = searchAgainst;
				sh.OrderBy = OrderBy;
				sh.Do();

				command.Parameters.AddWithValue("@c", sh.StemmedWord);
				var description = string.Empty;

				if (sh.WildCard)
				{
					description = sh.GetDescriptionSearch(command);
				}

				SQL = string.Format(sh.SelectSql, description);
				if (string.IsNullOrEmpty(filterSql) == false)
				{
					SQL = @"select  F.FilterCount, T.ItemId, T.`Item Number`, T.Description,  T.Units,T.PriceA,T.SalePrice,T.ItemTempOOS,T.CKPSpecial,
                        T.DateExpCKPS,T.SaleEndCKPS,T.QuantityCKPS,T.FriendlyUrl,T.MasterItemNumber,T.WholesalePriceA, T.ImageUrl,q.Quantity,q.Price AS QuantityPrice  from (" + SQL;
					SQL += ") as T inner join";
					SQL += "(select L.FilterCount, L.ItemNumber, L.ProductId from (" + filterSql + ") as L ) as F on T.ItemId = F.ProductId left JOIN QuantityDiscount q ON F.ProductId = q.ItemId" + GetSortByString("T", OrderBy);
				}

				command.CommandText = SQL;

				if (string.IsNullOrEmpty(filterSql))
				{
					sh.TotalRecordCount(command);
					SearchCount = sh.SearchCount;
					CountTotalSearch();
				}

				// Handle paging
				if ((PageToGet == GetAllItems) || string.IsNullOrEmpty(filterSql) == false)
				{
					// do not add limit get all items
				}
				else
				{
					int startNum = (PageToGet - 1) * MiniPerPage;

					//NumberOfResultsToGet(PageToGet, numRecordsPerPage);
					SQL += string.Format(" limit {0}, {1}", startNum, MiniPerPage);
					command.CommandText = SQL;
				}

			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "SearchResults::CreateStemmedFilterSearchSql-1",
					string.Format("SQL: {0},searchAgainst: {1}", SQL, searchAgainst)));
			}

			return command;
		}

		/// <summary>
		/// Make the final row of the search a complete row.
		/// </summary>
		private int NumberOfResultsToGet(int startNumber, int numResultsPerPage)
		{
			int resultsToGet = 0;
			int resultsPage = (startNumber + 1) * numResultsPerPage;
			// SearchCount
			// NumberResults

			if (SearchCount.ContainsKey(1))
			{
				if (SearchCount[1] > resultsPage)
				{
					resultsToGet = resultsPage;
				}
				else if (SearchCount[1] > (resultsPage - numResultsPerPage))
				{
					resultsToGet = (resultsPage - SearchCount[1]);
				}
			}

			if (resultsToGet < numResultsPerPage)
			{
				if (SearchCount.ContainsKey(3))
				{
					if (SearchCount[3] + SearchCount[1] > resultsPage)
					{
						resultsToGet = resultsPage;
					}
					else if (SearchCount[3] + SearchCount[1] > (resultsPage - numResultsPerPage))
					{
						resultsToGet = (resultsPage - SearchCount[3]);
					}
				}
			}

			return resultsToGet;
		}

		private void CountTotalSearch()
		{
			NumberResults = 0;
			foreach (var s in SearchCount)
			{
				NumberResults += s.Value;
			}
		}

		private string CreatePromotionSearchSql(string searchAgainst, string filterSql, bool countSql)
		{
			string SQL = string.Empty;
			bool filterEmpty = string.IsNullOrEmpty(filterSql);

			try
			{
				var orderBy = GetSortByString("A", OrderBy);

				var dateFormat = "%m/%d/%Y";
				/*
				sql = string.Format(@"Select Units, ItemId, `Item Number`, Description," +
					" NonTaxable, NonDiscountable, `Extra Shipping`," +
					" Weight, Price, SalePrice, ItemTempOOS, CKPSpecial, DateExpCKPS, SaleEndCKPS, QuantityCKPS, FriendlyUrl" +
					" from `item numbers, descrip, page` where CKPSpecial = {0} and STR_TO_DATE(SaleEndCKPS,'{1}') > STR_TO_DATE('{2}','%m/%d/%Y')" +
					" order by CKPSpecial desc" +
					" {3}", Parameter, dateFormat, DateTime.Now.ToShortDateString(), limit);
				*/

				var selectStatement = countSql ? @"count(A.ItemId)" :
						@"A.ItemId, A.`Item Number`, A.Description,A.Units,A.Price,A.SalePrice,A.ItemTempOOS,A.CKPSpecial,A.DateExpCKPS, A.SaleEndCKPS,A.QuantityCKPS,A.FriendlyUrl, A.Popularity, A.MasterItemNumber,A.WholesalePriceA,A.ImageUrl, D.Quantity AS discountQuantity,D.Price AS discountPice";

				SQL = string.Format(@"Select {0}
                                    from `item numbers, descrip, page` as A left JOIN quantitydiscount AS D ON D.ItemId=A.ItemId where CKPSpecial = {1} and
                                    STR_TO_DATE(SaleEndCKPS,'{2}') >= STR_TO_DATE('{3}','%m/%d/%Y') {4}",
										selectStatement,
										searchAgainst,
										dateFormat, DateTime.Now.ToShortDateString(),
										filterEmpty ? orderBy : string.Empty,
										countSql == false && string.IsNullOrEmpty(filterSql) ? GetLimitCount() : string.Empty);
				if (filterEmpty == false)
				{
					SQL = @"select  F.FilterCount, T.ItemId, T.`Item Number`, T.Description,  T.Units,T.Price,T.SalePrice,T.ItemTempOOS,T.CKPSpecial,T.DateExpCKPS,T.SaleEndCKPS,T.QuantityCKPS,T.FriendlyUrl, T.MasterItemNumber,T.WholesalePriceA,T.ImageUrl,T.discountQuantity,T.discountPice  
                          from (" + SQL;
					SQL += ") as T inner join";
					SQL += "(select L.FilterCount, L.ItemNumber, L.ProductId from (" + filterSql + ") as L ) as F on T.ItemId = F.ProductId " + GetSortByString("T", OrderBy);
				}

			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "SearchResults::CreateMiniFilterSearchSql", SQL));
			}

			return SQL;
		}

		private string CreateMiniFilterSearchSql(string searchAgainst, string filterSql, bool countSql)
		{
			string SQL = string.Empty;
			bool filterEmpty = string.IsNullOrEmpty(filterSql);

			try
			{
				var orderBy = GetSortByString("A", OrderBy);

				// A table has been created to merge unit information with this informatio,A.ItemTempOOS   n so everything can flow as normal.
				// Use the unitId as the ItemId.
				var selectStatement = countSql ? @"count(A.ItemId)" :
					string.IsNullOrEmpty(filterSql) ?
						  @"c.UnitId as ItemId, A.`Item Number`, A.Description,A.Units,A.Price,A.SalePrice,A.ItemTempOOS,A.CKPSpecial,A.DateExpCKPS, A.SaleEndCKPS,A.QuantityCKPS,A.FriendlyUrl, A.Popularity, A.MasterItemNumber,A.WholesalePriceA, A.ImageUrl,D.Quantity,D.Price"
						: @"A.ItemId, A.`Item Number`, A.Description,A.Units,A.Price,A.SalePrice,A.ItemTempOOS,A.CKPSpecial,A.DateExpCKPS, A.SaleEndCKPS,A.QuantityCKPS,A.FriendlyUrl, A.Popularity,A.MasterItemNumber,A.WholesalePriceA, A.ImageUrl,D.Quantity AS DiscountQuantity,D.Price AS DiscountPrice ";

				var sqlUnitJoin = string.Empty;

				if (!countSql && string.IsNullOrEmpty(filterSql))
				{
					sqlUnitJoin = "Inner Join allunit as C On C.ProductId = A.ItemId";
				}

				SQL = string.Format(@"Select {0}
	                                from `item numbers, descrip, page` as A 
	                                Inner Join `category 3 and items` As B On A.ItemId=B.ItemId 
									left JOIN quantitydiscount AS D ON D.ItemId=A.ItemId
	                                {4}
	                                where B.Cat3ID={1} {2} {3}  
	                                ",
										selectStatement,
										searchAgainst,
										filterEmpty ? orderBy : string.Empty,
										countSql == false && string.IsNullOrEmpty(filterSql) ? GetLimitCount() : string.Empty,
										sqlUnitJoin);

				if (filterEmpty == false)
				{
					SQL = @"select  F.FilterCount, T.ItemId, T.`Item Number`, T.Description,  T.Units,T.Price,T.SalePrice,T.ItemTempOOS,T.CKPSpecial,T.DateExpCKPS,T.SaleEndCKPS,T.QuantityCKPS,T.FriendlyUrl,T.MasterItemNumber,T.WholesalePriceA, T.ImageUrl,T.DiscountQuantity,T.DiscountPrice
                          from (" + SQL;
					SQL += ") as T inner join";
					SQL += "(select L.FilterCount, L.ItemNumber, L.ProductId from (" + filterSql + ") as L ) as F on T.ItemId = F.ProductId " + GetSortByString("T", OrderBy);
				}

			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "SearchResults::CreateMiniFilterSearchSql", SQL));
			}

			return SQL;
		}

		private string GetLimitCount()
		{
			if (PageToGet == GetAllItems)
			{
				return string.Empty;
			}
			else
			{
				int startIndex = (PageToGet - 1) * MiniPerPage;
				return string.Format("Limit {0},{1}", startIndex, MiniPerPage);
			}
		}

		/// <summary>
		/// Add the filter value to the filter list. 
		/// </summary>
		private void AddFilterValue(FilterValue value, int fTypeId, bool extraCheck = false)
		{
			var filterList = FindFilterList(value.FilterSet);

			if (filterList == null)
			{
				if (Filter.AvailableFilters.Keys.Contains(value.FilterSet))
				{
					filterList = new FilterList();
					filterList.FilterGroupId = fTypeId;
					filterList.FilterGroupName = value.FilterSet;
					filterList.Filters = new List<FilterValue>();
					filterList.SortValue = Filter.AvailableFilters[filterList.FilterGroupName];
					TheFilters.Add(filterList);
				}
			}

			//
			// Only sometimes do I need to do this check.
			//
			bool found = false;
			if (extraCheck)
			{
				foreach (var f in filterList.Filters)
				{
					if (f.FilterId == value.FilterId)
					{
						found = true;
						break;
					}
				}
			}

			if (found == false && filterList != null)
			{
				filterList.Filters.Add(value);
			}
		}

		public FilterList FindFilterList(string groupName)
		{
			FilterList found = null;

			foreach (var f in TheFilters)
			{
				if (f.FilterGroupName == groupName)
				{
					return f;
				}
			}

			return found;
		}

		public FilterList FindFilterList(int fTypeId)
		{
			FilterList found = null;

			foreach (var f in TheFilters)
			{
				if (f.FilterGroupId == fTypeId)
				{
					return f;
				}
			}

			return found;
		}

		#endregion Private

		#region Static

		/// <summary>
		/// Create the select statement for each filter type: color/flavor/matrial
		/// </summary>
		private static string CreateFilterSelect(string filterId)
		{
			return string.Format("select count(ProductId) as FilterCount, ProductId, ItemNumber from FilterProduct where fId in ({0}) group by ProductID",
				filterId);
		}

		public static List<int> SeparateFilterList(List<string> filters)
		{
			var filterList = new List<int>();

			foreach (var f in filters)
			{
				// 1,2
				var split = f.Split(',');
				foreach (var s in split)
				{
					filterList.Add(Convert.ToInt16(s));
				}
			}

			return filterList;
		}

		#endregion Static


		#region Load Filters for UI
		/// <summary>
		/// This used only for Filter.aspx test page.
		/// </summary>
		public void LoadFilterTypes(MySqlConnection mysql)
		{
			var sql = "select fTypeId, fTypeName from filtertype";

			try
			{
				using (var command = mysql.CreateCommand())
				{
					command.CommandText = sql;
					using (MySqlDataReader dataReader = command.ExecuteReader())
					{
						while (dataReader.Read())
						{
							var filters = new FilterList();
							filters.FilterGroupId = dataReader.GetInt16(0);
							filters.FilterGroupName = dataReader.GetString(1);
							TheFilters.Add(filters);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "Filter.LoadFilterTypes", sql));
			}
		}

		/// <summary>
		/// This used only for Filter.aspx test page.
		/// </summary>
		public void LoadFilterValues(MySqlConnection mySql, List<string> filters)
		{
			var filterSeparated = SeparateFilterList(filters);
			var sql = string.Empty;

			try
			{
				foreach (var f in TheFilters)
				{
					sql = @"select T.ProductsInFilter, T.fId, A.fValue 
            from (select count(Id) ProductsInFilter, fId from filterproduct group by fId order by ProductsInFilter desc) as T
            inner join filterValue as A on A.fId = T.fId where A.fTypeId = @c0 order by A.fValue";

					using (var command = mySql.CreateCommand())
					{
						command.CommandText = sql;
						command.Parameters.AddWithValue("@c0", f.FilterGroupId);
						using (MySqlDataReader dataReader = command.ExecuteReader())
						{
							f.Filters = new List<FilterValue>();
							while (dataReader.Read())
							{
								var filter = new FilterValue();
								filter.Count = dataReader.GetInt16(0);
								filter.FilterId = dataReader.GetInt16(1);
								filter.FilterName = dataReader.GetString(2);

								if (filterSeparated.Contains(filter.FilterId))
								{
									filter.IsFilterUiSet = "checked";
								}

								f.Filters.Add(filter);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "Filter", "LoadFilterValues"));
			}
		}

		public void SetCheckedFiltersInUi(List<string> filters)
		{
			var filterSeparated = Filter.SeparateFilterList(filters);

			foreach (var f in TheFilters)
			{
				foreach (var fId in f.Filters)
				{
					if (filterSeparated.Contains(fId.FilterId))
					{
						fId.IsFilterUiSet = "checked";
					}
				}
			}
		}

		#endregion Load Filters for UI


		static public List<string> LoadFiltersFromRequest(HttpRequest request, out string singleGroupName)
		{
			singleGroupName = string.Empty;

			if (AvailableFilters == null)
			{
				string[] filterNames = {
				"Color",
				"Flavor Base",
				"Flavor",
				"General Shape",
				"Candy Mold Cavity Type",
				"Cookie Cutter Size",
				"Material",
				"Edible Decoration Type",
				"Food Color Base",
				"Finish",
				"Dietary Features",
				"Price",
				"Ethnicity",
				"Seasonal and Holiday",
				"Special Occasions",
				"Baking Cup Size",
				"Popular Themes (Non Licensed)",
				"Pattern",
				"Baking/Candy Cup Material",
				"Cake Board Material",
				"Brush Material",
				"Chocolate Type",
				"Brand",
				"Candy Box Size/Volume",
				"Box Construction",
				"Box Feature",
				"Box Material",
				"Icing Type",
				"Ingredients",
				"Piping Tip or Coupler Type",
				"Candy Cup Size",
				"Pearl / Dragee Size",
				"Wire Gauge Size",
				"Shipping",
				"Special Pricing"
			};

				//"Shape",
				//"Size",
				//"Material",
				//"Edible Type",
				//"Finish",
				//"Dietary Features",
				//"Origin",
				//"Flat Rate Shipping",
				var counter = 1;
				AvailableFilters = new Dictionary<string, int>();

				foreach (var f in filterNames)
				{
					AvailableFilters[f] = counter;
					counter++;
				}
			}

			var filters = new List<string>();

			// determine if there is just one group
			singleGroupName = request.Query["y"];
			var x = 0;
			do
			{
				var f = request.Query["x" + x.ToString()];
				if (string.IsNullOrEmpty(f))
					break;
				else
					filters.Add(f);
				x++;
			} while (true);


			// and by the type and or across
			// so get color:black or blue and flavor:apple
			// alternative is color: black and blue and flavor:apple
			/*
			foreach (var af in AvailableFilters)
			{
				if (request[af.Key] != null)
				{
					filters.Add(request[af.Key]);
					if (string.IsNullOrEmpty(singleGroupName))
					{
						singleGroupName = af.Key;
					}
				}
			}

			if (filters.Count > 1)
			{
				singleGroupName = string.Empty;
			}
			*/
			return filters;
		}
	}
}