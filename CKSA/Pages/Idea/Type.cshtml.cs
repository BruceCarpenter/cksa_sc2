using ckLib;
using CKSA.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace CKSA.Pages.Idea
{
    public class TypeModel : PageModel
    {
		public IdeaModel TypeDataModel { get; set; }
		public string _Occasion { get; set; }
		public int _OccasionId { get; set; }

		private bool CheckParameters(string occasionId)
		{
			bool isValid = int.TryParse(occasionId, out int cId);

			if (isValid)
			{
				_OccasionId = cId;
			}

			return isValid;
		}

		public void OnGet(string name, string id)
        {
			try
			{
				CheckParameters(name);

				var key = $"{CacheKeys.IdeaTypeKey}{_OccasionId}";
				var cacher = new PageCacher<IdeaModel>();

				TypeDataModel = cacher.Retrieve(key);

				if (TypeDataModel == null)
				{
					TypeDataModel = new IdeaModel();
					TypeDataModel.Parser = new UrlIdeaParser(UrlIdeaParser.Step.Type, RouteData);

					GetData();

					TypeDataModel.Canonical = CreateCanonical(TypeDataModel.Parser.OccasionId);

					TypeDataModel.Breadcrumbs = TypeDataModel.Parser.GenerateListBreadcrumb(UrlIdeaParser.Step.Occasion);					
				}
			}
			catch (System.Exception ex)
			{
				ErrorHandler.Handle(ex, "ckideas_type.Page_Load");
				//Server.Transfer("~/catalog/shops.aspx");
			}

		}

		/// <summary>
		/// This should always be the same but someone could change the text in the url. This will force it to
		/// match what is in the database.
		/// </summary>
		private string CreateCanonical(int occId)
		{
			var canonical = Request.GetDisplayUrl();

			try
			{
				using(var conn = DbDriver.OpenConnection())
				using (var command = conn.CreateCommand())
				{
					command.CommandType = CommandType.Text;
					command.CommandText = @"select  Occasion, occ1id from 1occasion  where Occ1ID = @c0 order by occ1id limit 1";
					command.Parameters.AddWithValue("@c0", occId);

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							var parser = new UrlIdeaParser();

							parser.OccasionId = occId;
							parser.OccasionName = reader.GetString(0);
							parser.OccasionUrl = UrlBaseParser.MakeUrlFriendly(parser.OccasionName);

							canonical = "https://www.countrykitchensa.com" + parser.CreateUrl(string.Empty);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "ckideas::CreateCanonical", "", canonical));
			}

			return canonical;
		}

		private void GetData()
		{
			var imagePath = FileLocations.GetIdeaBigImagePath350();

			var sql = @"WITH RankedIdeas AS (SELECT I.IdeaID,T.Type2ID,T.Type,O.HtmlTitle,O.HtmlDescription,O.GenDescrip,ROW_NUMBER() OVER 
			(PARTITION BY T.Type2ID ORDER BY I.IdeaID DESC) AS rn
    FROM `2Types` AS T JOIN `1occasion` AS O ON O.Occ1ID = T.Occ1ID
    JOIN `Ideas and types` AS IT ON IT.TypeID = T.Type2ID
    JOIN `3idea` AS I ON I.IdeaID = IT.IdeaID WHERE T.Occ1ID = @c0)
SELECT IdeaID, Type2ID, Type, HtmlTitle, HtmlDescription, GenDescrip FROM RankedIdeas
WHERE rn = 1 ORDER BY Type2ID DESC;";

			var firstRead = true;

			using(var conn = DbDriver.OpenConnection())
			using (var command = conn.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = sql;
				command.Parameters.AddWithValue("@c0", TypeDataModel.Parser.OccasionId);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (firstRead)
						{
							firstRead = false;
							var htmlTitle = reader.ReadString(3);
							if (string.IsNullOrEmpty(htmlTitle))
							{
								htmlTitle = TypeDataModel.Parser.OccasionName;
							}
							ViewData["Title"] = htmlTitle;
							TypeDataModel.H1Tag = htmlTitle;

							TypeDataModel.GenDescription = reader.ReadString(5);
							ViewData["MetaDescription"] = reader.ReadString(4);
						}
						var d = new IdeaData();
						var ideaId = reader.ReadInt32(0);
						d.Id = reader.ReadInt32(1);
						d.C = reader.ReadString(2);
						d.Img = imagePath + ideaId + ".jpg";
						d.Img = CreateImageUrl(d);
						TypeDataModel.Data.Add(d);
					}
				}
			}
		}
		private string CreateImageUrl(IdeaData c)
		{

			UrlIdeaParser parser = new UrlIdeaParser();
			parser.OccasionName = TypeDataModel.Parser.OccasionName;
			parser.OccasionId = TypeDataModel.Parser.OccasionId;
			parser.OccasionUrl = TypeDataModel.Parser.OccasionUrl;
			parser.TypeId = c.Id;
			parser.TypeUrl = UrlBaseParser.MakeUrlFriendly(c.C);

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
	}
}
