using ckLib;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace CKSA.Pages.blog
{
	public class BlogModel : PageModel
	{
		public Blog? TheBlog { get; set; }
		public string Url {get;set;}
		public string Canonical { get; set; }
		public void OnGet(string name)
		{
			try
			{
				Url = Request.GetDisplayUrl();

				// Check the database. Eventually the above blogs should be moved to database.
				TheBlog = BlogHelper.Load(name, BlogHelper.BlogType.Standard);

				if (TheBlog != null)
				{
					ViewData["Title"] = TheBlog.Title;
					ViewData["MetaDescription"] = TheBlog.MetaDescription;
				}
				else
				{
					// Not sure what is going on should never be null.
					//CKDefines.Redirect(Page, "/blog/");
				}

				Canonical = "https://www.countrykitchensa.com/blog/" + name + "/";
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(ex, "blog_Blog:Page_Load()", name);
			}
		}

		public string EmailBodyText()
		{
			return string.Format(@"I couldn't resist forwarding it to you because I know how passionate you are about baking and creating sweet treats. I'm sure you'll find plenty of inspiration and new ideas to try out in your own kitchen!
			To read the blog post, simply click on the link below:%0d%0a
			{0}%0d%0a
			If you enjoy it as much as I did, I encourage you to share it with your friends and fellow baking enthusiasts.Together, we can spread the joy and love for baking!", ViewData["Canonical"]);
		}
	}
}
