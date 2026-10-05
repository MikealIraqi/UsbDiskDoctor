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
- 11 اختبار

## Phase 5 — Diagnostic Engine + Reports ✅
- **Commits**: fe034ee → e8fff87
- IDiagnosticEngine + DiagnosticEngine
- IReportGenerator + JsonReportGenerator + HtmlReportGenerator (RTL)
- WebView2 Report tab + زر "فحص كامل"
- 20 اختبار في Diagnostics.Tests

## Phase 6 — Knowledge Base ✅
- **Commits**: ce2071c → d97e8a1
- IKnowledgeService + JsonKnowledgeService
- knowledge.json EmbeddedResource (LogicalName صريح)
- 6 مقالات لكل problem code
- 9 اختبارات JsonKnowledgeService في Core.Tests

## Phase 7 — Repair Proposals ✅
- **Commits**: 81bc72f → (هذا الـ commit)
- **النتيجة**:
  - IRepairPlanner + RepairPlanner (منطق بحت، لا تنفيذ)
  - RiskLevel classification rules:
    * SMART_PREDICT_FAILURE → Dangerous
    * RAW_FILESYSTEM → Dangerous
    * OPERATIONAL_STATUS_* → Safe
    * FILESYSTEM_CHECK_FAILED → Medium
    * DIRTY_BIT* → Medium
    * Fallback: article.RequiresConfirmation ? Dangerous : Safe
  - CommandPreview نصوص وصفيّة فقط (لا Process.Start)
  - IsExecutableNow = false دائماً
  - Repair.csproj: TFM → net8.0-windows + refs Diagnostics + Knowledge
  - 13 اختبار RepairPlanner (Fake IKnowledgeService)
- **قرارات**:
  - RiskLevel ثابت عبر الكود (rule-based، ليس من Article فقط)
  - Article يُستخدم للعناوين الوصفية فقط
  - لا تنفيذ فعلي (المرحلة 8)
  - Fake KnowledgeService يدوي (بدون Moq)

---

## ⚠️ ملاحظة اختبارية
**لم يُختبر مع فلاشة USB حقيقية.** Filter InterfaceType='USB' قد لا يلتقط UASP HDDs. مؤجل لـ Phase 12.

---

## Phase 8 — Safe Execution (قيد التخطيط)
- **الهدف**: تنفيذ الإصلاحات الآمنة بعد موافقة المستخدم
- **مخطط**:
  - IRepairExecutor + SafeRepairExecutor
  - Level 1: read-only commands
  - Level 2: low-risk مع موافقة
  - Level 3: يحتاج كتابة "FORMAT" صريحة
  - CommandConfirmationDialog في UI
  - Audit log لكل عملية