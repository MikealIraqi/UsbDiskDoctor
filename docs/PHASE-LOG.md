# UsbDiskDoctor — Phase Log

سجل مختصر لكل مرحلة مكتملة. يُقرأ عند بدء محادثة جديدة.

---

## Phase 0 — Solution Scaffold
- **Commit**: 65396da
- 9 مشاريع (7 src + 2 tests)، 14 مرجع

## Phase 1 — Enums + Models + Serilog
- **Commits**: 3b1839e → e639382
- 9 Enums + 7 Models + LoggingSetup

## Phase 2 — USB Discovery
- **Commits**: 48bb193 → c5bcb0a
- IDeviceDiscoveryService + WmiDeviceDiscoveryService + MVVM يدوي
- **قرار**: TFM = net8.0-windows لأي مشروع Windows APIs

## Phase 3 — Device Details + Volumes
- **Commits**: b7d7390 → 73879fa
- CommunityToolkit.Mvvm 8.4.2
- IVolumeReader + WmiVolumeReader
- TabControl (الأجهزة + التفاصيل)

## Phase 4 — Health Check ✅
- **Commits**: dec319b → 80f777f
- ISmartReader, IFileSystemChecker, IHealthEvaluator
- 11 اختبار لـ HealthEvaluator

## Phase 5 — Diagnostic Engine + Reports ✅
- **Commits**: fe034ee → e8fff87
- IDiagnosticEngine + DiagnosticEngine (fail-soft orchestrator)
- IReportGenerator + JsonReportGenerator + HtmlReportGenerator (RTL عربي)
- WebView2 Report tab + زر "فحص كامل"
- 9 اختبارات جديدة → 20 إجمالي Diagnostics.Tests

## Phase 6 — Knowledge Base ✅
- **Commits**: ce2071c → (هذا الـ commit)
- **النتيجة**:
  - IKnowledgeService + JsonKnowledgeService
  - knowledge.json مُضمَّن كـ EmbeddedResource (LogicalName صريح)
  - 6 مقالات: SMART_PREDICT_FAILURE, OPERATIONAL_STATUS_ERROR,
    OPERATIONAL_STATUS_DEGRADED, RAW_FILESYSTEM,
    FILESYSTEM_CHECK_FAILED, DIRTY_BIT_SET
  - كل مقال: TitleAr/En, CauseAr/En, SafeActions, DangerousActions,
    RequiresConfirmation, ExternalSearchKeywords
  - 9 اختبارات JsonKnowledgeService
- **قرارات**:
  - EmbeddedResource بدلاً من ملف خارجي (يستحيل يضيع)
  - LogicalName صريح (لا يعتمد على مسار)
  - Idempotent Load + EnsureLoaded auto
  - OrdinalIgnoreCase للبحث عن الأكواد
  - اختبارات Knowledge في Core.Tests (احترام "2 test projects")
- **التحقق العملي**: GetManifestResourceNames() يرجع الاسم الصحيح

---

## ⚠️ ملاحظة اختبارية
**لم يُختبر مع فلاشة USB حقيقية.** Filter InterfaceType='USB' قد لا يلتقط UASP HDDs. مؤجل لـ Phase 12.

---

## Phase 7 — Repair Proposals (قيد التخطيط)
- **الهدف**: ربط Diagnostics بـ Knowledge لإنتاج RepairProposal
- **مخطط**:
  - IRepairPlanner + RepairPlanner
  - لكل DiagnosticResult مع Code → KnowledgeArticle → RepairProposal
  - RiskLevel من Article.RequiresConfirmation + Code
  - عرض في TabDetails