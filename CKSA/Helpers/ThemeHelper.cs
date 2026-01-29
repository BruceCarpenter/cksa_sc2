using ckLib;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Text.Json;

namespace CKSA.Helpers
{
	public static class ThemeHelper
	{
		public static List<CustomImages> GetThemeInfo()
		{
			List<CustomImages>? themeMenu = null;

			try
			{
				// This data is the final menu that is made up of 2 json files.
				// One file has the main menu info and the other is the seasonal info.
				var cache = new CacheHelper(new MemoryCache(new MemoryCacheOptions()));
				themeMenu = cache.Get<List<CustomImages>>("finalThemeMenu");

				if (Debugger.IsAttached) themeMenu = null;

				// Need to load the rotating themes then insert into existing themes
				// in the correct spot. They get displayed row 1 then row 2 then back
				// to row 1 colm 2 and so on.
				// TODO: Make these two json files the same. Just ignore the date
				// on the themepictures.txt file. Rename to json as well.
				if (themeMenu == null)
				{
					// TODO: This is finally cached in the _Navication.cshtml file so this should not be needed to save in cache here.
					themeMenu = cache.LoadJsonCache<CustomImages>("themeMenu", "thememain.json");
					var ideas = cache.LoadJsonCache<IdeaHelper.IdeaPiece>("themeMenuSeasonal", "themeSeasonal.json");
					DateTime today = DateTime.Now;

					// For testing purposes set date here.
					//today = new DateTime(2025,1,3);
					DateTime normalizedToday = new DateTime(2000, today.Month, today.Day);

					// Start at -10 to indicate we are at the start of the loop.
					int targetIndex = -10;

					// 1. Find the first item where the end date has NOT passed
					for (int i = 0; i < ideas.Count; i++)
					{
						DateTime endDate = ideas[i].end;
						DateTime normalizedEnd = new DateTime(2000, endDate.Month, endDate.Day);
						if (normalizedEnd >= normalizedToday)
						{
							targetIndex = i - 1;
							break;
						}
					}

					// Default to first item if all dates passed
					if (targetIndex == -10) targetIndex = 0;

					// 2. Get the 3 preceding items
					int totalCount = ideas.Count;

					for (int i = 1; i <= 3; i++)
					{
						// The modulo operator combined with adding totalCount handles the wrap-around
						int prevIndex = (targetIndex + i + totalCount) % totalCount;
						var seasonalToAdd = new CustomImages
						{
							Alt = ideas[prevIndex].Name,
							Image = ideas[prevIndex].img,
							Link = ideas[prevIndex].URL
						};

						themeMenu.Insert(i - 1, seasonalToAdd);
					}

					// This menu should have 8 items in it and no more. Can have less but more will mess up the UI.
					if (themeMenu.Count > 8)
					{
						themeMenu = themeMenu.Take(8).ToList();
					}

					cache.Save("finalThemeMenu", themeMenu);
				}
			}
			catch (Exception ex)
			{
				// Need to make sure if something bad happens all will be fine or the entier site is down.
			}

			if (themeMenu == null)
			{
				themeMenu = new List<CustomImages>();
			}

			return themeMenu;
		}

	}
}
