using ckLib;
using CKSA.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace CKSA.Pages.Catalog
{
	public class SearchResultsModel : PageModel
	{
		public string SearchTerm { get; set; }
		public string UrlBase { get; set; }
		public string Sep { get; set; }
		public Filter F { get; set; }
		public Dictionary<string, string> SubCategories { get; set; }
		public bool FilterSearch { get; set; } // Is search based off of filter selections.

		private readonly CookieHelper _cookies;
		public PagerCreator Pager { get; set; }

		public SearchResultsModel(CookieHelper cookies)
		{
			_cookies = cookies;
		}

		public IActionResult OnGet()
		{
			Stopwatch sw = Stopwatch.StartNew();

			int page = Filter.GetAllItems;
			string pageTitle;

			SearchTerm = Request.Query["description"];
			var pageExists = Request.Query["page"];

			if (string.IsNullOrEmpty(SearchTerm)) return Page();

			if (Int16.TryParse(pageExists, out short parsedPage))
			{
				page = parsedPage;
			}
			else
			{
				page = 1;
			}

			try
			{
				SearchTerm = CleanSearch(SearchTerm);
				var Parser = new UrlProductParser();
				var productUrl = ItemNumberSearch(SearchTerm);
				if(!string.IsNullOrEmpty(productUrl))
				{
					return Redirect(productUrl);
				}

				UrlBase = "/catalog/searchresults/?description=" + HttpUtility.UrlEncode(SearchTerm);
				var sortBy = Request.Query["sort"];
				pageTitle = "Search | " + SearchTerm;

				if (!string.IsNullOrEmpty(sortBy))
				{
					sortBy = "2"; // default to popular.
				}
				Sep = "&";
				UrlBase += "&sort=" + sortBy;

				F = new Filter(_cookies.GetWholesaleValue());
				F.SetOrderBy(sortBy);
				pageTitle += " | " + F.OrderBy;
				F.PageToGet = page;
				F.Mode = Filter.FilterMode.Search;
				F.SearchText = SearchTerm;
				string singleGroupSelected;
				var filters = Filter.LoadFiltersFromRequest(Request, out singleGroupSelected);

				// If SearchSubCategory is still taking long time then look into making this await.
				SearchSubCategory(F.SearchText);
				//SearchSubCategory2(F.SearchText);

				F.GetProducts(filters);

				if (filters.Count > 0)
				{
					F.BuildUiFromSearch(singleGroupSelected);
					F.SetCheckedFiltersInUi(filters);
					pageTitle += " | " + string.Join(",", filters);
				}
				else
				{
					F.BuildUiFromSearch(singleGroupSelected);
				}

				if (filters.Count > 0)
				{
					F.NumberResults = F.Products.Count;
					FilterSearch = true;
				}

				// Sort the items by how they were found and then put OOS at the end.
				F.Products = F.Products.OrderBy(x => x.HowFound).ThenBy(x => x.TempOutOfStock != 0).ToList();

				F.ReadyFiltersForUi();

				ViewData[ViewDataKeys.Title] = pageTitle;
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "SearchResults.OnGet", SearchTerm));
			}

			sw.Stop();
			Debug.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");
			return Page();
		}

				/// <summary>
		/// Search against the departments/category 3.
		/// Each call to UrlProductParser can be a lot of DB calls to get other ids and names.
		/// Going to try to get this all in one call
		/// </summary>
		private void SearchSubCategory(string searchOn)
		{
			SubCategories = new Dictionary<string, string>();

			using (var mysql = DbDriver.OpenConnection())
			{
				using (var aCommand = mysql.CreateCommand())
				{
					try
					{
						string searchParam = CreateLikeSearchString(searchOn, false, " `Category 3` ", aCommand);

						//
						// 48 is Brands category we don't want these to show.
						aCommand.CommandType = CommandType.Text;
						aCommand.CommandText = @"select `Category 3`,Cat3ID from `category 3` as A 
                        inner join `category 2` as B on A.Cat2ID = B.Cat2ID
                        where " + searchParam + " and B.Category != 48 and Popularity > 0 order by Popularity desc";

						aCommand.CommandText = @"SELECT s.`category 1` AS ShopName, s.Cat1ID AS ShopId, c.`category 2` AS CategoryName, 
    c.CAT2ID,sc.`Category 3` AS SubCategoryName,  sc.Cat3ID AS SubCategoryId FROM `Category 3` AS sc INNER JOIN `category 2` AS c ON sc.Cat2ID = c.Cat2ID
INNER JOIN category AS s ON c.category = s.Cat1ID WHERE " + searchParam + @" AND c.category != 48 AND sc.Popularity > 0
    ORDER BY sc.Popularity DESC;";

						using (var reader = aCommand.ExecuteReader())
						{
							while (reader.Read())
							{
								UrlProductParser categoryParser = new UrlProductParser();
								categoryParser.CurrentStep = UrlProductParser.Step.SubCategory;
								categoryParser.ShopName = reader.ReadString(0);
								categoryParser.ShopId = reader.ReadInt32(1);
								categoryParser.ShopUrl = UrlProductParser.MakeUrlFriendly(categoryParser.ShopName);
								
								categoryParser.CategoryName = reader.ReadString(2);
								categoryParser.CategoryId = reader.ReadInt32(3);
								categoryParser.CategoryUrl = UrlProductParser.MakeUrlFriendly(categoryParser.CategoryName);


								categoryParser.SubCategoryName = reader.GetString(4);
								categoryParser.SubCategoryId = reader.GetInt32(5);
								categoryParser.SubCategoryUrl = UrlProductParser.MakeUrlFriendly(categoryParser.SubCategoryName);

								//categoryParser.ParseStandardMethod();

								var link = categoryParser.GenerateSingleLink(UrlProductParser.Step.SubCategory);

								try
								{
									SubCategories.Add(categoryParser.SubCategoryName, link);
								}
								catch
								{
									// duplicate, remove eventually
								}
							}
						}
					}
					catch (Exception ex)
					{
						ErrorHandler.Handle(ex, "SearchSubCategory", searchOn, Request.GetDisplayUrl());
					}
				}
			}
		}

		/// <summary>
		/// Search against the departments/category 3.
		/// Each call to UrlProductParser can be a lot of DB calls to get other ids and names.
		/// Going to try to get this all in one call
		/// </summary>
		private void SearchSubCategory2(string searchOn)
		{
			var subCatsToCheck = new Dictionary<string, string>();

			using (var mysql = DbDriver.OpenConnection())
			{
				using (var aCommand = mysql.CreateCommand())
				{
					try
					{
						string searchParam = CreateLikeSearchString(searchOn, false, " `Category 3` ", aCommand);

						//
						// 48 is Brands category we don't want these to show.
						aCommand.CommandType = CommandType.Text;
						aCommand.CommandText = @"select `Category 3`,Cat3ID from `category 3` as A 
                        inner join `category 2` as B on A.Cat2ID = B.Cat2ID
                        where " + searchParam + " and B.Category != 48 and Popularity > 0 order by Popularity desc";

						using (var reader = aCommand.ExecuteReader())
						{
							while (reader.Read())
							{
								string name = reader.GetString(0);
								int id = reader.GetInt32(1);

								UrlProductParser categoryParser = new UrlProductParser();
								categoryParser.CurrentStep = UrlProductParser.Step.SubCategory;
								categoryParser.SubCategoryUrl = UrlProductParser.MakeUrlFriendly(name);
								categoryParser.SubCategoryName = name;
								categoryParser.SubCategoryId = id;
								categoryParser.ParseStandardMethod();

								var link = categoryParser.GenerateSingleLink(UrlProductParser.Step.SubCategory);

								try
								{
									subCatsToCheck.Add(name, link);
								}
								catch
								{
									// duplicate, remove eventually
								}
							}
						}

						bool areEqual = subCatsToCheck.Count == SubCategories.Count && !subCatsToCheck.Except(SubCategories).Any();

					}
					catch (Exception ex)
					{
						ErrorHandler.Handle(ex, "SearchSubCategory", searchOn, Request.GetDisplayUrl());
					}
				}
			}
		}

		private string CreateLikeSearchString(string searchOn, bool makeSingular, string searchAgainst, MySqlCommand aCommand)
		{
			ArrayList searchTerms = GetSearchTerms(searchOn);
			int nCount = 0;
			string sql = string.Empty;

			foreach (string item in searchTerms)
			{
				if (item.Length == 0)
				{
					continue;
				}

				string itemToSearchOn = item;
				if (nCount > 0)
				{
					sql += " AND";
				}

				if (makeSingular)
				{
					itemToSearchOn = MakeStringSingular(itemToSearchOn);
				}

				string finalItem = string.Format("%{0}%", CheckSQL(itemToSearchOn));
				nCount++;
				string paramName = string.Format("@c{0}", nCount);
				string concateMe = string.Format("{0}Like {1}", searchAgainst, paramName);

				aCommand.Parameters.AddWithValue(paramName, finalItem);
				sql += concateMe;
			}

			return sql;
		}

		static public string CheckSQL(string SQL)
		{
			SQL = SQL.Replace("\"", "\\\"");
			SQL = SQL.Replace("'", "\\'");
			SQL = SQL.Replace("’", "\\’");
			SQL = SQL.Replace("‘", "\\‘");


			return SQL;
		}

		private string MakeStringSingular(string item)
		{
			if (item[item.Length - 1] == 'S' ||
				item[item.Length - 1] == 's')
			{
				item = item.Substring(0, item.Length - 1);
			}

			return item;
		} 

		private ArrayList GetSearchTerms(string searchOn)
		{
			ArrayList list = new ArrayList();
			try
			{
				searchOn = searchOn.Trim();
				int nSpaceLoc = searchOn.IndexOf(" ");
				if (nSpaceLoc > 0)
				{
					while (searchOn.Length > 0)
					{
						string item = searchOn.Substring(0, nSpaceLoc);
						list.Add(item);
						if (nSpaceLoc == searchOn.Length)
						{
							break;
						} // end if
						searchOn = searchOn.Substring(nSpaceLoc + 1);
						nSpaceLoc = searchOn.IndexOf(" ");
						if (nSpaceLoc < 0)
						{
							nSpaceLoc = searchOn.Length;
						} // end if
					} // end while
				}
				else
				{
					// Just one item.
					list.Add(searchOn);
				} // end if

			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(
					new ckExceptionData(ex, "SearchResults::GetSearchTerms"));
			} // end try

			return (list);
		}


		private string ItemNumberSearch(string searchOn)
		{
			var friendlyUrl = string.Empty;

			try
			{
				using (var conn = DbDriver.OpenConnection())
				using (var aCommand = conn.CreateCommand())
				{
					aCommand.CommandType = CommandType.Text;
					aCommand.CommandText = @"Select ItemId, b.UnitId, a.FriendlyUrl from `Item numbers, descrip, page` as a
                        inner join allunit as b on a.ItemId = b.ProductId WHERE `Item Number` = @c0";
					aCommand.Parameters.AddWithValue("@c0", searchOn);

					using (var reader = aCommand.ExecuteReader())
					{
						if (reader.Read())
						{
							friendlyUrl = reader.GetString(2);
						}
					}
				}
				if(!string.IsNullOrEmpty(friendlyUrl))
					friendlyUrl = friendlyUrl.Substring(1, friendlyUrl.Length - 2);
			}
			catch
			{
				throw;
			}

			return friendlyUrl;
		}

		public string MiniLoad(string itemNumber, int itemId, string description, string friendlyUrl, string? mastUnit = null, string? imageName = null)
		{
			string imageLinkStart = $"<a href={friendlyUrl}>";
			string newDescription = HttpUtility.HtmlEncode(description);
			string miniPathName = "ProductError.jpg";

			try
			{
				miniPathName = CkDefines.ImageMiniUrl(mastUnit, imageName);
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, Request.GetDisplayUrl(), "SearchResults-MiniLoad");
			}
			string image =
				string.Format("{0}<img src='{1}' title='{2}' alt='{2}' /></a>",
				imageLinkStart,
				miniPathName,
				newDescription);

			return image;
		}

		public string PageControl()
		{
			if (FilterSearch || F.NumberResults == 0)
			{
				return string.Empty;
			}

			if (Pager == null)
			{
				Pager = new PagerCreator();
				Pager.SearchPage = true;
				Pager.UrlBase = UrlBase;
				Pager.PageLink = Sep + "page=";
				Pager.CurrentPage = F.PageToGet;
				Pager.ItemsPerPage = Filter.MiniPerPage;
				Pager.NumberOfItems = F.NumberResults;
			}
			return Pager.Create();
		}

		public string PartialMatchString()
		{
			bool firstGo = true;
			string combinedWithAnd = string.Empty;
			if (string.IsNullOrEmpty(F.SearchText))
			{
				combinedWithAnd = "no search term provided.";
			}
			else
			{
				foreach (var s in F.SearchText.Split(' '))
				{
					if (string.IsNullOrEmpty(s) == false)
					{
						if (firstGo == false)
						{
							combinedWithAnd += "<span style='color:red;font-weight:bold;'> OR</span> ";
						}
						firstGo = false;
						combinedWithAnd += s;
					}
				}
			}
			return combinedWithAnd;
		}

		public string ExactMatchString()
		{
			bool firstGo = true;
			string combinedWithAnd = string.Empty;

			foreach (var s in F.SearchText.Split(' '))
			{
				if (string.IsNullOrEmpty(s) == false)
				{
					if (firstGo == false)
					{
						combinedWithAnd += "<span style='color:red;font-weight:bold;'> AND</span> ";
					}
					firstGo = false;
					combinedWithAnd += s;
				}
			}

			return combinedWithAnd;
		}

		public string Description(object description)
		{
			return CkDefines.FormatStringToLength(description.ToString(), 30);
		} // Description

		public string FormatPrice(decimal price, decimal sale, DateTime endSaledate)
		{
			if (endSaledate < DateTime.Now.Date)
			{
				sale = decimal.MinValue;
			}

			return CkDefines.CreatePriceHtml(price, sale);
		}

		private string CleanSearch(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return string.Empty;

			// Trim whitespace
			input = input.Trim();

			// Remove excessive whitespace (multiple spaces to single space)
			input = Regex.Replace(input, @"\s+", " ");

			// Limit length
			if (input.Length > 100)
				input = input.Substring(0, 100);

			return input;
		}
	}
}
