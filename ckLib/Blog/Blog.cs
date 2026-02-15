using System.Text;

/// <summary>
/// Summary description for Blog
/// </summary>
namespace ckLib
{
	public class Blog
	{
		public enum BlogType
		{
			Standard,
			Idea
		}
		public int Id { get { return MainSection.Id; } set { MainSection.Id = value; } }

		public string Url { get { return MainSection.Url; } set { MainSection.Url = value; } }
		public string Name { get { return MainSection.Name; } set { MainSection.Name = value; } }

		public string Title { get { return MainSection.PageTitle; } set { MainSection.PageTitle = value; } }
		public string Description { get { return MainSection.Description; } set { MainSection.Description = value; } }
		public string MetaDescription { get { return MainSection.MetaDescription; } set { MainSection.MetaDescription = value; } }

		public string Image { get { return MainSection.Image; } set { MainSection.Image = value; } }
		public string ImageAltText { get { return MainSection.ImageAltText; } set { MainSection.ImageAltText = value; } }
		public string MiniImageAltText { get { return MainSection.MiniImageAltText; } set { MainSection.MiniImageAltText = value; } }
		public string Date { get { return MainSection.Date; } set { MainSection.Date = value; } }
		public int Active { get { return MainSection.Active; } set { MainSection.Active = value; } }
		public string Mini { get { return MainSection.Mini; } set { MainSection.Mini = value; } }
		public BlogType BlogTypeValue { get; set; }

		public BlogMain MainSection { get; set; }
		public List<BlogSection>? BlogSections { get; set; }

		/// <summary>
		/// Needed when updating a blog via webservice.
		/// </summary>
		public string Json { get; set; }

		public Blog()
		{
			MainSection = new BlogMain();
			BlogSections = new List<BlogSection>();
		}

		public string NameHtml()
		{
			return string.Format("<h1 class='title'>{0}</h1>", Name);
		}

		public string Head()
		{
			var headerBuilder = new StringBuilder();

			headerBuilder.AppendFormat("<div class='col-lg-12 bheadTxt'>{0}</div>", Description);
			if (!string.IsNullOrEmpty(Image))
			{
				headerBuilder.AppendFormat("<img class='offset-2 col-lg-12 imgBlogPrint' src='{0}' alt='{1}'>", Image, ImageAltText);
			}

			return headerBuilder.ToString();
		}

		/// <summary>
		/// This is getting all the section in one time instead of breaking the section down into smaller pieces.
		/// </summary>
		public string GetNewSection(int section)
		{
			return BlogSections[section].Html();
		}

		public string BlogTableName()
		{
			var name = BlogTypeValue == BlogType.Standard ? "blog" : "blogidea";

			return name;
		}
		public string BlogPartTableName()
		{
			var name = BlogTypeValue == BlogType.Standard ? "blogpart" : "blogpartidea";

			return name;
		}
	}
}