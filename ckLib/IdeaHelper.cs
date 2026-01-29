/// <summary>
/// Summary description for IdeaHelper
/// </summary>
namespace ckLib
{
	public class IdeaHelper
	{
		public class IdeaPiece
		{
			public string Name { get; set; } = string.Empty;
			public DateTime end { get; set; }
			public string URL { get; set; } = string.Empty;
			public string img { get; set; } = string.Empty;
		}


		
        /// <summary>
        /// Using IdeaPiece class to get the latest blog.
        /// </summary>
        static public IdeaPiece LatestBlog()
        {
            IdeaPiece ideaPiece = new IdeaPiece();
			IdeaPiece latestBlog = ideaPiece;
            //var dynamicBlog = BlogHelper.LatestBlog();

            //if (dynamicBlog != null)
            //{
            //    latestBlog.Name = "Latest Blog";
            //    latestBlog.URL = dynamicBlog.Url;
            //    latestBlog.img = dynamicBlog.Mini;
            //}

            return latestBlog;
        }
/*
        /// <summary>
        /// Get the latest idea added.
        /// </summary>
        static public IdeaPiece Latest()
        {
            var ideaPiece = new IdeaPiece();

            try
            {
                using (var connection = CKDefines.OpenConnection())
                {
                    var id = string.Empty;

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "select IdeaId, Idea from 3idea where Idea is not null and IdeaId != 3100 order by ideaID desc limit 0,1 ";
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                id = reader.GetString(0);
                                ideaPiece.Name = reader.GetString(1);
                                ideaPiece.img = FileLocations.GetIdeaImagePathName350(id);
                            }
                        }
                    }
                    ideaPiece.URL = CreateUrl(id);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(new ckExceptionData(ex, "IdeaHeler:Latest", string.Empty));
            }

            return ideaPiece;
        }

        static public string CreateUrl(string id)
        {
            using (var connection = new MySQLHelper(MySQLHelper.DataBases.pricelist))
            {
                return CreateUrl(connection, id);
            }
        }

        /// <summary>
        /// Return the url of the passed in id.
        /// TODO: Use the new version in shared.
        /// </summary>
        static public string CreateUrl(MySQLHelper connection, string id)
        {
            var sql = @"select b.idea, b.IdeaID, c.Type, type2id, d.Occasion, d.occ1id from `ideas and types` as a
				inner join 3idea as b on a.IdeaID = b.IdeaId 
				inner join 2Types as c on a.TypeID = c.Type2ID
				inner join 1occasion as d on d.occ1id = c.Occ1ID
				where b.ideaID = @c0
				order by type2id limit 1";
            var url = string.Empty;

            try
            {
                using (var command = connection.Connection().CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@c0", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var parser = new UrlIdeaParser();
                            parser.IdeaId = id;
                            parser.IdeaName = reader.GetString(0);
                            parser.IdeaUrl = UrlBaseParser.MakeUrlFriendly(parser.IdeaName);

                            parser.OccasionId = reader.GetString(5);
                            parser.OccasionName = reader.GetString(4);
                            parser.OccasionUrl = UrlBaseParser.MakeUrlFriendly(parser.OccasionName);

                            parser.TypeId = reader.GetString(3);
                            parser.TypeName = reader.GetString(2);
                            parser.TypeUrl = UrlBaseParser.MakeUrlFriendly(parser.TypeName);

                            url = parser.CreateUrl();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(new ckExceptionData(ex, "IdeaHeler:CreateUrl", id));
            }

            return url;
        }
        */
	}
}