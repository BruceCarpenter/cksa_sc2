namespace CKSA.Models
{
	public class MiniData : CategoryData
	{
		// Need Filter, Pager

		public string UrlBase { get; set; }
		
		/// <summary>
		/// Depending on the url will need to seperate either by a / or &.
		/// </summary>
		public string Sep { get; set; }

	}
}
