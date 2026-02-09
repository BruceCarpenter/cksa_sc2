using Microsoft.AspNetCore.Http;

///
/// I should not use the Get/Set functions in this class directly. I should use the GetCookieValue/SetCookieValue functions which will handle encryption and other things.
/// 


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
		private HttpContext HttpContext => _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No active HttpContext.");

		public CookieHelper(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public bool Exists(string name)
		{
			return HttpContext.Request.Cookies.ContainsKey(name);
		}

		public string? Get(string name)
		{
			HttpContext.Request.Cookies.TryGetValue(name, out var value);
			return value;
		}

		public void Set(string name, string value, CookieOptions? options = null)
		{
			options ??= new CookieOptions
			{
				Expires = DateTimeOffset.UtcNow.AddDays(365),
				HttpOnly = true,
				Secure = HttpContext.Request.IsHttps,
				SameSite = SameSiteMode.Lax,
				Path = "/"
			};

			HttpContext.Response.Cookies.Append(name, value, options);
		}

		public void Delete(string name)
		{
			HttpContext.Response.Cookies.Delete(name);
		}

		// These are my functions. Above are Chat generated functions. 
		public void SetCookieValue(string cookieName, string cookieValue, bool encrypt)
		{
			try
			{
				if (DoNotEncryptDebugging)
				{
					encrypt = false;
				}

				// Keep a cookie around if an order is running, that is not encrypted.
				if (cookieName == CookieHelper.ORDER_NAME)
				{
					Set(CookieHelper.OrderStarted, "1");
				}

				if (encrypt)
				{
					EncryptHelper eh = new EncryptHelper();
					cookieName = eh.Encrypt(cookieName);
					cookieValue = eh.Encrypt(cookieValue);
				}

				Set(cookieName, cookieValue);
			}
			catch (Exception)
			{
			}

		}
		public string? GetCookieValue(string cookieName, bool encrypted)
		{
			string originalCookieName = cookieName;
			string? cookieValue = null;

			if (DoNotEncryptDebugging)
			{
				encrypted = false;
			}

			try
			{
				EncryptHelper eh = new EncryptHelper();
				if (encrypted)
				{
					cookieName = eh.Encrypt(cookieName);
				}

				cookieValue = Get(cookieName);

				if (cookieValue == null)
				{
					return null;
				}

				if (encrypted)
				{
					cookieValue = eh.Decrypt(cookieValue);
				}
			}
			catch
			{
				throw;
			}

			return cookieValue;
		}

		/// <summary>
		/// For ease of testing get cookie here so I can get it unencrypted.
		/// </summary>
		public int GetWholesaleValue()
		{
			int wholesale = 0;

			try
			{
				var cookieValue = GetCookieValue(CookieHelper.WholeSale, true);
				if(cookieValue != null)
					wholesale = Convert.ToInt32(cookieValue);
			}
			catch
			{
				// do nothing.
			}

			return wholesale;
		}
	}

}
