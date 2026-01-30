using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Routing;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ckLib
{
	/// <summary>
	/// Summary description for UrlParser
	/// </summary>
	public class UrlProductParser : UrlProductParserBase
	{
		public enum Step
		{
			Shop,
			Category,
			SubCategory,
			Product
		}

		#region Properties

		public Step CurrentStep { get; set; }
		public bool ValidRoute { get; set; }

		/// <summary>
		/// There were a couple RouteData namespaces and I picked this one.
		/// </summary>
		/// 
		[JsonIgnore]
		public Microsoft.AspNetCore.Routing.RouteData? RouteData { get; set; }

		#endregion Properties

		public UrlProductParser(Step step, Microsoft.AspNetCore.Routing.RouteData route) : base()
		{
			try
			{
				CurrentStep = step;
				RouteData = route;

				Parse(step);

				if (ValidRoute)
				{
					GetParentIds();
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "UrlProductParser");
			}
		}


		public UrlProductParser() : base()
		{
		}

		/// <summary>
		/// Determine if the url is valid. Depending on the step depends on 
		/// what part should be valid. This way if a product id is passed in
		/// with a shop id but nothing else this will return false. As an
		/// example Google has this page indexed:
		/// https://www.countrykitchensa.com/catalog/product.aspx?ShopId=39&CatId=645&SubCatId=1236&productId=631971
		/// Some of those steps are now invalid so the url comes back missing parts.
		/// </summary>
		/// <returns></returns>
		public bool IsValidUrl()
		{
			var valid = true;

			switch (CurrentStep)
			{
				case Step.Product:
					if (
						string.IsNullOrEmpty(CategoryId) ||
						string.IsNullOrEmpty(CategoryName) ||
						string.IsNullOrEmpty(CategoryUrl) ||
						string.IsNullOrEmpty(ProductId) ||
						string.IsNullOrEmpty(ProductName) ||
						string.IsNullOrEmpty(ProductUrl) ||
						string.IsNullOrEmpty(ShopId) ||
						string.IsNullOrEmpty(ShopName) ||
						string.IsNullOrEmpty(ShopUrl) ||
						string.IsNullOrEmpty(SubCategoryId) ||
						string.IsNullOrEmpty(SubCategoryName) ||
						string.IsNullOrEmpty(SubCategoryUrl))
						return false;
					break;
			}

			return valid;
		}

		/// <summary>
		/// User got here using an old link.
		/// </summary>
		/// <param name="id"></param>
		public void ParseStandardMethod()
		{
			if (ValidRoute == false)
			{
				switch (CurrentStep)
				{
					case Step.Shop:
						if (string.IsNullOrEmpty(ShopName))
						{
							ShopName = GetShopName();
						}
						break;
				}
				GetParentIds();
			}
		}


		public string GenerateSingleLink(Step step)
		{
			try
			{
				object result = null;
				var path = UrlProductParser.UrlPath;

				switch (step)
				{
					case Step.Shop:
						// If ShopUrl is empty, we return a different structure
						if (string.IsNullOrEmpty(ShopUrl))
							return "[{\"name\":\"Shops\",\"link\":\"/shops/\"}]";

						// Return an array with two objects
						result = new[] {
					new { name = "Shops", link = "/shops/" },
					new { name = SafeString(ShopName), link = $"{path}/{ShopUrl}/{ShopId}/" }
				};
						break;

					case Step.Category:
						result = $"{path}/{ShopUrl}/{CategoryUrl}/{ShopId}/{CategoryId}/";
						break;

					case Step.SubCategory:
						result = $"{path}/{ShopUrl}/{SubCategoryUrl}/{ShopId}/{CategoryId}/{SubCategoryId}/";
						break;

					case Step.Product:
						result = new
						{
							name = SafeString(ProductName),
							link = $"{path}/{ShopUrl}/{ProductUrl}/{ShopId}/{CategoryId}/{SubCategoryId}/{ProductId}/"
						};
						break;
				}

				// Serialize the object to a JSON string (handles quotes and escaping for you!)
				return result is string s ? s : JsonSerializer.Serialize(result);
			}
			catch (Exception ex)
			{
				// Use the line number trick we talked about earlier!
				ErrorHandler.Handle(ex, "GenerateJsonBreadcrumb");
				return string.Empty;
			}
		}

		public Dictionary<string, string> GenerateListBreadcrumb(Step step)
		{
			var crumbs = new Dictionary<string, string>();
			try
			{
				using (var conn = DbDriver.OpenConnection())
				{
					using (var command = conn.CreateCommand())
					{
						if (step >= Step.Shop)
						{
							crumbs["Shops"] = "/shops/";

							command.CommandText = "select url from category where Cat1id=@c0";
							command.Parameters.AddWithValue("@c0", ShopId);
							using (var reader = command.ExecuteReader())
							{
								if (reader.Read())
								{
									var url = reader.ReadString(0);
									crumbs[SafeString(ShopName)] = url;
								}
							}
						}

						if (step >= Step.Category)
						{
							command.Parameters.Clear();
							command.CommandText = "select url from `category 2` where Cat2id=@c0";
							command.Parameters.AddWithValue("@c0", CategoryId);
							using (var reader = command.ExecuteReader())
							{
								if (reader.Read())
								{
									var url = reader.ReadString(0);
									crumbs[SafeString(CategoryName)] = url;
								}
							}
						}

						if (step >= Step.SubCategory)
						{
							command.Parameters.Clear();
							command.CommandText = "select url from `category 3` where Cat3id=@c0";
							command.Parameters.AddWithValue("@c0", SubCategoryId);
							using (var reader = command.ExecuteReader())
							{
								if (reader.Read())
								{
									var url = reader.ReadString(0);
									crumbs[SafeString(SubCategoryName)] = url;
								}
							}
						}

						if (step >= Step.Product)
						{
							command.Parameters.Clear();
							command.CommandText = "select Friendlyurl from `ITEM NUMBERS, DESCRIP, PAGE` where ItemId=@c0";
							command.Parameters.AddWithValue("@c0", ProductId);
							using (var reader = command.ExecuteReader())
							{
								if (reader.Read())
								{
									var url = reader.ReadString(0);
									if (!string.IsNullOrEmpty(url))
									{
										url = url.Substring(1, url.Length - 2);
										crumbs[SafeString(ProductName)] = url;
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (string.IsNullOrEmpty(ProductId))
				{
					ProductId = string.Empty;
				}
				ErrorHandler.Handle(ex, "GenerateJsonBreadcrumb", string.Format("product Id:{0}", ProductId));
			}

			return crumbs;
		}


		public string GenerateJsonBreadcrumb(Step step)
		{
			// 1. Start with a clean List of anonymous objects
			var breadcrumbs = new List<object>
	{
		new { name = "Home", link = "/" }
	};

			try
			{
				var path = UrlProductParser.UrlPath;

				// 2. Add each level based on the 'Step' hierarchy
				if (step >= Step.Shop)
				{
					breadcrumbs.Add(new { name = "Shops", link = "/shops/" });

					if (!string.IsNullOrEmpty(ShopUrl))
					{
						breadcrumbs.Add(new
						{
							name = SafeString(ShopName),
							link = $"{path}/{ShopUrl}/{ShopId}/"
						});
					}
					else
					{
						// If ShopUrl is empty, we stop here per your original logic
						return JsonSerializer.Serialize(breadcrumbs);
					}
				}

				if (step >= Step.Category)
				{
					breadcrumbs.Add(new
					{
						name = SafeString(CategoryName),
						link = $"{path}/{ShopUrl}/{CategoryUrl}/{ShopId}/{CategoryId}/"
					});
				}

				if (step >= Step.SubCategory)
				{
					breadcrumbs.Add(new
					{
						name = SafeString(SubCategoryName),
						link = $"{path}/{ShopUrl}/{SubCategoryUrl}/{ShopId}/{CategoryId}/{SubCategoryId}/"
					});
				}

				if (step >= Step.Product)
				{
					breadcrumbs.Add(new
					{
						name = SafeString(ProductName),
						link = $"{path}/{ShopUrl}/{ProductUrl}/{ShopId}/{CategoryId}/{SubCategoryId}/{ProductId}/"
					});
				}

				// 3. One single call to convert the entire list to valid JSON
				return JsonSerializer.Serialize(breadcrumbs);
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "GenerateJsonBreadcrumb");
				// Return at least a valid empty JSON array so the JS doesn't crash
				return "[]";
			}
		}

		protected bool IsValidRoute()
		{
			if (string.IsNullOrEmpty(ShopUrl))
			{
				return false;
			}

			return ShopId != "0";
		}

		protected void Parse(Step step)
		{
			try
			{
				ShopUrl = GetIfContains("ShopName");
				ShopId = GetIfContains("ShopId");
				ShopId = CheckIfDigit(ShopId);

				CategoryId = GetIfContains("CategoryId");
				CategoryUrl = GetIfContains("CategoryName");
				SubCategoryId = GetIfContains("SubCategoryId");
				SubCategoryUrl = GetIfContains("SubCategoryName");
				ProductId = GetIfContains("ProductId");
				ProductUrl = GetIfContains("ProductName");

				ValidRoute = IsValidRoute();
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "Parse");
			}
		}


		protected string GetIfContains(string key)
		{
			if (RouteData != null && RouteData.Values.TryGetValue(key, out var value))
			{
				return value?.ToString() ?? string.Empty;
			}

			return string.Empty;
		}

		public string GetShopName()
		{
			var shopName = "Unknown Shop";
			var sql = string.Empty;

			try
			{
				if (string.IsNullOrEmpty(ShopId) == false)
				{
					sql = "Select `category 1` from category where Cat1ID=@c0";
					using var MySql = DbDriver.OpenConnection();
					using (var command = MySql.CreateCommand())
					{
						command.CommandText = sql;
						command.Parameters.AddWithValue("@c0", ShopId);
						using (var reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								shopName = reader.ReadString("category 1");
								ShopUrl = MakeUrlFriendly(shopName);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "GetShopName", sql);
			}

			return shopName;
		}
		public string SafeString(string input)
		{
			return WebUtility.HtmlEncode(input);
		}

	}

}