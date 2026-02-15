using MySqlConnector;

namespace ckLib
{
	/// <summary>
	/// Going to see if a class to help loading minis is useful.
	/// This might take over ckMini.ascx.
	/// </summary>
	public class MiniLoader
	{
		public MiniLoader()
		{
		}

		public void Load(string itemLookup)
		{

		}


		/// <summary>
		/// This to update:
		/// ImageUrl currently getting 350. Do I get all or pass which I want?
		/// </summary>
		public List<Product> Load(List<string> itemsLookup, bool smallMini)
		{
			List<Product> products = new List<Product>();
			if (itemsLookup.Count == 0)
			{
				return products;
			}

			var sql = @"Select A.Units, A.ItemId, A.`Item Number`, A.Description,
							A.NonTaxable, A.NonDiscountable, A.`Extra Shipping`,
							A.Weight, A.Price, A.SalePrice, A.ItemTempOOS, CKPSpecial, DateExpCKPS, SaleEndCKPS, 
							QuantityCKPS, FriendlyUrl, A.MasterItemNumber, A.ImageUrl
							from `item numbers, descrip, page` as A
							where A.`Item Number` in (";
			try
			{
				using (var conn = DbDriver.OpenConnection())
				{
					using (var command = conn.CreateCommand())
					{
						var inStatement = CreateParametersForIn(command, itemsLookup);
						sql += inStatement + ")";
						command.CommandText = sql;
						using (var reader = command.ExecuteReader())
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
								p.Price = reader.ReadDecimal( "Price");
								p.SalePrice = reader.ReadDecimal( "SalePrice");
								p.TempOutOfStock = reader.ReadInt16( "ItemTempOOS");
								p.Special = reader.ReadInt16( "CKPSpecial");
								p.ProductExpiration = reader.ReadDateTime( "DateExpCKPS");
								p.SaleExpiration = reader.ReadDateTimeAsString( "SaleEndCKPS");
								p.Quantity = reader.ReadInt16( "QuantityCKPS");
								p.ImageUrlBase = reader.ReadString("ImageUrl");
								p.ImageUrl = CkDefines.ImageCategoryUrl(p.MasterItemNumber, p.ImageUrlBase);

								//
								// Not sure what is going on here but I do not see 135 images being used?
								// Using updated ImageUrl from database. I cant get this breakpoint to be hit.
								//if (smallMini)
								//{
								//	p.ImageUrl = FileLocations.GetImagePathName135(p.ItemNumber);
								//}
								//else
								//{
								//p.ImageUrl = FileLocations.GetImagePathName350(p.ItemNumber);
								//}
								products.Add(p);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "Miniloader.Load(string)", sql);
			}

			return products;
		}

		private string CreateParametersForIn(MySqlCommand command, List<string> itemsLookup)
		{
			string inString = string.Empty;

			for (var i = 0; i < itemsLookup.Count; i++)
			{
				var paramName = string.Format("@c{0}", i);
				command.Parameters.AddWithValue(paramName, itemsLookup[i]);

				if (i > 0)
				{
					inString += ",";
				}

				inString += paramName;
			}

			return inString;
		}

		public void Load(int itemId)
		{

		}
		public void Load(List<int> itemIds)
		{

		}
	}
}