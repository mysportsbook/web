using System;

public partial class StoredProcedures
{

    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void SqlSendEmail(string paymentFor = "", string paymentBy = "", string paymentMonth = "",
        string paymentAmount = "", string paymentMode = "", string venueEmailID = "", string emailID = "", string SMSTransactionID = "", string password = "")
    {
        // Put your code here
        Mailer mailer = new Mailer();
        string subject = string.Empty;
        subject = DateTime.Now.ToString("dd MMMM yyyy")
                    + " - " + paymentBy
                    + " - " + paymentMonth
                    + " - " + paymentAmount
                    + " - " + paymentMode;
        mailer.SendEmail(subject, venueEmailID, GetHTMLFromSMSRequest(paymentFor, paymentBy, paymentMonth, paymentAmount), null, password);
    }
    private static string GetHTMLFromSMSRequest(string paymentFor, string paymentBy, string paymentMonth, string paymentAmount, string messageBody = "", string responseString = "")
    {
        string body = string.Empty;
        body = "<table cellspacing='0' cellpadding='0' style='border:1px solid #ccc; border-bottom:0px; border-right:0px;'>";
        body = body + "<tr> <td style ='padding:5px; border-bottom:1px solid #ccc; border-right:1px solid #ccc;font-weight: bold;'> Date" + "</td><td style ='padding:5px; border-bottom:1px solid #ccc; border-right:1px solid #ccc;'>" + DateTime.Now.ToString("dd MMMM yyyy") + "</td></tr>";
        body = body + "<tr> <td style ='padding:5px; border-bottom:1px solid #ccc; border-right:1px solid #ccc;font-weight: bold;'> Player" + "</td><td style ='padding:5px; border-bottom:1px solid #ccc; border-right:1px solid #ccc;'>" + paymentBy + "</td></tr>";
        body = body + "<tr> <td style ='padding:5px; border-bottom:1px solid #ccc; border-right:1px solid #ccc;font-weight: bold;'> Payment For" + "</td><td style ='padding:5px; border-bottom:1px solid #ccc; border-right:1px solid #ccc;'>" + paymentFor + "</td></tr>";
        body = body + "<tr> <td style ='padding:5px; border-bottom:1px solid #ccc; border-right:1px solid #ccc;font-weight: bold;'> Month & Receipt#" + "</td><td style ='padding:5px; border-bottom:1px solid #ccc; border-right:1px solid #ccc;'>" + paymentMonth + "</td></tr>";
        body = body + "<tr> <td style ='padding:5px; border-bottom:1px solid #ccc; border-right:1px solid #ccc;font-weight: bold;'> Amount & Payment Mode" + "</td><td style ='padding:5px; border-bottom:1px solid #ccc; border-right:1px solid #ccc;'>" + paymentAmount + "</td></tr>";
        body = body + "<tr> <td style ='padding:5px; border-bottom:1px solid #ccc; border-right:1px solid #ccc;font-weight: bold;'> Message" + "</td><td style ='padding:5px; border-bottom:1px solid #ccc; border-right:1px solid #ccc;'>" + messageBody + "</td></tr>";
        body = body + "<tr> <td style ='padding:5px; border-bottom:1px solid #ccc; border-right:1px solid #ccc;font-weight: bold;'> ResponseString" + "</td><td style ='padding:5px; border-bottom:1px solid #ccc; border-right:1px solid #ccc;'>" + responseString + "</td></tr></table>";
        body = "<html><body> " + body + " </body></html>";
        return body;
    }
}
