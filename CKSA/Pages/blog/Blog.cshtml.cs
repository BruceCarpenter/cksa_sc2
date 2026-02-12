using ckLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CKSA.Pages.blog
{
	public class BlogModel : PageModel
	{
		public Blog TheBlog { get; set; }

		public void OnGet()
		{
		}
	}
}
