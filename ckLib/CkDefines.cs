using System.Diagnostics;
using System.Globalization;

namespace ckLib
{
	public class CkDefines
	{
		// If these two change need to update common.js
		public static decimal FlatRateShippingCost = 6.95m;
		public static decimal FreeWeShipCost = 60;
		public static decimal WholesaleMinOrder = 250;

		/// <summary>
		/// Convert the passed in units to units that contain html <abbr title="What does this mean">abv</abbr>.
		/// </summary>
		static public string UnitsConversion(string units, DateTime expirationDate)
		{
			var conversion = units;
			var abbrToConvert = new Dictionary<string, string>()
		{
			{ "pcs","pieces" },
			{ "pkg","package" }
		};

			foreach (var abr in abbrToConvert)
			{
				var index = units.IndexOf(abr.Key);
				if (index != -1)
				{
					var abbrHtml = string.Format("<abbr title='{0}'>", abr.Value);
					conversion = conversion.Insert(index + abr.Key.Length, "</abbr>");
					conversion = conversion.Insert(index, abbrHtml);
				}
			}

			if (!(expirationDate == DateTime.MaxValue || expirationDate == DateTime.MinValue))
			{
				conversion = string.Format("{0} (BB: {1})", conversion, expirationDate.ToString("M/d/yyyy"));
			}

			return conversion;
		}

		/// <summary>
		/// Generate the url to the category image.
		/// </summary>
		/// <param name="itemNumber">This should be the main unit and indicates the folder.</param>
		/// <param name="imageName">This is from the database</param>
		public static string ImageCategoryUrl(string itemNumber, string imageName)
		{
			if (!string.IsNullOrEmpty(itemNumber))
			{
				var path = FileLocations.GetImagePath(itemNumber);

				if(Debugger.IsAttached)
					return "https://www.countrykitchensa.com/catalog/350/" + path + "/" + imageName;

				return "/catalog/350/" + path + "/" + imageName;
			}

			return string.Empty;
		}

		/// <summary>
		/// Generate the url to the main unit image.
		/// </summary>
		/// <param name="itemNumber">This should be the main unit and indicates the folder.</param>
		/// <param name="imageName">This is from the database</param>
		public static string ImageUrl(string itemNumber, string imageName)
		{
			if (!string.IsNullOrEmpty(itemNumber))
			{
				var path = FileLocations.GetImagePath(itemNumber);
				if (Debugger.IsAttached)
				{
					return "https://www.countrykitchensa.com/catalog/images/" + path + "/" + imageName;
				}
				return "/catalog/images/" + path + "/" + imageName;
			}

			return string.Empty;
		}

		public static string IdeaImagePath(int ideaId)
		{
			string miniPathLoc = FileLocations.GetIdeaBigImagePath();
			return $"{miniPathLoc}{ideaId}.jpg";
		}

		public static bool ProductValidWholesale(string itemNumber)
		{
			string[] invalidPrefixes = { "ACR-", "TCC-", "TSA" };

			return !invalidPrefixes.Any(itemNumber.StartsWith);
		}

		static public string FormatStringToLength(string toFormat, int maxSize)
		{
			if (toFormat.Length > maxSize)
			{
				toFormat = toFormat.Substring(0, maxSize) + "...";
			}
			//else if (shortDecription.Length < 20)
			//{
			//    shortDecription = shortDecription.PadRight(38, ' ');
			//}

			return toFormat;

		} // Description

		/// <summary>
		/// NOTE: Should be deleted and only use the decimal version.
		/// </summary>
		/// <param name="price"></param>
		/// <returns></returns>
		static public string FormatPrice(string price)
		{
			string formatedPrice = "error";
			if (string.IsNullOrEmpty(price) == false)
			{
				NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
				double dPrice = Convert.ToDouble(price);
				formatedPrice = string.Format("{0}", dPrice.ToString("C", nfi));
			}
			return formatedPrice;
		}

		static public string FormatPrice(decimal price)
		{
			return CkDefines.FormatPrice(price.ToString());
		}

		static public decimal CalculatePercent(decimal price, decimal salePrice)
		{
			return Math.Round(((price - salePrice) / price) * 100m, 0);
		}

		static public string CreatePriceHtmlSnippet(decimal dPrice, decimal dSalePrice)
		{
			string finalPriceString = string.Empty;

			try
			{
				if (dPrice < 0)
				{
					finalPriceString = string.Format("<div'></div>");
				}
				else if (dSalePrice != decimal.MinValue && dSalePrice < dPrice)
				{
					finalPriceString = string.Format("<span style='text-decoration: line-through;'>{0}</span> " +
						"<span class='pSalePrice'>${1} ({2}% Off)</span>",
						CkDefines.FormatPrice(dPrice.ToString()),
						dSalePrice.ToString("N2"),
						CkDefines.CalculatePercent(dPrice, dSalePrice));
				}
				else
				{
					finalPriceString = string.Format("<div>$<span>{0}</span></div>",
						dPrice.ToString("N2"));
				}
			}
			catch (Exception)
			{
				finalPriceString = string.Format("{0}", CkDefines.FormatPrice(dPrice.ToString()));
			}

			return finalPriceString;
		}

		static public string CreateOptionPriceHtmlSnippet(decimal dPrice, decimal dSalePrice)
		{
			string finalPriceString = string.Empty;

			try
			{
				if (dSalePrice != decimal.MinValue && dSalePrice < dPrice)
				{
					finalPriceString = string.Format("<span style='text-decoration: line-through;'>{0}</span> <span>Sale ${1} ({2}% Off)</span>",
						CkDefines.FormatPrice(dPrice.ToString()),
						dSalePrice.ToString("N2"),
						CkDefines.CalculatePercent(dPrice, dSalePrice));
				}
				else
				{
					finalPriceString = string.Format("<span>${0}</span>",
						dPrice.ToString("N2"));
				}
			}
			catch (Exception)
			{
				finalPriceString = string.Format("{0}", CkDefines.FormatPrice(dPrice.ToString()));
			}

			return finalPriceString;
		}

		static public string CreatePriceHtml(decimal dPrice, decimal dSalePrice)
		{
			string finalPriceString = string.Empty;
			try
			{
				if (dPrice < 0)
				{
				}
				else if (dSalePrice != 0 && dSalePrice < dPrice)
				{
					finalPriceString = string.Format("<span style='text-decoration: line-through;'>{0}</span> " +
						"<span style='color:#FF0000'>{1} ({2}% Off)</span>",
						CkDefines.FormatPrice(dPrice),
						CkDefines.FormatPrice(dSalePrice),
						CkDefines.CalculatePercent(dPrice, dSalePrice));
				}
				else
				{
					finalPriceString = string.Format("{0}", CkDefines.FormatPrice(dPrice));
				}
			}
			catch (Exception)
			{
				finalPriceString = string.Format("{0}",CkDefines.FormatPrice(dPrice));
			}

			return finalPriceString;
		}

		/// <summary>
		/// Generate the url to the mini image.
		/// </summary>
		/// <param name="itemNumber">This should be the main unit and indicates the folder.</param>
		/// <param name="imageName">This is from the database</param>
		public static string ImageMiniUrl(string itemNumber, string imageName)
		{
			if (!string.IsNullOrEmpty(itemNumber))
			{
				var path = FileLocations.GetImagePath(itemNumber);
				if (Debugger.IsAttached)
				{
					return "https://www.countrykitchensa.com//catalog/252/" + path + "/" + imageName;
				}
				return "/catalog/252/" + path + "/" + imageName;
			}

			return string.Empty;
		}

		static public string GetAllergyWarning()
		{
			return "This product is prepared and packaged using machines that may come into contact with Wheat/Gluten, Eggs, Dairy or Dairy Products, Peanuts, Tree Nuts and Soy.";
		}
		/// <summary>
		/// According to standard 1-2 sentence description.
		/// http://ogp.me/
		/// </summary>
		/// <param name="description"></param>
		/// <returns></returns>
		static public string OgDescription(string description)
		{
			//int index = description.IndexOf(".");
			string final = description;
			//if (index > 0)
			//{
			//    final = description.Substring(0, index + 1);
			//}

			final = final.Replace('"', ' ');
			final = final.Replace('\'', ' ');
			final = final.Replace('<', ' ');
			final = final.Replace('=', ' ');

			return final;
		}

		static public string CreatePinIt(string url, string imageUrl)
		{
			return string.Format("//www.pinterest.com/pin/create/button/?url=http%3A%2F%2Fwww.countrykitchensa.com{0}&media={1}",
				url, imageUrl);
		}

		/// <summary>
		/// Check if an alternative route is available for this value.
		/// </summary>
		static public string AlternativeRoute(int id)
		{
			var altRoute = string.Empty;

			try
			{
				using (var mySql = DbDriver.OpenConnection())
				{
					using (var command = mySql.CreateCommand())
					{
						command.CommandType = System.Data.CommandType.Text;
						command.CommandText = "select ToUrl from AutoRedirect where FromKey = @c1";
						command.Parameters.AddWithValue("@c1", id);
						using (var reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								altRoute = reader.ReadString(0);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "CKDefines/AlternativeRoute", id.ToString());
			}

			return altRoute;
		}
	}
}
