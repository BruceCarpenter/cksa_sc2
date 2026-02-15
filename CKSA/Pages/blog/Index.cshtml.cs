using ckLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace CKSA.Pages.blog
{
	public class IndexModel : PageModel
	{
		public List<dynamic> Blogs { get; set; }

		[BindProperty(SupportsGet = true)]
		public string? search { get; set; }

		public void OnGet()
		{
			try
			{
				Blogs = new List<dynamic>();

				if (string.IsNullOrEmpty(search))
				{
					DefaultPageLoad();
				}
				else
				{
					SearchPageLoad(search);
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "blog_Default:Page_Load()", search);
			}
		}


		private void SearchPageLoad(string searchOn)
		{
			try
			{
				var stemmed = GetSearchText(CleanForSqlSearch(searchOn));

				using (var mySQL = DbDriver.OpenConnection())
				{
					using (var command = mySQL.CreateCommand())
					{
						// Took out active=1 and  for blogIdea
						command.CommandText = string.Format(@"SELECT Id, Url, Mini, Json,1 as blogType from Blogidea where MATCH(searchable) AGAINST({0}) union
					SELECT Id, Url, Mini, Json,2 as blogType from Blog where active = 1 and MATCH(searchable) AGAINST({0})", stemmed);

						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								var url = reader.ReadString(1) + "/";
								if (url.ToLower().Contains("needs to be determined"))
								{
									continue;
								}
								var json = reader.ReadString(3);
								var dynamicJson = JsonNode.Parse(json);
								var mainImg = dynamicJson["Image"]?.GetValue<string>();
								var description = dynamicJson["Description"]?.GetValue<string>();
								var stripHtmlDescription = CkDefines.StripHTML(description);
								var blogType = reader.ReadInt32(4); // 1 idea, 2 blog
																	// Do not append blog if the blog is really an idea.
								var blogUrl = blogType == 2 ? "/blog/" : string.Empty;

								stripHtmlDescription = CkDefines.TruncateString(stripHtmlDescription, 200);
								stripHtmlDescription += string.Format("<a href={0}>Read More</a>", blogUrl + url);

								if (Debugger.IsAttached)
								{
									url = Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.PathAndQuery : url;
								}

								var aBlog = new
								{
									Id = reader.ReadInt32(0),
									Url = blogUrl + url,
									Mini = reader.ReadString(2),
									MiniAlt = CkDefines.TruncateString(stripHtmlDescription, 20),
									Description = stripHtmlDescription
									// Alt might be in the json?
								};

								Blogs.Add(aBlog);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "blog_Default:()-Search", searchOn);
			}
		}

		/// <summary>
		/// Get the sql search term to search against.
		/// </summary>
		/// <returns></returns>
		private string GetSearchText(string searchOn)
		{
			var ps = new ckLib.PorterStemmerAlgorithm.PorterStemmer();
			var searchText = string.Empty;
			var seachTermsList = searchOn.Split(' ');

			foreach (var term in seachTermsList)
			{
				if (searchText.Length == 0)
					searchText = "\"";
				else
					searchText += " ";

				searchText += ps.stemTerm(term.ToLower());
			}

			searchText += "\"";

			return searchText;
		}

		public string CleanForSqlSearch(string userInput)
		{
			if (string.IsNullOrEmpty(userInput))
			{
				return userInput; // Empty string remains empty
			}

			// Use Regex to remove non-alphabetic characters
			string cleanTerm = Regex.Replace(userInput, "[^a-zA-Z]", "");

			// Truncate to 20 characters
			return cleanTerm.Substring(0, Math.Min(cleanTerm.Length, 20));
		}

		private void DefaultPageLoad()
		{
			try
			{
				using (var mySQL = DbDriver.OpenConnection())
				{
					using (var command = mySQL.CreateCommand())
					{
						// Skip #20 wholesale blog.
						command.CommandText = "select Id, Url, Mini, Json from Blog where active=1 and Id != 20 order by Id desc";

						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								var url = reader.ReadString(1) + "/";
								var json = reader.ReadString(3);
								var jsonNode = JsonNode.Parse(json);
								var mainImg = jsonNode["Image"]?.GetValue<string>();
								var description = jsonNode["Description"]?.GetValue<string>();
								var stripHtmlDescription = CkDefines.StripHTML(description);

								stripHtmlDescription = CkDefines.TruncateString(stripHtmlDescription, 200);
								stripHtmlDescription += string.Format("<a href=/blog/{0}>Read More</a>", url);

								var aBlog = new
								{
									Id = reader.ReadInt32(0),
									Url = "/blog/" + url,
									Mini = reader.ReadString(2),
									MiniAlt = CkDefines.TruncateString(stripHtmlDescription, 20),
									Description = stripHtmlDescription
									// Alt might be in the json?
								};
								Blogs.Add(aBlog);
							}
						}
					}
				}


				///
				/// Lets add the two that are currently static blogs
				/// Eventually these will be in the db.
				/// 
				var tempDescription = "Whether you sell candies, make them for friends and family, or just enjoy a fun project with your children, the proper candy-making supplies are essential.";
				tempDescription = CkDefines.TruncateString(tempDescription, 200);
				tempDescription += string.Format("<a href='/about/candy-making-supplies.aspx'>Read More</a>");

				var sprinkleBlog = new
				{
					Url = "/about/candy-making-supplies.aspx",
					Mini = "/blog/2023/ctb-candy-making-supplies.jpg",
					MiniAlt = "Cake Decorating Supplies",
					Description = tempDescription
					// Alt might be in the json?
				};
				Blogs.Add(sprinkleBlog);
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "blog_Default:()-DefaultPageLoad");
			}

		}
	}
}
