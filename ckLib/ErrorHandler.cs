using System.Net.Mail;

namespace ckLib
{
    /// <summary>
    /// Notify me of any errors in the program.
    /// </summary>
    public class ErrorHandler
    {
        public ErrorHandler()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        static public void Handle(ckExceptionData error)
        {
            error.Send();
        }

        static public void Handle(Exception ex, string location)
        {
            try
            {
                var st = new System.Diagnostics.StackTrace(ex, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                var line = string.Empty;

                // Get the line number from the stack frame
                if (frame == null)
                {
                    line = "No frame, no line number.";
                }
                else
                {
                    line = frame.GetFileLineNumber().ToString();
                }

                using (MailMessage mailMsg = new MailMessage())
                {
                    mailMsg.Body = string.Format("{0}\n{1}\n{2}\n{3}\n Line Number: {4}",
                        location, ex.Message, ex.Source, ex.StackTrace, line);
                    mailMsg.Subject = string.Format("CK Error => {0}",DateTime.Now.ToLongTimeString());

                    EMailHelper.SendMail(mailMsg, "brucec@countrykitchensa.com", "Bruce Carpenter");
                }
            }
            catch
            {
            }
        } // end Handle

        static public void Handle(Exception ex, string location, string orderNo)
		{
			string newStr = string.Format("OrderNo: {0}\n{1}", orderNo, location);
            Handle(ex, newStr);
        }

        static public void Handle(Exception ex, string location, string orderNo, string url)
        {
            string newStr = string.Format("OrderNo: {0}\n{1}\n\nUrl:{2}", orderNo, location, url);
            Handle(ex, newStr);
        }

        //static public void URLError(string URL, string number)
        //{
        //    using (MailMessage mailMsg = new MailMessage())
        //    {
        //        mailMsg.Body = string.Format("URL not found: {0}", URL);
        //        mailMsg.Subject = string.Format("CK Error: {0}", number);
        //        EMailHelper.SendMail(mailMsg, "brucec@countrykitchensa.com", "Bruce Carpenter");
        //    }
        //} // end URLError

        //static public void BadURL(string URL)
        //{
        //    using (MailMessage mailMsg = new MailMessage())
        //    {
        //        mailMsg.Body = string.Format("URL not found: {0}", URL);
        //        mailMsg.Subject = "CK Missing File";
        //        EMailHelper.SendMail(mailMsg, "brucec@countrykitchensa.com", "Bruce Carpenter");
        //    }
        //} // end BadURL

        //static public void UnknownError(string URL)
        //{
        //    using (MailMessage mailMsg = new MailMessage())
        //    {
        //        mailMsg.Body = string.Format("Unknown error {0}", URL);
        //        mailMsg.Subject = "Unknown web site error";
        //        EMailHelper.SendMail(mailMsg, "brucec@countrykitchensa.com", "Bruce Carpenter");
        //    }
        //} // end UnknownError

    } // end class ErrorHandler
}