using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Haack.AIDemoWeb.Library;

public static class StringExtensions
{
    public static string ToOrdinal(this int position)
    {
        return (position % 10) switch
        {
            1 when position % 100 != 11 => $"{position}st",
            2 when position % 100 != 12 => $"{position}nd",
            3 when position % 100 != 13 => $"{position}rd",
            _ => $"{position}th"
        };
    }

    /// <summary>
    /// Capitalizes the first letter of the string.
    /// </summary>
    /// <param name="text">The string to capitalize.</param>
    public static string Capitalize(this string text)
    {
        return text.Length switch
        {
            0 => text,
            1 => text.ToUpperInvariant(),
            _ => text[0].ToString().ToUpperInvariant() + text[1..]
        };
    }

    /// <summary>
    /// Un Capitalizes the first letter of the string.
    /// </summary>
    /// <param name="text">The string to uncapitalize.</param>
    public static string UnCapitalize(this string text)
    {
        return text.Length switch
        {
            0 => text,
#pragma warning disable CA1308
            1 => text.ToLowerInvariant(),
            _ => string.Concat((text[0] + string.Empty).ToLowerInvariant(), text.AsSpan(1))
#pragma warning restore CA1308
        };
    }

    /// <summary>
    /// Returns a string containing with a suffix truncated off. If the suffix is not found in the string, then
    /// the whole string is returned.
    /// </summary>
    /// <param name="s">The original string.</param>
    /// <param name="suffix">The suffix to remove</param>
    /// <param name="comparisonType">Determines whether or not to use case sensitive search.</param>
    public static string LeftBefore(this string s, string suffix, StringComparison comparisonType)
    {
        //Shortcut.
        if (suffix.Length > s.Length || suffix.Length == 0)
        {
            return s;
        }
        var suffixIndex = s.LastIndexOf(suffix, comparisonType);

        return suffixIndex < 0
            ? s
            : s[..suffixIndex];
    }

    /// <summary>
    /// Returns a string containing with a suffix truncated off.
    /// </summary>
    /// <param name="s">The original string.</param>
    /// <param name="delimiter">The delimiter that marks the end of the string to keep.</param>
    public static string LeftBefore(this string s, char delimiter)
    {
        var delimiterIndex = s.LastIndexOf(delimiter);

        return delimiterIndex < 0
            ? s
            : s[..delimiterIndex];
    }

    /// <summary>
    /// Strips the leading character from the beginning of the string if it's at the end, otherwise returns whole string.
    /// </summary>
    /// <param name="text">The original string</param>
    /// <param name="leadingCharacter">the leading character to strip.</param>
    public static string TrimLeadingCharacter(this string text, char leadingCharacter)
    {
        return text.Length > 0 && text[0] == leadingCharacter
            ? text[1..]
            : text;
    }

    /// <summary>
    /// Strips the trailing character from the end of the string if it's at the end, otherwise returns whole string.
    /// </summary>
    /// <param name="text">The original string</param>
    /// <param name="trailingCharacter">the leading character to strip.</param>
    public static string TrimTrailingCharacter(this string text, char trailingCharacter)
    {
        return text.Length > 0 && text[^1] == trailingCharacter
            ? text[..^1]
            : text;
    }

    /// <summary>
    /// Returns a <see cref="TrimResult"/> with whitespace trimmed off the beginning of the string and the number
    /// of characters trimmed.
    /// </summary>
    /// <param name="text">The text to trim.</param>
    /// <returns>A <see cref="TrimResult"/>.</returns>
    public static TrimResult TrimLeadingWhitespace(this string text)
    {
        var trimmed = text.TrimStart();
        return new TrimResult(trimmed, text.Length - trimmed.Length);
    }

    /// <summary>
    /// Returns a <see cref="TrimResult"/> with whitespace trimmed off the end of the string and the number
    /// of characters trimmed.
    /// </summary>
    /// <param name="text">The text to trim.</param>
    /// <returns>A <see cref="TrimResult"/>.</returns>
    public static TrimResult TrimTrailingWhitespace(this string text)
    {
        var trimmed = text.TrimEnd();
        return new TrimResult(trimmed, text.Length - trimmed.Length);
    }

    /// <summary>
    /// Ensures the string ends with the trailing character if it does not so already.
    /// </summary>
    /// <param name="text">The original string</param>
    /// <param name="trailingCharacter">The character to append</param>
    public static string EnsureTrailingCharacter(this string text, char trailingCharacter)
    {
        return text.Length == 0 || text[^1] != trailingCharacter
            ? text + trailingCharacter
            : text;
    }

    /// <summary>
    /// Strips the suffix off the end of the string if it's at the end, otherwise returns whole string.
    /// </summary>
    /// <param name="text">The original string</param>
    /// <param name="suffix">the string to strip.</param>
    /// <param name="comparison">The comparison type</param>
    public static string TrimSuffix(this string text, string suffix, StringComparison comparison)
    {
        return text.EndsWith(suffix, comparison)
            ? text[..^suffix.Length]
            : text;
    }

    public static string RightAfter(this string s, string prefix, StringComparison comparisonType)
    {
        if (prefix.Length > s.Length || prefix.Length == 0)
        {
            return s;
        }
        int prefixIndex = s.IndexOf(prefix, 0, comparisonType);

        return prefixIndex < 0
            ? s
            : s.Substring(prefixIndex + prefix.Length);
    }

    public static string RightAfter(this string s, char prefix)
    {
        int prefixIndex = s.IndexOf(prefix, 0);

        return prefixIndex < 0
            ? s
            : s.Substring(prefixIndex + 1);
    }

    public static string RightAfterLast(this string s, string prefix, StringComparison comparisonType)
    {
        if (prefix.Length > s.Length || prefix.Length == 0)
        {
            return s;
        }
        int prefixIndex = s.LastIndexOf(prefix, comparisonType);

        return prefixIndex < 0
            ? s
            : s.Substring(prefixIndex + prefix.Length);
    }

    public static string RightAfterLast(this string s, char prefix)
    {
        int prefixIndex = s.LastIndexOf(prefix);

        return prefixIndex < 0
            ? s
            : s.Substring(prefixIndex + 1);
    }

    public static string DefaultIfEmpty(this string value, string defaultValue)
    {
        return value.Length > 0 ? value : defaultValue;
    }

    public static string PrefixWithIndefiniteArticle(this string sentence)
    {
        const string vowels = "aeiou";
        if (sentence.Length == 0)
            return string.Empty;
        int whitespaceIndex = sentence.IndexOf(' ', StringComparison.Ordinal);
        string nextWord = whitespaceIndex < 0
            ? sentence
            : sentence.Substring(0, whitespaceIndex);
        var article =
            nextWord.StartsWith("hour", StringComparison.OrdinalIgnoreCase)
            || nextWord.StartsWith("honor", StringComparison.OrdinalIgnoreCase)
            || !nextWord.Equals("unique", StringComparison.OrdinalIgnoreCase)
            && !nextWord.Equals("one", StringComparison.OrdinalIgnoreCase)
            && !nextWord.StartsWith("one-", StringComparison.OrdinalIgnoreCase)
            && vowels.Any(vowel => vowel == nextWord[0])
                ? "An"
                : "A";
        return $"{article} {sentence}";
    }

    public static string EnsureEndsWithPunctuation(this string sentence)
    {
        if (sentence.Length == 0)
            return sentence;
        var lastChar = sentence[^1];
        const string punctuation = ".!?\"";
        return punctuation.Contains(lastChar, StringComparison.Ordinal) ? sentence : sentence + '.';
    }

    /// <summary>
    /// Appends the text to the current value if the text to append is not empty.
    /// </summary>
    /// <param name="value">The value to append to</param>
    /// <param name="textToAppend">The text to append</param>
    /// <param name="appendPrefix">The prefix to put in front of the text to append if it's not empty</param>
    /// <param name="appendSuffix">The suffix to put at the end of the text to append if it's not empty</param>
    /// <returns></returns>
    public static string AppendIfNotEmpty(
        this string value,
        string textToAppend,
        string appendPrefix = " ",
        string appendSuffix = "")
    {
        return value + (textToAppend is { Length: > 0 }
            ? $"{appendPrefix}{textToAppend}{appendSuffix}"
            : string.Empty);
    }

    /// <summary>
    /// Prepends the text to the current value if the string doesn't already start with the prefix.
    /// If the value is empty, returns empty string.
    /// </summary>
    /// <param name="value">The value to prepend to</param>
    /// <param name="prefix">The text to prepend</param>
    public static string EnsurePrefix(
        this string value,
        string prefix)
    {
        return value.Length is 0 || value.StartsWith(prefix, StringComparison.Ordinal)
            ? value
            : prefix + value;
    }

    /// <summary>
    /// Prepends the text to the current value if the string doesn't already start with the prefix.
    /// If the value is empty, returns empty string.
    /// </summary>
    /// <param name="value">The value to prepend to</param>
    /// <param name="prefix">The text to prepend</param>
    public static string EnsurePrefix(
        this string value,
        char prefix)
    {
        return value.Length is 0 || value.StartsWith(prefix)
            ? value
            : prefix + value;
    }

    public static string ToQuantity(this int count, string singular)
    {
        return count.ToQuantity(singular, singular + "s");
    }

    public static string ToQuantity(this int count, string singular, string plural)
    {
        return count switch
        {
            0 => $"0 {plural}",
            1 => $"1 {singular}",
            _ => $"{count} {plural}"
        };
    }

    static readonly char[] WordBoundaryCharacters = {'_', '-', ' '};

    /// <summary>
    /// Split a string into contingent parts. Especially useful for case conversions.
    /// </summary>
    /// <param name="text"></param>
    static string[] GetWords(string text)
    {
        // Types of splits to consider:
        // 1. Spaces
        // 2. Dashes
        // 3. Underscores
        // 4. Capital letters preceded by lowercase characters

        // This regex is:
        // (?:[-_\s]) => find a space, underscore, or dash
        // OR
        // (?<=[a-z])(?=[A-Z]) => find capital letters preceded by lowercase
        var splitRegex = new Regex("([-_\\s])|(?<=[a-z])(?=[A-Z])");
        var allMatches = splitRegex.Split(text);

        // Regex.Split returns matching characters in the case of _, -, and spaces; remove them from the
        // match list before returning results to the caller.
        // See "Remarks" section in documentation for more info:
        // https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.split?view=netcore-3.1
        var words = allMatches.Where(w => w.IndexOfAny(WordBoundaryCharacters) < 0);

        return words.ToArray();
    }

    /// <summary>
    /// Convert a string to PascalCase from another format.
    /// </summary>
    /// <param name="text"></param>
    public static string ToPascalCase(this string text)
    {
        var words = GetWords(text);
#pragma warning disable CA1308
        var capitalizedWords = words.Select(w => w.ToLowerInvariant().Capitalize());
#pragma warning restore CA1308
        return string.Join("", capitalizedWords);
    }

    /// <summary>
    /// Convert a string to camelCase from another format.
    /// </summary>
    /// <param name="text"></param>
    public static string ToCamelCase(this string text)
    {
        return text.ToPascalCase().UnCapitalize();
    }

    static readonly Regex InvalidSlugCharactersRegex = new(@"[^\w0-9\s\.-]",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    static readonly Regex WhiteSpaceRegex = new(@"[\s-]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Converts a string to a lower-cased dash separated string suitable for using in a URL.
    /// </summary>
    /// <remarks>
    /// Similar to the <see cref="ToDashCase"/> method, but only splits on spaces.
    /// </remarks>
    /// <param name="text">The text to convert.</param>
    /// <returns>A string that can be used in a path segment of a URL.</returns>
    public static string ToSlug(this string text)
    {
#pragma warning disable CA1308
        var lowerCase = text.ToLowerInvariant().RemoveDiacritics();
#pragma warning restore CA1308

        var clean = InvalidSlugCharactersRegex.Replace(lowerCase, string.Empty);

        var words = WhiteSpaceRegex.Split(clean)
            .Where(word => word is { Length: > 0 });
        return string.Join('-', words);
    }

    /// <summary>
    /// Converts diacritics to an ASCII equivalent.
    /// </summary>
    /// <remarks>
    /// CREDIT: https://meta.stackexchange.com/a/300564/133746
    /// </remarks>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string RemoveDiacritics(this string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        for (var i = 0; i < normalized.Length; i++)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(normalized[i]) != UnicodeCategory.NonSpacingMark)
                sb.Append(normalized[i]);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Convert a string to snake_case from another format.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    public static string ToSnakeCase(this string text)
    {
        var words = GetWords(text);
#pragma warning disable CA1308
        var lowerCasedWords = words.Select(w => w.ToLowerInvariant());
#pragma warning restore CA1308
        return string.Join("_", lowerCasedWords);
    }

    /// <summary>
    /// Convert a string to dash-case from another format.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    public static string ToDashCase(this string text)
    {
        var words = GetWords(text);
        var lowerCasedWords = words.Select(w => w.UnCapitalize());
        return string.Join("-", lowerCasedWords);
    }

    /// <summary>
    /// Parses the specified version string into its SemVer components.
    /// </summary>
    /// <param name="version">A version string in the format Major.Minor.Patch.</param>
    /// <returns>A tuple of three integers.</returns>
    /// <exception cref="ArgumentException">If the version string is not three components or any part is not an integer.</exception>
    public static (int, int, int) ToSemver(this string version)
    {
        var parts = version.Split('.');
        if (parts.Length != 3)
        {
            throw new ArgumentException($"{version} does not match SemVer", nameof(version));
        }

        int ParsePart(int index, string name)
        {
            var part = parts[index];
            return int.TryParse(part, out var versionPart)
                ? versionPart
                : throw new ArgumentException($"{name} version {part} is not an integer.", nameof(version));
        }

        return (ParsePart(0, "Major"), ParsePart(1, "Minor"), ParsePart(2, "Patch"));
    }

    /// <summary>
    /// Returns true if the text ends with any of the specified suffixes.
    /// </summary>
    /// <param name="text">The text to test.</param>
    /// <param name="comparison">The type of comparison to conduct.</param>
    /// <param name="suffixes">The set of suffixes to check.</param>
    /// <returns>True if the text ends with one of the suffixes, otherwise false.</returns>
    public static bool EndsWithAny(this string text, StringComparison comparison, params string[] suffixes)
    {
        return suffixes.Any(suffix => text.EndsWith(suffix, comparison));
    }

    /// <summary>
    /// Returns true if the text ends with any of the specified suffixes using an Ordinal comparison.
    /// </summary>
    /// <param name="text">The text to test.</param>
    /// <param name="suffixes">The set of suffixes to check.</param>
    /// <returns>True if the text ends with one of the suffixes, otherwise false.</returns>
    public static bool EndsWithAny(this string text, params string[] suffixes)
    {
        return suffixes.Any(suffix => text.EndsWith(suffix, StringComparison.Ordinal));
    }

    public static string? ToNullIfEmpty(this string? text) => string.IsNullOrWhiteSpace(text) ? null : text;

    public static string ToStringInvariant<T>(this T value, string? format = null)
    {
        return value switch
        {
            IFormattable formattable => formattable.ToString(format, CultureInfo.InvariantCulture),
            null => "",
            _ => value.ToString() ?? "",
        };
    }

    public static string TruncateToLength(this string s, int maxLength, bool appendEllipses = false) => s.Length <= maxLength
        ? s
        : s[..maxLength] + (appendEllipses ? "…" : string.Empty);

    public static string TruncateAtWordBoundary(this string text, int truncateLength, bool appendEllipses)
    {
        if (truncateLength >= text.Length)
        {
            return text;
        }

        var truncateIndex = truncateLength - 1;

        while (truncateIndex < text.Length && !char.IsWhiteSpace(text[truncateIndex]))
        {
            truncateIndex++;
        }

        return truncateIndex == text.Length
            ? text
            : text[..truncateIndex] + (appendEllipses ? "…" : string.Empty);
    }

    /// <summary>
    /// Find the longest common prefix in a set of strings.
    /// </summary>
    /// <param name="strings">The set of strings to search for a common prefix.</param>
    /// <param name="threshold">The % of strings with the prefix.</param>
    /// <returns>The longest common prefix.</returns>
    public static string FindLongestCommonPrefix(this IEnumerable<string> strings, double threshold = 0.8)
    {
        var sorted = strings.OrderBy(s => s.Length).ToList();
        return sorted switch
        {
            [] => string.Empty,
            [var single] => single,
            _ => FindLongestCommonPrefixCore(sorted, threshold)
        };

        static string FindLongestCommonPrefixCore(IReadOnlyList<string> sorted, double threshold)
        {
            string first = sorted[0];
            string last = sorted[^1];

            int minLength = Math.Min(first.Length, last.Length);

            for (int i = minLength; i >= 0; i--)
            {
                if (PercentWithPrefix(first[..i], sorted) >= threshold)
                    return first[..i];
            }

            return string.Empty;
        }

        static double PercentWithPrefix(string prefix, IReadOnlyCollection<string> strings)
        {
            var count = strings.Count(s => s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            return (double)count / strings.Count;
        }
    }

    static readonly JsonSerializerOptions PrettifyJsonOptions = new JsonSerializerOptions { WriteIndented = true };

    public static string JsonPrettify(this string json)
    {
        using var jDoc = JsonDocument.Parse(json);
        try
        {
            return JsonSerializer.Serialize(jDoc, PrettifyJsonOptions);
        }
        catch(JsonException)
        {
            return json;
        }
    }
}

/// <summary>
/// A result of a string trimming operation.
/// </summary>
/// <param name="Text">The trimmed string.</param>
/// <param name="TrimmedCount">The number of characters trimmed.</param>
public record TrimResult(string Text, int TrimmedCount);
