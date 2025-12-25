using System.Text.RegularExpressions;

namespace SmokeSoft.Shared.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Checks if string is null or whitespace
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Checks if string is a valid email
    /// </summary>
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Truncates string to specified length
    /// </summary>
    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength) + suffix;
    }

    /// <summary>
    /// Converts string to Title Case
    /// </summary>
    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(value.ToLower());
    }

    /// <summary>
    /// Removes special characters from string
    /// </summary>
    public static string RemoveSpecialCharacters(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        return Regex.Replace(value, @"[^a-zA-Z0-9\s]", "");
    }
}
