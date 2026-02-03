using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using System.Diagnostics;
using System.Net;

/// <summary>
/// Summary description for productCtrl
/// </summary>
/// 
namespace ckLib
{
	public class Product
	{
		public class AlsoLike
		{
			public string ItemNumber { get; set; }
			public string ItemId { get; set; }
			public string ImageUrl { get; set; }
			public string Description { get; set; }
			public string Price { get; set; }
			public string ImageLink { get; set; }
		}

		public class Discount
		{
			public int Quantity { get; set; }
			public decimal Price { get; set; }

		}

		#region Properties

		public const Int64 InvalidItemId = -1;

		public ShipOptions Ship { get; set; }
		public ShipType.Options ShipMinimum { get; set; }
		public List<AlsoLike> MightAlsoLike { get; set; }

		public string ItemNumber { get; set; }
		public string MasterItemNumber { get; set; }
		public Int64 ItemId { get; set; }
		public decimal Price { get; set; }
		public decimal WholeSalePriceA { get; set; }
		               
		public decimal RealPrice { get; set; } // This is price from `item num description' table, price of items can change
		public decimal SalePrice { get; set; }
		public decimal PriceForDiscount // Pick the amount this item can be discounted for
		{
			get
			{
				if (Discountable == false)
					return 0.0m;
				if (SalePrice > 0)
					return SalePrice;
				return Price;
			}
		}
		public int CKPSpecial { get; set; }
		public string Details { get; set; }
		public double Weight { get; set; }
		public double DW { get; set; }
		public bool Taxable { get; set; }
		public bool Discountable { get; set; }
		public bool Discontinued { get; set; }
		public bool DoNotIndex { get; set; }
		public bool NonEdible { get; set; }
		public bool SmallParts { get; set; }
		public string Description { get; set; }
		public string Units { get; set; }
		/// <summary>
		/// 0 means not out of stock, anything else is out of stock.
		/// </summary>
		public int TempOutOfStock { get; set; }
		public int SpecialOrderItem { get; set; }
		public string SpecialOrderItemText { get; set; }
		public string Ingredients { get; set; }
		public string HeatWarning { get; set; }
		public string Kosher { get; set; }
		public string AllergyWarning { get; set; }
		public string ImageName { get; set; }
		public string ImageUrl { get; set; }
		public string ImageUrlBase { get; set; }
		public string ImageAltText { get; set; }
		public string PriceSnippet { get; set; }
		public string OptionPriceSnippet { get; set; } // Used to display in the radio selector on final page.
		public string UpcCode { get; set; }
		public string Dimensions { get; set; }
		public string SpecialCare { get; set; }
		public string FriendlyUrlLink { get; set; }
		public List<string> PdfLink { get; set; }
		public string Brand { get; set; }
		public string Cat2Id { get; set; } // Used to create brand link.
		public string CountryOfOrigin { get; set; }
		public string Harminization { get; set; }
		public int Length { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		/// <summary>
		/// This is defined in the database as PageNumber.
		/// </summary>
		public bool CanShipExpress { get; set; }

		public int Special { get; set; }
		public DateTime ProductExpiration { get; set; }
		public DateTime SaleExpiration { get; set; }
		public DateTime SaleExpirationUsedGoogle { get; set; }
		public DateTime DateEndCKPS { get; set; }
		public DateTime SaleEndCKPS { get; set; }
		public Int32 QuanitityCKPS { get; set; }
		public Int32 Inventory { get; set; }
		public int Quantity { get; set; }

		public List<string> AlternativeImages { get; set; }
		public List<string> MiniAlternativeImages { get; set; }
		public List<int> SubCatIds { get; set; }

		/// <summary>
		/// This is special subcategory and used for items that are
		/// discontinued and on sale...
		/// </summary>
		public bool IsIn1416 { get; set; }

		public bool Valid { get; set; }

		public Dictionary<string, string> FilterProperties { get; private set; }

		public List<Video> Videos { get; set; }
		public List<Discount> Discounts { get; set; }
		public const int NumberOfDiscounts = 2;
		public int DiscountHashCode { get; set; }

		public string HtmlTitle { get; set; }
		public string HtmlMetaDescription { get; set; }

		public const int MaxQuantityColumns = 3;
		public decimal[] PriceRange { get; set; }
		public decimal[] SalePriceRange { get; set; }

		
		public int WholeSaleCustomer { get; set; }

		public bool IsPartOfUnit { get; set; }

		public bool AllUnitsOOS { get; set; }

		public bool IsUnit
		{
			get
			{
				return PriceRange != null && PriceRange.Length == 2;
			}
		}

		/// <summary>
		/// If all items in the unit are oos then mark this as true.
		/// </summary>
		public void SetAllUnitsOOS(bool isOOS)
		{
			if (!isOOS)
			{
				AllUnitsOOS = false;
			}
		}


		public void AddPrice(decimal price, decimal salePrice)
		{
			if (PriceRange == null)
			{
				PriceRange = new decimal[2];
				PriceRange[0] = Math.Min(price, this.Price);
				PriceRange[1] = Math.Max(price, this.Price);
			}
			else
			{
				PriceRange[0] = Math.Min(PriceRange[0], price);
				PriceRange[1] = Math.Max(PriceRange[1], price);
			}
			try
			{
				if (salePrice != decimal.MinValue)
				{
					if (SalePriceRange == null)
					{
						SalePriceRange = new decimal[2];
						SalePriceRange[0] = Math.Min(salePrice, salePrice);
						SalePriceRange[1] = Math.Max(salePrice, salePrice);
					}
					else
					{
						SalePriceRange[0] = Math.Min(SalePriceRange[0], salePrice);
						SalePriceRange[1] = Math.Max(SalePriceRange[1], salePrice);
					}
				}
			}
			catch (Exception ex)
			{
				// Could be a sale price string to double conversion
			}
		}

		/// <summary>
		/// Latest mini page is getting the data a bit differently then other pages. This might be a faster way to get data but
		/// need to set the price range a bit differently.
		/// </summary>
		public void AddMultiUnitPrice(decimal minPrice, decimal maxPrice)
		{
			if (PriceRange == null)
			{
				PriceRange = new decimal[2];
				PriceRange[0] = minPrice;
				PriceRange[1] = maxPrice;
			}
			else
			{
				// This should not be called in this case but leaving here for time being
				PriceRange[0] = Math.Min(PriceRange[0], minPrice);
				PriceRange[1] = Math.Max(PriceRange[1], minPrice);
			}
		}

		/// <summary>
		/// There are assumptions here.
		/// 1) Always be 3 columns
		/// 2) If mulitiple items in this product they all have same breakdown
		/// </summary>
		/// <param name="columnNum"></param>
		public string DiscountHeader(int columnNum)
		{
			var header = string.Empty;

			if (columnNum == 0)
			{
				header = string.Format("1-{0}", Discounts[1].Quantity - 1);
			}
			else if (columnNum == 1)
			{
				if (Discounts.Count == 3)
				{
					header = string.Format("{0}-{1}", Discounts[1].Quantity, Discounts[2].Quantity - 1);
				}
				else
				{
					header = string.Format("{0}+", Discounts[1].Quantity);
				}
			}
			else if (columnNum == 2)
			{
				if (Discounts.Count == 3)
				{
					header = string.Format("{0}+", Discounts[2].Quantity);
				}
			}

			return header;
		}

		public void DiscountPrice(int columnNum, out decimal discountPrice, out decimal cheapeastRegularPrice)
		{
			if (columnNum == 2 && Discounts.Count == 2)
			{
				discountPrice = Discounts[columnNum - 1].Price;
			}
			else
			{
				discountPrice = Discounts[columnNum].Price;
			}
			cheapeastRegularPrice = Math.Min(discountPrice, SalePrice == 0 ? discountPrice : SalePrice);


			// Check if any other costs are less
			for (var i = columnNum; i >= 0; i--)
			{
				if (columnNum == 2 && Discounts.Count == 2)
				{
					cheapeastRegularPrice = Math.Min(cheapeastRegularPrice, Discounts[1].Price);
				}
				else
				{
					cheapeastRegularPrice = Math.Min(cheapeastRegularPrice, Discounts[i].Price);
				}
			}
		}

		/// <summary>
		/// For mobile devices get the starting price of the item.
		/// </summary>
		public string DiscountPriceMobile()
		{
			var mobilePrice = string.Empty;

			mobilePrice = string.Format("{0} ${1}",
				Discounts.Count == 0 ? string.Empty : "Starting At", UserPrice.ToString("N2"));

			return mobilePrice;
		}

		/// <summary>
		/// This should only be called if the product is being displayed in a multi row format.
		/// </summary>
		/// <param name="columnNum"></param>
		/// <returns></returns>
		public string DiscountPriceHtml(int columnNum)
		{
			var price = string.Empty;
			decimal discountPrice;
			decimal cheapeastRegularPrice;

			///
			/// If an item has a sale price and there are 2 discounts for this item then for some reason
			/// a 3rd column would be made that spans the second column with exact same data as the 2nd 
			/// column. This is my attempt to prevent this extra column being shown.
			/// 
			if ((columnNum + 1) > Discounts.Count && Discounts.Count != 0)
			{
				return string.Empty;
			}

			///
			/// If any of these prices are the same then need to mark them in the UI a certain way
			/// Wrap in a span and then create css to go with.
			/// 
			if (Discounts.Count > 0)
			{

				DiscountPrice(columnNum, out discountPrice, out cheapeastRegularPrice);

				if (SalePrice > 0 && SalePrice <= cheapeastRegularPrice)
				{
					price = string.Format("<span style='text-decoration: line-through;'>{0}</span> <span class='pSalePrice'>${1} ({2}% Off)</span>",
							CkDefines.FormatPrice(discountPrice.ToString()),
							cheapeastRegularPrice.ToString("N2"),
							CkDefines.CalculatePercent(discountPrice, cheapeastRegularPrice));
				}
				else
				{
					// Sale price should be ignored here.
					price = string.Format("<div>$<span>{0}</span></div>", cheapeastRegularPrice.ToString("N2"));
					// Check if this price is the same as any other price in the row.
					for (var i = 0; i <= NumberOfDiscounts; i++)
					{
						decimal tmpDiscountPrice;
						decimal tmpCheapeastRegularPrice;
						DiscountPrice(i, out tmpDiscountPrice, out tmpCheapeastRegularPrice);

						// If this is the same price as another column set its css a bit different.
						if (cheapeastRegularPrice == tmpDiscountPrice && i != columnNum)
						{
							// If there is no discount price for this column return nothing. 
							if (columnNum < Discounts.Count)
							{
								price = string.Format("<div class='productUnitSamePrice'>$<span>{0}</span></div>", cheapeastRegularPrice.ToString("N2"));
							}
							else
							{
								price = string.Empty;
							}
						}
					}
				}
			}
			else if (columnNum == 0)
			{
				// This should be the same price all across the product page so just get the price for the first column.
				if (SalePrice > 0 && SalePrice < Price)
				{
					price = string.Format("<span style='text-decoration: line-through;'>{0}</span> <span class='pSalePrice'>${1} ({2}% Off)</span>",
							CkDefines.FormatPrice(Price.ToString()),
							SalePrice.ToString("N2"),
							CkDefines.CalculatePercent(Price, SalePrice));
				}
				else
				{
					price = PriceSnippet; // string.Format("<div class='productUnitSamePrice'>$<span>{0}</span></div>", Price.ToString("N2"));
				}
			}

			return price;
		}

		/// <summary>
		/// Need to keep track of more of this products information so it can be
		/// used for coupons: Brand/Category/Filter
		/// Also used for remarketing, only show if certain brand?
		/// </summary>

		public decimal ExtendedPrice
		{
			get
			{
				Price = UserPrice;

				return Price * Quantity;
			}
		}

		public decimal DiscountPrice(int quantity)
		{
			var finalPrice = Price;

			if (quantity > 0)
			{
				if (Discounts.Count != 0)
				{
					for (var i = Discounts.Count - 1; i >= 0; i--)
					{
						if (quantity >= Discounts[i].Quantity)
						{
							finalPrice = Discounts[i].Price;
							break;
						}
					}
				}
			}

			return finalPrice;
		}

		public decimal UserPrice
		{
			get
			{
				decimal finalPrice = DiscountPrice(Quantity);

				if (SalePrice != 0)
				{
					finalPrice = Math.Min(finalPrice, SalePrice);
					finalPrice = Math.Round(finalPrice, 2);
				}

				return Math.Round(finalPrice, 2);
			}
		}

		#endregion Fields

		public Product(Int64 itemId, int wholeSaleCustomer)
		{
			Valid = false;

			Init();

			ItemId = itemId;

			WholeSaleCustomer = wholeSaleCustomer;

			GetProductData();
			LoadVideos();
		}

		public Product()
		{
			Init();
		}

		protected void Init()
		{
			ShipMinimum = ShipType.Options.SmallFlat;
			Ship = new ShipOptions();
			SaleExpiration = DateTime.MaxValue;
			SaleExpirationUsedGoogle = DateTime.MaxValue;
			ProductExpiration = DateTime.MaxValue;
			Inventory = Int32.MaxValue;
			ItemId = Product.InvalidItemId;
			IsIn1416 = false;
			CanShipExpress = true;
			Videos = new List<Video>();
			Discounts = new List<Discount>();
		}

		public string UnitsBBDate()
		{
			var bbDate = DateEndCKPS == DateTime.MinValue ? string.Empty : string.Format("(BB: {0})", DateEndCKPS.ToString("M/d/yyyy"));

			return $"{Units} {bbDate}";
		}

		/// <summary>
		/// Create the html meta description.
		/// </summary>
		/// <returns></returns>
		public string CreateHtmlMetaDescription()
		{
			// If we specified the meta description use that
			if (!string.IsNullOrEmpty(HtmlMetaDescription))
			{
				return HtmlMetaDescription;
			}

			if (DoNotIndex)
			{
				return string.Empty;
			}

			// No specific description set so create our own.
			var firstPart = Details;

			if (string.IsNullOrEmpty(firstPart))
				return firstPart = Description;

			// Some pages have huge descriptions /shop/cutters-molds/101-piece-cookie-cutter-set/44/603/308/627379/
			if (Dimensions.Length > 50)
				return WebUtility.HtmlEncode(firstPart);

			return WebUtility.HtmlEncode(firstPart + " " + Dimensions);
		}

		public string CreateHtmlTitle()
		{
			var title = HtmlTitle;

			if (string.IsNullOrEmpty(HtmlTitle))
			{
				title = Description;
			}

			// If the title is short then add the company name.
			// Assuming the ideal length is less than 65
			//if(title.Length < 38) // 65 - 27
			//{
			//	var ckName = " | Country Kitchen SweetArt"; // 27 length
			//	title += ckName;
			//}

			return title;
		}

		public void LoadFromReader(MySqlDataReader reader)
		{
			Valid = true;
			if (ItemId == Product.InvalidItemId)
			{
				ItemId = reader.ReadInt32(19);
			}

			SpecialCare = reader.ReadString("SpecialCare");
			Dimensions = reader.ReadString("Dimensions");
			UpcCode = reader.ReadString("UPCCode");
			ItemNumber = reader.ReadString("Item Number").Trim();
			Price = reader.ReadDecimal("Price");

			if (CkDefines.ProductValidWholesale(ItemNumber))
			{
				WholeSalePriceA = reader.ReadDecimal("WholeSalePriceA");
				// If user is wholesale customer then give them the wholesale cost
				// or give them 10% off of the current price.
				Price = GetWholesalePriceA(Price);
			}

			Price = Math.Round(Price, 2);

			Details = reader.ReadString("Details");
			HtmlTitle = reader.ReadString("HtmlTitle");
			HtmlMetaDescription = reader.ReadString("HtmlMetaDescription");

			// This is temp as I upload all pieces of the web site and some might not have ImageUrl passed in yet....
			var imageUrl = string.Empty;
			try
			{
				imageUrl = reader.ReadString("ImageUrl").ToLower();
				ImageUrl = CkDefines.ImageUrl(ItemNumber, imageUrl);
			}
			catch
			{
				ImageUrl = FileLocations.GetImagePathName(ItemNumber.Trim());
			}

			Weight = reader.ReadDouble("Weight");
			DW = reader.ReadDouble("DW");
			CountryOfOrigin = reader.ReadString("CountryCode");
			Harminization = reader.ReadString("CountryOfOrigin2");
			Length = reader.ReadInt16("L");
			Width = reader.ReadInt16("W");
			Height = reader.ReadInt16("H");

			Description = reader.ReadString("Description");
			Units = reader.ReadString("Units");
			SalePrice = reader.ReadDecimal("SalePrice");
			TempOutOfStock = reader.ReadInt16("ItemTempOOS");
			SpecialOrderItem = reader.ReadInt16("SpecialOrderItem");
			SpecialOrderItemText = LoadSpecialOrderItemText(SpecialOrderItem);
			Ingredients = reader.ReadString("Ingredients");
			Taxable = !reader.ReadBool("NonTaxable");
			Discountable = !reader.ReadBool("NonDiscountable");
			NonEdible = reader.ReadBool("NonEdible");
			CanShipExpress = reader.ReadInt16("PageNumber") == 1 ? false : true;

			DoNotIndex = reader.ReadBool("DoNotIndex");

			if (reader.ReadBool("AllergWarn"))
			{
				AllergyWarning = CkDefines.GetAllergyWarning();
			}

			Kosher = reader.ReadString("KosherType");
			HeatWarning = reader.ReadString("HeatWarning");
			SmallParts = reader.ReadBool("SmallParts");

			if (reader.IsDBNull(17) == false)
			{
				var dt = new DateTime();
				DateTime.TryParse(reader.GetString(17), out dt);
				SaleExpiration = dt;
				if (SaleExpiration < DateTime.Now.Date)
				{
					SalePrice = 0;
				}
				else
				{
					SaleExpirationUsedGoogle = SaleExpiration;
				}

				// No idea why this is being cleared out here. Sales need to be updated and redone.
				SaleExpiration = DateTime.MaxValue;
			}

			if (reader.IsDBNull(29) == false)
			{
				Ship.ValidShipType = new List<ShipType>();
				AddShipType(reader.ReadInt16(29), Int16.MaxValue);
			}

			Special = reader.ReadInt16("CKPSpecial");
			if (Special > 0)
			{
				var dt = new DateTime();
				if (reader.IsDBNull(16) == false)
				{
					DateTime.TryParse(reader.GetString(16), out dt);
					ProductExpiration = dt;
				}
				if (reader.IsDBNull(18) == false)
				{
					Inventory = reader.GetInt32(18);
				}
				else
				{
					// If null then there is no limit to the number we have available
					Inventory = Int32.MaxValue - 1;
				}
			}

			FriendlyUrlLink = reader.ReadString("FriendlyUrl");
			PdfLink = PdfStringParser(reader.ReadString("PdfLink"));
			PriceSnippet = CkDefines.CreatePriceHtmlSnippet(Price, SalePrice);
			OptionPriceSnippet = CkDefines.CreateOptionPriceHtmlSnippet(Price, SalePrice);
		}

		public void LoadVideos()
		{
			Videos = new List<Video>();

			using( var conn = DbDriver.OpenConnection())
			using (var command = conn.CreateCommand())
			{
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = @"select distinct b.Description, b.VName, b.Url from videoproduct as a inner join videos as b
                on a.VideoId = b.Id where ProductId = @c0 order by b.Id desc limit 6";
				command.Parameters.AddWithValue("@c0", ItemId);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var v = new Video();
						if (!reader.IsDBNull(0))
						{
							v.Description = reader.GetString(0);
						}
						v.Name = reader.GetString(1);
						if (!reader.IsDBNull(2))
						{
							v.Url = reader.GetString(2) + "?rel=0";
							Videos.Add(v);
						}
					}
				}
			}
		}

		public void LoadItemId()
		{
			if (string.IsNullOrEmpty(ItemNumber))
			{
				throw new Exception("ItemNumber must be set");
			}

			try
			{
				using (var conn = DbDriver.OpenConnection())
				{
					using (var command = conn.CreateCommand())
					{
						command.CommandText = "Select ItemId from `Item numbers, descrip, page` where `Item Number` = @c0";
						command.CommandType = System.Data.CommandType.Text;
						command.Parameters.AddWithValue("c0", ItemNumber);

						using (var reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								ItemId = reader.GetInt32(0);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "productCtrl::LoadItemId", ItemId.ToString(), ""));
			}
		}

		public void LoadPrices()
		{
			try
			{
				using (var conn = DbDriver.OpenConnection())
				{
					using (var command = conn.CreateCommand())
					{
						command.CommandText = "Select Price,SalePrice,SaleEndCKPs,WholesalePriceA from `Item numbers, descrip, page` where ItemId = @c0";
						command.CommandType = System.Data.CommandType.Text;
						command.Parameters.AddWithValue("c0", ItemId);

						using (var reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								Price = reader.GetDecimal(0);
								if (CkDefines.ProductValidWholesale(ItemNumber))
								{
									WholeSalePriceA = reader.ReadDecimal(3);
									Price = GetWholesalePriceA(Price);
								}

								if (!reader.IsDBNull(2) && !reader.IsDBNull(1))
								{
									// TODO: Do I need to check if the sale has expired?
									// Or not read this value?
									//var saleEnd = CKDefines.ReadDbDateTime(reader, "SaleEndCKPS");
									SalePrice = reader.GetInt32(1);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "productCtrl::LoadPrices", ItemId.ToString(), ""));
			}
		}

		/// <summary>
		/// These are discounts for purchasing many products.
		/// </summary>
		public void LoadMultiDiscounts()
		{
			var concateDiscounts = string.Empty;

			Discounts = new List<Discount>();
			try
			{
				if (WholeSalePriceA == Price)
				{
					return;
				}

				using (var conn = DbDriver.OpenConnection())
				{
					using (var command = conn.CreateCommand())
					{
						command.CommandText = "Select Quantity, Price from QuantityDiscount where ItemId = @c0 order by Price desc";
						command.CommandType = System.Data.CommandType.Text;
						command.Parameters.AddWithValue("c0", ItemId);

						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								// Add the first price for now...
								if (Discounts.Count == 0)
								{
									// The Price is already discounted by 10% so do not discount again.
									Discounts.Add(new Discount
									{
										Price = this.Price,
										Quantity = 1
									});
								}

								var discountUpper = reader.GetInt32(0);
								var dbPrice = reader.GetDecimal(1);
								var price = dbPrice;

								if (CkDefines.ProductValidWholesale(ItemNumber))
								{
									price = GetWholesalePriceA(dbPrice);
								}

								Discounts.Add(new Discount
								{
									Price = price,
									Quantity = discountUpper
								});

								concateDiscounts += discountUpper.ToString();
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "productCtrl::LoadMultiDiscounts", ItemId.ToString(), ""));
			}

			DiscountHashCode = String.IsNullOrEmpty(concateDiscounts) ? 0 : Convert.ToInt32(concateDiscounts);
		}

		public void GetProductData()
		{
			string SQL = string.Empty;
			string where = string.Empty;

			if (ItemId != Product.InvalidItemId)
			{
				where = string.Format("where A.ItemId={0}", ItemId);
			}
			else if (string.IsNullOrEmpty(ItemNumber) == false)
			{
				where = string.Format("where `Item Number`='{0}'", ItemNumber);
			}
			else
			{
				return;
			}

			try
			{
				SQL = string.Format(@"Select `Item Number`, Price,
                Details, Weight, NonTaxable, NonDiscountable,
                `Extra Shipping`, Description, Units, SalePrice, ItemTempOOS, Ingredients, AllergWarn, D.KosherType,
                E.HeatWarning, CKPSpecial, DateExpCKPS, SaleEndCKPS, QuantityCKPS, A.ItemId, UPCCode, Dimensions, 
                SpecialCare, DW, SpecialOrderItem, FriendlyUrl,PdfLink,B.Cat3ID,TypeOfShip,NonEdible,
                PageNumber,SmallParts, F.CountryCode, L, W, H, CountryOfOrigin2, HtmlTitle, HtmlMetaDescription, DoNotIndex,WholeSalePriceA,ImageUrl from `Item numbers, descrip, page` as A
                inner join `category 3 and items` as B on A.ItemID = B.ItemID
                left join kosher as D  on D.KosherCode = A.Kosher
                left join HeatWarn as E on E.ID=A.HeatWarn
                left join `Foreign Countries` as F on F.ID=A.CountryOfOrigin
                {0}", where);

				using (var reader = DbDriver.ExecuteReader(SQL))
				{
					if (reader != null && reader.HasRows)
					{
						SubCatIds = new List<int>();
						var firstTimeRead = true;
						while (reader.Read())
						{
							if (firstTimeRead)
							{
								LoadFromReader(reader);
								firstTimeRead = false;
							}
							// Every read get the subcategory id might be in multiple subcategories.
							var subCatId = reader.ReadInt16("Cat3ID");
							if (subCatId == 1416)
							{
								IsIn1416 = true;
							}
							SubCatIds.Add(subCatId);
						}
					}
				}

				GetBrand();
				LoadMultiDiscounts();
			}
			catch
			{
				//
				// 7/2012 - getting a lot of exceptions where the last parameter is some random word.
				// examples: /shop/cake-decorating-supplies/cupcake-boxes-and-carriers/38/538/1613/undefined/
				// 
				//ErrorHandler.Handle(
				//new ckExceptionData(ex, "productCtrl::GetProductData", SQL, page.Request.ServerVariables["URL"]));
			}

		}

		public void GetBrand()
		{
			var sql = @"select D.`Category 2`, P.`Category 3`, P.ItemID, P.Cat3ID, P.Cat2ID  from
            (
            select C.`Category 3`, Q.ItemID, Q.Cat3ID, C.Cat2ID  from
            (
	            select B.Cat3ID, A.ItemID from `item numbers, descrip, page` as A
	            inner join
	            `category 3 and items` as B on
	            A.ItemID = B.ItemID          
                   where A.ItemID = @c0
            ) as Q
            inner join `Category 3` as C on 
            Q.Cat3ID = C.Cat3ID
            ) as P
            inner join `Category 2` as D on
            D.Cat2ID = P.Cat2ID
            and D.Category = 48";
			var brand = string.Empty;
			using( var conn = DbDriver.OpenConnection())
			using (var command = conn.CreateCommand())
			{
				command.CommandText = sql;
				command.Parameters.AddWithValue("@c0", ItemId);
				using (var reader = command.ExecuteReader())
				{
					if (reader != null && reader.HasRows && reader.Read())
					{
						Brand = reader.ReadString(0);
						Cat2Id = reader.ReadString(4);
					}
				}
			}
		}

		protected List<string> PdfStringParser(string pdfString)
		{
			var pdfs = new List<string>();
			if (string.IsNullOrEmpty(pdfString) == false)
			{
				var split = pdfString.Split(',');
				foreach (var s in split)
				{
					pdfs.Add(s.Trim().ToLower());
				}
			}
			return pdfs;
		}

		/// <summary>
		/// Load filter information about this product. This will give the final product
		/// page additional information about this product to display.
		/// </summary>
		public void GetFilterData()
		{
			FilterProperties = new Dictionary<string, string>();
			using(var conn = DbDriver.OpenConnection())
			using (var command = conn.CreateCommand())
			{
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = @"select R.fTypeName as GroupName, V.fValue from
            (select F.fId from FilterProduct as F where F.ProductId = @c0) as Q
            inner join FilterValue as V on Q.fId = V.fId
            inner join FilterType as R on V.fTypeId = R.fTypeId";
				command.Parameters.AddWithValue("@c0", ItemId);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						FilterProperties.Add(reader.GetString(0), reader.GetString(1));
					}
				}

			}
		}

		public void AddShipType(int t, int limit)
		{
			ShipType type = new ShipType();
			type.SetType(t);
			type.Limit = limit;
			if (type.Limit <= 0)
			{
				type.Limit = int.MaxValue;
			}
			Ship.ValidShipType.Add(type);
			if (type.Type < ShipMinimum)
			{
				ShipMinimum = type.Type;
			}
		}

		public void SetInitialShip()
		{
			if (Ship.ValidShipType == null)
			{
				Ship.ValidShipType = new List<ShipType>();
			}
			// All items get the standard ship type.
			Ship.ValidShipType.Add(new ShipType(ShipType.Options.SmallFlat, int.MaxValue));
		}

		/// <summary>
		/// Return the number of items that can fit in the selected type, -1 if invalid.
		/// </summary>
		public int CanShip(ShipType.Options option)
		{
			try
			{
				var found = Ship.ValidShipType.Find(s => s.Type == option);
				if (found != null)
				{
					return found.Limit;
				}
			}
			catch
			{
				// If not found an exception is thrown.
			}
			return -1;
		}

		/// <summary>
		/// Return true if the item is valid for sale.
		/// Might not be eligable because out of stock or seasonal....
		/// </summary>
		public bool IsAvailableForSale()
		{
			return (TempOutOfStock == 0);
		}

		public string SpecialTitle
		{
			get
			{
				return Product.SpecialText(Special);
			}
		}

		public string ExpirationLabel
		{
			get
			{
				if (!(ProductExpiration == DateTime.MaxValue || ProductExpiration == DateTime.MinValue))
				{
					if (ProductExpiration < DateTime.Now)
					{
						return string.Format("Please Note: This food item's “best by” date has passed.<div>The “best by” date is {0}.</div><div style='font-size:.75em;font-style:italic;'>A “best by date” refers strictly to the quality, not safety of the product. This date is recommended for best flavor or quality. It is not a purchase or safety date.  With many items, there may not be a noticeable difference. The date is determined by the manufacturer of the product.</div>", ProductExpiration.ToShortDateString());
					}
					else
					{
						return string.Format("<div>This food item's “best by” date is {0}.</div><div style='font-size:.75em;font-style:italic;'>A “best by date” refers strictly to the quality, not safety of the product. This date is recommended for best flavor or quality. It is not a purchase or safety date.  With many items, there may not be a noticeable difference. The date is determined by the manufacturer of the product.</div>", ProductExpiration.ToShortDateString());
					}
				}

				return string.Empty;
			}
		}

		public string SaleExpirationLabel
		{
			get
			{
				//if (SaleExpiration != DateTime.MaxValue)
				//{
				//    if (SaleExpiration < DateTime.Now)
				//    {
				//        return string.Format("This sale has ended.");
				//    }
				//    else
				//    {
				//        return string.Format("This sale will end at {0} {1}.", SaleExpiration, TimeZone.CurrentTimeZone.DaylightName);
				//    }
				//}
				return string.Empty;
			}
		}

		/// <summary>
		/// This text is displayed above the product on the final product page.
		/// </summary>
		/// <param name="specialNumber"></param>
		/// <returns></returns>
		public static string SpecialText(int specialNumber)
		{
			switch (specialNumber)
			{

				case 1:
					return @"Order now, limited quantities available.";
				case 2:
					return "Purchasing over 10 cases of bulk sale items? We can offer even greater savings on select items! Call Mindy at 260-482-4835 or email shopmanager@countrykitchensa.com with the bulk items.";
				case 3:
					return "Order now, limited quantities available.";
				case 4:
					return "Order now, limited quantities available.";
				case 5:
					return string.Format(@"{0}
                        <div style='color:black;font-size:.6em;'>Order now, limited quantities available.</div>",
							Product.SpecialHeader(specialNumber));
				case 6:
					return @"Select Bulk Baking Cups 40% off.";
				case 7:
					return @"Order now, limited quantities available.";
				case 8:
					return @"This item was featured at our Open House. On sale, now through November 24. Now's a great time to try this fantastic product that we love.";
				case 9:
					return @"Order now, limited quantities available.";
				case 10:
					return @"Select Halloween Cutters 30% off!";
				case 11:
					return @"50% off select shaped pans from Fat Daddio’s";
				case 12:
					return @"Kitchen Tools and Gadgets- 50% off! Limited quantities available!";
				case 15:
					return @"Seasonal Savings on 1 lb. bags of Merckens Candy Coating";
				case 16:
					return @"New CK Food Color Gels. Special introductory price!";
				case 19:
					return @"Overstock or Last Chance Seasonal Item! Limited Quantities available.";
				case 21:
					return @"25% off Our Top Selling Items of 2015!";
				default:
					return "Sale price!";
			}
		}

		/// <summary>
		/// Display the textual reason as to why item is out of stock.
		/// </summary>
		public static string OutOfStockReason(int idReason)
		{
			switch (idReason)
			{
				case 1:
					return "Temporarily out of stock";
				case 2:
					return "Currently unavailable, out of season";
				case 3:
					return "Currently unavailable, expected in the fall";
				case 8:
					return "Now's a great time to try these unique flavors! Shop now, quantities limited.";
				case -1:
					return "Item is no longer available";
			}

			return string.Empty;
		}

		/// <summary>
		/// Display the textual reason as to why item is a special order item.
		/// </summary>
		public static string LoadSpecialOrderItemText(int idReason)
		{
			switch (idReason)
			{
				case 3:
					return "Non-stock item, usually ships in 2-3 business days.";
				case 60:
					return "Non-stock item, usually ships in 2-3 weeks.";
				case 61:
					return "Personalized items typically take 3-4 weeks.";
			}

			return string.Empty;
		}

		/// <summary>
		/// This text is displayed on the bargains mini page.
		/// </summary>
		/// <param name="specialNumber"></param>
		/// <returns></returns>
		public static string SpecialHeader(int specialNumber)
		{
			switch (specialNumber)
			{
				case 1:
					return "Caramel Apple Sale";
				case 2:
					return @"Up to 76% off bulk items";
				case 3:
					return @"Last Minute Christmas Sale up to 75% off";
				case 4:
					return @"We've ordered too much! Great deals on overstock items.";
				case 5:
					return @"20% off goodies for Easter baskets.";
				case 6:
					return @"25% off Wafer paper punches and wafer paper.";
				case 7:
					return @"25% off Pattern Sheets, Sultan Tip and Select Tools";
				case 8:
					return @"25-50% off Flavors of the Open House!";
				case 9:
					return @"Wedding Cake Topper Sale!";
				case 10:
					return @"Select Halloween Cutters 30% off!";
				case 11:
					return @"50% off select shaped pans from Fat Daddio’s";
				case 12:
					return @"Kitchen Tools and Gadgets 50% off!";
				case 15:
					return @"Seasonal Savings on 1 lb. bags of Merckens Candy Coating";
				case 16:
					return @"New CK Color Gels! Special introductory price!";
				case 19:
					return @"Celebrate the Holidays! Sale- Up to 50% off!";
				case 21:
					return @"25% off Our Top Selling Items of 2015!";
				default:
					return "New lower price!";
			}
		}

		public static string SpecialTagImage(int specialNumber)
		{
			string path = "/graphics/buttons/";

			switch (specialNumber)
			{
				case 1:
					return path + "bargain-20.png";
				case 2:
					return path + "bargain-75.png";
				case 3:
					return path + "bargain-75.png";
				case 4:
					return path + "bargain-50.png";
				case 5:
					return path + "bargain-20.png";
				case 6:
					return path + "bargain-40.png";
				case 7:
					return path + "bargain-25.png";
				case 8:
					return path + "bargain-50.png";
				case 9:
					return path + "bargain-75.png";
				case 10:
					return path + "bargain-30.png";
				case 11:
					return path + "bargain-50.png";
				case 12:
					return path + "bargain-50.png";
				case 15:
					return path + "seasonal-savings.png";
				case 16:
					return path + "introductory.png";
				case 19:
					return path + "bargain-50.png";
				case 21:
					return path + "bargain-25.png";

				default:
					return path + "bargain-deep.png";
			}
		}

		/// <summary>
		/// Load similiar items.
		/// </summary>
		public void LoadSimiliarItems(List<Product> otherProductsInUnit)
		{
			var sql = "Select Distinct A.ItemId, A.Units, A.`Item Number`, A.Description, " +
						   " A.NonTaxable, A.NonDiscountable, A.`Extra Shipping`," +
						   " A.Weight, A.Price, A.SalePrice, CKPSpecial, DateExpCKPS, SaleEndCKPS, QuantityCKPS," +
						   " B.MatchingSupId, FriendlyUrl, A.MasterItemNumber, A.ImageUrl" +
						   " from `item numbers, descrip, page` as A" +
						   " Inner Join `matching supplies` As B" +
						   " On A.ItemId=B.MatchingSupId" +
						   " where B.ItemID";
			var otherIds = string.Empty;

			using(var conn = DbDriver.OpenConnection())
			using (var command = conn.CreateCommand())
			{
				if (otherProductsInUnit != null && otherProductsInUnit.Count > 0)
				{
					var ids = string.Join(",", otherProductsInUnit.Select(x => x.ItemId).ToList());
					sql += string.Format(" in ({0})", ids);
				}
				else
				{
					sql += "=@c1";
					command.Parameters.AddWithValue("@c1", ItemId);
				}
				command.CommandText = sql;
				using (var reader = command.ExecuteReader())
				{
					MightAlsoLike = new List<AlsoLike>();
					if (reader != null)
					{
						while (reader.Read())
						{
							var l = new AlsoLike();
							l.ItemId = reader.ReadString("ItemId");
							l.ItemNumber = reader.ReadString("Item Number").Trim();
							l.Description = reader.ReadString("Description");
							var price = reader.ReadDecimal("Price");
							var salePrice = reader.ReadDecimal("SalePrice");
							l.Price = CkDefines.CreatePriceHtml(price, salePrice);
							var masterItemId = reader.ReadString("MasterItemNumber").Trim();
							l.ImageUrl = reader.ReadString("ImageUrl");
							l.ImageUrl = CkDefines.ImageMiniUrl(masterItemId, l.ImageUrl);
							l.ImageLink = reader.ReadString("FriendlyUrl");
							//
							// TODO: Might want to make sure the same id is not added twice.
							MightAlsoLike.Add(l);
						}
					}
				}
			}
		}

		/// <summary>
		/// </summary>
		public void LoadAlternativeImages(List<Product> otherProductsInUnit)
		{
			AlternativeImages = new List<string>();
			MiniAlternativeImages = new List<string>();
			var sql = "select AltImageName, AltImageNameSmall from productaltimage where ProductId ";

			if (otherProductsInUnit != null && otherProductsInUnit.Count > 0)
			{
				var ids = string.Join(",", otherProductsInUnit.Select(x => x.ItemId).ToList());
				sql += string.Format(" in ({0})", ids);
			}
			else
			{
				sql += string.Format("= {0}", ItemId);
			}

			using (var reader = DbDriver.ExecuteReader(sql))
			{
				while (reader.Read())
				{
					AlternativeImages.Add(reader.ReadString(0));
					MiniAlternativeImages.Add(reader.ReadString(1));
				}
			}

			// Add the original.
			if (AlternativeImages.Count > 0)
			{
				AlternativeImages.Insert(0, ImageUrl);
				AlternativeImages = AlternativeImages.Distinct().ToList();
				// Add the small item to the list. But this should be the normal size image.
				MiniAlternativeImages.Insert(0, ImageUrl);
				MiniAlternativeImages = MiniAlternativeImages.Distinct().ToList();
			}
		}

		/// <summary>
		/// Determine what the price should be depending on wholesale cost.
		/// NOTE: Might need to compare the price to WholesalePrice just incase?
		/// </summary>
		public decimal GetWholesalePriceA(decimal price)
		{
			if (WholeSaleCustomer != 0)
			{
				if (WholeSalePriceA > 0)
				{
					return Math.Min(WholeSalePriceA, price * .9m);
				}
				else
				{
					return price * .9m;
				}
			}

			return price;
		}

		#region static

		/// <summary>
		/// Important to note most of this item is being loaded from the [Items] table which is different
		/// then loading from the `Item numbers, descrip, page` table.
		/// </summary>
		static public Product LoadFromOrder(MySqlDataReader reader, bool? wholesaleOrder = null)
		{
			string kosher = string.Empty;
			string heatWarning = string.Empty;
			Product p = new Product();
			try
			{
				if (wholesaleOrder.HasValue)
				{
					p.WholeSaleCustomer = wholesaleOrder.Value ? 1 : 0;
				}
				else
				{
					Debugger.Break();
					//p.WholeSaleCustomer = CookieHelper.GetWholesaleValue();
				}
				p.Valid = true;
				p.ItemId = reader.ReadInt32("ItemID");
				p.ItemNumber = reader.ReadString("ItemNumber").Trim();
				p.MasterItemNumber = reader.ReadString("MasterItemNumber").Trim();

				// I no longer care about the price the user added the item to their cart. I only care
				// about the cost of the item now.
				// RealPrice member can be deleted once this is all tested and straigtned out.
				p.RealPrice = reader.ReadDecimal("RealPrice");
				p.Price = reader.ReadDecimal("RealPrice");
				if (CkDefines.ProductValidWholesale(p.ItemNumber))
				{
					p.WholeSalePriceA = reader.ReadDecimal("WholeSalePriceA");
					p.Price = p.GetWholesalePriceA(p.Price);
				}
				p.SalePrice = reader.ReadDecimal("SalePrice");

				p.Price = Math.Round(p.Price, 2);
				p.SalePrice = Math.Round(p.SalePrice, 2);

				p.Weight = reader.ReadDouble("Weight");
				p.DW = reader.ReadDouble("DW");
				p.Taxable = reader.ReadBool("Tax");
				p.Discountable = reader.ReadBool("Discountable");
				p.Discontinued = reader.ReadBool("Discontinued");
				p.NonEdible = reader.ReadBool("NonEdible");
				p.SmallParts = reader.ReadBool("SmallParts");
				p.HeatWarning = reader.ReadString("HeatWarn");
				p.UpcCode = reader.ReadString("UPCCode");
				p.Description = reader.ReadString("Description");
				p.Quantity = reader.ReadInt16("Quantity");
				p.Special = reader.ReadInt16("CKPSpecial");
				p.SaleExpiration = reader.ReadDateTime("SaleEndCKPS");
				p.FriendlyUrlLink = reader.ReadString("FriendlyUrl");
				if (p.Special > 0)
				{
					p.ProductExpiration = reader.ReadDateTime("DateExpCKPS");
					if (reader.IsDBNull(reader.GetOrdinal("QuantityCKPS")))
					{
						p.Inventory = Int32.MaxValue - 1;
					}
					else
					{
						p.Inventory = reader.ReadInt16("QuantityCKPS");
					}
				}

				if (p.SaleExpiration < DateTime.Now.Date)
				{
					p.SalePrice = 0;
					p.SaleExpiration = DateTime.MaxValue;
				}

				p.TempOutOfStock = reader.ReadInt16("ItemTempOOS");
				p.CanShipExpress = reader.ReadInt16("PageNumber") == 1 ? false : true;

				p.CountryOfOrigin = reader.ReadString("CountryCode");
				p.Harminization = reader.ReadString("CountryOfOrigin2");
				p.Length = reader.ReadInt16("L");
				p.Width = reader.ReadInt16("W");
				p.Height = reader.ReadInt16("H");

				// Once everything updated on server this can be removed.
				try
				{
					p.ImageUrlBase = reader.ReadString("ImageUrl");
					p.ImageUrl = CkDefines.ImageUrl(p.ItemNumber, p.ImageUrlBase);
				}
				catch
				{
					p.ImageUrl = FileLocations.GetImagePathName(p.ItemNumber.Trim());
				}
				p.Details = reader.ReadString("Details");
				p.LoadMultiDiscounts();
			}
			catch
			{
			}
			return p;
		}

		#endregion static
	}

	public class FavoriteProduct : Product
	{
		#region Properties

		/// <summary>
		/// This is the last order this product was on.
		/// </summary>
		public string OrderOn { get; set; }
		public string OrderId { get; set; }

		#endregion Properties
	}
}