namespace CKSA.Helpers
{
	public class CustomImages
	{
		public string Image { get; set; }
		public string Link { get; set; }
		public string Alt { get; set; }

		public CustomImages()
		{
			Image = string.Empty;
			Link = string.Empty;
			Alt = string.Empty;
		}

		public string GetThemeLink()
		{
			// Not sure if the srcset is working. Have to revisit this....
			return "<a href='" + Link + "'><img class='theme-image' alt='" + Alt + "' src ='" + Image + "' srcset='" + Image + " 1500w,/images/black-small.jpg 991w'></a>";
		}
	}
}
