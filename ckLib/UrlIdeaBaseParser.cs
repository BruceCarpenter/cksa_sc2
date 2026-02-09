namespace ckLib
{
	public class UrlIdeaBaseParser : UrlBaseParser
	{
		#region Properties

		public string OccasionName { get; set; }
		public string OccasionUrl { get; set; }
		public int OccasionId { get; set; }

		public string TypeName { get; set; }
		public string TypeUrl { get; set; }
		public int TypeId { get; set; }

		public string IdeaName { get; set; }
		public string IdeaUrl { get; set; }
		public int IdeaId { get; set; }

		static public string UrlPath = "/idea";

		#endregion Properties

		public UrlIdeaBaseParser()
		{
			Clear();
		}

		public void Clear()
		{
			OccasionName = string.Empty;
			OccasionUrl = string.Empty;
			OccasionId = 0;

			TypeName = string.Empty;
			TypeUrl = string.Empty;
			TypeId = 0;

			IdeaName = string.Empty;
			IdeaUrl = string.Empty;
			IdeaId = 0;
		}

		#region Public

		/// <summary>
		/// Create the final Url based off of the id.
		/// This is duplicated from website IdeaHelper.CreateUrl. Eventuallly have website use this call
		/// and delete IdeaHelper version.
		/// </summary>
		//    public static string CreateUrl(string id)
		//    {
		//        var sql = @"select b.idea, b.IdeaID, c.Type, type2id, d.Occasion, d.occ1id from `ideas and types` as a
		//inner join 3idea as b on a.IdeaID = b.IdeaId 
		//inner join 2Types as c on a.TypeID = c.Type2ID
		//inner join 1occasion as d on d.occ1id = c.Occ1ID
		//where b.ideaID = @c0
		//order by type2id limit 1";
		//        var url = string.Empty;

		//        try
		//        {
		//            using(var connection = MySQLHelper.OpenConnection())
		//            using (var command = connection.CreateCommand())
		//            {
		//                command.CommandText = sql;
		//                command.Parameters.AddWithValue("@c0", id);

		//                using (var reader = command.ExecuteReader())
		//                {
		//                    if (reader.Read())
		//                    {
		//                        var parser = new UrlIdeaParser();
		//                        parser.IdeaId = id;
		//                        parser.IdeaName = reader.GetString(0);
		//                        parser.IdeaUrl = UrlBaseParser.MakeUrlFriendly(parser.IdeaName);

		//                        parser.OccasionId = reader.GetString(5);
		//                        parser.OccasionName = reader.GetString(4);
		//                        parser.OccasionUrl = UrlBaseParser.MakeUrlFriendly(parser.OccasionName);

		//                        parser.TypeId = reader.GetString(3);
		//                        parser.TypeName = reader.GetString(2);
		//                        parser.TypeUrl = UrlBaseParser.MakeUrlFriendly(parser.TypeName);

		//                        url = parser.CreateUrl();
		//                    }
		//                }
		//            }
		//        }
		//        catch (Exception ex)
		//        {
		//            ErrorHandler.Handle(new ckExceptionData(ex, "UrlIdeaBaseParser:CreateUrl", id));
		//        }

		//        return url;
		//    }

		public void GetParentIds()
		{
			try
			{
				if (GetFirstParamValue(IdeaName, IdeaId.ToString()))
				{
					IdeaName = GetValue("Select Idea from 3idea where IdeaID=@c0", IdeaId.ToString(), "Idea");
					IdeaUrl = MakeUrlFriendly(IdeaName);
				}

				if (GetFirstParamValue(TypeId.ToString(), IdeaId.ToString()))
				{
					TypeId = GetValueInt("Select TypeID from `ideas and types` where IdeaID=@c0 order by typeid", IdeaId.ToString(), "TypeID");
				}

				if (GetFirstParamValue(TypeName, TypeId.ToString()))
				{
					TypeName = GetValue("Select Type from 2types where Type2ID=@c0", TypeId.ToString(), "Type");
					TypeUrl = MakeUrlFriendly(TypeName);
				}

				if (GetFirstParamValue(OccasionId.ToString(), TypeId.ToString()))
				{
					OccasionId = GetValueInt("Select Occ1ID from `2types` where Type2ID=@c0 order by Occ1ID", TypeId.ToString(), "Occ1ID");
				}

				if (GetFirstParamValue(OccasionName, OccasionId.ToString()))
				{
					OccasionName = GetValue("Select Occasion from 1occasion where Occ1ID=@c0", OccasionId.ToString(), "Occasion");
					OccasionUrl = MakeUrlFriendly(OccasionName);
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "GetParentIds");
			}
		}

		public bool IsValid()
		{
			bool isValid = false;

			if (IdeaId != 0)
			{
				isValid = true;
			}
			else if (TypeId != 0)
			{
				isValid = true;
			}
			else if (OccasionId != 0)
			{
				isValid = true;
			}

			return isValid;
		}

		public string CreateUrl()
		{
			return CreateUrl("\"");
		}

		public string CreateUrl(string quote)
		{
			string url = string.Empty;

			if (IdeaId != 0)
			{
				url = string.Format("{6}{0}/{1}/{2}/{3}/{4}/{5}/{6}",
								UrlPath,
								OccasionUrl,
								IdeaUrl,
								OccasionId,
								TypeId,
								IdeaId,
								quote);
			}
			else if (TypeId != 0)
			{
				url = string.Format("{5}{0}/{1}/{2}/{3}/{4}/{5}",
									UrlPath,
									OccasionUrl,
									TypeUrl,
									OccasionId,
									TypeId,
									quote);
			}
			else if (OccasionId != 0)
			{
				url = string.Format("{3}{0}/{1}/{2}/{3}",
					UrlPath, OccasionUrl, OccasionId, quote);
			}

			return url;
		}

		#endregion Public

	}
}
