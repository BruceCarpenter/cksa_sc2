using ckLib;
using CKSA.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using System.Data;
using System.Net;
using System.Reflection.PortableExecutable;

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

			if(!CheckParameters(id, typeId, ideaId))
			{

			}	

			IdeaModel = new ckIdeaData();
			IdeaModel.Parser = new UrlIdeaParser(UrlIdeaParser.Step.Idea, RouteData);
			IdeaModel.IdeaNumber = IdeaModel.Parser.IdeaId;
			IdeaModel.Breadcrumbs = IdeaModel.Parser.GenerateListBreadcrumb(UrlIdeaParser.Step.Idea);

			// blog goes here
			if(false)
			{

			}

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

		private void CreateCanonicalLink(string url)
		{
			ViewData["Canonical"] = $"https://www.countrykitchensa.com{url[1..^1]}";
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
				using(var conn = DbDriver.OpenConnection())
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
				using(var conn = DbDriver.OpenConnection())
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
	}
}
