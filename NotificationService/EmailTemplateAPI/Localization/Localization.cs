public class Localization
{
    private readonly Dictionary<string, Dictionary<string, string>> _localizations;

    public Localization()
    {
        _localizations = new Dictionary<string, Dictionary<string, string>>
        {
            { "en", new Dictionary<string, string>
                {
                    { "DocumentUploadedToCase", "{0} uploaded a new document \"{1}\" to the case \"{2}\"" },
                    { "OpenDocument", "Open document" },
                    { "YouReceiveThisEmailBecauseYouHaveAUserInIMSCase", "You receive this email because you have a user in <b>IMS Case</b>" }
                }
            },
            { "da", new Dictionary<string, string>
                {
                    { "DocumentUploadedToCase", "{0} uploadede et nyt dokument \"{1}\" til sagen \"{2}\"" },
                    { "OpenDocument", "Åbn dokument" },
                    { "YouReceiveThisEmailBecauseYouHaveAUserInIMSCase", "Du modtager denne email fordi du har en bruger i <b>IMS Case</b>" }
                }
            }
        };
    }

    public string GetMessage(string key, string language)
    {
        return _localizations.TryGetValue(language, out var entries) && entries.TryGetValue(key, out var message)
            ? message
            : key;
    }

    public string FormatMessage(string key, string language, params object[] args)
    {
        string message = GetMessage(key, language);
        return args.Length > 0 ? string.Format(message, args) : message;
    }
}