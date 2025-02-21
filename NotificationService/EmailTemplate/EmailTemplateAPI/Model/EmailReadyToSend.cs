public class EmailReadyToSend
{
    public string To { get; set; }
    public string From { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }

    public EmailReadyToSend(string to, string from, string subject, string body)
    {
        To = to;
        From = from;
        Subject = subject;
        Body = body;
    }
    
}