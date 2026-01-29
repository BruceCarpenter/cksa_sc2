using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ckLib
{
	public class CkDefines
	{
		/// <summary>
		/// Generate the url to the category image.
		/// </summary>
		/// <param name="itemNumber">This should be the main unit and indicates the folder.</param>
		/// <param name="imageName">This is from the database</param>
		public static string ImageCategoryUrl(string itemNumber, string imageName)
		{
			if (!string.IsNullOrEmpty(itemNumber))
			{
				var path = FileLocations.GetImagePath(itemNumber);

				if(Debugger.IsAttached)
					return "https://www.countrykitchensa.com/catalog/350/" + path + "/" + imageName;

				return "/catalog/350/" + path + "/" + imageName;
			}

			return string.Empty;
		}
	}
}
