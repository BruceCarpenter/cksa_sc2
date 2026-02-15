using ckLib;
using CKSA.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace CKSA.Pages.Idea
{
	/// <summary>
	/// This page circles back to itself if a value is passed in. Might need to change.
	/// </summary>
	public class IndexModel : PageModel
	{
		public IdeaModel DefaultIdeaModel { get; set; }

		public void OnGet()
		{
			try
			{
				var key = $"{CacheKeys.IdeaDefaultKey}0";

				var cacher = new PageCacher<IdeaModel>();
				DefaultIdeaModel = cacher.Retrieve(key);

				if (DefaultIdeaModel == null)
				{
					DefaultIdeaModel = new IdeaModel();

					CreateStaticPage();

					DefaultIdeaModel.MetaDescription = "Country Kitchen SweetArt library of ideas and recipes for all occasions and events.";
					DefaultIdeaModel.Parser = new UrlIdeaParser(UrlIdeaParser.Step.Type, RouteData);
					DefaultIdeaModel.Breadcrumbs = DefaultIdeaModel.Parser.GenerateListBreadcrumb(UrlIdeaParser.Step.Occasion);

					ViewData[ViewDataKeys.Title] = "Candy Making, Cookie & Cake Decorating Tips, Guides, Recipes";
					ViewData[ViewDataKeys.Canonical] = "https://www.countrykitchensa.com/idea/";
					ViewData[ViewDataKeys.Description] = DefaultIdeaModel.MetaDescription;

					cacher.Store(DefaultIdeaModel, key);
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "idea/index");
			}
		}


		/// <summary>
		/// This is data for default idea page but currently not in the database.
		/// </summary>
		private void CreateStaticPage()
		{
			DefaultIdeaModel.Data = new List<IdeaData>();

			{
				var d = new IdeaData();
				d.C = "Recipes";
				d.Url = "\"/idea/1/\"";
				d.Img = string.Format(
						"<a href={0}><img src='{1}' alt='{2}' title='{2}' /></a>",
					d.Url,
					"/homepageimages/01-rolling-cookie-dough.jpg",
					WebUtility.HtmlEncode(d.C));
				d.Desc = "";
				DefaultIdeaModel.Data.Add(d);
			}
			{
				var d = new IdeaData();
				d.C = "Charts and Useful Information";
				d.Url = "\"/idea/4/\"";
				d.Img = string.Format(
						"<a href={0}><img src='{1}' alt='{2}' title='{2}' /></a>",
					d.Url,
					"/homepageimages/01-cake-ingredients.jpg",
					WebUtility.HtmlEncode(d.C));
				d.Desc = "";
				DefaultIdeaModel.Data.Add(d);
			}
			{
				var d = new IdeaData();
				d.C = "Basic How Tos for Cake Decorating";
				d.Url = "\"idea/basic-how-tos/53/\"";
				d.Img = string.Format(
						"<a href={0}><img src='{1}' alt='{2}' title='{2}' /></a>",
					d.Url,
					"/homepageimages/baking-decorating-how-tos.jpg",
					WebUtility.HtmlEncode(d.C));
				d.Desc = "";
				DefaultIdeaModel.Data.Add(d);
			}
			{
				var d = new IdeaData();
				d.C = "Inspiration Gallery";
				d.Url = "\"/idea/3/\"";
				d.Img = string.Format(
						"<a href={0}><img src='{1}' alt='{2}' title='{2}' /></a>",
					d.Url,
					"/homepageimages/01-baby-shoe-cupcakes.jpg",
					WebUtility.HtmlEncode(d.C));
				d.Desc = "";
				DefaultIdeaModel.Data.Add(d);
			}

			// Latest link, no other way to get if using mobile.
			{
				var d = new IdeaData();
				d.C = "Just Added";
				d.Url = "\"/newestideas/\"";
				d.Img = string.Format(
						"<a href={0}><img src='{1}' alt='{2}' title='{2}' /></a>",
					d.Url,
					"/homepageimages/newest-ideas.jpg",
					WebUtility.HtmlEncode(d.C));
				d.Desc = "";
				DefaultIdeaModel.Data.Add(d);
			}

			if (Debugger.IsAttached)
			{
				foreach (var item in DefaultIdeaModel.Data)
				{
					item.Img = Regex.Replace(item.Img, @"src='(/[^']+)'", "src='https://www.countrykitchensa.com$1'");
				}
			}
		}
	}
}
