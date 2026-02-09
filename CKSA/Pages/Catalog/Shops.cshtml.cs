using ckLib;
using CKSA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace CKSA.Pages.Catalog
{
	public class ShopModel : PageModel
	{
		public string H1Tag { get; set; } = "Cake decorating and candy making supplies";
		public List<ShopItem> Shops { get; set; } = new List<ShopItem>();

		[BindProperty(SupportsGet = true)] 
		public string? i { get; set; }

		public ShopModel()
		{
		}

		public void OnGet()
		{
			try
			{
				var cacher = new PageCacher<List<ShopItem>>();
				Shops = cacher.Retrieve(CacheKeys.ShopKey);

				if(!string.IsNullOrEmpty(i))
				{
					H1Tag = "Whoops! We can’t find that page. Perhaps you would like to browse another area.";
				}

				if (Shops == null)
				{
					GetDataItem();
					ResortData();
					if(Shops != null)
						cacher.Store(CacheKeys.ShopKey, Shops);
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "catalog/shops");
			}
		}
		private void GetDataItem()
		{
			Shops = new List<ShopItem>();
			var parser = new UrlProductParser(UrlProductParser.Step.Shop, RouteData);
			var shopImgs = new Dictionary<int, string>
		{
			{53,"/images/assembly-display.jpg" },
			{52,"/images/bakeware.jpg" },
			{49,"/images/chocolate-shop.jpg" },
			{51,"/images/cupcake-liners.jpg" },
			{44,"/images/cutters-molds.jpg" },
			{40,"/images/baking-essentials.jpg" },
			{55,"/images/gifts-apparel.jpg" },
			{38,"/images/icing-fondant-tools.jpg" },
			{46,"/images/ingredients.jpg" },
			{42,"/images/occasions.jpg" },
			{39,"/images/sweets-packaging.jpg" },
			{50,"/images/decorations.jpg" },
			{48,"/images/brands.jpg" },
			{54,"/images/instructional.jpg" },
			{45,"/images/sales-promotions.jpg" },
			{56,"/images/wholesale-bulk-discounts.jpg" }
		};

			try
			{
				using var conn = DbDriver.OpenConnection();
				using (var command = conn.CreateCommand())
				{
					command.CommandType = CommandType.Text;
					command.CommandText = "Select * from category order by `Category 1`";
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var shopData = new ShopItem();
							var categoryId = reader.ReadInt32(1);

							shopData.Id = categoryId.ToString();

							// This is a special case for chocolate. Skip straight to the subcategory since just one item under the shop.
							var img = shopImgs[categoryId];
							shopData.Category = reader.ReadString(0);
							shopData.Img = img;
							shopData.Desc = reader.ReadString(5);

							if (categoryId == 49)
							{
								shopData.Url = "/shop/chocolate/chocolate-candy-coating/49/588/";
							}
							else
							{
								shopData.Url = reader.ReadString(6);
							}

							Shops.Add(shopData);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "/shops/GetDataItem");
			}
		}

		/// <summary>
		/// Move certain items to the end of the list.
		/// 45 - Gift cards
		/// 48 - Brand
		/// 54 - Instructional
		/// </summary>
		private void ResortData()
		{
			var moveIds = new string[] { "48", "54", "45" };
			foreach (var i in moveIds)
			{
				var f = Shops.Find(x => x.Id == i);
				if (f != null)
				{
					var index = Shops.IndexOf(f);
					Shops.RemoveAt(index);
					Shops.Add(f);
				}
			}
		}
	}
}
