using ckLib;
using System.Globalization;

namespace CKSA.Helpers
{
	public class CkHtmlHelper
	{
		const string Title = " | Country Kitchen SweetArt";

		static public string CreatePriceHtml(decimal dPrice, decimal dSalePrice)
		{
			string finalPriceString = string.Empty;
			try
			{
				if (dPrice < 0)
				{
				}
				else if (dSalePrice != decimal.MinValue && dSalePrice < dPrice)
				{
					finalPriceString = string.Format("<span style='text-decoration: line-through;'>{0}</span> " +
						"<span style='color:#FF0000'>{1} ({2}% Off)</span>",
						CkHtmlHelper.FormatPrice(dPrice),
						CkHtmlHelper.FormatPrice(dSalePrice),
						CkHtmlHelper.CalculatePercent(dPrice, dSalePrice));
				}
				else
				{
					finalPriceString = string.Format("{0}", CkHtmlHelper.FormatPrice(dPrice));
				}
			}
			catch (Exception)
			{
				finalPriceString = $"{CkHtmlHelper.FormatPrice(dPrice)}";
			}

			return finalPriceString;
		}

		static public string FormatPrice(decimal price)
		{
			string formatedPrice = "error";
			if (price != decimal.MinValue)
			{
				NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
				double dPrice = Convert.ToDouble(price);
				formatedPrice = string.Format("{0}", dPrice.ToString("C", nfi));
			}
			return formatedPrice;
		}
		static public decimal CalculatePercent(decimal price, decimal salePrice)
		{
			return Math.Round(((price - salePrice) / price) * 100m, 0);
		}

		static public string AddItemButton(string itemNumber, int outOfStock, int bSpecial,
		   DateTime expDate, DateTime saleEnd, int quantity, decimal price, bool isUnit = false, string friendlyUrl = "")
		{
			string format = string.Empty;
			var handled = false;

			// Taken from App_Code\Filter.cs GetProducts
			// If the date is max then assume no isSpecial set if not then the Add To Cart button is disabled.
			if (expDate == DateTime.MinValue)
			{
				bSpecial = 0;
			}

			/*if (bSpecial > 0 && outOfStock == 0)
			{
				handled = true;
				// Need to determine if quantity is Max value -1 indicating unlimited value.
				int nQuantity = 0;
				DateTime dtSaleEnd;

				if (int.TryParse(quantity.ToString(), out nQuantity) == false)
				{
					// Might indicate quantity is null and not a number.
					nQuantity = Int32.MaxValue - 1;
				}

				DateTime.TryParse(saleEnd.ToString(), out dtSaleEnd);
				if ((dtSaleEnd != DateTime.MaxValue && dtSaleEnd != DateTime.MinValue) && dtSaleEnd < DateTime.Now.Date)
				{
					handled = false;
				}
				else
				{
					if (nQuantity <= 0)
					{
						format = "<div class='UnavailableItem'>Sold out.</div>";
					}
					else
					{
						format = string.Format("<a href={0} class='button buyBt'><i class='glyphicon glyphicon-shopping-cart'></i> Sale or Promotion</a>",
							friendlyUrl);
					}
				}
			}*/
			if (handled == false)
			{
				if (price < 0)
				{
					format = "<div>Click image for more detail</div>";
				}
				else if (outOfStock == 0)
				{
					string link = string.Format("'{0}','{1}'", itemNumber.ToString(), 1);
					// the javascript needs to be wrapped in " because there are two strings inside that function.
					format = string.Format("<a href=\"javascript:addToCart({0})\" class='button buyBt'><i class='glyphicon glyphicon-shopping-cart'></i> Add to Cart</a>", link);
				}
				else
				{
					//format = string.Format("<div style='text-align:center;'>{0}</div>", Product.OutOfStockReason(outOfStock));
					//format = string.Format("<a class='button buyBt'>Temporarily Out Of Stock </a>");

					// the javascript needs to be wrapped in " because there are two strings inside that function.
					format = string.Format("<a class='button buyBt' style='background-color:lightgrey;border: 1px solid lightgrey ;'><i class='glyphicon glyphicon-remove-circle'></i> Out Of Stock</a>");
				}
			}

			return format;
		}


		static public string CreateTitle(string title, string titleType)
		{
			string finalTitle = title + titleType;
			/*
			int finalCutPoint = 65;

			if (finalTitle.Length > finalCutPoint)
			{
				//
				// Make title short.
				finalTitle = finalTitle.Substring(0, 65);

				if (finalTitle[finalTitle.Length-1] != ' ')
				{
					finalCutPoint = finalTitle.LastIndexOf(' ');
					if (finalCutPoint == -1)
					{
						finalCutPoint = 65;
					}
				}
				finalTitle = finalTitle.Substring(0, finalCutPoint);
			}
			else if (finalTitle.Length <= 5)
			{
				finalTitle = finalTitle + titleType;
			}
			*/
			return finalTitle;
		}

		static public string CreateTitle(string title)
		{
			return CreateTitle(title, Title);
		}

	}
}
