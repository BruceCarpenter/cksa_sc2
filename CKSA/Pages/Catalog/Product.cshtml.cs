using ckLib;
using CKSA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

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
				ProductDataModel = new ProductData();
				ProductDataModel.Parser = new UrlProductParser(UrlProductParser.Step.Product,RouteData);

				Int64 itemId = ProductDataModel.Parser.ProductId;
				if (itemId != 0)
				{
					var unitId = IsUnitProduct(ProductDataModel.Parser.ProductId);

				}

			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "catalog/product-OnGet");
			}

			return Page();
		} // OnGet

		private int IsUnitProduct(Int64 id)
		{
			var unitId = int.MinValue;

			using(var conn = DbDriver.OpenConnection())
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
	}
}
