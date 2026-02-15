using ckLib;
using CKSA.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Net;

namespace CKSA.Pages.Idea
{
	public class ckIdeaModel : PageModel
	{
		public ckIdeaData IdeaModel { get; set; }
		public int _OccasionId { get; set; }
		public int _TypeId { get; set; }
		public int _IdeaId { get; set; }
		public MiniImageModel _MiniImageModel { get; set; }
		private readonly CookieHelper _cookieHelper;
		public Blog TheBlog { get; set; }
		public string FB { get; set; }
		public string FbTitle { get; set; }
		public string Description { get; set; }
		public string Canonical { get; set; }

		public ckIdeaModel(CookieHelper cookieHelper)
		{
			_cookieHelper = cookieHelper;
		}

		private bool CheckParameters(string occasionId, string typeid, string ideaIs)
		{
			int cTypeid = 0;
			int cIdeaid = 0;

			bool isValid = int.TryParse(occasionId, out int cId) &&
				int.TryParse(typeid, out cTypeid) &&
				int.TryParse(ideaIs, out cIdeaid);

			if (isValid)
			{
				_OccasionId = cId;
				_TypeId = cTypeid;
				_IdeaId = cIdeaid;
			}

			return isValid;
		}

		/// <summary>
		/// id is occasionId but to change would require change in parser and all previous pages.
		/// </summary>
		public void OnGet(string name, string typename, string id, string typeId, string ideaId)
		{

			if (!CheckParameters(id, typeId, ideaId))
			{

			}

			IdeaModel = new ckIdeaData();
			IdeaModel.Parser = new UrlIdeaParser(UrlIdeaParser.Step.Idea, RouteData);
			IdeaModel.IdeaNumber = IdeaModel.Parser.IdeaId;
			IdeaModel.Breadcrumbs = IdeaModel.Parser.GenerateListBreadcrumb(UrlIdeaParser.Step.Idea);

			// blog goes here
			if (BlogHelper.DoesExist(string.Empty, BlogHelper.BlogType.Idea, Convert.ToInt32(IdeaModel.IdeaNumber)))
			{
				TheBlog = BlogHelper.Load(string.Empty, BlogHelper.BlogType.Idea, Convert.ToInt32(IdeaModel.IdeaNumber));
				if (TheBlog != null)
				{
					CreateCanonicalLink(IdeaHelper.CreateUrl(IdeaModel.Parser.IdeaId));

					ViewData[ViewDataKeys.Title] = TheBlog.Title;
					ViewData[ViewDataKeys.Description] = TheBlog.MetaDescription;
					ViewData[ViewDataKeys.Canonical] = Canonical;

					var canonical = IdeaHelper.CreateUrl(IdeaModel.Parser.IdeaId);
					if (!string.IsNullOrEmpty(canonical))
					{
						canonical = canonical.Substring(1, canonical.Length - 2);
						CreateCanonicalLink(canonical);
					}

					// Set open graph information
					FbTitle = TheBlog.Title;
					FB = canonical;
					Description = CkDefines.OgDescription(TheBlog.MetaDescription);

					return;
				}
			}
			else
			{
				IdeaModel.ImageUrl = CkDefines.IdeaImagePath(IdeaModel.Parser.IdeaId);
				IdeaModel.CurrentUrl = Request.GetDisplayUrl();

				if (SetupSection_1(IdeaModel.Parser.IdeaId))
				{
					OutputOtherSupplies(IdeaModel.Parser.IdeaId);

					SocialMediaContent(IdeaModel.Parser);

					IdeaModel.Videos = GetVideos.GetVideosForIdeas(IdeaModel.Parser.IdeaId);

					ViewData["MetaDescription"] = "Learn how to make " + IdeaModel.TitleTxt;

					CreateCanonicalLink(IdeaHelper.CreateUrl(IdeaModel.Parser.IdeaId));
				}
				else
				{
					// No idea found - redirect to previous page
				}

				_MiniImageModel = new MiniImageModel
				{
					RunAs = 2,
					Id = IdeaModel.Parser.IdeaId,
					WholeSaleCustomer = _cookieHelper.GetWholesaleValue()
				};
				_MiniImageModel.LoadProduct();
			}
		}

		public string JsonRecipe()
		{
			var json = string.Empty;

			//if (TheBlog != null)
			//{
			//	foreach (var secton in TheBlog.BlogSections)
			//	{
			//		if (secton.BlogType == Blogger.BlogPartsOrdinal.RecipeId)
			//		{
			//			var recipeSection = (BlogRecipe)secton;
			//			json = recipeSection.Json();
			//			break;
			//		}
			//	}
			//}

			return json;
		}
		private void CreateCanonicalLink(string url)
		{
			Canonical = $"https://www.countrykitchensa.com{url[1..^1]}";
		}

		private void SocialMediaContent(UrlIdeaParser parser)
		{
			// Facebook
			string ideaUrl = parser.CreateUrl(string.Empty);
			IdeaModel.FbTitle = parser.IdeaName;
			IdeaModel.FB = "https://www.countrykitchensa.com" + ideaUrl;
			IdeaModel.Description = CkDefines.OgDescription(parser.IdeaName);

			// Pinterest
			string pinUrl = WebUtility.UrlEncode(parser.CreateUrl(string.Empty));
			string imageUrl = "https://www.countrykitchensa.com/" + WebUtility.UrlEncode(IdeaModel.ImageUrl);
			IdeaModel.Pinit = CkDefines.CreatePinIt(pinUrl, imageUrl);
		}

		protected void OutputOtherSupplies(int ideaId)
		{
			try
			{
				using (var conn = DbDriver.OpenConnection())
				using (var command = conn.CreateCommand())
				{
					command.CommandType = CommandType.Text;
					command.CommandText = "Select * from 3idea where IdeaID = @c0";
					command.Parameters.AddWithValue("@c0", ideaId);
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							// this table has a bunch of columns with ingrediants
							// loop through them all getting ingredients.
							for (int i = 4; i < 31; i++)
							{
								var columnValue = reader.ReadString(i);
								if (string.IsNullOrEmpty(columnValue))
								{
									break;
								}

								IdeaModel.Ingredients.Add(columnValue);
							}
						}
					}
				}
			}
			catch
			{
				// dont care
			}
		}

		protected bool SetupSection_1(int ideaId)
		{
			var ideaFound = false;

			try
			{
				using (var conn = DbDriver.OpenConnection())
				using (var command = conn.CreateCommand())
				{
					command.CommandType = CommandType.Text;
					command.CommandText = "Select Idea, Instructions, HtmlTitle, HtmlDescription, GenDescrip from 3idea where IdeaID = @c0";
					command.Parameters.AddWithValue("@c0", ideaId);
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							IdeaModel.TitleTxt = reader.ReadString("HtmlTitle");
							if (string.IsNullOrEmpty(IdeaModel.TitleTxt))
							{
								IdeaModel.TitleTxt = reader.ReadString("Idea");
							}
							var description = reader.ReadString("HtmlDescription");
							if (!string.IsNullOrEmpty(description))
							{
								//ViewData["MetaDescription"] = description;
							}
							IdeaModel.GenDescription = reader.ReadString("GenDescrip");
							ViewData["Title"] = IdeaModel.TitleTxt;
							IdeaModel.Instructions = reader.ReadString("Instructions");
							ideaFound = true;
						}
					}

					if (ideaFound)
					{
						// get alt images
						command.Parameters.Clear();
						command.CommandText = "select AltImageName from ideaaltimage where IdeaId = @c0";
						command.Parameters.AddWithValue("@c0", ideaId);
						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								var path = reader.GetString(0);
								var miniPath = path.Replace("big", "68");
								IdeaModel.AltImages.Add(path);
								IdeaModel.MiniAltImages.Add(miniPath);
							}

							// Only add the original if there are other items.
							if (IdeaModel.AltImages.Count > 0)
							{
								IdeaModel.AltImages.Insert(0, IdeaModel.ImageUrl);
								IdeaModel.MiniAltImages.Insert(0, IdeaModel.ImageUrl);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "ckideas::SetupSection_1", "", Request.GetDisplayUrl()));
			}

			return ideaFound;
		}

		public string EmailBodyText()
		{
			return string.Format(@"I couldn't resist forwarding it to you because I know how passionate you are about baking and creating sweet treats. I'm sure you'll find plenty of inspiration and new ideas to try out in your own kitchen!
			&nbsp;&nbsp;To read the blog post, simply click on the link below:%0d%0a
			{0}%0d%0a
			If you enjoy it as much as I did, I encourage you to share it with your friends and fellow baking enthusiasts.Together, we can spread the joy and love for baking!", Canonical);
		}
	}
}
