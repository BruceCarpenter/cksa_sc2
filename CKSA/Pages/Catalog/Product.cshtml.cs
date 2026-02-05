using ckLib;
using CKSA.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using System.Diagnostics;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Web;

namespace CKSA.Pages.Catalog
{
	public class ProductModel : PageModel
	{
		public string _ShopName { get; private set; } = string.Empty;
		public string _ProductName { get; private set; } = string.Empty;
		public int _ShopId { get; private set; }
		public int _CategoryId { get; private set; }
		public int _SubCategoryId { get; private set; }
		public int _ProductId { get; private set; }

		public ProductData? ProductDataModel { get; set; }

		public int WholeSaleId { get; set; }
		public const string Kosher = "<a href=\"javascript:kosherLink('{0}')\">Kosher</a>";

		public IActionResult OnGet(string ShopName, string ProductName, int ShopId, int CategoryId, int SubCategoryId, int ProductId)
		{
			_ShopName = ShopName;
			_ProductName = ProductName;
			_ShopId = ShopId;
			_CategoryId = CategoryId;
			_SubCategoryId = SubCategoryId;
			_ProductId = ProductId;

			try
			{
				// Can't save the product because the cost depends on wholesale or not.
				WholeSaleId = CookieHelper.GetWholesaleValue(Request.Cookies);

				var key = $"{CacheKeys.ProductKey}{ProductId}";
				var cacher = new PageCacher<UrlProductParser>();

				ProductDataModel = new ProductData();
				ProductDataModel.Parser = cacher.Retrieve(key);
				var saveCached = (ProductDataModel.Parser == null);

				if(saveCached)
					ProductDataModel.Parser = new UrlProductParser(UrlProductParser.Step.Product, RouteData);

				if (ProductDataModel.Parser.ProductId != 0)
				{
					var unitId = IsUnitProduct(ProductDataModel.Parser.ProductId);

					if (unitId != int.MinValue)
					{
						ProductDataModel.AllProducts = LoadAllProducts(ProductDataModel.Parser.ProductId);
						if (ProductDataModel.AllProducts.Count > 0)
						{
							OrganizeDiscountsForUI();
							// Make sure the unit item is still valid. An item can be removed from the system
							// but still be in the unit table. If this happens index of 0 is invalid and exception
							// will happen.

							// Dont grab just the first item grab the master item.
							ProductDataModel.TheProduct = ProductDataModel.AllProducts.Find(x => x.ItemId == unitId);
							if (ProductDataModel.TheProduct == null)
							{
								// Just incase not sure this should ever happen.
								ProductDataModel.TheProduct = ProductDataModel.AllProducts[0];
							}

							LoadAllProductsVideos(ProductDataModel.AllProducts);
							FixUpImages(ProductDataModel.TheProduct, ProductDataModel.AllProducts);
							ProductDataModel.TheProduct.GetBrand();
						}

					}

					///
					/// Loading just 1 product.
					/// 
					if (ProductDataModel.TheProduct == null)
					{
						ProductDataModel.TheProduct = new Product(ProductDataModel.Parser.ProductId, WholeSaleId);
						ProductDataModel.NoDiscounts = (ProductDataModel.TheProduct.Discounts.Count == 0);
						ProductDataModel.AllProducts = new List<Product> { ProductDataModel.TheProduct };
					}

					ProductDataModel.TheProduct.LoadSimiliarItems(ProductDataModel.AllProducts);

					// I am skipping no productId here but was doing something before might happen if product no longer exists.

					ProductDataModel.TheProduct.ImageAltText = WebUtility.HtmlEncode(ProductDataModel.TheProduct.Description);
					ProductDataModel.TheProduct.LoadAlternativeImages(ProductDataModel.AllProducts);
					ProductDataModel.CurrentUrl = Request.GetDisplayUrl();

					ProductDataModel.lblDescription = WebUtility.UrlDecode(ProductDataModel.TheProduct.Description);
					if (string.IsNullOrEmpty(ProductDataModel.TheProduct.Ingredients) == false)
					{
						ProductDataModel.TheProduct.Ingredients = "Ingredients: " + ProductDataModel.TheProduct.Ingredients;
					}

					ViewData["Title"] = ProductDataModel.TheProduct.CreateHtmlTitle();

					ProductDataModel.Breadcrumbs = ProductDataModel.Parser.GenerateListBreadcrumb(UrlProductParser.Step.Product);

					SocialMediaContent(ProductDataModel.Parser);

					ProductDataModel.SuperSaver = true;

					// Determine if more info tab should be shown
					ProductDataModel.ShowMoreInfoTab = (string.IsNullOrEmpty(ProductDataModel.TheProduct.Ingredients) == false) ||
						string.IsNullOrEmpty(ProductDataModel.TheProduct.SpecialCare) == false ||
						string.IsNullOrEmpty(ProductDataModel.TheProduct.Kosher) == false ||
						string.IsNullOrEmpty(ProductDataModel.TheProduct.UpcCode) == false;

					CheckCanonicalLink(ProductDataModel.TheProduct.FriendlyUrlLink);

					if (!string.IsNullOrEmpty(ProductDataModel.TheProduct.Brand))
					{
						ProductDataModel.BrandUrl = string.Format("/shop/brand-name/{0}/48/{1}/",
							UrlBaseParser.MakeUrlFriendly(ProductDataModel.TheProduct.Brand),
							ProductDataModel.TheProduct.Cat2Id);
					}

					if (ProductDataModel.TheProduct.DoNotIndex)
					{
						ViewData["Robots"] = true;
					}
				}
				if(saveCached)
					cacher.Store(ProductDataModel.Parser, key);
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "catalog/product-OnGet");
			}

			return Page();
		} // OnGet

		public bool IsMobile()
		{
			var userAgent = Request.Headers["User-Agent"].ToString().ToLower();
			return userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone");
		}

		/// <summary>
		/// Render the radio buttons or none depending on if this is part of a unit.
		/// </summary>
		/// <param name="p">The product to render.</param>
		/// <param name="itemIndex">This is the order number of the product 1.2.3.4....</param>
		public string CreateMultiDiscountRadioOption(int itemIndex)
		{
			Product p = ProductDataModel.AllProducts[itemIndex];
			var radio = string.Empty;
			long productId = Int32.MaxValue;
			var newUi = string.Empty;
			var header = string.Empty;
			var doProductTopCss = false;
			var kosher = string.IsNullOrEmpty(p.Kosher) ? string.Empty : string.Format(Kosher, p.ItemId);

			if (IsMobile())
			{
				if (itemIndex == 0)
				{
					doProductTopCss = true;
				}
			}
			else
			{
				if (p.Discounts.Count > 0)
				{
					var headerTwo = p.DiscountHeader(2);

					header = string.Format(@"<div class='hidden-xs'>
						<div class='col-sm-3 MproductUnitLeftLine productUnit'>Units</div>
						<div class='col-sm-2 MproductUnitLeftLine productUnit'>{0}</div>
						<div class='col-sm-2 MproductUnitLeftLine productUnit'>{1}</div>
						<div class='col-sm-2 {2} productUnit'>{3}</div>
						<div class='col-sm-3 MproductUnitLeftLine productUnit' style='border-right: 1px solid black;'>Add/Favorite</div>
					</div>", p.DiscountHeader(0), p.DiscountHeader(1), string.IsNullOrEmpty(headerTwo) ? string.Empty : "MproductUnitLeftLine", headerTwo);
				}

				if (itemIndex == 0 && p.Discounts.Count() == 0)
				{
					header = string.Format(@"<div class='hidden-xs'>
						<div class='col-sm-3 MproductUnitLeftLine productUnit'>Units</div>
						<div class='col-sm-6 MproductUnitLeftLine productUnit'>1+</div>
						<div class='col-sm-3 MproductUnitLeftLine productUnit' style='border-right: 1px solid black;'>Add/Favorite</div>
					</div>");
				}
				else if (itemIndex > 0)
				{
					if (((p.Discounts.Count() > 0 && ProductDataModel.AllProducts[itemIndex - 1].Discounts.Count() == 0) || (p.Discounts.Count() == 0 && ProductDataModel.AllProducts[itemIndex - 1].Discounts.Count() > 0))
						&& string.IsNullOrEmpty(header))
					{
						doProductTopCss = true;
					}
				}
			}

			try
			{
				var lastUrl = HttpContext.Request.Path.Value.Split('/').Where(x => !string.IsNullOrWhiteSpace(x)).LastOrDefault();
				productId = Convert.ToInt32(lastUrl);
			}
			catch (Exception ex)
			{
				var msg = string.Format("url: {0}", HttpContext.Request.Path.Value);
				ErrorHandler.Handle(new ckExceptionData(ex, "product::CreateRadioOption parsing URL", msg));
			}

			radio = string.Format(@"<input style='margin-right:5px;' type='radio' id='{0}' name='unit' value='{1}' {2}>",
				p.ItemId,
				p.ItemNumber,
				productId == p.ItemId ? "checked" : string.Empty);

			if (itemIndex == 0)
			{
				newUi += header;
			}
			else
			{
				// Only add the 
				if (p.DiscountHashCode != ProductDataModel.AllProducts[itemIndex - 1].DiscountHashCode && !IsMobile())
				{
					var spaceDiv = "<div class='col-sm-12' style='height:10px;'></div>";
					newUi += spaceDiv + header;
				}
			}

			var option = string.Empty;
			if (p.TempOutOfStock != 0)
			{
				option = string.Format(@"<div class='noPrint col-sm-3 col-xs-5 MproductUnitLeftLine MproductUnit {0}' style='padding-top:8px;border-right:1px solid black;'>
								<input disabled='disabled' maxlength='3' title='Enter number of items to purchase' type='text' class='form-control' placeholder='' style='width:45px;display:inline-block;'>
								<div class='smOOSBt'><i class='glyphicon glyphicon-remove-circle'></i> Out</div>
								<a href={1}javascript:addMyList('{2}'){1} class='smaddCartBt hidden-md hidden-xs hidden-sm' style='width:140px;'><i class='glyphicon glyphicon-heart'></i>&nbsp;Fav</a>
								{3}
							</div>", doProductTopCss ? "MproductTop" : string.Empty, "\"", p.ItemId, kosher);
			}
			else
			{
				option = string.Format(@"<div class='noPrint col-sm-3 col-xs-5 MproductUnitLeftLine MproductUnit {0}' style='padding-top:8px;border-right:1px solid black;'>
								<input maxlength='3' title='Enter number of items to purchase' id='quantity{1}' type='text' class='form-control' placeholder='' style='width:45px;display:inline-block;'>
								<a href={2}javascript:multiToAddCart('{3}','{4}',{5}){2} class='smaddCartBt'><i class='glyphicon glyphicon-shopping-cart hidden-md hidden-xs hidden-sm'></i> Add</a>
								<a href={2}javascript:addMyList('{3}'){2} class='smaddCartBt hidden-md hidden-xs hidden-sm' style='width:140px;'><i class='glyphicon glyphicon-heart'></i>&nbsp;Fav</a>
								<a href={2}javascript:addMyList('{3}'){2} class='smaddCartBt hidden-lg' style='width:140px;'><i class='glyphicon glyphicon-heart'></i></a>
								{6}
							</div>", doProductTopCss ? "MproductTop" : string.Empty, itemIndex + 1, "\"", p.ItemId, p.ItemNumber, itemIndex + 1, kosher);
			}

			if (IsMobile())
			{
				newUi += string.Format(@"<div>
							<div class='col-xs-7 MproductUnitLeftLine MproductUnit {3}'><div class='MUnitPrice'>{0}{1} {2}</div></div>
							{4}
					</div>", radio, CkDefines.UnitsConversion(p.Units, p.ProductExpiration), p.DiscountPriceMobile(), doProductTopCss ? "MproductTop" : string.Empty, option);
			}
			else
			{
				// If no price then do not show the css for the left side.
				var priceTwo = p.DiscountPriceHtml(1);
				var priceThree = p.DiscountPriceHtml(2);

				newUi += string.Format(@"<div>
							<div class='col-sm-3 MproductUnitLeftLine MproductUnit {4}'>{5}{0}</div>
							<div class='col-sm-2 MproductUnitLeftLine MproductUnit {4}'>{1}</div>
							<div class='col-sm-2 {7} MproductUnit {4}'>{2}</div>
							<div class='col-sm-2 {8} MproductUnit {4}'>{3}</div>
							{6}
					</div>", CkDefines.UnitsConversion(p.Units, p.ProductExpiration), p.DiscountPriceHtml(0), priceTwo, priceThree,
							doProductTopCss ? "MproductTop" : string.Empty, radio, option,
							string.IsNullOrEmpty(priceTwo) ? string.Empty : "MproductUnitLeftLine",
							string.IsNullOrEmpty(priceThree) ? string.Empty : "MproductUnitLeftLine");
			}
			return newUi;
		}

		/// <summary>
		/// Tell crawlers to index only one product page.
		/// </summary>
		private void CheckCanonicalLink(string url)
		{
			//ViewData["Canonical"] = 
			//link.Attributes.Add("rel", "canonical");
			//var linkToPointTo = url.Trim(new char[] { '"' });
			//link.Attributes.Add("href", string.Format("https://www.countrykitchensa.com{0}", linkToPointTo));
			//Header.Controls.Add(link);
		}

		private void SocialMediaContent(UrlProductParser parser)
		{
			// Facebook
			string productUrl = parser.CreateUrl(string.Empty);
			ProductDataModel.FbTitle = CkDefines.OgDescription(HtmlRemoval.StripTagsRegex(ProductDataModel.TheProduct.Description));
			ProductDataModel.Description = CkDefines.OgDescription(HtmlRemoval.StripTagsRegex(ProductDataModel.TheProduct.Details));
			ViewData["Description"] = ProductDataModel.TheProduct.CreateHtmlMetaDescription();

			//
			string pinUrl = WebUtility.UrlEncode(productUrl);
			string imageUrl = WebUtility.UrlEncode(ProductDataModel.TheProduct.ImageUrl);

			ProductDataModel.Pinit = CkDefines.CreatePinIt(pinUrl, imageUrl);
		}

		/// <summary>
		/// The main unit image might be in a different folder than the other products. Example is 1lb chocolate:
		/// 7500-702100 however the 5/45 lbs are in the 70-xxxx so they are in 70 folder.
		/// </summary>
		private void FixUpImages(Product theProduct, List<Product> allProducts)
		{
			foreach (var p in allProducts)
			{
				p.ImageUrl = theProduct.ImageUrl;
			}
		}

		private void LoadAllProductsVideos(List<Product> products)
		{
			var whereStatement = string.Empty;
			//var sql = "select distinct Description, VName, Url from videoproduct where productid {0}  order by videoId desc limit 6";
			var sql = @"select distinct b.Description, b.VName, b.Url from videoproduct as a inner join videos as b
                on a.VideoId = b.Id where ProductId {0} order by b.Id desc limit 6";

			try
			{
				using (var conn = DbDriver.OpenConnection())
				using (var command = conn.CreateCommand())
				{
					if (products.Count > 1)
					{
						var commaSeparatedIds = string.Join(",", products.Select(product => product.ItemId));
						whereStatement = string.Format("in ({0})", commaSeparatedIds);
					}
					else
					{
						command.Parameters.AddWithValue("@c0", products[0].ItemId);
						whereStatement = " = @c0";
					}

					command.CommandText = string.Format(sql, whereStatement);
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var video = new Video();
							if (!reader.IsDBNull(0))
							{
								video.Description = reader.GetString(0);
							}
							video.Name = reader.GetString(1);
							if (!reader.IsDBNull(2))
							{
								video.Url = reader.GetString(2) + "?rel=0";
								ProductDataModel.TheProduct.Videos.Add(video);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "product::LoadAllProductsVideos", sql));
			}

		}

		/// <summary>
		/// Need to group the UI so the headers for discounts
		/// </summary>
		private void OrganizeDiscountsForUI()
		{
			ProductDataModel.AllProducts = ProductDataModel.AllProducts.OrderBy(d => d.Price).ThenBy(item => item.DiscountHashCode).ToList();
		}

		private List<Product> LoadAllProducts(int unitId)
		{
			var products = new List<Product>();

			try
			{
				using (var conn = DbDriver.OpenConnection())
				using (var command = conn.CreateCommand())
				{
					command.CommandText = @"Select `Item Number`, Price,
                        Details, Weight, NonTaxable, NonDiscountable,
                        `Extra Shipping`, Description, Units, SalePrice, ItemTempOOS, Ingredients, AllergWarn, D.KosherType,
                        E.HeatWarning, CKPSpecial, DateExpCKPS, SaleEndCKPS, QuantityCKPS,  A.ItemId, UPCCode, Dimensions, 
                        SpecialCare, DW, SpecialOrderItem, FriendlyUrl,PdfLink,B.Cat3ID,TypeOfShip,NonEdible,
                        PageNumber,A.SmallParts, F.CountryCode, L, W, H, CountryOfOrigin2, HtmlTitle, HtmlMetaDescription,DoNotIndex,WholesalePriceA,ImageUrl
						from `Item numbers, descrip, page` as A
		                inner join ProductUnit as C on A.ItemID = C.ProductId 
                        inner join `category 3 and items` as B  on A.ItemID = B.ItemID
                        left join kosher as D  on D.KosherCode = A.Kosher
                        left join HeatWarn as E on E.ID=A.HeatWarn
                        left join `Foreign Countries` as F on F.ID=A.CountryOfOrigin
		                where C.UnitId = @c0";
					///
					/// TODO: When order by A.Price is added to the above the boolean values are coming back as 1's when they 
					/// should be 0's. For instance SmallParts is always 0. Removing the order by fixes this?
					/// 
					command.Parameters.AddWithValue("@c0", unitId);

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							// Load all the product data for all products that are part of this
							// group but only really need the Units detail from each and then
							// all the info from one. Eventually there can be one table with product
							// info in it and then this table will relate to it.
							// Will need to have more detail then that because of items can be on sale.....

							// Can be the same product multiple times because this gets all the categories
							// this product is in. This info will be ignored for time being.
							var id = reader.GetInt32(19);
							if (products.Find(x => x.ItemId == id) == null)
							{
								var p = new Product();
								p.WholeSaleCustomer = WholeSaleId;
								p.LoadFromReader(reader);
								p.LoadMultiDiscounts();
								if (p.Discounts.Count() > 0 && ProductDataModel.NoDiscounts)
								{
									ProductDataModel.NoDiscounts = false;
								}

								products.Add(p);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "product::LoadAllProducts"));
			}

			var sortedProduct = from element in products orderby element.Price select element;
			return sortedProduct.ToList<Product>();
		}

		private int IsUnitProduct(Int64 id)
		{
			var unitId = int.MinValue;

			using (var conn = DbDriver.OpenConnection())
			using (var command = conn.CreateCommand())
			{
				command.CommandText = @"select UnitId from ProductUnit where ProductId = @c0";
				command.Parameters.AddWithValue("@c0", id);
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						unitId = reader.GetInt32(0);
					}
				}
			}

			return unitId;
		}

		/// <summary>
		/// https://schema.org/Product
		/// This is how google wants the product pages to be laid out.
		/// Validate the page: https://search.google.com/test/rich-results
		/// https://schema.org/ProductGroup
		/// 
		/// Might be able to do something similiar for ideas: https://schema.org/docs/schemas.html
		/// </summary>
		public string CreateProductSchema()
		{
			///
			/// Before returning this string validate that it is good json.
			/// 
			var finalString = string.Empty;

			// NOTE: In ckIdea I create a dynamic object and use this to serialize into a json object.

			try
			{
				if (ProductDataModel.AllProducts.Count == 1)
				{
					finalString = CreateSingleProductSchema();
				}
				else
				{
					finalString = CreateMultipleProductSchema();
				}

				return finalString.Trim();
			}
			catch (Exception ex)
			{
				var msg = $"url: {Request.GetDisplayUrl()}";
				ErrorHandler.Handle(new ckExceptionData(ex, "product::CreateProductSchema", msg));
			}

			return string.Empty;
		}

		/// <summary>
		/// Using ChatGPT to help create this schema
		/// </summary>
		public string CreateMultipleProductSchema()
		{
			///
			/// Before returning this string validate that it is good json.
			/// 
			var finalString = string.Empty;

			// NOTE: In ckIdea I create a dynamic object and use this to serialize into a json object.

			try
			{
				var counter = 0;
				finalString = @"{""@context"": ""https://schema.org/"",""@graph"": [";

				foreach (var p in ProductDataModel.AllProducts)
				{
					if (counter > 0)
					{
						// There is already a product so add the , to separate them.
						finalString += ",";
					}

					// Check for invalid characters in title and other fields.
					var json = string.Format(@"{{'@type': 'Product',
					'name' : '{0}',
					'image' : {1},
					'description':'{2}',
					{3}{4}{5}{6}{7}}}",
							ProductDataModel.FbTitle,
							ImageJson(p),
							HttpUtility.JavaScriptStringEncode(ProductDataModel.TheProduct.Description),
							SkuJson(p),
							GtinJson(p),
							BrandJson(p),
							Url(),
							OffersJson(p));

					var jsonTest = json.Replace('\'', '"');

					counter++;
					finalString += jsonTest;
				}

				finalString += "]}";

				return finalString.Trim();
			}
			catch (Exception ex)
			{
				var msg = $"url: {Request.GetDisplayUrl()}";
				ErrorHandler.Handle(new ckExceptionData(ex, "product::CreateMultipleProductSchema", msg));
			}

			return string.Empty;
		}

		public string CreateSingleProductSchema()
		{
			///
			/// Before returning this string validate that it is good json.
			/// 
			var finalString = string.Empty;

			// NOTE: In ckIdea I create a dynamic object and use this to serialize into a json object.

			try
			{
				foreach (var p in ProductDataModel.AllProducts)
				{
					// Check for invalid characters in title and other fields.
					var json = string.Format(@"{{'@context': 'https://schema.org/','@type': 'Product',
					'name' : '{0}',
					'image' : {1},
					'description':'{2}',
					{3}{4}{5}{6}{7}}}",
							ProductDataModel.FbTitle,
							ImageJson(p),
							HttpUtility.JavaScriptStringEncode(ProductDataModel.TheProduct.Description),
							SkuJson(p),
							GtinJson(p),
							BrandJson(p),
							Url(),
							OffersJson(p));

					var jsonTest = json.Replace('\'', '"');

					finalString += jsonTest;
				}
				return finalString.Trim();
			}
			catch (Exception ex)
			{
				var msg = $"url: {Request.GetDisplayUrl()}";
				ErrorHandler.Handle(new ckExceptionData(ex, "product::CreateSingleProductSchema", msg));
			}

			return string.Empty;
		}

		private string GtinJson(Product p)
		{
			var str = string.Empty;

			if (!string.IsNullOrEmpty(p.UpcCode))
			{
				str = $"'gtin': '{p.UpcCode}',";
			}

			return str;
		}

		private string SkuJson(Product p)
		{
			// this is our number
			return $"'sku':'{p.ItemNumber}',";
		}

		private string OffersJson(Product p)
		{
			var availability = "'availability': 'https://schema.org/InStock'";
			var str = new StringBuilder();
			// If multiple products on this page create a json array.
			var price = p.Price;
			var saleEnd = string.Empty;

			if (p.SalePrice > 0 && p.SaleExpirationUsedGoogle != DateTime.MaxValue)
			{
				price = p.SalePrice;
				saleEnd = $"'priceValidUntil':'{p.SaleExpirationUsedGoogle}',";
			}

			if (p.TempOutOfStock != 0)
			{
				availability = "'availability': 'https://schema.org/OutOfStock'";
			}

			str.Append("'offers':{");

			str.AppendFormat(Url());
			str.Append("'priceCurrency': 'USD',");
			str.AppendFormat("'price': '{0}',", price);
			str.Append("'@type':'Offer',");
			str.Append(saleEnd);
			str.Append("'itemCondition': 'https://schema.org/NewCondition',");
			str.Append(availability);
			str.Append("}");

			return str.ToString();
		}

		private string Url()
		{
			if (string.IsNullOrEmpty(ProductDataModel.TheProduct.FriendlyUrlLink))
			{
				// This can happen if the web site is being updated and friendlys have not been run.
				return string.Empty;
			}

			var url = ProductDataModel.TheProduct.FriendlyUrlLink.Substring(1, ProductDataModel.TheProduct.FriendlyUrlLink.Length - 2);
			return $"'url':'https://www.countrykitchensa.com{url}'";
		}

		private string BrandJson(Product p)
		{
			var brand = !string.IsNullOrEmpty(p.Brand) ? p.Brand : ProductDataModel.TheProduct.Brand;

			if (string.IsNullOrEmpty(brand))
			{
				return string.Empty;
			}

			return string.Format("'brand' : {{'@type' : 'Brand','name':'{0}'}},", HttpUtility.JavaScriptStringEncode(brand));
		}

		/// <summary>
		/// Get the image and the alt images.
		/// </summary>
		/// <returns></returns>
		private string ImageJson(Product p)
		{
			var prefixUrl = Debugger.IsAttached ? string.Empty : "https://www.countrykitchensa.com/catalog/images/";
			var url = p.ImageUrl != null ? p.ImageUrl : ProductDataModel.TheProduct.ImageUrl;
			var images = string.Format("['{0}{1}'", prefixUrl, url);
			var altImages = p.AlternativeImages != null ? p.AlternativeImages : ProductDataModel.TheProduct.AlternativeImages;

			foreach (var alt in altImages)
			{
				if (alt != url)
				{
					if (alt.Contains("https"))
					{
						images += $",'{alt}'";
					}
					else
					{
						images += $",'{prefixUrl}{alt}'";
					}
				}
			}

			images += "]";

			return images;
		}

		public string EmailSubject()
		{
			return "Product from Country Kitchen SweetArt";
		}

		public string EmailBodyText()
		{
			return string.Format(@"I couldn't resist forwarding it to you because I know how passionate you are about baking and creating sweet treats. I'm sure you'll find plenty of inspiration and new ideas to try out in your own kitchen!
			To read the blog post, simply click on the link below:%0d%0a
			{0}%0d%0a
			If you enjoy it as much as I did, I encourage you to share it with your friends and fellow baking enthusiasts.Together, we can spread the joy and love for baking!", Request.GetDisplayUrl());
		}
	}
}
