namespace CKSA.Models
{
	public class ShopItem
	{
		public string Category { get; set; }
		public string Id { get; set; }
		public string Img { get; set; }
		public string Desc { get; set; }
		public string Url { get; set; }

		public ShopItem()
		{
			Category = string.Empty;
			Id = string.Empty;
			Img = string.Empty;
			Desc = string.Empty;
			Url = string.Empty;
		}
	}
}
