using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace ckLib
{
	/// <summary>
	/// Summary description for EMailHelper.
	/// </summary>
	public class EMailHelper
	{
		static public string FromEmailAddress = "account@countrykitchensa.com";
		const string MyName = "Bruce Carpenter";
		const string MyEmail = "brucec@countrykitchensa.com";

		public EMailHelper()
		{
		}

		/// <summary>
		/// Send an email from mail@countrykitchensa.com.
		/// </summary>
		static public void SendFromAccount(string subject, string message, string toEmail, string toName)
		{
			try
			{
				var email = new MailMessage
				{
					From = new MailAddress("account@countrykitchensa.com","Country Kitchen SweetArt"),
					Subject = subject,
					Body = message,
					IsBodyHtml = true
				};
				email.To.Add(new MailAddress(toEmail, toName));

				var client = new SmtpClient
				{
					Host = "secure.emailsrvr.com",
					Port = 587,
					Credentials = new NetworkCredential("account@countrykitchensa.com", "^^6zc@j*4VnCNu5V"),
					EnableSsl = true,
				};

				client.Send(email);
			}
			catch
			{
				EMailHelper.SendFromGMail(subject, message, toEmail, toName);
				EMailHelper.SendFromGMail("SendFromMail Failed", "SendFromMail failed.", MyEmail, MyName);
			}
		}

		/// <summary>
		/// Send an email from mail@countrykitchensa.com.
		/// </summary>
		static public void SendFromMail(string subject, string message, string toEmail, string toName)
		{
			try
			{
				var email = new MailMessage
				{
					From = new MailAddress("mail@countrykitchensa.com", "Country Kitchen SweetArt"),
					Subject = subject,
					Body = message,
					IsBodyHtml = true
				};
				email.To.Add(new MailAddress(toEmail, toName));

				var smtp = new SmtpClient
				{
					Host = "secure.emailsrvr.com",
					Port = 587,
					Credentials = new NetworkCredential("mail@countrykitchensa.com", "g^76^fF1"),
					EnableSsl = true,
				};

				smtp.Send(email);
			}
			catch
			{
				EMailHelper.SendFromGMail(subject, message, toEmail, toName);
				EMailHelper.SendFromGMail("SendFromMail Failed", "SendFromMail failed.", MyEmail, MyName);
			}
		}

		/// <summary>
		/// Send email from GMail account. This account has to be setup in a special way using Google to create a
		/// uniquie password for the email to send from.
		/// </summary>
		static public void SendFromGMail(string subject, string message, string toEmail, string toName)
		{
			try
			{
				var email = new MailMessage
				{
					From = new MailAddress("mail@countrykitchensa.com", "Country Kitchen SweetArt"),
					Subject = subject,
					Body = message,
					IsBodyHtml = true
				};
				email.To.Add(new MailAddress(toEmail,toName));

				var smtp = new SmtpClient
				{
					Host = "smtp.gmail.com",
					Port = 587,
					EnableSsl = true,
					DeliveryMethod = SmtpDeliveryMethod.Network,
					UseDefaultCredentials = false,
					Credentials = new System.Net.NetworkCredential("cksamobile@gmail.com", "uzmdsvnvfmnzppiv")
				};

				smtp.Send(email);
			}
			catch
			{
				// If this fails not sure what to do...
			}
		}

		static public void SendMeDebugInfo(string debugInfo)
		{
			using (var mailMsg = new MailMessage())
			{
				mailMsg.Subject = string.Format("Debug info: {0}.", DateTime.Now.ToLongTimeString());
				mailMsg.Body = debugInfo;
				mailMsg.To.Add(new MailAddress("brucec@countrykitchensa.com"));
				mailMsg.From = FromAddress(FromEmailAddress);
				mailMsg.IsBodyHtml = true;
				using (var client = Client())
				{
					client.Send(mailMsg);
				}
			}
		}

		/// <summary>
		/// The fromEmail has to be account or Amazon will not send it.
		/// </summary>
		static public void SendMail(MailMessage mailMsg, string toEmail, string toName, string fromEmail = "account@countrykitchensa.com", bool forceForTesting = true)
		{
			//
			// Account has sent too many so use this for time being
			//
			//fromEmail = "mail@countrykitchensa.com";
			MailAddress toAddress = new MailAddress(toEmail, toName);

			try
			{
				mailMsg.To.Clear();

				if (EMailHelper.IsEmailValidString(toEmail))
				{
					mailMsg.From = FromAddress(fromEmail);
					mailMsg.To.Add(toAddress);

					mailMsg.IsBodyHtml = true;
					if (System.Diagnostics.Debugger.IsAttached == false || forceForTesting)
					{
						using (var client = Client())
						{
							client.Send(mailMsg);
						}
					}
				} // end if
			}
			catch (Exception ex)
			{
				throw (ex);
			}
		}

		static public void SendMail(string subject, string body, List<string> to)
		{
			try
			{
				using (var mailMessage = new MailMessage())
				{
					mailMessage.From = FromAddress(FromEmailAddress);
					mailMessage.IsBodyHtml = true;
					mailMessage.Subject = subject;
					mailMessage.Body = body;

					foreach (var toEmail in to)
					{
						mailMessage.To.Add(new MailAddress(toEmail));
					}

					using (var client = Client())
					{
						client.Send(mailMessage);
					}
				}
			}
			catch (Exception ex)
			{
				throw (ex);
			}
		}

		static public void SendMail(string subject, string body, string to)
		{
			try
			{
				var toList = new List<string>();
				toList.Add(to);
				EMailHelper.SendMail(subject, body, toList);
			}
			catch (Exception ex)
			{
				throw (ex);
			}
		}

		/// <summary>
		/// Using Amazon Simple Email Server to send these emails.
		/// This is only account@countrykitchensa.com.
		/// </summary>
		static public SmtpClient Client()
		{
			var SMTP_USERNAME = "AKIAJFBGM6HZWHCGXYBA";
			var SMTP_PASSWORD = "Ao0lh/Bq229V7OFbUp7SxrZj6VkDUayWJkFqroG0LvgE";
			var HOST = "email-smtp.us-east-1.amazonaws.com";
			var PORT = 587;

			var client = new SmtpClient(HOST);

			client.Port = PORT;
			client.EnableSsl = true;
			client.UseDefaultCredentials = false;
			client.Credentials = new NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

			return client;
		}
		private static bool ValidateServerCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
		{
			// Allow certificates with any errors
			return true;
		}

		/// <summary>
		/// This is how emails used to be sent via RackSpace. As of 2022 Rackspace and .net are no longer
		/// compatable.
		/// </summary>
		/// <param name="fromEmail"></param>
		/// <returns></returns>
		//static public SmtpClient Client(string fromAddress)
		//{
		//	System.Net.NetworkCredential NTLMAuthentication =
		//	   new System.Net.NetworkCredential(
		//	   fromAddress,
		//	   "g^76^fF1");

		//	//System.Net.NetworkCredential NTLMAuthentication =
		//	//   new System.Net.NetworkCredential(
		//	//   "brucec@countrykitchensa.com",
		//	//   "Tuna!Fish71");

		//	SmtpClient client = new SmtpClient();

		//	client.DeliveryMethod = SmtpDeliveryMethod.Network;


		//	//465
		//	//SSL
		//	//client.Host = "secure.emailsrvr.com";
		//	//client.Port = 587;
		//	//client.DeliveryMethod = SmtpDeliveryMethod.Network;

		//	client.Host = "smtp.emailsrvr.com";
		//	client.Port = 587;

		//	client.UseDefaultCredentials = false;
		//	client.Credentials = NTLMAuthentication;

		//	return client;
		//}

		static public MailAddress FromAddress(string fromEmail)
		{
			return (new MailAddress(fromEmail, "Country Kitchen SweetArt"));
		}

		//static public bool DoesEMailAlreadyExist(string inputEmail)
		//{
		//	var alreadyExists = false;
		//	var sql = "SELECT EmailAddress FROM mailinglist where EmailAddress=@c0 and (IsRemoved is NULL or IsRemoved=0)";

		//	try
		//	{

		//		using (var mySql = new MySQLHelper(MySQLHelper.DataBases.pricelist))
		//		{
		//			using (var command = mySql.Connection().CreateCommand())
		//			{
		//				command.CommandType = CommandType.Text;
		//				command.CommandText = sql;
		//				command.Parameters.AddWithValue("@c0", inputEmail);

		//				using (var reader = command.ExecuteReader())
		//				{
		//					if (reader.Read())
		//					{
		//						alreadyExists = true;
		//					}
		//				}
		//			}
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		ErrorHandler.Handle(ex, "EMailHelper::DoesEMailAlreadyExist", sql);
		//	}

		//	return alreadyExists;
		//}

		//static public bool IsEmailMarkedAsRemoved(string inputEmail)
		//{
		//	var isRemoved = false;
		//	var sql = "SELECT EmailAddress FROM mailinglist where EmailAddress=@c0";

		//	try
		//	{
		//		using (var mySQL = new MySQLHelper(MySQLHelper.DataBases.ckorders))
		//		{
		//			using (var command = mySQL.Connection().CreateCommand())
		//			{
		//				command.CommandText = sql;
		//				command.Parameters.AddWithValue("@c0", inputEmail);

		//				using (var reader = command.ExecuteReader())
		//				{
		//					isRemoved = reader.HasRows;
		//				}
		//			}
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		ErrorHandler.Handle(ex, "EMailHelper::IsEmailMarkedAsRemoved", sql);
		//	}

		//	return isRemoved;
		//}

		static public bool IsEmailValidString(string inputEmail)
		{
			if (string.IsNullOrEmpty(inputEmail) || BlockList(inputEmail))
				return false;

			var strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
				@"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
				@".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
			Regex re = new Regex(strRegex);

			return re.IsMatch(inputEmail);
		} 

		static public bool BlockList(string email)
		{
			var badEmails = new List<string>
			{
				"sample@email.tst",
				"testing@example.com"
			};

			var isBad = badEmails.Contains(email.ToLower());

			return isBad;
		}

	}
}