using System.Dynamic;
using System.Text;
using System.Text.Json;
using System.Web;

namespace ckLib
{
	public enum BlogPartsOrdinal
	{
		BaseSectionId = 1,
		StandardImageId = 2,
		RecipeId = 3,
		Product = 4,
		Keyword = 5
	};

	/// <summary>
	/// The standard blog header
	/// </summary>
	public class BlogMain
	{
		public int Id { get; set; }
		public string Url { get; set; }
		public DateTime DateCreated { get; set; }
		public string Name { get; set; }
		public string PageTitle { get; set; }
		public string Description { get; set; }
		public string Image { get; set; }
		public string ImageAltText { get; set; }
		public string MiniImageAltText { get; set; }
		public string MetaDescription { get; set; }
		public string Date { get; set; }
		public int Active { get; set; }
		public string Mini { get; set; }

		public void Encoder()
		{
			ImageAltText = HttpUtility.HtmlEncode(ImageAltText);
		}


	}

	public class BlogSection
	{
		public BlogMain MainBlog { get; set; }
		/// <summary>
		/// Unique id of the section, database id.
		/// </summary>
		public int Id { get; set; }
		
		/// <summary>
		/// This is saved in the database as BlogSectionId and used to organize how the blog is displayed.
		/// </summary>
		public int OrderNumber { get; set; }

		/// <summary>
		/// This allows the type to be sent across in json.
		/// </summary>
		public BlogPartsOrdinal BlogType { get; set; }

		/// <summary>
		/// The position in the tab order and html order rendered.
		/// </summary>
		public int Position { get; set; }
		public string Header { get; set; }
		public string Description { get; set; }

		public BlogSection()
		{
			Id = int.MinValue;
		}
		public virtual string Html()
		{
			return "BlogSection";
		}
		/// <summary>
		/// This is the number of the class represented of the json object. Needed to load the json to a class.
		/// </summary>
		public virtual BlogPartsOrdinal JsonId()
		{
			return BlogPartsOrdinal.BaseSectionId;
		}

		public virtual void Encoder()
		{

		}

		/// <summary>
		/// Based on the input values create an href or just a text if no link is provided.
		/// </summary>
		/// <returns></returns>
		public string CreateImageTag(string image, string imageAltText, string imageWidth, string imageLink)
		{
			var result = string.Empty;

			if (string.IsNullOrEmpty(imageLink))
			{
				result = string.Format("<div><img class='{2}' src='{0}' alt='{1}'></div>", image, imageAltText, imageWidth);
			}
			else
			{
				result = string.Format("<div><a href='{3}'><img class='{2}' src='{0}' alt='{1}'></a></div>", image, imageAltText, imageWidth, imageLink);
			}

			return result;
		}
	}

	/// <summary>
	/// This object represents a blog section that has a header, 1 or 2 images and then text.
	/// </summary>
	public class BlogImages : BlogSection
	{
		#region Fields

		public string ImageOne { get; set; }
		public string ImageOneLink { get; set; }
		public string ImageOneAltText { get; set; }
		public string ImageOneAttribute { get; set; }
		public string ImageOneAttributeLink { get; set; }
		public string ImageTwo { get; set; }
		public string ImageTwoLink { get; set; }
		public string ImageTwoAltText { get; set; }
		public string ImageTwoAttribute { get; set; }
		public string ImageTwoAttributeLink { get; set; }

		#endregion Fields

		public override string Html()
		{
			string imageWidth;
			var sectionBuilder = new StringBuilder();

			sectionBuilder.Append("<div>");
			sectionBuilder.AppendFormat("<h2 class='sectionHeader'>{0}</h2>", Header);

			if (!string.IsNullOrEmpty(ImageOne) && !string.IsNullOrEmpty(ImageTwo))
			{
				imageWidth = "col-xs-6";
				sectionBuilder.AppendFormat("<div class='bImgS'>");
				sectionBuilder.Append(base.CreateImageTag(ImageOne, ImageOneAltText, imageWidth, ImageOneLink));
				sectionBuilder.Append(base.CreateImageTag(ImageTwo, ImageTwoAltText, imageWidth, ImageTwoLink));
				if (!string.IsNullOrEmpty(ImageTwoAttribute))
				{
					sectionBuilder.AppendFormat("<div><a href='{0}'>{1}</a></div>", ImageTwoAttributeLink, ImageTwoAttribute);
				}
				sectionBuilder.AppendFormat("</div>");
			}
			else
			{
				if (!string.IsNullOrEmpty(ImageOne))
				{
					imageWidth = "col-xs-6 col-xs-offset-2";
					sectionBuilder.Append(base.CreateImageTag(ImageOne, ImageOneAltText, imageWidth, ImageOneLink));
				}
				if (!string.IsNullOrEmpty(ImageTwo))
				{
					imageWidth = "col-xs-6 col-xs-offset-2";
					sectionBuilder.Append(base.CreateImageTag(ImageOne, ImageOneAltText, imageWidth, ImageOneLink));
				}
			}

			sectionBuilder.AppendFormat("<div class='col-xs-12 bDesc'>{0}</div></div>", Description);

			return sectionBuilder.ToString();
		}

		public override BlogPartsOrdinal JsonId()
		{
			return BlogPartsOrdinal.StandardImageId;
		}

		public BlogImages()
		{
			BlogType = BlogPartsOrdinal.StandardImageId;
		}
	}

	public class BlogRecipe : BlogSection
	{
		// List of recipe
		public string Name { get; set; }
		public string Author { get; set; }
		public DateTime DatePublished { get; set; }
		public string ImageUrl { get; set; }
		public string ImageAltText { get; set; }
		public List<String> Ingredients { get; set; }
		public string PrepTime { get; set; }
		public string Yield { get; set; }
		public string Instructions { get; set; }
		/// <summary>
		/// This video link is only for the Recipe Schema
		/// </summary>
		public string JsonVideo { get; set; }

		public BlogRecipe()
		{
			Author = "Country Kitchen SweetArt";
			Ingredients = new List<string>();
			BlogType = BlogPartsOrdinal.RecipeId;
		}

		/// <summary>
		/// Code to create the html string.
		/// </summary>
		public override string Html()
		{
			var finalHtml = new StringBuilder();

			finalHtml.Append("<div class='noPrint'>");
			finalHtml.AppendFormat("<h2 class='sectionHeader col-lg-12'>{0}</h2>", Name);
			finalHtml.AppendFormat("<div class='col-lg-12'><img class='col-lg-8' src='{0}' alt='{1}' title='{1}'></div>", ImageUrl, ImageAltText);
			finalHtml.AppendFormat("<div class='col-lg-12'>{0}</div>",Description);
			finalHtml.Append("</div>");

			if (Ingredients.Count > 0)
			{
				finalHtml.AppendFormat("<h3 class='col-lg-12'>Ingredients</h3>");
				finalHtml.Append("<ul>");
				finalHtml.AppendFormat(string.Join("", Ingredients.Select(item => string.Format("<li>{0}</li>", item))));
				finalHtml.Append("</ul>");
			}

			if (Instructions.Length > 0)
			{
				finalHtml.Append("<div id='recipeId'>");
				// Instructions could be needed in a numbered list.
				finalHtml.AppendFormat("<h3 class='col-lg-12'>Instructions</h3>");
				if (Instructions.Contains("\n"))
				{
					var instructionsAsList = Instructions.Split(new char[] { '\n' });
					instructionsAsList = instructionsAsList.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray();
					finalHtml.AppendFormat(string.Join("", instructionsAsList.Select(item => string.Format("<div class='col-lg-12' style='padding-bottom:7px;'>{0}</div>", item))));
				}
				else
				{
					finalHtml.AppendFormat("<div>{0}</div>", Instructions);
				}
				finalHtml.Append("</div>");
			}

			finalHtml.Append("<div class='noPrint'>");

			if (!string.IsNullOrEmpty(Yield))
			{
				finalHtml.AppendFormat("<div class='col-lg-12'>Yield: {0}</div>", Yield);
			}
			if (!string.IsNullOrEmpty(PrepTime))
			{
				finalHtml.AppendFormat("<div class='col-lg-12'>Preparation Time: {0}</div>", PrepTime);
			}

			finalHtml.Append("</div>");

			return finalHtml.ToString();
		}

		/// <summary>
		/// Create the json recipe based on Schema.org.
		/// https://schema.org/recipeIngredient
		/// </summary>
		public string Json()
		{
			dynamic item = new ExpandoObject();

			// Do not add Author here or it will be jsoned and duplicated.
			//item.author = Author;
			item.datePublished = DatePublished.ToString("yyyy-MM-dd");
			item.name = MainBlog.Name;
			if (!string.IsNullOrEmpty(MainBlog.Image))
			{
				item.image = MainBlog.Image;
			}
			item.description = MainBlog.Description;
			item.recipeIngredient = Ingredients;
			item.recipeInstructions = Instructions;

			if (!string.IsNullOrEmpty(Yield))
			{
				item.recipeYield = Yield;
			}
			if(!string.IsNullOrEmpty(PrepTime))
			{
				item.prepTime = PrepTime;
			}
			if(!string.IsNullOrEmpty(JsonVideo))
			{
				item.video = JsonVideo;
			}

			var jsonString = new StringBuilder(JsonSerializer.Serialize(item));

			// Add on the extra stuff.
			jsonString.Insert(1, string.Format("\"author\": {{\"@type\":\"Organization\",\"name\":\"{0}\"}},",Author));
			jsonString.Insert(1, "\"@type\": \"Recipe\",");
			jsonString.Insert(1, "\"@context\": \"https://schema.org\",");
			jsonString.Insert(0, "<script type=\"application/ld+json\">");
			jsonString.Append("</script>");

			return jsonString.ToString();
		}

		public override BlogPartsOrdinal JsonId()
		{
			return BlogPartsOrdinal.RecipeId;
		}

		public override void Encoder()
		{
			ImageAltText = HttpUtility.HtmlEncode(ImageAltText);
		}

	}
	public class BlogProduct : BlogSection
	{
		public List<string> ItemNumbers { get; set; }
		public List<string> OtherItemNumbers { get; set; }
		public BlogProduct()
		{
			BlogType = BlogPartsOrdinal.Product;
			ItemNumbers = new List<string>();
			OtherItemNumbers = new List<string>();
		}
		public void Add(string itemNumber)
		{
			ItemNumbers.Add(itemNumber);
		}
		public void AddOther(string itemNumber)
		{
			OtherItemNumbers.Add(itemNumber);
		}
		public override string Html()
		{
			var html = new StringBuilder();

			html.Append("<h2 class='noPrint sectionHeader'>Products used</h2>");

			var miniLoader = new MiniLoader();
			var products = miniLoader.Load(ItemNumbers, true);

			html.Append("<div class='col-xs-12 col-lg-12 prodGrid noPrint'>");

			// This has been taken from ckMini.ascx
			foreach (var item in products)
			{
				html.Append("<div class='col-xs-12 col-sm-4 col-md-3'><div class='card'>");
				html.Append(MiniHelper.MiniLoad(item.ItemNumber, item.ItemId, item.Description, item.FriendlyUrlLink,item.MasterItemNumber,item.ImageUrlBase));
				html.AppendFormat("<p><b><a href={0}>{1}</a></b><br>", item.FriendlyUrlLink, item.Description);

				html.Append("</div></div>");
			}
			html.Append("</div>");

			// Other Products
			if (OtherItemNumbers.Count > 0)
			{
				products = miniLoader.Load(OtherItemNumbers, true);
				html.Append("<h2 class='noPrint sectionHeader'>Other Products you May Like</h2>");
				html.Append("<div class='col-xs-12 col-lg-12 prodGrid noPrint'>");

				// This has been taken from ckMini.ascx
				foreach (var item in products)
				{
					html.Append("<div class='col-lg-12'>");
					html.AppendFormat("<a href={0}>{1}</a>", item.FriendlyUrlLink, item.Description);
					html.Append("</div>");
				}
				html.Append("</div>");
			}

			return html.ToString();
		}

		public override BlogPartsOrdinal JsonId()
		{
			return BlogPartsOrdinal.Product;
		}
	}
	public class BlogKeywords : BlogSection
	{
		public string Keywords { get; set; }

		public BlogKeywords()
		{
			BlogType = BlogPartsOrdinal.Keyword;
		}

		/// <summary>
		/// NOTE: Maybe this is not needed since each class should set its type. Just let the parent return this value?
		/// </summary>
		/// <returns></returns>
		public override BlogPartsOrdinal JsonId()
		{
			return BlogPartsOrdinal.Keyword;
		}

	}
}
