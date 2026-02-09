using ckLib;
using System.Reflection.Metadata;

namespace CKSA.Models
{
	public class ckIdeaData
	{
		public string ImageUrl { get; set; }
		public string Description { get; set; }
		public int IdeaNumber { get; set; } // needed?
		public string TitleTxt { get; set; }
		public string Instructions { get; set; }
		public string GenDescription { get; set; }

		public string FB { get; set; }
		public string FbTitle { get; set; }
		public string Pinit { get; set; }
		public string CurrentUrl { get; set; }

		public List<string> AltImages { get; set; }
		public List<string> MiniAltImages { get; set; }
		public Dictionary<string, string> Breadcrumbs { get; set; }
		public List<string> Ingredients { get; set; }
		public UrlIdeaParser Parser { get; set; }


		public List<Video> Videos { get; set; }
		public string Canonical { get; set; }
		//public Blog TheBlog { get; set; }

		public ckIdeaData()
		{
			AltImages = new List<string>();
			MiniAltImages = new List<string>();
			Ingredients = new List<string>();
		}

	}
}
