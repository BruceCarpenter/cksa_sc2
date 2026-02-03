using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ckLib
{
	public class Video
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Url { get; set; }

		public Video()
		{
			Name = string.Empty;
			Description = string.Empty;
			Url = string.Empty;
		}

	}

}
