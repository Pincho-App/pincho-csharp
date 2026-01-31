using Pincho.Validation;
using Xunit;

namespace Pincho.Tests;

public class TagNormalizerTests
{
    [Fact]
    public void NormalizeTags_WithNull_ReturnsNull()
    {
        var result = TagNormalizer.NormalizeTags(null);
        Assert.Null(result);
    }

    [Fact]
    public void NormalizeTags_WithEmptyArray_ReturnsNull()
    {
        var result = TagNormalizer.NormalizeTags(Array.Empty<string>());
        Assert.Null(result);
    }

    [Fact]
    public void NormalizeTags_ConvertsToLowercase()
    {
        var tags = new[] { "PROD", "PrOd", "prod" };
        var result = TagNormalizer.NormalizeTags(tags);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("prod", result[0]);
    }

    [Fact]
    public void NormalizeTags_TrimsWhitespace()
    {
        var tags = new[] { "  prod  ", " backend ", "frontend  " };
        var result = TagNormalizer.NormalizeTags(tags);

        Assert.NotNull(result);
        Assert.Equal(3, result.Length);
        Assert.Contains("prod", result);
        Assert.Contains("backend", result);
        Assert.Contains("frontend", result);
    }

    [Fact]
    public void NormalizeTags_RemovesDuplicates()
    {
        var tags = new[] { "prod", "PROD", "Prod", "backend", "backend" };
        var result = TagNormalizer.NormalizeTags(tags);

        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Contains("prod", result);
        Assert.Contains("backend", result);
    }

    [Fact]
    public void NormalizeTags_PreservesOrder()
    {
        var tags = new[] { "zulu", "alpha", "bravo" };
        var result = TagNormalizer.NormalizeTags(tags);

        Assert.NotNull(result);
        Assert.Equal(3, result.Length);
        Assert.Equal("zulu", result[0]);
        Assert.Equal("alpha", result[1]);
        Assert.Equal("bravo", result[2]);
    }

    [Fact]
    public void NormalizeTags_RemovesInvalidCharacters()
    {
        var tags = new[] { "tag@123", "tag!test", "tag$name", "tag%foo" };
        var result = TagNormalizer.NormalizeTags(tags);

        Assert.NotNull(result);
        Assert.Equal(4, result.Length);
        Assert.Contains("tag123", result);
        Assert.Contains("tagtest", result);
        Assert.Contains("tagname", result);
        Assert.Contains("tagfoo", result);
    }

    [Fact]
    public void NormalizeTags_AllowsValidCharacters()
    {
        var tags = new[] { "tag-123", "tag_test", "tag-name_123" };
        var result = TagNormalizer.NormalizeTags(tags);

        Assert.NotNull(result);
        Assert.Equal(3, result.Length);
        Assert.Contains("tag-123", result);
        Assert.Contains("tag_test", result);
        Assert.Contains("tag-name_123", result);
    }

    [Fact]
    public void NormalizeTags_RemovesEmptyStrings()
    {
        var tags = new[] { "prod", "", "  ", "backend" };
        var result = TagNormalizer.NormalizeTags(tags);

        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Contains("prod", result);
        Assert.Contains("backend", result);
    }

    [Fact]
    public void NormalizeTags_RemovesTagsThatBecomeEmpty()
    {
        var tags = new[] { "prod", "!!!", "@@@", "backend" };
        var result = TagNormalizer.NormalizeTags(tags);

        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Contains("prod", result);
        Assert.Contains("backend", result);
    }

    [Fact]
    public void NormalizeTags_ReturnsNullIfAllTagsInvalid()
    {
        var tags = new[] { "!!!", "@@@", "   " };
        var result = TagNormalizer.NormalizeTags(tags);

        Assert.Null(result);
    }

    [Fact]
    public void NormalizeTags_ComplexScenario()
    {
        var tags = new[] { "  PROD  ", "prod", "Backend@123", "  ", "test-env_1", "TEST-ENV_1", "!!!" };
        var result = TagNormalizer.NormalizeTags(tags);

        Assert.NotNull(result);
        Assert.Equal(3, result.Length);
        Assert.Equal("prod", result[0]);
        Assert.Equal("backend123", result[1]);
        Assert.Equal("test-env_1", result[2]);
    }

    [Fact]
    public void IsValidTag_WithNull_ReturnsFalse()
    {
        Assert.False(TagNormalizer.IsValidTag(null!));
    }

    [Fact]
    public void IsValidTag_WithEmpty_ReturnsFalse()
    {
        Assert.False(TagNormalizer.IsValidTag(""));
    }

    [Fact]
    public void IsValidTag_WithWhitespace_ReturnsFalse()
    {
        Assert.False(TagNormalizer.IsValidTag("   "));
    }

    [Fact]
    public void IsValidTag_WithValidLowercaseTag_ReturnsTrue()
    {
        Assert.True(TagNormalizer.IsValidTag("prod"));
    }

    [Fact]
    public void IsValidTag_WithValidTagWithHyphens_ReturnsTrue()
    {
        Assert.True(TagNormalizer.IsValidTag("test-env"));
    }

    [Fact]
    public void IsValidTag_WithValidTagWithUnderscores_ReturnsTrue()
    {
        Assert.True(TagNormalizer.IsValidTag("test_env"));
    }

    [Fact]
    public void IsValidTag_WithValidTagWithNumbers_ReturnsTrue()
    {
        Assert.True(TagNormalizer.IsValidTag("env123"));
    }

    [Fact]
    public void IsValidTag_WithUppercase_ReturnsFalse()
    {
        Assert.False(TagNormalizer.IsValidTag("PROD"));
    }

    [Fact]
    public void IsValidTag_WithInvalidCharacters_ReturnsFalse()
    {
        Assert.False(TagNormalizer.IsValidTag("tag@test"));
    }

    [Fact]
    public void IsValidTag_WithSpaces_ReturnsFalse()
    {
        Assert.False(TagNormalizer.IsValidTag("tag test"));
    }
}
