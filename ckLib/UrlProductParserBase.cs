using System.Diagnostics;

namespace ckLib
{
	public class UrlProductParserBase : UrlBaseParser
	{
		#region Properties

		public string ShopName { get; set; }
		public string ShopUrl { get; set; }
		public int ShopId { get; set; }

		public string CategoryName { get; set; }
		public string CategoryUrl { get; set; }
		public int CategoryId { get; set; }

		public string SubCategoryName { get; set; }
		public string SubCategoryUrl { get; set; }
		public int SubCategoryId { get; set; }

		public string ProductName { get; set; }
		public string ProductUrl { get; set; }
		public int ProductId { get; set; }

		/// <summary>
		/// The url is probably old since there is a number
		/// but no name with it.
		/// </summary>
		public bool OldPath { get; set; }

		static public string UrlPath = "/shop";

		#endregion Properties

		public UrlProductParserBase() : base()
		{
			Clear();
			OldPath = false;
		}

		public void Clear()
		{
			ShopName = string.Empty;
			ShopUrl = string.Empty;

			CategoryName = string.Empty;
			CategoryUrl = string.Empty;
			CategoryId = 0;

			SubCategoryName = string.Empty;
			SubCategoryUrl = string.Empty;
			SubCategoryId = 0;

			ProductName = string.Empty;
			ProductUrl = string.Empty;
			ProductId = 0;
		}

		public (string shopName, int shopId, string categoryName, int categoryId, string subCategoryName, int subCategoryId, string productName, int productId) GetParentIds2()
		{
			//string select = "select ";
			//string from = " from ";
			//string where = " where ";
			//bool readShop = false;
			//bool readCat = false;
			//bool readSubCat = false;
			//bool readProduct = false;
			string shopName = string.Empty, categoryName = string.Empty, subCategoryName = string.Empty, productName = string.Empty;
			int shopId = 0, categoryId = 0, subCategoryId = 0, productId = 0;

			using (var conn = DbDriver.OpenConnection())
			using (var command = conn.CreateCommand())
			{
				// Shop
				/*if (string.IsNullOrEmpty(ShopName) && ShopId != 0)
				{
					readShop = true;
					select += " s.`category 1` AS ShopName, s.Cat1ID AS ShopId ";
					from += " category s ";
					where += " (s.Cat1ID = @ShopId) ";
					command.Parameters.AddWithValue("@ShopId", ShopId);
				}

				// Category 
				// If there is no shopId then I need to modify the LEFT join
				if (string.IsNullOrEmpty(CategoryName) && CategoryId != 0)
				{
					readCat = true;
					if (select.Length > 5) select += ",";
					select += "c.`category 2` AS CategoryName, c.CAT2ID as CategoryId";
					from += " LEFT JOIN `category 2` AS c ON c.category = s.Cat1ID";
					if (where.Length > 5) where += " AND ";
					where += " (@CategoryId IS NULL OR c.Cat2ID = @CategoryId) ";
					command.Parameters.AddWithValue("@CategoryId", CategoryId);
				}

				// SubCategory
				if (string.IsNullOrEmpty(SubCategoryName) && SubCategoryId != 0)
				{
					readSubCat = true;
					if (select.Length > 5) select += ",";
					select += " sc.`Category 3` AS SubCategoryName, sc.Cat3ID AS SubCategoryId ";
					from += " LEFT JOIN `Category 3` sc ON sc.Cat2ID = c.Cat2ID ";
					if (where.Length > 5) where += " AND ";
					where += " (@SubCategoryId IS NULL OR sc.Cat3ID = @SubCategoryId) ";
					command.Parameters.AddWithValue("@SubCategoryId", SubCategoryId);
				}

				// Product
				if (string.IsNullOrEmpty(ProductName) && ProductId != 0)
				{
					readProduct = true;
					if (select.Length > 5) select += ",";
					select += " d.Description AS ProductName, p.ProductId ";
					from += @"LEFT JOIN `category 3 and items` map ON map.Cat3ID = sc.Cat3ID
LEFT JOIN allunit p ON p.ProductId = map.ItemID
LEFT JOIN `item numbers, descrip, page` d ON d.itemId = p.ProductId";
					if (where.Length > 5) where += " AND ";
					where += " (@ProductId IS NULL OR d.itemId = @ProductId) ";
					command.Parameters.AddWithValue("@ProductId", ProductId);
				}

				var sql = select + from + where;*/
				command.CommandText = @"SELECT 
    s.`category 1` AS ShopName, s.Cat1ID AS ShopId,
    c.`category 2` AS CategoryName, c.CAT2ID as CategoryId,
    sc.`Category 3` AS SubCategoryName,  sc.Cat3ID AS SubCategoryId,
    d.Description AS ProductName, p.ProductId
FROM category s 
LEFT JOIN `category 2` AS c ON c.category = s.Cat1ID
LEFT JOIN `Category 3` sc ON sc.Cat2ID = c.Cat2ID
LEFT JOIN `category 3 and items` map ON map.Cat3ID = sc.Cat3ID
LEFT JOIN allunit p ON p.ProductId = map.ItemID 
    AND (@ProductId IS NULL OR p.ProductId = @ProductId)
LEFT JOIN `item numbers, descrip, page` d ON d.itemId = p.ProductId
WHERE 
    (@ShopId IS NULL OR s.Cat1ID = @ShopId)
    AND (@CategoryId IS NULL OR c.Cat2ID = @CategoryId)
    AND (@SubCategoryId IS NULL OR sc.Cat3ID = @SubCategoryId)
     LIMIT 1";

				command.Parameters.Clear();
				command.Parameters.AddWithValue("@ShopId", ShopId == 0 ? DBNull.Value : ShopId);
				command.Parameters.AddWithValue("@CategoryId", CategoryId == 0 ? DBNull.Value : CategoryId);
				command.Parameters.AddWithValue("@SubCategoryId", SubCategoryId == 0 ? DBNull.Value : SubCategoryId);
				command.Parameters.AddWithValue("@ProductId", ProductId == 0 ? DBNull.Value : ProductId);

				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						//if (readShop)
						{
							shopName = reader.ReadString("ShopName");
							shopId = reader.ReadInt32("ShopId");
							//Debug.WriteLine($"{shopName} {shopId}");
						}
						//if (readCat)
						{
							categoryName = reader.ReadString("CategoryName");
							categoryId = reader.ReadInt32("CategoryId");
							//Debug.WriteLine($"{categoryName} {categoryId}");
						}
						//if (readSubCat)
						{
							subCategoryName = reader.ReadString("SubCategoryName");
							subCategoryId = reader.ReadInt32("SubCategoryId");
							//Debug.WriteLine($"{subCategoryName} {subCategoryId}");
						}
						//if (readProduct)
						{
							productName = reader.ReadString("ProductName");
							productId = reader.ReadInt32("ProductId");
							//Debug.WriteLine($"{productName} {productId}");
						}
					}
				}
			}
			return (shopName, shopId, categoryName, categoryId, subCategoryName, subCategoryId, productName, productId);
		}

		public void GetParentIds()
		{
			try
			{
				var parents = GetParentIds2();

				
				if (SubCategoryId == 0 && ProductId != 0)
				{
					SubCategoryId = GetValueInt("Select Cat3ID from `category 3 and items` where ItemID=@c0 order by Cat3ID", ProductId, "Cat3ID");
					Debugger.Break();
				}

				if (string.IsNullOrEmpty(ProductName) && ProductId != 0)
				{
					ProductName = GetValue(@"select a.ProductId,b.Description from allunit as a
                                inner join `item numbers, descrip, page` as b
                                on a.ProductId = b.itemId
                                where a.ProductId = @c0", ProductId, "Description");
					ProductUrl = MakeUrlFriendly(ProductName);
				}

				if (string.IsNullOrEmpty(SubCategoryName) && SubCategoryId != 0)
				{
					SubCategoryName = GetValue("Select `Category 3`  from `Category 3` where Cat3ID=@c0", SubCategoryId, "Category 3");
					if (string.IsNullOrEmpty(SubCategoryName))
					{
						SubCategoryId = 0;
						OldPath = true;
					}
					SubCategoryUrl = MakeUrlFriendly(SubCategoryName);
				}

				if (CategoryId == 0 && SubCategoryId != 0)
				{
					// This gets called during search against SearchSubCategory
					CategoryId = GetValueInt("Select Cat2ID from `category 3` where Cat3ID=@c0 order by Cat2ID", SubCategoryId, "Cat2ID");
					//Debugger.Break();
				}

				if (string.IsNullOrEmpty(CategoryName) && CategoryId != 0)
				{
					CategoryName = GetValue("Select `category 2` from `category 2` where Cat2ID=@c0", CategoryId, "category 2");
					if (string.IsNullOrEmpty(CategoryName))
					{
						CategoryId = 0;
						OldPath = true;
					}
					CategoryUrl = MakeUrlFriendly(CategoryName);
				}

				if (ShopId == 0 && CategoryId != 0)
				{
					// This gets called during search against SearchSubCategory
					ShopId = GetValueInt("Select Category from `category 2` where Cat2ID=@c0 order by Category", CategoryId, "Category");
					//Debugger.Break();
				}

				if (string.IsNullOrEmpty(ShopName) && ShopId != 0)
				{
					ShopName = GetValue("Select `category 1` from category where Cat1ID=@c0", ShopId, "category 1");
					ShopUrl = MakeUrlFriendly(ShopName);
				}

				Debug.Assert(parents.shopName == ShopName && parents.shopId == ShopId);
				if(!string.IsNullOrEmpty(CategoryName)) Debug.Assert(parents.categoryName == CategoryName && parents.categoryId == CategoryId);
				if (!string.IsNullOrEmpty(SubCategoryName)) Debug.Assert(parents.subCategoryName == SubCategoryName && parents.subCategoryId == SubCategoryId);
				if(!string.IsNullOrEmpty(ProductName)) Debug.Assert(parents.productName == ProductName && parents.productId == ProductId);
				
				ShopName = parents.shopName;
				CategoryName = parents.categoryName;
				SubCategoryName = parents.subCategoryName;
				ProductName = parents.productName;

				ShopUrl = MakeUrlFriendly(ShopName);
				CategoryUrl = MakeUrlFriendly(CategoryName);
				SubCategoryUrl = MakeUrlFriendly(SubCategoryName);
				ProductUrl = MakeUrlFriendly(ProductName);

			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "UrlProductPaserBase.GetParentIds");
				throw;
			}
		}

		public bool IsValid()
		{
			bool isValid = false;

			if (ProductId != 0)
			{
				isValid = true;
			}
			else if (SubCategoryId != 0)
			{
				isValid = true;
			}
			else if (CategoryId != 0)
			{
				isValid = true;
			}
			else if (ShopId != 0)
			{
				isValid = true;
			}

			return isValid;
		}

		public string CreateUrl()
		{
			return CreateUrl("\"");
		}

		public string CreateUrl(string quote)
		{
			string url = string.Empty;

			if (ProductId != 0)
			{
				url = $"{quote}{UrlPath}/{ShopUrl}/{ProductUrl}/{ShopId}/{CategoryId}/{SubCategoryId}/{ProductId}/{quote}";
			}
			else if (SubCategoryId != 0)
			{
				url = $"{quote}{UrlPath}/{ShopUrl}/{SubCategoryUrl}/{ShopId}/{CategoryId}/{SubCategoryId}/{quote}";
			}
			else if (CategoryId != 0)
			{
				url = $"{quote}{UrlPath}/{ShopUrl}/{CategoryUrl}/{ShopId}/{CategoryId}/{quote}";
			}
			else if (ShopId != 0)
			{
				url = $"{quote}{UrlPath}/{ShopUrl}/{ShopId}/{quote}";
			}

			return url;
		}

	}
}