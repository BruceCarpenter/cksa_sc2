using ckLib;
using CKSA.Helpers;
using CKSA.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using System.Data;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CKSA.Pages.Catalog
{
	public class SubCategoryModel : PageModel
	{
		public string _ShopName { get; private set; } = string.Empty;
		public string _SubCategoryName { get; private set; } = string.Empty;
		public int _ShopId { get; private set; }
		public int _CategoryId { get; private set; }
		public SubCategoryData? SubCatModel { get; set; }


		public IActionResult OnGet(string ShopName, string SubCategoryName, int ShopId, int CategoryId)
		{
			try
			{
				_ShopName = ShopName;
				_ShopId = ShopId;
				_SubCategoryName = SubCategoryName;
				_CategoryId = CategoryId;

				var key = $"{CacheKeys.SubCatKey}{CategoryId}";
				var cacher = new PageCacher<SubCategoryData>();

				SubCatModel = cacher.Retrieve(key);

				if (SubCatModel == null)
				{
					SubCatModel = new SubCategoryData();

					SubCatModel.Parser = new UrlProductParser(UrlProductParser.Step.Category, RouteData);

					LoadHtmlInfo();
					GetData();
					CreateImages();

					SubCatModel.Breadcrumbs = SubCatModel.Parser.GenerateListBreadcrumb(UrlProductParser.Step.Shop);
					SubCatModel.H1Tag = SubCatModel.Parser.CategoryName;

					cacher.Store(SubCatModel, key);
				}

				ViewData[ViewDataKeys.Title] = CkHtmlHelper.CreateTitle(SubCatModel.HtmlTitle);
				ViewData[ViewDataKeys.Description] = SubCatModel.HtmlDescription;
				ViewData[ViewDataKeys.Canonical] = SubCatModel.Canonical;

				return Page();
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "catalog/category");
			}

			return RedirectToPage("/catalog/shops", new { i = "1" });
		}

		private void LoadHtmlInfo()
		{
			var canonical = Request.GetDisplayUrl();

			try
			{
				using (var conn = DbDriver.OpenConnection())
				using (var command = conn.CreateCommand())
				{
					command.CommandType = System.Data.CommandType.Text;
					command.CommandText = "Select HtmlTitle, HtmlDescription, GenDescrip, Url, Quicklinks from `category 2` where cat2id = @c0";
					command.Parameters.AddWithValue("@c0", SubCatModel.Parser.CategoryId);
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							SubCatModel.HtmlTitle = reader.ReadString(0);
							SubCatModel.HtmlDescription = reader.ReadString(1);
							SubCatModel.GeneralDescription = reader.ReadString(2);
							SubCatModel.Canonical = "https://www.countrykitchensa.com" + reader.ReadString(3);
							SubCatModel.QuickLinks = reader.ReadString(4);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "Catalog_category.LoadHtmlInfo", canonical);
			}
		}

		private void GetData()
		{
			SubCatModel.Cats = new List<CategoryItem>();

			using (var conn = DbDriver.OpenConnection())
			using (var command = conn.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = "SELECT Cat3ID, `Category 3`, Descrip3, Url FROM `category 3` where Cat2ID = @c0 Order BY `Category 3`";
				command.Parameters.AddWithValue("@c0", SubCatModel.Parser.CategoryId);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var d = new CategoryItem();
						d.Id = reader.ReadInt32(0);
						d.C = reader.ReadString(1);
						d.Url = reader.ReadString(3);
						//d.Desc = CKDefines.ReadDb(reader, 2);
						SubCatModel.Cats.Add(d);
					}
				}
			}
		}

		private void CreateImages()
		{
			var invalidCats = new List<CategoryItem>();

			foreach (var c in SubCatModel.Cats)
			{
				c.Img = CreateSubCatImage(c);
				if (string.IsNullOrEmpty(c.Img))
				{
					// If no img found then this should be removed from the category but in the mean time
					// remove it from the SubCats so a bad link is not visible.	
					invalidCats.Add(c);
				}
			}

			// Remove the invalid items here. No sense showing on page since link will not work.
			foreach (var c in invalidCats)
			{
				SubCatModel.Cats.Remove(c);
			}
		}


		private string CreateSubCatImage(CategoryItem c)
		{
			var finalString = new StringBuilder();
			var image = string.Empty;
			var sql = string.Empty;

			try
			{
				sql = @"Select  A.ItemId, A.Description, A.MasterItemNumber, A.ImageUrl
                from `item numbers, descrip, page` as A
                Inner Join `category 3 and items` As B
                On A.ItemId=B.ItemId
                where B.Cat3ID = @c0 Order by A.Popularity asc limit 1";

				using (var conn = DbDriver.OpenConnection())
				using (var command = conn.CreateCommand())
				{
					command.CommandType = CommandType.Text;
					command.CommandText = sql;
					command.Parameters.AddWithValue("@c0", c.Id);

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							string itemNumber = reader.ReadString("MasterItemNumber");
							string itemDescription = reader.ReadString("Description");
							string imageUrl = reader.ReadString("ImageUrl");

							var miniPathName = CkDefines.ImageCategoryUrl(itemNumber, imageUrl);

							image = $@"<a href='{c.Url}'><img src='{miniPathName}' alt='{WebUtility.HtmlEncode(c.C)}' title='{WebUtility.HtmlEncode(c.C)}'/></a>";

							finalString.Append(image);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "SubCategory::CreateSubCatImage", sql));
			}

			return finalString.ToString();
		}

		public string GetQuickLinks()
		{
			var bar = "<div style='height: 12px;background-color:#54c4b8;margin-bottom:15px;'></div><div style='margin-bottom:10px;font-size:1.5em;'>Quick Links</div>";
			var result = Regex.Replace(SubCatModel.QuickLinks, "<a", "<a class='col-lg-4' style='height: 25px'", RegexOptions.IgnoreCase);


			return bar + result;
		}
	}
}
