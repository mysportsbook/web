using System;
using System.Collections.Generic;
using System.Net.Mail;
/// <summary>
/// Represents the Email Message
/// </summary>
public class Mailer
{

    #region "Private Variables"
    string from;
    string fromName;
    string to;
    string cc;
    string subject;
    string host;
    string port;
    string body;
    string userName;
    string password;
    #endregion "Private Variables"

    #region "Public Variables"
    public bool UseSSL { get; set; }
    public string From { get; set; }
    public string FromName
    {
        get { return fromName; }
        set { fromName = value; }
    }
    public string To
    {
        get { return to; }
        set { to = value; }
    }
    public string Cc
    {
        get { return cc; }
        set { cc = value; }
    }
    public string Bcc { get; set; }
    public string Subject
    {
        get { return subject; }
        set { subject = value; }
    }
    public string Host
    {
        get { return host; }
        set { host = value; }
    }
    public string Body
    {
        get { return body; }
        set { body = value; }
    }
    #endregion "Public Variables"

    public Mailer()
    {
        from = "sportsbooknotification@gmail.com";
        fromName = "MSB Notification";
        host = "smtp.gmail.com";
        port = "587";
        userName = from;
        password = "winners1@3";
        UseSSL = true;
    }

    public void SendMail(List<string> AttachFile)
    {
        SendMail(from, AttachFile);
    }

    public void SendMail(string _from, List<string> AttachFile)
    {
        //Mail will not be sent if from address or host or port not available 
        if (string.IsNullOrEmpty(_from) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port))
            return;
        SmtpClient smtpClient = new SmtpClient();
        MailMessage message = new MailMessage();
        //try
        //{
        MailAddress fromAddress = new MailAddress(from, _from, System.Text.Encoding.UTF8);
        smtpClient.Host = host;
        smtpClient.Port = Convert.ToInt32(port);
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtpClient.UseDefaultCredentials = true;
        smtpClient.EnableSsl = UseSSL;
        smtpClient.Credentials = new System.Net.NetworkCredential(from, password);

        message.From = fromAddress;
        //if (!string.IsNullOrEmpty(to))
        //    message.To.Add(to);
        //else
        //    return;
        message.Subject = subject;
        if (to != null)
        {
            if ((to != "") && (to.LastIndexOf(";") >= 0))
            {
                string[] strSplit = to.Split(';');

                int _intValue = 0;
                for (_intValue = 0; _intValue < strSplit.Length; _intValue++)
                {
                    if (!string.IsNullOrEmpty(strSplit[_intValue]))
                        message.To.Add(strSplit[_intValue]);
                }
            }
            else if ((to != "") && (to.LastIndexOf(",") >= 0))
            {
                string[] strSplit = to.Split(',');

                int _intValue = 0;
                for (_intValue = 0; _intValue < strSplit.Length; _intValue++)
                {
                    if (!string.IsNullOrEmpty(strSplit[_intValue]))
                        message.To.Add(strSplit[_intValue]);
                }
            }
            else
            {
                if (to != "")
                    message.To.Add(to);
            }
        }
        if (cc != null)
        {
            if ((cc != "") && (cc.LastIndexOf(",") >= 0))
            {
                string[] strSplit = cc.Split(',');

                int _intValue = 0;
                for (_intValue = 0; _intValue < strSplit.Length; _intValue++)
                {
                    if (!string.IsNullOrEmpty(strSplit[_intValue]))
                        message.CC.Add(strSplit[_intValue]);
                }
            }
            else
            {
                if (cc != "")
                    message.CC.Add(cc);
            }
        }
        if (Bcc != null)
        {
            if ((Bcc != "") && (Bcc.LastIndexOf(",") >= 0))
            {
                string[] strSplit = Bcc.Split(',');

                int _intValue = 0;
                for (_intValue = 0; _intValue < strSplit.Length; _intValue++)
                {
                    if (!string.IsNullOrEmpty(strSplit[_intValue]))
                        message.Bcc.Add(strSplit[_intValue]);
                }
            }
            else
            {
                if (Bcc != "")
                    message.Bcc.Add(Bcc);
            }
        }

        if (AttachFile != null && AttachFile.Count > 0)
        {
            for (int i = 0; i < AttachFile.Count; i++)
            {
                /* Create the email attachment with the uploaded file */
                Attachment attach = new Attachment(AttachFile[i].ToString());
                /* Attach the newly created email attachment */
                message.Attachments.Add(attach);
            }
        }

        //message.Bcc.Add(new MailAddress("mailercopy2@einztion.com"));
        message.IsBodyHtml = true;
        message.Body = body;
        try
        {
            smtpClient.Send(message);
        }
        catch (Exception ex)
        {
            throw new Exception("Send Email Failed." + ex.Message);
        }
        //"Email successfully sent.";
        //}
        //catch (Exception ex)
        //{
        //    throw new Exception("Send Email Failed." + ex.Message);
        //}
    }

    public static void SendEmail(string subject, string to, string cc, string body, List<string> AttachFiles)
    {
        SendEmail(subject, null, to, cc, body, AttachFiles);
    }
    public static void SendEmail(string subject, string from, string to, string cc, string body)
    {
        SendEmail(subject, from, to, cc, body, null);
    }
    public static void SendEmail(string subject, string _from, string to, string cc, string body, List<string> AttachFiles)
    {
        string toMailAddress = string.Empty;
        string CCMailAddress = string.Empty;
        //toMailAddress = to;
        //CCMailAddress = cc;
        bool isTesting = System.Configuration.ConfigurationManager.AppSettings["IsTesting"] != null ? Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["IsTesting"]) : false;

        if (isTesting)
        {
            //if (ConvertHelper.ConvertToString(to) != null && to.Contains("@"))
            //{
            //    toMailAddress = to;
            //    CCMailAddress = ConvertHelper.ConvertToString(System.Configuration.ConfigurationManager.AppSettings["CCEmailTesting"]);
            //}
            //else
            //{
            toMailAddress = System.Configuration.ConfigurationManager.AppSettings["ToEmail"].ToString();
            CCMailAddress = System.Configuration.ConfigurationManager.AppSettings["CCEmailTesting"].ToString();
            //For testing: we can't change from name when we use gmail as smtp, so we assigned testing from name to send email.
            _from = System.Configuration.ConfigurationManager.AppSettings["from"].ToString();
            //}
        }
        else
        {
            toMailAddress = to;
            CCMailAddress = cc;
        }
        Mailer objMailer = new Mailer();
        if (!string.IsNullOrEmpty(_from))
        {
            objMailer.from = _from;
            objMailer.fromName = _from;
        }
        objMailer.To = toMailAddress;
        objMailer.Cc = CCMailAddress;
        objMailer.Subject = subject;
        objMailer.Body = body;
        if (!string.IsNullOrEmpty(_from))
            objMailer.SendMail(_from, AttachFiles);
        else
            objMailer.SendMail(AttachFiles);
    }
    public void SendEmail(string subject, string to, string body, List<string> AttachFiles)
    {
        string toMailAddress = string.Empty;
        string CCMailAddress = string.Empty;
        bool isTesting = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["IsTesting"]);

        if (isTesting)
        {
            toMailAddress = System.Configuration.ConfigurationManager.AppSettings["ToEmail"].ToString();
            CCMailAddress = System.Configuration.ConfigurationManager.AppSettings["CCEmailTesting"].ToString();
        }
        else
        {
            toMailAddress = to;
            CCMailAddress = cc;
        }
        Mailer objMailer = new Mailer();
        {
            objMailer.from = from;
            objMailer.fromName = fromName;
        }
        objMailer.To = toMailAddress;
        objMailer.Cc = CCMailAddress;
        objMailer.Subject = subject;
        objMailer.Body = body;
        if (!string.IsNullOrEmpty(from))
            objMailer.SendMail(fromName, AttachFiles);
        else
            objMailer.SendMail(AttachFiles);
    }

}
