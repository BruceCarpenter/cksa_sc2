using ckLib;

namespace CKSA.Models
{
	public class ProductData
	{
		public Dictionary<string, string> Breadcrumbs { get; set; }
		public UrlProductParser? Parser { get; set; }
		public Product? TheProduct { get; set; }

		public List<Product>? AllProducts { get; set; }
		/// <summary>
		/// If true none of the loaded items have discounts. This will effect how the page is rendered.
		/// </summary>
		public bool NoDiscounts { get; set; }

		public string CurrentUrl { get; set; }

		public string lblDescription { get; set; }

		public bool SuperSaver { get; set; }

		public bool ShowMoreInfoTab { get; set; }

		public string BrandUrl { get; set; }
		public string FbTitle { get; set; }
		public string Description { get; set; }
		public string Pinit { get; set; }


	}
}
