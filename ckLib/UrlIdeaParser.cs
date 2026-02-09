using System.Text;
using System.Text.Json.Serialization;

/// <summary>
/// Summary description for UrlParser
/// </summary>
/// 
namespace ckLib
{
	public class UrlIdeaParser : UrlIdeaBaseParser
	{
		public enum Step
		{
			Occasion, // list all the occasions
			Type,
			Mini,
			Idea
		}

		#region Properties

		public Step CurrentStep { get; set; }
		public bool ValidRoute { get; set; }

		[JsonIgnore]
		public Microsoft.AspNetCore.Routing.RouteData? RouteData { get; set; }

		#endregion Properties

		public UrlIdeaParser(Step step, Microsoft.AspNetCore.Routing.RouteData route) : base()
		{
			CurrentStep = step;
			RouteData = route;

			Parse(step);

			if (ValidRoute)
			{
				GetParentIds();
			}
		}

		public UrlIdeaParser() : base()
		{
		}

		/// <summary>
		/// User got here using an old link.
		/// </summary>
		/// <param name="id"></param>
		public void ParseStandardMethod()
		{
			if (ValidRoute == false)
			{
				switch (CurrentStep)
				{
					case Step.Occasion:
						OccasionName = GetOccasionName();
						break;
				}
				GetParentIds();
			}
		}

		public Dictionary<string, string> GenerateListBreadcrumb(Step step)
		{
			var crumbs = new Dictionary<string, string>();

			try
			{
				crumbs["Home"] = "/";
				if (step >= Step.Occasion)
				{
					crumbs["Ideas"] = "/idea/";

					if (string.IsNullOrEmpty(OccasionUrl))
					{
						return crumbs;
					}
					crumbs[OccasionName] = $"{UrlIdeaParser.UrlPath}/{OccasionUrl}/{OccasionId}/";
				}

				if (step >= Step.Type)
				{
					crumbs[TypeName] = $"{UrlIdeaParser.UrlPath}/{OccasionUrl}/{TypeUrl}/{OccasionId}/{TypeId}/";
				}

				if (step >= Step.Idea)
				{
					crumbs[IdeaName] = $"{UrlIdeaParser.UrlPath}/{OccasionUrl}/{IdeaUrl}/{OccasionId}/{TypeId}/{IdeaId}/";
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "GenerateListBreadcrumb");
			}

			return crumbs;
		}

		public string GenerateJsonBreadcrumb(Step step)
		{
			StringBuilder test = new StringBuilder("[{\"name\":\"Home\",\"link\":\"/\"}");

			try
			{
				if (step >= Step.Occasion)
				{
					test.Append(",{\"name\":\"Ideas\",\"link\":\"/idea/\"}");

					if (string.IsNullOrEmpty(OccasionUrl))
					{
						test.Append("]");
						return test.ToString();
					}

					test.Append(",{\"name\":\"");
					test.Append(string.Format("{0}\",\"link\":\"", OccasionName));
					test.Append(string.Format("{0}/{1}/{2}/\"", UrlIdeaParser.UrlPath, OccasionUrl, OccasionId));
					test.Append("}");
				}

				if (step >= Step.Type)
				{
					test.Append(",{\"name\":\"");
					test.Append(string.Format("{0}\",\"link\":\"", TypeName));
					test.Append(string.Format("{0}/{1}/{2}/{3}/{4}\"", UrlIdeaParser.UrlPath, OccasionUrl, TypeUrl, OccasionId, TypeId));
					test.Append("}");
				}

				if (step >= Step.Idea)
				{
					test.Append(",{\"name\":\"");
					test.Append(string.Format("{0}\",\"link\":\"", IdeaName));
					test.Append(string.Format("{0}/{1}/{2}/{3}/{4}/{5}\"", UrlIdeaParser.UrlPath, OccasionUrl, IdeaUrl, OccasionId, TypeId, IdeaId));
					test.Append("}");
				}

				test.Append("]");
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "GenerateJsonBreadcrumb");
			}

			return test.ToString();
		}

		protected bool IsValidRoute()
		{
			if (string.IsNullOrEmpty(OccasionUrl))
			{
				return false;
			}

			return OccasionId != 0;
		}

		protected void Parse(Step step)
		{
			try
			{
				OccasionUrl = GetIfContains("name");
				OccasionId = GetIfContainsInt("id");

				TypeUrl = GetIfContains("typename");
				TypeId = GetIfContainsInt("typeid");

				IdeaUrl = GetIfContains("description");
				IdeaId = GetIfContainsInt("ideaid");

				ValidRoute = IsValidRoute();

			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "Parse");
			}
		}

		protected string GetIfContains(string key)
		{
			if (RouteData != null && RouteData.Values.TryGetValue(key, out var value))
			{
				return value?.ToString() ?? string.Empty;
			}
			return string.Empty;
		}

		protected int GetIfContainsInt(string key)
		{
			if (RouteData?.Values.TryGetValue(key, out var value) == true && value != null)
			{
				if (int.TryParse(value.ToString(), out int result))
				{
					return result;
				}
			}

			return 0;
		}
		/// <summary>
		/// TODO: Might not be used.
		/// Maybe an email here to see if this is ever called. Same with UrlProductParser:GetShopName()?
		/// </summary>
		/// <returns></returns>
		public string GetOccasionName()
		{
			var occasionName = "Unknown Shop";

			try
			{
				if (OccasionId !=0 )
				{
					using(var conn = DbDriver.OpenConnection())
					using (var command = conn.CreateCommand())
					{
						command.CommandText = "Select `Occasion` from 1occassion where Occ1ID=@c0";
						command.Parameters.AddWithValue("@c0", OccasionId);
						using (var reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								occasionName = reader.ReadString("category 1");
								OccasionUrl = MakeUrlFriendly(occasionName);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "GetOccasionName");
			}

			return occasionName;
		}
	}
}