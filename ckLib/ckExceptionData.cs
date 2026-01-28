using System.Net.Mail;
using System.Text;

namespace ckLib
{
	/// <summary>
	/// Summary description for ckExceptionData.
	/// </summary>
	public class ckExceptionData
	{
		Exception _Exception;
		string _Location;
		string _OrderNo;
		string _SQL;

		const string EndOfLine = "\r\n";

		public ckExceptionData(Exception ex, string location)
		{
			_Exception = ex;
			_Location = location;
		}
		public ckExceptionData(Exception ex, string location, string SQL)
		{
			_Exception = ex;
			_Location = location;
			_SQL = SQL;
		} 
		public ckExceptionData(Exception ex, string location, string SQL, string orderNo)
		{
			_Exception = ex;
			_Location = location;
			_OrderNo = orderNo;
			_SQL = SQL;
		} 

		protected string BuildBodyMsg()
		{
			StringBuilder body = new StringBuilder();
			if (_Exception != null)
			{
				var st = new System.Diagnostics.StackTrace(_Exception, true);
				// Get the top stack frame
				var frame = st.GetFrame(0);
				// Get the line number from the stack frame

				body.Append(string.Format("Exception: {0}\n<br/>", _Exception.Message));
				body.Append(string.Format("Stack Trace: {0}\n<br/>", _Exception.StackTrace));
				body.Append(string.Format("Source: {0}\n<br/>", _Exception.Source));
				if (frame != null)
				{
					var line = frame.GetFileLineNumber();
					body.Append(string.Format("Line Number: {0}\n<br/>", line));
				}
			}
			if (_Location != null && _Location.Length > 0)
			{
				body.Append(string.Format("Location: {0}\n", _Location));
			}
			if (_OrderNo != null && _OrderNo.Length > 0)
			{
				body.Append(string.Format("Order Number: {0}\n", _OrderNo));
			}
			if (_SQL != null && _SQL.Length > 0)
			{
				body.Append(string.Format("SQL: {0}", _SQL));
			}

			return (body.ToString());
		} // BuildBodyMsg

		public void Send()
		{
			try
			{
				using (MailMessage mailMsg = new MailMessage())
				{
					mailMsg.Body = BuildBodyMsg();
					mailMsg.Subject = "CK Error";
					EMailHelper.SendMail(mailMsg, "brucec@countrykitchensa.com", "Bruce Carpenter");
				}
			}
			catch (Exception)
			{
			}
		} // Send
	} // end class ckExceptionData
}