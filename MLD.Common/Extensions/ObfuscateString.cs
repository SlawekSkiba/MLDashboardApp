namespace MLD.Common.Extensions;

public static class ObfuscateString
{
    public static string GetObfuscatedEmail(string email)
    {
        if (!email.HasValue())
        {
            return email;
        }

        var emailHandle = email.Substring(0, email.IndexOf("@", StringComparison.Ordinal));
        var domain = email.Substring(email.IndexOf("@", StringComparison.Ordinal) + 1,
                                        email.LastIndexOf(".", StringComparison.Ordinal) - (email.IndexOf("@", StringComparison.Ordinal) + 1));
        var xes = string.Join("", domain.Select(x => 'x'));
        var tld = email.Substring(email.LastIndexOf(".", StringComparison.Ordinal));
        return $"{emailHandle}@{xes}{tld}";
    }
}
