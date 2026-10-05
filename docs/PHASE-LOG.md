# UsbDiskDoctor — Phase Log

سجل مختصر لكل مرحلة مكتملة.

---

## Phases 0-8 — ملخص
- Setup + Enums + Models + Serilog
- USB Discovery + MVVM + TabControl
- Health Check + Diagnostic Engine + Reports (HTML/JSON/WebView2)
- Knowledge Base (6 articles + EmbeddedResource)
- Repair Planning + Safe Execution (whitelist + token + timeout)
- **57 اختبار** ناجح قبل Phase 9

---

## Phase 9 — Basic Recovery ✅
- **Commits**: 407bd9e → (هذا الـ commit)
- **النتيجة**:
  - Models: RecoveryScanMode, RecoveryOptions, RecoveryProgressInfo, RecoveredFileInfo
  - Scanning: IVolumeScanner + MountedVolumeScanner (read-only enumerate)
  - Restoring: IFileRestorer + SafeFileRestorer (multi-layer validation)
  - Service: IRecoveryService + MountedVolumeRecoveryService (orchestrator)
  - 12 اختبار أمان (بدون file I/O فعلي)
- **قرارات أمنية**:
  - **Mounted volume فقط** — لا raw device access في Phase 9
  - raw device + carving → Phase 10 (يحتاج admin)
  - SourceFullPath للـ mounted (SourceOffset يبقى للـ raw لاحقاً)
  - 3 طبقات validation قبل أي كتابة:
    1. Target ليس داخل Source
    2. SourceRoot ≠ TargetRoot
    3. Free space + 1MB buffer
  - لا File.Delete، لا File.Move، لا overwrite
  - أسماء فريدة تلقائية: file.jpg → file_1.jpg → file_2.jpg → GUID
  - لا exceptions تخرج من الطبقات الأمنية (ترجع Failed + رسالة)
- **ملاحظة UI**: لا Recovery tab بعد — يُضاف في Phase 11 مع UI كامل
- **التحقق**: 69 اختبار ناجح (26 Core + 43 Diagnostics)

---

## ⚠️ ملاحظة اختبارية
**لم يُختبر مع فلاشة USB حقيقية.** Filter `InterfaceType='USB'` قد لا يلتقط UASP HDDs.
الجهاز الحالي: قرص داخلي واحد فقط (NVMe) — لا يوجد قسم ثانٍ لاختبار restore فعلي.

---

## Phase 10 — Advanced Recovery (قيد التخطيط)
- **الهدف**: Raw device + File Carving لأنواع محددة
- **مخطط**:
  - `RawDiskReader` (Windows-only، يحتاج Admin)
  - Sector-level reading (512B/4KB alignment)
  - Signature detection: JPEG (FF D8 FF), PNG (89 50 4E 47), PDF (%PDF)
  - File carving من أي مكان في القرص
  - `IDeepRecoveryService` جديد