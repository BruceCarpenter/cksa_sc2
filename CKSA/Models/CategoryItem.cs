namespace CKSA.Models
{
	public class CategoryItem
	{
		public string C { get; set; } // Category2
		public int Id { get; set; }
		public string Img { get; set; } // image calculated
		public string Desc { get; set; }
		public string Url { get; set; } // Category2Url

		public CategoryItem()
		{
			C = string.Empty;
			Id = 0;
			Img = string.Empty;
			Desc = string.Empty;
			Url = string.Empty;
		}

	}
}
