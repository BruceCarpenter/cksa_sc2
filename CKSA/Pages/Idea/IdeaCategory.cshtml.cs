using ckLib;
using CKSA.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CKSA.Pages.Idea
{
	public class IdeaCategoryModel : PageModel
	{
		public IdeaModel DefaultIdeaModel { get; set; }

		public int _IdeaId { get; set; }

		public IdeaCategoryModel()
		{
			DefaultIdeaModel = new IdeaModel();
		}

		private bool CheckParameters(string ideaId)
		{
			bool isValid = int.TryParse(ideaId, out int cId);

			if (isValid)
			{
				_IdeaId = cId;
			}

			return isValid;
		}

		public void OnGet(string id)
		{
			try
			{
				if(!CheckParameters(id))
				{
					// return to error page.
				}

				var key = $"{CacheKeys.IdeaDefaultKey}{_IdeaId}";

				var cacher = new PageCacher<IdeaModel>();
				DefaultIdeaModel = cacher.Retrieve(key);

				if (DefaultIdeaModel == null)
				{
					DefaultIdeaModel = new IdeaModel();
					DefaultIdeaModel.Parser = new UrlIdeaParser(UrlIdeaParser.Step.Type, RouteData);

					//
					// Special case to add some aspx pages to list.
					var crumbName = string.Empty;
					switch (_IdeaId)
					{
						case 1:
							crumbName = "Cake";
							DefaultIdeaModel.H1Tag = "Candy Making, Cookie & Cake Decorating Tips & Recipes.";
							break;
						case 3:
							crumbName = "Birthday";
							DefaultIdeaModel.H1Tag = "Cake, Candy & Cookie Inspiration Gallery";
							break;
						case 4:
							crumbName = "Information";
							DefaultIdeaModel.H1Tag = "Candy Making, Cookie & Cake Decorating Guides ";
							AddStaticPages(_IdeaId);
							break;
					}


					DefaultIdeaModel.Breadcrumbs = DefaultIdeaModel.Parser.GenerateListBreadcrumb(UrlIdeaParser.Step.Occasion);
					DefaultIdeaModel.Breadcrumbs[crumbName] = DefaultIdeaModel.Breadcrumbs["Ideas"] + id + "/";
					ViewData["Canonical"] = $"https://www.countrykitchensa.com/idea/{_IdeaId}/";
					CreateDbPage();
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "idea/ideaCategory");
			}
		}

		private void AddStaticPages(int id)
		{
			{
				var d = new IdeaData();
				d.C = "Cake Charts";
				d.Url = "/ckideas/cakechart.aspx";
				d.Img = string.Format(
						"<a href='{0}'><img src='{1}' srcset='{1} 1500w, /images/black-small.jpg 991w' sizes='100%' alt='{2}' title='{2}' /></a>",
					d.Url,
					"/ckideas/charts.jpg",
					d.C);
				d.Desc = "";
				DefaultIdeaModel.Data.Add(d);
			}
			{
				var d = new IdeaData();
				d.C = "Culinary Guide";
				d.Url = "/culinary_basics/culinary_guides.aspx";
				d.Img = string.Format(
						"<a href='{0}'><img src='{1}' srcset='{1} 1500w, /images/black-small.jpg 991w' sizes='100%' alt='{2}' title='{2}' /></a>",
					d.Url,
					"/ckideas/guides.jpg",
					d.C);
				d.Desc = "";
				DefaultIdeaModel.Data.Add(d);
			}
		}

		private string CreateImageUrl(IdeaData c)
		{
			UrlIdeaParser parser = new UrlIdeaParser();
			parser.OccasionName = c.C;
			parser.OccasionId = c.Id;
			parser.OccasionUrl = UrlBaseParser.MakeUrlFriendly(c.C);

			c.Url = parser.CreateUrl();

			//
			// Use the " for title and alt because some of the names have ' in them.
			var image = string.Format(
				"<a href={0}><img src='{1}' alt='{2}' title='{2}' /></a>",
			c.Url,
			c.Img.ToLower(),
			c.C);

			return image;
		}

		/// <summary>
		/// Display the data for the passed in category id (1occasion.CategoryId).
		/// </summary>
		private void CreateDbPage()
		{
			var imagePath = FileLocations.GetIdeaBigImagePath350();

			using var conn = DbDriver.OpenConnection();
			using (var command = conn.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = @"SELECT A.Occ1Id, MAX(A.Occasion) AS Occasion, MAX(B.Type2ID) AS Type2ID, MAX(D.IdeaID) AS IdeaID
					FROM 1occasion AS A
					INNER JOIN 2types AS B ON A.Occ1ID = B.Occ1ID
					INNER JOIN `Ideas and types` AS C ON C.TypeId = B.Type2ID
					INNER JOIN 3idea AS D ON D.IdeaId = C.IdeaID
					WHERE A.CategoryId = @c0 
					GROUP BY A.Occ1Id
					ORDER BY A.Occ1Id DESC;";
				command.Parameters.AddWithValue("@c0", _IdeaId);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var d = new IdeaData();
						d.Id = reader.ReadInt32(0);
						d.C = reader.ReadString(1);
						var type2Id = reader.ReadInt32(2);
						var ideaId = reader.ReadInt32(3);
						d.Img = imagePath + ideaId + ".jpg";

						d.Img = CreateImageUrl(d);

						DefaultIdeaModel.Data.Add(d);
					}
				}
			}
		}
	}
}