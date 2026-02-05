using ckLib;
using CKSA.Helpers;
using CKSA.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace CKSA.Pages.Catalog
{
	public class MiniModel : PageModel
	{
		public string _ShopName { get; private set; } = string.Empty;
		public string _SubCategoryName { get; private set; } = string.Empty;
		public int _ShopId { get; private set; }
		public int _CategoryId { get; private set; }
		public int _SubCategoryId { get; private set; }
		public MiniData? MiniDataModel { get; set; }

		public Filter F { get; set; }

		public MiniHelper CkMini1 = new MiniHelper();


		// Get parameters
		[BindProperty(SupportsGet = true)]
		public string? sort { get; set; }

		public IActionResult OnGet(string ShopName, string SubCategoryName, int ShopId, int CategoryId, int SubCategoryId)
		{
			_ShopName = ShopName;
			_ShopId = ShopId;
			_SubCategoryName = SubCategoryName;
			_CategoryId = CategoryId;
			_SubCategoryId = SubCategoryId;

			bool promotional = false;
			try
			{
				MiniDataModel = new MiniData();

				// Currently no caching here due to the various options. Might cache just the base option and then render all
				// other options?
				// var key = $"{CacheKeys.MiniFilterKey}{CategoryId}";
				// var cacher = new PageCacher<SubCategoryData>();


				// Need to check for promotional Key here.

				if (promotional)
				{
					MiniDataModel.Parser = new UrlProductParser();
					MiniDataModel.Breadcrumbs = new Dictionary<string, string>();
					MiniDataModel.Breadcrumbs.Add("Home", "\\");
					MiniDataModel.Breadcrumbs.Add("Shops", "\\shops");
					ViewData["Title"] = "Country Kitchen SweetArt Promotion: " + RouteData.Values["promotion"];
					MiniDataModel.H1Tag = "Promotional Items";
					MiniDataModel.Canonical = "https://www.countrykitchensa.com" + Request.GetDisplayUrl();
				}
				else
				{
					// Could cache the parser here. It is doing 4 db calls to get all this data.
					// Look into if I need all this data from the parser or if some is already passed in route.
					// For example the parser generated SubCategoryUrl but this was passed in?
					MiniDataModel.Parser = new UrlProductParser(UrlProductParser.Step.SubCategory, RouteData);
					CkMini1.RunAs = 0;
					CkMini1.ShopId = _ShopId;
					CkMini1.CatId = _CategoryId;
					CkMini1.Id = _SubCategoryId;
					CkMini1.SubCatId = _SubCategoryId;

					MiniDataModel.Breadcrumbs = MiniDataModel.Parser.GenerateListBreadcrumb(UrlProductParser.Step.SubCategory);
					MiniDataModel.H1Tag = MiniDataModel.Parser.SubCategoryName;

					CkMini1.ShopName = MiniDataModel.Parser.ShopUrl;
					CkMini1.ShopId = MiniDataModel.Parser.ShopId;
					CkMini1.CatId = MiniDataModel.Parser.CategoryId;
					CkMini1.Id = MiniDataModel.Parser.SubCategoryId;	
					CkMini1.SubCatId = CkMini1.Id;

					MiniDataModel.Canonical = CreateCanonical(MiniDataModel.Parser.SubCategoryId);
					MiniDataModel.UrlBase = MiniDataModel.Canonical;
					
					if (string.IsNullOrEmpty(sort) == false)
					{
						MiniDataModel.Sep = "&";
						MiniDataModel.UrlBase += "?sort=" + sort;
					}
					else
					{
						MiniDataModel.Sep = "?";
					}
				} // end not promotional

				string singleGroupSelected;
				Filter.FilterMode filterMode = Filter.FilterMode.Mini;
				var filters = Filter.LoadFiltersFromRequest(Request, out singleGroupSelected);

				if (promotional)
				{
					filterMode = Filter.FilterMode.Promotion;
				}

				if (F == null)
				{
					F = new Filter(CookieHelper.GetWholesaleValue(HttpContext.Request.Cookies));
					F.SetOrderBy(sort);
					F.PageToGet = -1;
					F.Mode = filterMode;
					F.SearchText = promotional ? RouteData.Values["promotion"].ToString() : CkMini1.SubCatId.ToString();

					F.GetProducts(filters);

					if (filters.Count > 0)
					{
						F.BuildUiFromSearch(singleGroupSelected);
						F.SetCheckedFiltersInUi(filters);
					}
					else
					{
						F.BuildUiFromSearch();
					}

					F.ReadyFiltersForUi();
				}

				GetDescription();


				return Page();
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "catalog/mini:Page_Load");
			}

			return Redirect($"/shop/{_ShopName}/{_SubCategoryName}/{_ShopId}/{_CategoryId}");
		}

		private string CreateCanonical(int subCategoryId)
		{
			var canonical = Request.GetDisplayUrl();

			try
			{
				using var conn = DbDriver.OpenConnection();
				using (var command = conn.CreateCommand())
				{
					command.CommandType = CommandType.Text;
					command.CommandText = @"select url from `category 3` where Cat3id=@c0";
					command.Parameters.AddWithValue("@c0", subCategoryId);

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							canonical = "https://www.countrykitchensa.com" + reader.ReadString(0);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "mini::CreateCanonical", "", Request.GetDisplayUrl()));
			}

			return canonical;
		}

		public string CreateMoreButton(Product pi)
		{
			var moreBtn = string.Empty;

			if (pi.Discounts == null || pi.Discounts.Count == 0)
			{
				moreBtn = $"<a href={pi.FriendlyUrlLink} class='button moreBt'>More Info</a>";
			}
			else
			{
				moreBtn = string.Format("<div style='width:200px;display:inline-block'><a data-qty1='{0}' data-price1='{1}' data-qty2='{2}+' data-price2='{3}' data-toggle='popover' data-placement='top' href={4} class='button moreBt'>Buy More & Save</a></div>",
					pi.Discounts[0].Quantity, pi.Discounts[0].Price.ToString("C"),
					pi.Discounts[1].Quantity, pi.Discounts[1].Price.ToString("C"),
					pi.FriendlyUrlLink);
			}

			return moreBtn;
		}

		public string UnitOptionsText(Product product)
		{
			if (product.AllUnitsOOS)
			{
				return "<a class='button buyBt' style='background-color:lightgrey; border: 1px solid lightgrey; '><i class='glyphicon glyphicon-remove - circle'></i> Out Of Stock</a>";
			}

			var html = @"<a href={0} class='button moreUnitBt disabled' ><i class='glyphicon glyphicon-shopping-cart'></i> {1} </a> ";
			var btnText = "Available Options";
			var url = CkMini1.CreateUrl(product.ItemId, product.Description);

			return string.Format(html, url, btnText);

		}

		private void GetDescription()
		{
			// had to add the "undefined" when a bunch of errors were being generated because of it. Try to
			// remove every so often until errors stop.
			// 6-20-2013.
			if (_SubCategoryId != 0)
			{
				try
				{
					using( var conn = DbDriver.OpenConnection())
					using (var command = conn.CreateCommand())
					{
						command.CommandType = CommandType.Text;
						command.CommandText = "Select  `Category 3`, HtmlTitle, HtmlDesription, Descrip3 from `Category 3` where Cat3ID=@c0";
						command.Parameters.AddWithValue("@c0", _SubCategoryId);
						using (var reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								var cat3Text = reader.ReadString(0);
								MiniDataModel.HtmlTitle = reader.ReadString(1);
								MiniDataModel.HtmlDescription = reader.ReadString(2);
								MiniDataModel.GeneralDescription = reader.ReadString(3);

								if (string.IsNullOrEmpty(MiniDataModel.HtmlTitle))
								{
									MiniDataModel.HtmlTitle = cat3Text;
								}
								ViewData["Title"] = MiniDataModel.HtmlTitle;
								ViewData["MetaDescription"] = MiniDataModel.HtmlDescription;
							}
						}
					}
				}
				catch
				{
					//var extra = string.Format("refer: {0} URL:{1}, ShopName: {2}, ShopId: {3}, CatId: {4}. Id: {5}, SubCatId: {6}",
					//        Page.Request.ServerVariables["HTTP_REFERER"],
					//        Page.Request.ServerVariables["URL"],
					//        CkMini1.ShopName, CkMini1.ShopId, CkMini1.CatId, CkMini1.Id, CkMini1.SubCatId);

					//ErrorHandler.Handle(ex, "mini.aspx:GetDescription", sql,
					//    extra);
				}
			}
		}
	}
}
