using ckLib;
using CKSA.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace CKSA.Pages.Idea
{
    public class MiniIdeasModel : PageModel
    {
		public IdeaModel MiniDataModel { get; set; }
		public int _OccasionId { get; set; }
		public int _TypeId { get; set; }

		private bool CheckParameters(string occasionId, string typeid)
		{
			int cTypeid = 0;

			bool isValid = int.TryParse(occasionId, out int cId) &&
				int.TryParse(typeid, out cTypeid);

			if (isValid)
			{
				_OccasionId = cId;
				_TypeId = cTypeid;
			}

			return isValid;
		}

		public void OnGet(string name, string typename, string typeid, string id)
        {
			if(!CheckParameters(id,typeid))
			{
				// do something
			}

			var doNewest = RouteData.Values.Count == 0;
			var cacher = new PageCacher<IdeaModel>();
			var key = $"{CacheKeys.IdeaMiniKey}{typeid}";

			if (doNewest)
			{
				MiniDataModel.H1Tag = "Latest Ideas";
			}
			else
			{
				MiniDataModel = new IdeaModel();
				MiniDataModel.Parser = new UrlIdeaParser(UrlIdeaParser.Step.Mini, RouteData);
				MiniDataModel.Breadcrumbs = MiniDataModel.Parser.GenerateListBreadcrumb(UrlIdeaParser.Step.Mini);
				GetData();
			}
		}


		private void GetData()
		{
			var firstRead = true;
			MiniDataModel.Data = new List<IdeaData>();
			var sql = @"Select A.IdeaID, A.Idea, C.HtmlTitle, C.HtmlDescription, C.GenDescrip  From 3idea As A
Inner Join `Ideas and types` As B On A.IdeaID = B.IdeaID
inner join 2types as C  on c.type2id = @C0
left  join blogidea as e on E.id = A.IdeaID
Where B.TypeID = @C0 and (E.Active is null or E.Active = 1)
Order By A.IdeaID DESC";

			using(var conn = DbDriver.OpenConnection())
			using (var command = conn.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = sql;

				command.Parameters.AddWithValue("@c0", MiniDataModel.Parser.TypeId);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						if (firstRead)
						{
							firstRead = false;
							var htmlTitle = reader.ReadString(2);
							if (string.IsNullOrEmpty(htmlTitle))
							{
								htmlTitle = MiniDataModel.Parser.TypeName;
							}
							ViewData["Title"] = htmlTitle;
							MiniDataModel.H1Tag = htmlTitle;
							//Page.Title = ControlChecker.CreateIdeaTitle(HtmlTitle);

							MiniDataModel.GenDescription = reader.ReadString(4);
							MiniDataModel.MetaDescription = reader.ReadString(3);
						}
						var d = new IdeaData();
						d.Id = reader.ReadInt32(0);
						d.C = reader.ReadString(1);
						d.Img = CreateImageUrl(d);
						MiniDataModel.Data.Add(d);
					}
				}
			}
		}

		private string CreateImageUrl(IdeaData c)
		{
			try
			{
				UrlIdeaParser parser = new UrlIdeaParser();
				parser.OccasionName = MiniDataModel.Parser.OccasionName;
				parser.OccasionId = MiniDataModel.Parser.OccasionId;
				parser.OccasionUrl = MiniDataModel.Parser.OccasionUrl;
				parser.TypeId = MiniDataModel.Parser.TypeId;
				parser.TypeName = MiniDataModel.Parser.TypeName;
				parser.IdeaId = c.Id;
				parser.IdeaName = c.C;
				parser.IdeaUrl = UrlBaseParser.MakeUrlFriendly(c.C);

				c.Url = parser.CreateUrl();

				//
				// Use the " for title and alt because some of the names have ' in them.
				var image = string.Format("<a href={0}><img src='{1}' alt='{2}' title='{2}' /></a>",
					c.Url,
					FileLocations.GetIdeaImagePathName350(c.Id.ToString()),
					c.C);

				return image;

			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(
					new ckExceptionData(ex, "SubCategory::CreateSubCatImage", ""));
			}

			return string.Empty;
		}

	}
}
