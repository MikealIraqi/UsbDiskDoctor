using UsbDiskDoctor.Knowledge.Services;
using Xunit;

namespace UsbDiskDoctor.Core.Tests.Knowledge
{
    /// <summary>
    /// Unit tests for <see cref="JsonKnowledgeService"/>.
    /// Verifies embedded resource loading and article lookup.
    /// </summary>
    public class JsonKnowledgeServiceTests
    {
        [Fact]
        public void Load_ReturnsNineArticles()
        {
            var sut = new JsonKnowledgeService();

            var count = sut.Load();

            Assert.Equal(9, count);
            Assert.True(sut.IsLoaded);
            Assert.Equal(9, sut.ArticleCount);
        }

        [Fact]
        public void Load_IsIdempotent_SecondCallReturnsSameCount()
        {
            var sut = new JsonKnowledgeService();

            var firstCall = sut.Load();
            var secondCall = sut.Load();

            Assert.Equal(firstCall, secondCall);
            Assert.Equal(9, secondCall);
        }

        [Fact]
        public void GetArticle_ExistingCode_ReturnsArticleWithContent()
        {
            var sut = new JsonKnowledgeService();

            var article = sut.GetArticle("SMART_PREDICT_FAILURE");

            Assert.NotNull(article);
            Assert.Equal("SMART_PREDICT_FAILURE", article!.Code);
            Assert.NotEmpty(article.TitleAr);
            Assert.NotEmpty(article.TitleEn);
            Assert.NotEmpty(article.SafeActions);
            Assert.NotEmpty(article.DangerousActions);
            Assert.True(article.RequiresConfirmation);
        }

        [Fact]
        public void GetArticle_IsCaseInsensitive()
        {
            var sut = new JsonKnowledgeService();

            var lowercase = sut.GetArticle("smart_predict_failure");
            var uppercase = sut.GetArticle("SMART_PREDICT_FAILURE");

            Assert.NotNull(lowercase);
            Assert.NotNull(uppercase);
            Assert.Equal(lowercase!.Code, uppercase!.Code);
        }

        [Fact]
        public void GetArticle_UnknownCode_ReturnsNull()
        {
            var sut = new JsonKnowledgeService();

            var article = sut.GetArticle("NEVER_EXISTS_12345");

            Assert.Null(article);
        }

        [Fact]
        public void GetArticle_NullOrWhitespace_ReturnsNull()
        {
            var sut = new JsonKnowledgeService();

            Assert.Null(sut.GetArticle(null!));
            Assert.Null(sut.GetArticle(string.Empty));
            Assert.Null(sut.GetArticle("   "));
        }

        [Fact]
        public void GetAllArticles_ReturnsAllNineArticles()
        {
            var sut = new JsonKnowledgeService();

            var allArticles = sut.GetAllArticles();

            Assert.Equal(9, allArticles.Count);
        }

        [Fact]
        public void GetAllArticles_EveryArticleHasRequiredFields()
        {
            var sut = new JsonKnowledgeService();

            var allArticles = sut.GetAllArticles();

            foreach (var articleItem in allArticles)
            {
                Assert.False(string.IsNullOrWhiteSpace(articleItem.Code));
                Assert.False(string.IsNullOrWhiteSpace(articleItem.TitleAr));
                Assert.False(string.IsNullOrWhiteSpace(articleItem.TitleEn));
                Assert.False(string.IsNullOrWhiteSpace(articleItem.CauseAr));
                Assert.False(string.IsNullOrWhiteSpace(articleItem.CauseEn));
                Assert.NotNull(articleItem.SafeActions);
                Assert.NotNull(articleItem.DangerousActions);
                Assert.NotNull(articleItem.ExternalSearchKeywords);
            }
        }

        [Fact]
        public void AllExpectedProblemCodes_HaveMatchingArticles()
        {
            var sut = new JsonKnowledgeService();

            var expectedCodes = new[]
            {
                "SMART_PREDICT_FAILURE",
                "OPERATIONAL_STATUS_ERROR",
                "OPERATIONAL_STATUS_DEGRADED",
                "RAW_FILESYSTEM",
                "FILESYSTEM_CHECK_FAILED",
                "DIRTY_BIT_SET",
                "DEVICE_SIZE_ZERO",
                "NO_VOLUMES_DETECTED"
            };

            foreach (var codeToCheck in expectedCodes)
            {
                var matchedArticle = sut.GetArticle(codeToCheck);
                Assert.NotNull(matchedArticle);
                Assert.Equal(codeToCheck, matchedArticle!.Code);
            }
        }
    }
}