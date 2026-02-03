using ckLib;

namespace CKSA.Models
{
	public class SubCategoryData : CategoryData
	{
		public string QuickLinks { get; set; }

		public SubCategoryData() 
		{
			QuickLinks = string.Empty;	
		}
	}
}
