using ckLib;
using CKSA.Helpers;
using System.Data;
using System.Web;

namespace CKSA.Models
{
	/// <summary>
	/// This is the information that needs to be passed in to _Mini.cshtml.
	/// </summary>
	public class MiniImageModel
	{
		public int RunAs { get; set; }
		public int WholeSaleCustomer { get; set; }
		public int Id { get; set; }
		public List<Product>? Products { get; set; }
		public bool HasResults { get; set; }

		public int ShopId { get; set; } = 0;

		public void LoadProduct()
		{
			string sql = string.Empty;

			try
			{
				switch (RunAs)
				{
					case 2:
						sql = $@"Select A.Units, A.ItemId, A.`Item Number`, A.Description,
							A.NonTaxable, A.NonDiscountable, A.`Extra Shipping`,
							A.Weight, A.Price, A.SalePrice, A.ItemTempOOS, CKPSpecial, DateExpCKPS, SaleEndCKPS, QuantityCKPS, FriendlyUrl, A.MasterItemNumber, A.ImageUrl
							from `item numbers, descrip, page` as A
							Inner Join `supplies` As B
							On A.ItemId=B.ItemId
							where B.IdeaID={Id}";
						break;
				}
				Products = new List<Product>();

				using (var conn = DbDriver.OpenConnection())
				using (var aCommand = conn.CreateCommand())
				{
					aCommand.CommandText = sql;
					aCommand.CommandType = CommandType.Text;
					using (var reader = aCommand.ExecuteReader())
					{
						while (reader.Read())
						{
							var p = new Product();
							p.ItemNumber = reader.ReadString("Item Number").Trim();
							p.MasterItemNumber = reader.ReadString("MasterItemNumber").Trim();
							p.ItemId = reader.ReadInt32("ItemId");
							p.Description = reader.ReadString("Description");
							p.FriendlyUrlLink = reader.ReadString("friendlyUrl");
							p.Units = reader.ReadString("Units");
							p.Price = reader.ReadDecimal("Price");
							p.SalePrice = reader.ReadDecimal("SalePrice");
							p.TempOutOfStock = reader.ReadInt16("ItemTempOOS");
							p.Special = reader.ReadInt16("CKPSpecial");
							p.ProductExpiration = reader.ReadDateTime("DateExpCKPS");

							// I have converted this from ReadDateTime to ReadString - 2/12/2026
							p.SaleExpiration = reader.ReadDateTimeAsString("SaleEndCKPS");
							p.Quantity = reader.ReadInt16("QuantityCKPS");
							p.ImageUrlBase = reader.ReadString("ImageUrl");
							p.ImageUrl = CkDefines.ImageCategoryUrl(p.MasterItemNumber, p.ImageUrl);

							// This is running as latest see about grouping same items together.
							if (RunAs == 3)
							{
								p.IsPartOfUnit = reader.ReadInt16("ItemCount") > 1;
								var minPrice = reader.ReadDecimal(17);
								var maxPrice = reader.ReadDecimal(18);
								var minSalePrice = reader.ReadDecimal(19);
								var maxSalePrice = reader.ReadDecimal(20);
								var wholeSalePrice = reader.ReadDecimal(21);

								if (WholeSaleCustomer == 1 && wholeSalePrice > 0)
								{
									// If I do not check this it will give a range so if they are the same keep them the same price.
									var wholePriceMinPrice = Math.Min(minPrice, wholeSalePrice);
									if (minPrice == maxPrice)
									{
										minPrice = maxPrice = wholePriceMinPrice;
										p.Price = minPrice;
									}
									else
									{
										minPrice = wholePriceMinPrice;
									}
								}

								minSalePrice = minSalePrice == Decimal.MinValue ? 0 : minSalePrice;
								maxSalePrice = maxSalePrice == Decimal.MinValue ? 0 : maxSalePrice;

								if (maxPrice != minPrice)
								{
									// The sale price should be the 2nd value here.
									if (minSalePrice != 0 && maxSalePrice != 0)
									{
										minPrice = Math.Min(minSalePrice, maxSalePrice);
									}
									p.AddMultiUnitPrice(Math.Min(minPrice, maxPrice), Math.Max(minPrice, maxPrice));
								}
							}

							Products.Add(p);
						}
						HasResults = Products.Count > 0;
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "ckMini::LoadGrid", sql));
			}
		}

		public string MiniLoad(string itemNumber, int itemId, string description, string friendlyUrl, bool useLargeImage = false,
	string? masterItemNumber = null, string? imageBaseImage = null)
		{
			string sItemId = itemId.ToString();
			string sItemNumber = itemNumber.ToString();
			string imageLinkStart = string.Empty;
			if (ShopId == 0)
			{
				UrlProductParser parser = new UrlProductParser();
				parser.ProductId = itemId;
				parser.ProductUrl = UrlProductParser.MakeUrlFriendly(description.ToString());
				parser.GetParentIds();

				imageLinkStart = string.Format("<a href={0}>", parser.CreateUrl());
				//imageLinkStart = string.Format("<a href={0}>", friendlyUrl);
			}
			else
			{
				//
				// We have full path info use it.
				//UrlProductParser parser = new UrlProductParser();
				//parser.ShopName = ShopName;
				//parser.ShopUrl = ShopName;
				//parser.ShopId = ShopId;
				//parser.CategoryId = CatId;
				//parser.SubCategoryId = SubCatId;
				//parser.ProductUrl = UrlProductParser.MakeUrlFriendly(description.ToString());
				//parser.ProductId = sItemId;

				//imageLinkStart = string.Format("<a href={0}>", parser.CreateUrl());
			}

			string newDescription = HttpUtility.HtmlEncode(description.ToString());

			string miniPathName = "ProductError.jpg";
			try
			{
				if (useLargeImage)
				{
					miniPathName = FileLocations.GetImagePathName(sItemNumber);
				}
				else
				{
					if (masterItemNumber == null || imageBaseImage == null)
					{
						miniPathName = FileLocations.GetImagePathName252(sItemNumber);
					}
					else
					{
						miniPathName = CkDefines.ImageMiniUrl(masterItemNumber.ToString(), imageBaseImage.ToString());
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "MiniImageModel", $"MiniLoad: {itemNumber}");
			}
			string image =
				string.Format("{0}<img src='{1}' title='{2}' class='' alt='{2}' /></a>",
				imageLinkStart,
				miniPathName,
				newDescription);

			return (image);
		}


		public string FormatPrice(decimal price, decimal sale, DateTime endSaledate)
		{
			var endSale = endSaledate.ToString();
			var saleAsString = sale.ToString();

			if (string.IsNullOrEmpty(endSale) == false)
			{
				DateTime saleEndDate = DateTime.Parse(endSale);
				if (saleEndDate < DateTime.Now.Date && saleEndDate != DateTime.MinValue)
				{
					saleAsString = string.Empty;
				}
			}

			return CkDefines.CreatePriceHtml(price, sale);
		}

		public string AddItemButton(string itemNumber, int tempOutOfStock, int isSpecial,
	DateTime expDate, DateTime saleEnd, int quantity, decimal price, string friendlyUrl = null)
		{
			var friendlyUrlString = string.Empty;
			if (friendlyUrl != null)
			{
				friendlyUrlString = friendlyUrl.ToString();
			}

			// TODO
			// This is assuming not a unit item. Might need to update to be unit or not for real.
			return CkHtmlHelper.AddItemButton(itemNumber, tempOutOfStock, isSpecial, expDate, saleEnd, quantity, price, false, friendlyUrlString);
		}
	}
}
