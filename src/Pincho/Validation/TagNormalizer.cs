using System.Text.RegularExpressions;

namespace Pincho.Validation;

/// <summary>
/// Provides tag normalization and validation utilities.
/// </summary>
/// <remarks>
/// Tags are normalized by converting to lowercase, trimming whitespace,
/// removing duplicates, and filtering invalid characters.
/// Valid characters: alphanumeric, hyphens, underscores.
/// </remarks>
public static class TagNormalizer
{
    private static readonly Regex ValidTagPattern = new Regex(@"^[a-z0-9\-_]+$", RegexOptions.Compiled);
    private static readonly Regex InvalidCharsPattern = new Regex(@"[^a-z0-9\-_]", RegexOptions.Compiled);

    /// <summary>
    /// Normalizes an array of tags by converting to lowercase, trimming whitespace,
    /// removing duplicates, and filtering invalid characters.
    /// </summary>
    /// <param name="tags">The tags to normalize.</param>
    /// <returns>The normalized tags array, or null if input is null or empty.</returns>
    /// <remarks>
    /// Normalization steps:
    /// 1. Convert to lowercase
    /// 2. Trim whitespace
    /// 3. Remove invalid characters (keep only alphanumeric, hyphens, underscores)
    /// 4. Remove empty strings
    /// 5. Remove duplicates
    /// 6. Preserve order (first occurrence)
    /// </remarks>
    public static string[]? NormalizeTags(string[]? tags)
    {
        if (tags == null || tags.Length == 0)
            return null;

        var normalized = tags
            .Select(tag => tag?.ToLowerInvariant().Trim() ?? string.Empty)
            .Select(tag => InvalidCharsPattern.Replace(tag, ""))
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct()
            .ToArray();

        return normalized.Length > 0 ? normalized : null;
    }

    /// <summary>
    /// Validates whether a tag is valid according to Pincho rules.
    /// </summary>
    /// <param name="tag">The tag to validate.</param>
    /// <returns>True if the tag is valid; otherwise, false.</returns>
    /// <remarks>
    /// Valid tags must:
    /// - Be lowercase
    /// - Contain only alphanumeric characters, hyphens, and underscores
    /// - Not be empty or whitespace
    /// </remarks>
    public static bool IsValidTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return false;

        return ValidTagPattern.IsMatch(tag);
    }
}
