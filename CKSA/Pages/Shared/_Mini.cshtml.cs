using ckLib;
using CKSA.Helpers;
using CKSA.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CKSA.Pages.Shared
{
	public class _MiniModel : PageModel
	{
		public List<Product> MiniProducts { get; set; } = new();
		MiniImageModel _MiniImageModel { get; set; }

		public void OnGet()
		{
		}

	}
}

