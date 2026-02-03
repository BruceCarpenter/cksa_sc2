using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ckLib
{
	public class CkDefines
	{
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
				else if (dSalePrice != 0 && dSalePrice < dPrice)
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
				if (dSalePrice != 0 && dSalePrice < dPrice)
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

	}
}
