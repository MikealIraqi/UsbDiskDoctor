using System;
using System.Collections.Generic;
using UsbDiskDoctor.Core.Enums;
using UsbDiskDoctor.Core.Models;
using UsbDiskDoctor.Diagnostics.Models;
using UsbDiskDoctor.Knowledge.Services;
using UsbDiskDoctor.Repair.Planning;
using Xunit;

namespace UsbDiskDoctor.Diagnostics.Tests.Repair
{
    /// <summary>
    /// Minimal in-memory implementation of IKnowledgeService for testing.
    /// No external dependencies, no embedded resource needed.
    /// </summary>
    internal sealed class FakeKnowledgeService : IKnowledgeService
    {
        private readonly Dictionary<string, KnowledgeArticle> _articles =
            new(StringComparer.OrdinalIgnoreCase);

        public FakeKnowledgeService(params KnowledgeArticle[] articles)
        {
            foreach (var articleItem in articles)
            {
                if (!string.IsNullOrWhiteSpace(articleItem.Code))
                {
                    _articles[articleItem.Code] = articleItem;
                }
            }
        }

        public bool IsLoaded => true;

        public int ArticleCount => _articles.Count;

        public int Load() => _articles.Count;

        public KnowledgeArticle? GetArticle(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            _articles.TryGetValue(code, out var found);
            return found;
        }

        public IReadOnlyList<KnowledgeArticle> GetAllArticles()
        {
            return new List<KnowledgeArticle>(_articles.Values);
        }
    }

    /// <summary>
    /// Unit tests for <see cref="RepairPlanner"/>.
    /// </summary>
    public class RepairPlannerTests
    {
        private static DeviceDiagnosticReport MakeReport(
            params DiagnosticResult[] diagnostics)
        {
            return new DeviceDiagnosticReport
            {
                Device = new DeviceSummary { DeviceId = @"\\.\PHYSICALDRIVE1" },
                HealthEvaluation = new HealthEvaluationResult
                {
                    Diagnostics = diagnostics
                }
            };
        }

        private static DiagnosticResult MakeDiagnostic(string code,
            string titleAr = "عنوان", string titleEn = "Title")
        {
            return new DiagnosticResult
            {
                Code = code,
                TitleAr = titleAr,
                TitleEn = titleEn
            };
        }

        [Fact]
        public void Plan_ThrowsArgumentNullException_OnNullReport()
        {
            var sut = new RepairPlanner(new FakeKnowledgeService());

            Assert.Throws<ArgumentNullException>(() => sut.Plan(null!));
        }

        [Fact]
        public void Plan_EmptyDiagnostics_ReturnsEmptyList()
        {
            var sut = new RepairPlanner(new FakeKnowledgeService());
            var report = MakeReport();

            var proposals = sut.Plan(report);

            Assert.Empty(proposals);
        }

        [Fact]
        public void Plan_SmartPredictFailure_ReturnsDangerousProposal()
        {
            var sut = new RepairPlanner(new FakeKnowledgeService());
            var report = MakeReport(MakeDiagnostic("SMART_PREDICT_FAILURE"));

            var proposals = sut.Plan(report);

            Assert.Single(proposals);
            Assert.Equal(RiskLevel.Dangerous, proposals[0].RiskLevel);
            Assert.True(proposals[0].RequiresConfirmation);
            Assert.False(proposals[0].IsExecutableNow);
        }

        [Fact]
        public void Plan_RawFileSystem_ReturnsDangerousProposal()
        {
            var sut = new RepairPlanner(new FakeKnowledgeService());
            var report = MakeReport(MakeDiagnostic("RAW_FILESYSTEM"));

            var proposals = sut.Plan(report);

            Assert.Single(proposals);
            Assert.Equal(RiskLevel.Dangerous, proposals[0].RiskLevel);
        }

        [Fact]
        public void Plan_OperationalStatusError_ReturnsSafeProposal()
        {
            var sut = new RepairPlanner(new FakeKnowledgeService());
            var report = MakeReport(MakeDiagnostic("OPERATIONAL_STATUS_ERROR"));

            var proposals = sut.Plan(report);

            Assert.Single(proposals);
            Assert.Equal(RiskLevel.Safe, proposals[0].RiskLevel);
            Assert.False(proposals[0].RequiresConfirmation);
        }

        [Fact]
        public void Plan_DirtyBitSet_ReturnsMediumProposal()
        {
            var sut = new RepairPlanner(new FakeKnowledgeService());
            var report = MakeReport(MakeDiagnostic("DIRTY_BIT_SET"));

            var proposals = sut.Plan(report);

            Assert.Single(proposals);
            Assert.Equal(RiskLevel.Medium, proposals[0].RiskLevel);
        }

        [Fact]
        public void Plan_FileSystemCheckFailed_ReturnsMediumProposal()
        {
            var sut = new RepairPlanner(new FakeKnowledgeService());
            var report = MakeReport(MakeDiagnostic("FILESYSTEM_CHECK_FAILED"));

            var proposals = sut.Plan(report);

            Assert.Single(proposals);
            Assert.Equal(RiskLevel.Medium, proposals[0].RiskLevel);
        }

        [Fact]
        public void Plan_UnknownCode_ReturnsSafeByDefault()
        {
            var sut = new RepairPlanner(new FakeKnowledgeService());
            var report = MakeReport(MakeDiagnostic("SOMETHING_NEW_CODE"));

            var proposals = sut.Plan(report);

            Assert.Single(proposals);
            Assert.Equal(RiskLevel.Safe, proposals[0].RiskLevel);
        }

        [Fact]
        public void Plan_ArticleRequiresConfirmation_ForcesDangerous()
        {
            var article = new KnowledgeArticle
            {
                Code = "CUSTOM_CODE",
                TitleAr = "عنوان مخصص",
                TitleEn = "Custom Title",
                RequiresConfirmation = true
            };
            var sut = new RepairPlanner(new FakeKnowledgeService(article));
            var report = MakeReport(MakeDiagnostic("CUSTOM_CODE"));

            var proposals = sut.Plan(report);

            Assert.Single(proposals);
            Assert.Equal(RiskLevel.Dangerous, proposals[0].RiskLevel);
            Assert.True(proposals[0].RequiresConfirmation);
        }

        [Fact]
        public void Plan_UsesArticleTitle_WhenArticleExists()
        {
            var article = new KnowledgeArticle
            {
                Code = "SMART_PREDICT_FAILURE",
                TitleAr = "عنوان من المقال",
                TitleEn = "Article Title"
            };
            var sut = new RepairPlanner(new FakeKnowledgeService(article));
            var report = MakeReport(MakeDiagnostic("SMART_PREDICT_FAILURE",
                titleAr: "عنوان من التشخيص", titleEn: "Diagnostic Title"));

            var proposals = sut.Plan(report);

            Assert.Single(proposals);
            Assert.Equal("عنوان من المقال", proposals[0].DescriptionAr);
            Assert.Equal("Article Title", proposals[0].DescriptionEn);
        }

        [Fact]
        public void Plan_FallsBackToDiagnosticTitle_WhenArticleMissing()
        {
            var sut = new RepairPlanner(new FakeKnowledgeService());
            var report = MakeReport(MakeDiagnostic("UNKNOWN_CODE",
                titleAr: "عنوان تشخيصي", titleEn: "Diagnostic Title"));

            var proposals = sut.Plan(report);

            Assert.Single(proposals);
            Assert.Equal("عنوان تشخيصي", proposals[0].DescriptionAr);
            Assert.Equal("Diagnostic Title", proposals[0].DescriptionEn);
        }

        [Fact]
        public void Plan_MultipleDiagnostics_ReturnsSameCountProposals()
        {
            var sut = new RepairPlanner(new FakeKnowledgeService());
            var report = MakeReport(
                MakeDiagnostic("SMART_PREDICT_FAILURE"),
                MakeDiagnostic("RAW_FILESYSTEM"),
                MakeDiagnostic("DIRTY_BIT_SET"));

            var proposals = sut.Plan(report);

            Assert.Equal(3, proposals.Count);
        }

        [Fact]
        public void Plan_AlwaysSetsIsExecutableNowFalse()
        {
            var sut = new RepairPlanner(new FakeKnowledgeService());
            var report = MakeReport(
                MakeDiagnostic("SMART_PREDICT_FAILURE"),
                MakeDiagnostic("OPERATIONAL_STATUS_ERROR"),
                MakeDiagnostic("DIRTY_BIT_SET"));

            var proposals = sut.Plan(report);

            foreach (var proposal in proposals)
            {
                Assert.False(proposal.IsExecutableNow);
            }
        }
    }
}