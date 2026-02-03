using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ckLib
{
	public class CookieHelper
	{
		public const string ORDER_NAME = "order";
		public const string WholeSale = "wholesale"; // wholesale id
		public const string OrderStarted = "st";
		public const int ExpireDaysDefault = 365;

		const bool DoNotEncryptDebugging = false;

		private readonly IHttpContextAccessor _httpContextAccessor;

		public CookieHelper(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public static void Set(IResponseCookies cookies, string key, string value, bool encrypt, int? expireDays= ExpireDaysDefault)
		{
			if (DoNotEncryptDebugging)
			{
				encrypt = false;
			}

			// Keep a cookie around if an order is running, that is not encrypted.
			if (key == CookieHelper.ORDER_NAME)
			{
				CookieHelper.Set(cookies,CookieHelper.OrderStarted, "1", false);
			}

			if (encrypt)
			{
				EncryptHelper eh = new EncryptHelper();
				key = eh.Encrypt(key);
				value = eh.Encrypt(value);
			}

			var options = new CookieOptions
			{
				Expires = expireDays.HasValue ? DateTime.Now.AddDays(expireDays.Value) : DateTime.MaxValue,
				HttpOnly = true, // Prevents JavaScript from reading the cookie (Security!)
				Secure = true,   // Only send over HTTPS
				SameSite = SameSiteMode.Strict // Prevents CSRF attacks
			};

			cookies.Append(key, value, options);
		}

		public static string Get(IRequestCookieCollection cookies, string key, bool encrypt)
		{
			return cookies[key] ?? string.Empty;
		}


		/// <summary>
		/// For ease of testing get cookie here so I can get it unencrypted.
		/// </summary>
		public static int GetWholesaleValue(IRequestCookieCollection cookies)
		{
			int wholesale = 0;

			try
			{
				var isWholeSale = CookieHelper.Get(cookies, CookieHelper.WholeSale, true);
				if(!string.IsNullOrEmpty(isWholeSale))
					wholesale = Convert.ToInt32(isWholeSale);
			}
			catch
			{
				// do nothing.
			}

			return wholesale;
		}
	}
}
