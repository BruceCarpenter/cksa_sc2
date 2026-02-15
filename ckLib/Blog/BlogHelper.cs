using System.Text.Json;
using System.Text.RegularExpressions;

/// <summary>
/// The Blog structure will be based on the following 3 tables:
/// Blog - The main blog data for a blog.
/// NOTE: This table might bot be needed. Doesnt look like it is giving me anything.
/// BlogSectionFormat - This describes the layout of each blog section. Current thinking is
/// 1  2 These are images
///   3 This is text
/// If just 1 image than the image will be centered.
/// BlogPart - This is an individual blog section. The actual blog data.
/// </summary>
namespace ckLib
{
	public class BlogHelper
	{
		public enum BlogType
		{
			Standard,
			Idea
		}

		public BlogHelper()
		{
		}

		/// <summary>
		/// Create the Blog object straight from json. This can be used if the UI program sends a blog request to the website.
		/// Save this into the Cache and have it loaded up later.
		/// </summary>
		public static Blog BlogFromJson(string json)
		{
			var d = JsonSerializer.Deserialize<dynamic>(json);
			var mainJson = d["Main"]["Json"];
			var blogMain = JsonSerializer.Deserialize<BlogMain>(mainJson);
			var blogPieces = d["Parts"];

			var blog = new Blog
			{
				Id = d["Main"]["Id"],
				Url = d["Main"]["Url"],
				Mini = d["Main"]["Mini"],
				Date = d["Main"]["Date"],
				Active = d["Main"]["Active"],
				Name = blogMain.Name,
				Title = blogMain.PageTitle,
				Description = blogMain.Description,
				MetaDescription = blogMain.MetaDescription,
				Image = blogMain.Image,
				ImageAltText = blogMain.ImageAltText,
				Json = mainJson
			};

			blog.BlogTypeValue = (Blog.BlogType)Enum.Parse(typeof(Blog.BlogType), d["BlogType"]);

			// Load the pieces
			foreach (var piece in blogPieces)
			{
				var jsonType = (BlogPartsOrdinal)Enum.Parse(typeof(BlogPartsOrdinal), piece["BlogType"].ToString());

				if (jsonType == BlogPartsOrdinal.StandardImageId)
				{
					blog.BlogSections.Add(new BlogImages
					{
						Id = piece["Id"],
						OrderNumber = piece["OrderNumber"],
						Header = piece["Header"],
						ImageOne = piece["ImageOne"],
						ImageOneLink = piece["ImageOneLink"],
						ImageOneAltText = piece["ImageOneAltText"],
						ImageOneAttribute = piece["ImageOneAttribute"],
						ImageOneAttributeLink = piece["ImageOneAttributeLink"],
						ImageTwo = piece["ImageTwo"],
						ImageTwoLink = piece["ImageTwoLink"],
						ImageTwoAltText = piece["ImageTwoAltText"],
						ImageTwoAttribute = piece["ImageTwoAttribute"],
						ImageTwoAttributeLink = piece["ImageTwoAttributeLink"],
						Description = piece["Description"]
					});
				}
				else if (jsonType == BlogPartsOrdinal.RecipeId)
				{
					var ingredients = new List<object>(piece["Ingredients"]);
					blog.BlogSections.Add(new BlogRecipe
					{
						Id = piece["Id"],
						OrderNumber = piece["OrderNumber"],
						Name = piece["Name"],
						Author = piece["Author"],
						DatePublished = DateTime.Parse(piece["DatePublished"]),
						ImageUrl = piece["ImageUrl"],
						ImageAltText = piece["ImageAltText"],
						Ingredients = ingredients.ConvertAll(x => x.ToString()),
						PrepTime = piece["PrepTime"],
						Yield = piece["Yield"],
						Instructions = piece["Instructions"],
						Description = piece["Description"]
					});
				}
				else if (jsonType == BlogPartsOrdinal.Product)
				{
					var ingredients = new List<object>(piece["ItemNumbers"]);
					blog.BlogSections.Add(new BlogProduct
					{
						Id = piece["Id"],
						OrderNumber = piece["OrderNumber"],
						Description = piece["Description"],
						ItemNumbers = ingredients.ConvertAll(x => x.ToString()),
						OtherItemNumbers = ingredients.ConvertAll(x => x.ToString()),
					});
				}
				else if (jsonType == BlogPartsOrdinal.Keyword)
				{
					var keywords = (piece["Keywords"]) as String;
					blog.BlogSections.Add(new BlogKeywords
					{
						Id = piece["Id"],
						OrderNumber = piece["OrderNumber"],
						Keywords = keywords
					});
				}
			}

			return blog;
		}

		/// <summary>
		/// Get the type this idea belongs in.
		/// </summary>
		public static int BlogIdeaType(string json)
		{
			var typeId = int.MinValue;
			var d = JsonSerializer.Deserialize<dynamic>(json);

			typeId = d["TypeId"];

			return typeId;
		}

		public static bool DoesExist(string url, BlogType typeToLoad, int id = int.MinValue)
		{
			var exists = false;
			var blogTableName = "Blog";

			try
			{
				if (typeToLoad == BlogType.Idea)
				{
					blogTableName = "BlogIdea";
				}

				// Load in the main blog section.
				using (var mySQL = DbDriver.OpenConnection())
				{
					using (var command = mySQL.CreateCommand())
					{
						if (id == int.MinValue)
						{
							command.CommandText = string.Format("select Id, Json from {0} where lower(Url)=@c0", blogTableName);
							command.Parameters.AddWithValue("@c0", url.ToLower());
						}
						else
						{
							// Load via id.
							command.CommandText = string.Format("select Id, Json from {0} where id=@c0", blogTableName);
							command.Parameters.AddWithValue("@c0", id);
						}

						using (var reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								exists = true;
							}
						}
					}
				}
			}
			catch
			{
			}

			return exists;
		}

		/// <summary>
		/// Load the blog from the database using the url as the key.
		/// </summary>
		public static Blog Load(string url, BlogType typeToLoad, int id = int.MinValue)
		{
			Blog blog = null;
			var blogTableName = "Blog";
			var blogPartTableName = "BlogPart";

			try
			{
				if (typeToLoad == BlogType.Idea)
				{
					blogTableName = "BlogIdea";
					blogPartTableName = "BlogPartIdea";
				}

				// Load in the main blog section.
				using (var mySQL = DbDriver.OpenConnection())
				{
					using (var command = mySQL.CreateCommand())
					{
						if (id == int.MinValue)
						{
							if (!BlogHelper.ValidBlogUrl(url))
							{
								return null;
							}

							command.CommandText = string.Format("select Id, Json from {0} where lower(Url)=@c0", blogTableName);
							command.Parameters.AddWithValue("@c0", url.ToLower());
						}
						else
						{
							// Load via id.
							command.CommandText = string.Format("select Id, Json from {0} where id=@c0", blogTableName);
							command.Parameters.AddWithValue("@c0", id);
						}

						using (var reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								var blogSection = JsonSerializer.Deserialize<BlogMain>(reader.ReadString(1));
								blogSection.Encoder();

								blog = new Blog
								{
									Id = reader.ReadInt32(0),
									Url = url.ToLower(),
									Name = blogSection.Name,
									Title = blogSection.PageTitle,
									Description = blogSection.Description,
									MetaDescription = blogSection.MetaDescription,
									Image = blogSection.Image,
									ImageAltText = blogSection.ImageAltText
								};

							}
							else
							{
								throw new Exception(string.Format("The blog {0} was not loaded.", url));
							}
						}
					}

					// Load in the blog parts
					using (var command = mySQL.CreateCommand())
					{
						command.CommandText = string.Format("select Json,JsonType from {0} where BlogId=@c0 order by BlogSectionId", blogPartTableName);
						command.Parameters.AddWithValue("@c0", blog.Id);

						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								var json = reader.ReadString(0);
								var jsonType = (BlogPartsOrdinal)Enum.Parse(typeof(BlogPartsOrdinal), reader.ReadInt32(1).ToString());

								BlogSection blogSection = null;

								if (jsonType == BlogPartsOrdinal.StandardImageId)
								{
									blogSection = JsonSerializer.Deserialize<BlogImages>(json);
								}
								else if (jsonType == BlogPartsOrdinal.RecipeId)
								{
									blogSection = JsonSerializer.Deserialize<BlogRecipe>(json);
								}
								else if (jsonType == BlogPartsOrdinal.Product)
								{
									blogSection = JsonSerializer.Deserialize<BlogProduct>(json);
								}

								if (blogSection != null)
								{
									blogSection.MainBlog = blog.MainSection;
									blogSection.Encoder();
									blog.BlogSections.Add(blogSection);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "BlogHelper:Load()");
			}

			return blog;
		}

		/// <summary>
		/// Verify if the passed in url is a valid blog. Getting a lot of spam bots hitting this.
		/// Valid is Alpha, numeric and -
		/// </summary>
		public static bool ValidBlogUrl(string blogUrl)
		{
			string pattern = "^[a-zA-Z0-9-]*$";

			bool isAlphaNumeric = Regex.IsMatch(blogUrl, pattern);

			return isAlphaNumeric;
		}

		public static dynamic LatestBlog()
		{
			try
			{
				// Load in the main blog section.
				using (var mySQL = DbDriver.OpenConnection())
				{
					using (var command = mySQL.CreateCommand())
					{
						command.CommandText = "select Id, Url, Mini, Json from Blog where active=1 order by Id desc Limit 1";

						using (var reader = command.ExecuteReader())
						{
							if (reader.Read())
							{
								var blog = new
								{
									Id = reader.ReadString(0),
									Url = "blog/" + reader.ReadString(1) + "/",
									Mini = reader.ReadString(2),
								};

								return blog;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "BlogHelper:LatestBlog()");
			}

			return null;
		}

		/// <summary>
		/// Setup the blog tables.
		/// </summary>
		//public static void CreateTables()
		//{
		//	try
		//	{
		//		{
		//			//mySQL.ExecuteSQL("drop table Blog if exists");
		//			//mySQL.ExecuteSQL("drop table BlogPart if exists");
		//			var createBlog = @"CREATE TABLE `blog` (
		//				  `Id` int(11) NOT NULL AUTO_INCREMENT,
		//				  `Url` varchar(255) NOT NULL,
		//				  `Name` varchar(255) DEFAULT NULL,
		//				  `Title` varchar(255) DEFAULT NULL,
		//				  `Description` longtext,
		//				  `MetaDescription` varchar(255) DEFAULT NULL,
		//				  `Image` varchar(255) DEFAULT NULL,
		//				  `ImageAltText` varchar(255) DEFAULT NULL,
		//				  PRIMARY KEY (`Id`)
		//				) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=latin1";
		//			//mySQL.ExecuteSQL(createBlog);

		//			var createBlogParts = @"CREATE TABLE `blogpart` (
		//					`Id` int(11) NOT NULL AUTO_INCREMENT,
		//					`BlogId` int(11) NOT NULL,
		//					`BlogSectionId` int(11) DEFAULT NULL,
		//					`Json` longtext,
		//					PRIMARY KEY (`Id`),
		//					KEY `BlogSection` (`BlogSectionId`)
		//				) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=latin1";
		//			//mySQL.ExecuteSQL(createBlogParts);
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		ErrorHandler.Handle(ex, "BlogHelper:CreateTables()");
		//	}
		//}
	}
}