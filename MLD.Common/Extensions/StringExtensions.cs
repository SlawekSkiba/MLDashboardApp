using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MLD.Common.Extensions;

public static class StringExtensions
{
    public static string ToUTF8(this string text)
    {
        if (string.IsNullOrEmpty(text)) throw new ArgumentException($"{nameof(text)} is null or empty!");

        return Encoding.UTF8.GetString(Encoding.Default.GetBytes(text));
    }

    public static string HideCharacters(this string? value, int afterCharacterNumber)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        afterCharacterNumber = Math.Max(0, Math.Min(value.Length, afterCharacterNumber));

        return value.Substring(0, afterCharacterNumber) + new string('*', value.Length - afterCharacterNumber);
    }

    /// <summary>
    /// Just convenience sugar syntax for !string.IsNullOrWhiteSpace(str)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool HasValue([NotNullWhen(true)] this string? str)
    {
        return !string.IsNullOrWhiteSpace(str);
    }

    public static bool EqualsIgnoreCase(this string? str, string input)
    {
        if (str == null || input == null)
        {
            return str == input;
        }

        return str.Equals(input, StringComparison.InvariantCultureIgnoreCase);
    }
    public static string Capitalize(this string input)
    {
        switch (input)
        {
            case null: throw new ArgumentNullException(nameof(input));
            case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
            default: return input.First().ToString().ToUpper() + input.Substring(1);
        }
    }
    public static bool ContainsIgnoreCase(this string str, string input)
    {
        if (str == null || input == null)
        {
            return false;
        }

        return str.IndexOf(input, StringComparison.InvariantCultureIgnoreCase) >= 0;
    }
}