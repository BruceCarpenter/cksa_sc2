using ckLib;

namespace CKSA.Models
{
	public class IdeaModel
	{
		public List<IdeaData> Data { get; set; }
		public string H1Tag { get; set; }
		public string MetaDescription { get;set ; }
		public Dictionary<string, string> Breadcrumbs { get; set; }
		public string Canonical { get; set; }
		public UrlIdeaParser Parser { get; set; }

		public string GenDescription { get; set; }
		public IdeaModel()
		{
			Data = new List<IdeaData>();
		}

	}
}
