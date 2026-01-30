using ckLib;

namespace CKSA.Models
{
	public class CategoryData
	{
		public List<CategoryItem>? Cats { get; set; }
		public UrlProductParser? Parser { get; set; }

		public string H1Tag { get; set; }
		public string GeneralDescription { get; set; }
		public string HtmlTitle { get; set; }
		public string HtmlDescription { get; set; }
		public Dictionary<string, string> Breadcrumbs { get; set; }
		public string Canonical { get; set; }

		public CategoryData()
		{
			Cats = null;
			Parser = null;
			H1Tag = string.Empty;
			GeneralDescription = string.Empty;
			HtmlTitle = string.Empty;
			HtmlDescription = string.Empty;
			Breadcrumbs = new Dictionary<string, string>();
			Canonical = string.Empty;
		}	

	}
}
