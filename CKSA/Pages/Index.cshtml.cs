using ckLib;
using CKSA.Helpers;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace CKSA.Pages
{
    public class IndexModel : PageModel
    {
		public class SliderContentItem
		{
			public string Image { get; set; }
			public string Link { get; set; }
			public string AltText { get; set; }
			public string LinkText { get; set; }
			public string HeadingText { get; set; }
			public string Description { get; set; }
		}
		private readonly IMemoryCache _cache;
		private readonly ILogger<IndexModel> _logger;

		public IdeaHelper.IdeaPiece? TheLatestBlog { get; set; }

		public List<SliderContentItem>? FeaturedItems;
		public List<SliderContentItem>? BakingSupplies;
		public List<SliderContentItem>? CandyMakingSupplies;
		public List<SliderContentItem>? CookieDecoratingSupplies;

		public IndexModel(ILogger<IndexModel> logger, IMemoryCache cache)
        {
            _logger = logger;
			_cache = cache;

			LatestBlog();
			LoadSliderContent();
		}

        public void OnGet()
        {

        }

		private void LatestBlog()
		{
			try
			{
				if (TheLatestBlog == null)
				{
					TheLatestBlog = IdeaHelper.LatestBlog();
				}
			}
			catch (Exception ex)
			{

			}
		}

		private void LoadSliderContent()
		{
			try
			{
				FeaturedItems = LoadSliderContent("featured");
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "Index::featured"));
			}
			try
			{
				BakingSupplies = LoadSliderContent("baking-supplies");
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "Index::baking-supplies"));
			}
			try
			{
				CandyMakingSupplies = LoadSliderContent("candy-making-supplies");
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "Index::candy-making-supplies"));
			}
			try
			{
				CookieDecoratingSupplies = LoadSliderContent("cookie-decorating-supplies");
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "Index::cookie-decorating-supplies"));
			}

		}
		private List<SliderContentItem> LoadSliderContent(string sliderName)
		{
			List<SliderContentItem>? sliderContent = null;
			var cacheHelper = new CacheHelper(_cache);

			try
			{
				if (!Debugger.IsAttached)
				{
					try
					{
						sliderContent = cacheHelper?.Get<List<SliderContentItem>>(sliderName);
					}
					catch
					{

					}
				}

				if (sliderContent == null)
				{
					// Get the physical path to your JSON file
					string sliderFileName = sliderName + ".json";
					sliderContent = cacheHelper?.LoadJsonCache<SliderContentItem>(sliderName, sliderFileName);
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.Handle(new ckExceptionData(ex, "Index::LoadSliderContent"));
			}

			return sliderContent;
		}



	}
}
