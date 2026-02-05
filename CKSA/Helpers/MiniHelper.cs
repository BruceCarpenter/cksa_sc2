using ckLib;
using System.Diagnostics;
using System.Web;

/// <summary>
/// Summary description for MiniHelper
/// </summary>
/// 
namespace CKSA.Helpers
{
	public class MiniHelper
	{
		public int ShopId { get; set; }
		public int CatId { get; set; }
		public int SubCatId { get; set; }
		public string ShopName { get; set; } = string.Empty;
		public int Id { get; set; }
		public int NumberOnPage { get; set; }
		public int FirstLatest { get; set; }
		public int RunAs { get; set; }
		public string Parameter { get; set; } = string.Empty; // General parameter for more refined pages. ie promotions
		public bool HasResults { get; set; }

		public MiniHelper()
		{
		}

		public static string MiniLoad(object itemNumber, object itemId, object description, object friendlyUrl, object masterItemNumber = null, object imageUrl = null)
		{
			var sItemId = itemId.ToString();
			var sItemNumber = itemNumber.ToString();
			var imageLinkStart = string.Empty;
			var sMasterItem = sItemNumber;
			var imageLocation = imageUrl == null ? string.Empty : imageUrl.ToString();

			if (masterItemNumber != null)
			{
				sMasterItem = masterItemNumber.ToString();
			}

			imageLinkStart = $"<a href={friendlyUrl}>";

			var newDescription = HttpUtility.HtmlEncode(description.ToString());

			var miniPathName = string.Empty;

			if (!string.IsNullOrEmpty(imageLocation))
			{
				miniPathName = CkDefines.ImageMiniUrl(sMasterItem, imageLocation);
			}

			return $"{imageLinkStart}<img src='{miniPathName}' title='{newDescription}' class='' alt='{newDescription}' /></a>";
		}

		/// <summary>
		/// If possible use the product.FriendlyURL instead of rebuilding the url here.
		/// </summary>
		/// <param name="sItemId"></param>
		/// <param name="description"></param>
		/// <returns></returns>
		public string CreateUrl(int sItemId, string description)
		{
			//
			// We have full path info use it.
			var parser = new UrlProductParser();
			parser.ShopName = ShopName;
			parser.ShopUrl = ShopName;
			parser.ShopId = ShopId;
			parser.CategoryId = CatId;
			parser.SubCategoryId = SubCatId;
			parser.ProductUrl = UrlProductParser.MakeUrlFriendly(description.ToString());
			parser.ProductId = sItemId;

			var url = parser.CreateUrl();

			return url;
		}

		public string FormatPrice(decimal price, decimal sale, DateTime endSaledate)
		{
			if (endSaledate < DateTime.Now.Date && endSaledate != DateTime.MinValue)
			{
				sale = decimal.MinValue;
			}

			return CkHtmlHelper.CreatePriceHtml(price, sale);
		}

		public static string FormatUnitPrice(decimal price, decimal sale, DateTime endSaledate, decimal[] priceRange, decimal[] salePriceRange)
		{
			if (endSaledate != DateTime.MinValue)
			{
				if (endSaledate < DateTime.Now.Date && endSaledate != DateTime.MinValue)
				{
					sale = decimal.MinValue;
				}
			}

			if (salePriceRange != null)
			{
				var low = Math.Min(priceRange[0], salePriceRange[0]);
				var high = Math.Max(priceRange[1], salePriceRange[1]);

				// The sale price could be lower than the price.
				var formatPrice = string.Format("<span style='color:red'>{0}</span>-{1}",
					CkHtmlHelper.FormatPrice(low),
					CkHtmlHelper.FormatPrice(high));

				return formatPrice;
			}
			else if (priceRange != null)
			{
				var formatPrice = string.Format("{0}-{1}",
					CkHtmlHelper.FormatPrice(priceRange[0]),
					CkHtmlHelper.FormatPrice(priceRange[1]));
				return formatPrice;
			}

			return CkHtmlHelper.CreatePriceHtml(price, sale);
		}

		public string Description(object description)
		{
			return CkDefines.FormatStringToLength(description.ToString(), 27);
		}

		public string Unit(object unit, object price)
		{
			string unitFormat = string.Empty;

			if (Convert.ToDouble(price) > 0)
			{
				unitFormat = CkDefines.FormatStringToLength(unit.ToString(), 15);
			}

			return unitFormat;
		}



	}
}