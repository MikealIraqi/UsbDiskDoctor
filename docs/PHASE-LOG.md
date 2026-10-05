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
- **Commits**: fe034ee → (هذا الـ commit)
- **النتيجة**:
  - IDiagnosticEngine + DiagnosticEngine (fail-soft orchestrator)
  - IReportGenerator + JsonReportGenerator + HtmlReportGenerator (RTL عربي)
  - Reporting TFM تغيّر إلى net8.0-windows
  - ReportViewModel + MainViewModel محدّث
  - **WebView2 Report tab** مع fallback نصي
  - زر "فحص كامل"
  - 9 اختبارات جديدة (Json + Html) — 20 اختبار إجمالي
- **قرارات**:
  - Reporting TFM = net8.0-windows (يعتمد Diagnostics)
  - HTML في WebView2 بدلاً من متصفح خارجي
  - inline CSS (offline 100%)
  - UnsafeRelaxedJsonEscaping + JsonStringEnumConverter
  - WebUtility.HtmlEncode لكل نص مستخدم (XSS prevention)
  - تقرير يُعرض تلقائياً + لا يُحفظ بعد (الحفظ في Phase 6+)

---

## ⚠️ ملاحظة اختبارية
**لم يُختبر مع فلاشة USB حقيقية.** Filter InterfaceType='USB' قد لا يلتقط UASP HDDs. مؤجل لـ Phase 12.

---

## Phase 6 — Knowledge Base (قيد التخطيط)
- **الهدف**: قاعدة معرفة JSON + article lookup
- **مخطط**:
  - KnowledgeArticle JSON (data/knowledge.json)
  - IKnowledgeService + JsonKnowledgeService
  - ربط problem codes بـ articles
  - عرض في TabDetails