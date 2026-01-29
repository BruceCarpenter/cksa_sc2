namespace CKSA.Helpers
{
	public class HtmlHelper
	{
		const string Title = " | Country Kitchen SweetArt";

		static public string CreateTitle(string title, string titleType)
		{
			string finalTitle = title + titleType;
			/*
			int finalCutPoint = 65;

			if (finalTitle.Length > finalCutPoint)
			{
				//
				// Make title short.
				finalTitle = finalTitle.Substring(0, 65);

				if (finalTitle[finalTitle.Length-1] != ' ')
				{
					finalCutPoint = finalTitle.LastIndexOf(' ');
					if (finalCutPoint == -1)
					{
						finalCutPoint = 65;
					}
				}
				finalTitle = finalTitle.Substring(0, finalCutPoint);
			}
			else if (finalTitle.Length <= 5)
			{
				finalTitle = finalTitle + titleType;
			}
			*/
			return finalTitle;
		}

		static public string CreateTitle(string title)
		{
			return CreateTitle(title, Title);
		}

	}
}
