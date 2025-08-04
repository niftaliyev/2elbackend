using System.Text.RegularExpressions;

namespace TwoHandApp.Regexs;

public static class ValidEmail
{
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var regex = new Regex(@"^[a-zA-Z0-9._%+-]{3,}@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        return regex.IsMatch(email);
    }
}
