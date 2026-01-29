using ckLib;
using CKSA.Helpers;
using CKSA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using MySqlConnector;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace CKSA.Pages.Catalog
{
	public class CategoryModel : PageModel
	{
		public List<CategoryItem>? Cats { get; set; }
		private readonly IMemoryCache _cache;
		private UrlProductParser? Parser { get; set; }
		public string _ShopName { get; private set; } = string.Empty;
		public int _ShopId { get; private set; }
		public Dictionary<string, string> Breadcrumbs { get; set; }
		public string H1Tag { get; set; }
		public string GeneralDescription { get; set; }
		public string HtmlTitle { get; set; }
		public string HtmlDescription { get; set; }

		public CategoryModel(IMemoryCache cache)
		{
			_cache = cache;
			Cats = null;
			Parser = null;
		}

		public IActionResult OnGet(string ShopName, int ShopId)
		{
			try
			{
				_ShopName = ShopName;
				_ShopId = ShopId;

				Parser = new UrlProductParser(UrlProductParser.Step.Shop, RouteData);
				Breadcrumbs = Parser.GenerateListBreadcrumb(UrlProductParser.Step.Shop);
				LoadHtmlInfo(Parser.ShopId);
				H1Tag = Parser.ShopName;

				var key = $"Cat{ShopId}";
				var cacher = new PageCacher<List<CategoryItem>>();
				Cats = cacher.Retrieve(key);

				if (Cats == null)
				{
					GetDataItem();
					if (Cats == null || Cats.Count == 0)
					{
						return RedirectToPage("/catalog/shops", new { i = "1" });
					}

					CreateImages();
					cacher.Store(Cats, key);
				}

				return Page();
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "catalog/category");
			}

			return RedirectToPage("/catalog/shops", new { i="1"});
		}

		private void GetDataItem()
		{
			Cats = new List<CategoryItem>();

			using var conn = DbDriver.OpenConnection();
			using (var command = conn.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = "SELECT Cat2ID, `Category 2`, GenDescrip, Url FROM `category 2` where Category = @c0 Order BY `Category 2`";
				command.Parameters.AddWithValue("@c0", _ShopId);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var d = new CategoryItem();
						d.Id = reader.ReadInt32(0);
						d.C = reader.ReadString(1);
						d.Url = reader.ReadString(3);
						Cats.Add(d);
					}
				}
			}
		}

		private void CreateImages()
		{
			var invalidCats = new List<CategoryItem>();
			var useHardCodeImages = Parser?.ShopId == "42";
			Dictionary<int, string>? imageDictionary = null;

			// Hard code these for occasions.
			if (useHardCodeImages)
			{
				imageDictionary = new Dictionary<int, string>
			{
				{ 549, "baby-shower-baking-supplies.jpg" },
				{ 550, "birthday-baking-supplies.jpg" },
				{ 644, "wedding-baking-supplies.jpg" },
				{ 551, "christmas-baking-supplies.jpg" },
				{ 552, "easter-baking-supplies.jpg" },
				{ 553, "mothers-day-baking-supplies.jpg" },
				{ 554, "4th-july-baking-supplies.jpg" },
				{ 555, "graduation-baking-supplies.jpg" },
				{ 556, "halloween-baking-supplies.jpg" },
				{ 639, "mardi-gras-baking-supplies.jpg" },
				{ 558, "new-year-baking-supplies.jpg" },
				{ 559, "religious-baking-supplies.jpg" },
				{ 560, "retirement-baking-supplies.jpg" },
				{ 561, "st-patty-baking-supplies.jpg" },
				{ 562, "thanksgiving-baking-supplies.jpg" },
				{ 626, "licensed-baking-supplies.jpg" },
				{ 563, "themed-baking-supplies.jpg" },
				{ 564, "sports-baking-supplies.jpg" },
				{ 565, "valentine-baking-supplies.jpg" },
				{ 748, "winter-baking-supplies.jpg" }
			};
			}

			foreach (var c in Cats)
			{
				try
				{
					c.Img = CreateSubCatImage(c, imageDictionary != null ? imageDictionary[c.Id] : string.Empty);
					if (string.IsNullOrEmpty(c.Img))
					{
						// Nothing in a category so do not add it to the web page.
						invalidCats.Add(c);
					}
				}
				catch (Exception ex)
				{
					ErrorHandler.Handle(new ckExceptionData(ex, "category::CreateImages", c.Id.ToString()));
				}
			}

			foreach (var c in invalidCats)
			{
				Cats.Remove(c);
			}
		}


		private string CreateSubCatImage(CategoryItem c, string useImage)
		{
			StringBuilder finalString = new StringBuilder();
			string image = string.Empty;
			string sql = string.Empty;

			try
			{
				var cat3Ids = GetCat3Id(c.Id);

				if (cat3Ids.Count == 0)
				{
					// Should I notify someone about this?
					return string.Empty;
				}

				var inStmt = string.Join(",", cat3Ids.ToArray());

				sql = string.Format(@"Select  A.ItemId, A.Description, A.MasterItemNumber, A.ImageUrl
                from `item numbers, descrip, page` as A
                Inner Join `category 3 and items` As B
                On A.ItemId=B.ItemId
                where B.Cat3ID in ({0}) Order by A.Popularity asc limit 1", inStmt);

				using (var conn = DbDriver.OpenConnection())
				using (var command = conn.CreateCommand())
				{
					command.CommandType = CommandType.Text;
					command.CommandText = sql;

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							// IdeaID is just to get the correct image...
							string itemNumber = reader.ReadString("MasterItemNumber");

							string miniPathName;
							if (string.IsNullOrEmpty(useImage))
							{
								var imageUrl = reader.ReadString("ImageUrl");
								miniPathName = CkDefines.ImageCategoryUrl(itemNumber, imageUrl);
							}
							else
							{
								miniPathName = "/images/" + useImage;
							}

							var imgAlt = WebUtility.HtmlEncode(c.C);

							image = $"<a href={c.Url}><img src='{miniPathName}' title='{imgAlt}' alt='{imgAlt}'/></a>";

							finalString.Append(image);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "category::CreateSubCatImage", sql));
			}

			return finalString.ToString();
		}
		private List<int> GetCat3Id(int cat2Id)
		{
			int cat3Id;
			var cat3Ids = new List<int>();
			// Used to be limit of 1 but some categories dont have items in them so need to get all and then
			// later check each category until an item is found.
			string sql = "Select Cat3ID From `category 3` Where Cat2ID = @c0 Order By Cat3ID desc";

			try
			{
				using var conn = DbDriver.OpenConnection();
				using (var command = conn.CreateCommand())
				{
					command.CommandType = CommandType.Text;
					command.CommandText = sql;
					command.Parameters.AddWithValue("@c0", cat2Id);
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							cat3Id = reader.ReadInt32(0);
							cat3Ids.Add(cat3Id);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "CategoryImageList::GetCat3Id", sql));
				throw;
			}

			return cat3Ids;
		}
		/// <summary>
		/// Load information to place in the html file for search engines.
		/// </summary>
		private void LoadHtmlInfo(string shopId)
		{
			// TODO: 56 has no html stuff in db as of yet.
			if (shopId == "56") return;

			try
			{
				using (var conn = DbDriver.OpenConnection())
				using (var command = conn.CreateCommand())
				{
					command.CommandType = System.Data.CommandType.Text;
					command.CommandText = "Select HtmlTitle, HtmlDescription, GenDescrip from category where cat1id = @c0";
					command.Parameters.AddWithValue("@c0", shopId);
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							HtmlTitle = reader.GetString(0);
							HtmlDescription = reader.GetString(1);
							GeneralDescription = reader.GetString(2);
						}
					}
				}
				ViewData["Title"] = HtmlHelper.CreateTitle(HtmlTitle);
				ViewData["Description"] = HtmlDescription;
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, string.Format("Catalog_category.LoadHtmlInfo: shopId = {0}", shopId));
			}
		}
	}
}
