# UsbDiskDoctor — Phase Log

سجل مختصر لكل مرحلة مكتملة.

---

## Phases 0-10 — ملخص
- Setup + Enums + Models + Serilog
- USB Discovery (مع UASP) + MVVM + TabControl
- Health Check + Diagnostics + Reports (HTML/JSON/WebView2)
- Knowledge Base (8 articles + EmbeddedResource)
- Repair Planning + Safe Execution (whitelist + token + timeout)
- Basic Recovery (MountedVolumeScanner + SafeFileRestorer + orchestrator)
- Advanced Recovery (SectorReader + FileCarver: JPEG/PNG/PDF)
- **78 اختبار** ناجح قبل Phase 11

---

## Phase 11 — Polish بعد الاختبار الحقيقي ✅
- **Commits**: ee2f7b7 → (هذا الـ commit)

### 11.1 — USB Filter Fix
- كشف UASP HDDs (`InterfaceType='SCSI'` + `MediaType='External...'`)
- `IsExternal = (busType == BusType.USB)` بدل hardcoded

### 11.2 — Health Rules
- **`DEVICE_SIZE_ZERO`** (Critical) — عطل هاردويري
- **`NO_VOLUMES_DETECTED`** (Warning) — RAW أو غير مهيأ
- 3 اختبارات جديدة لـ HealthEvaluator

### 11.3 — Risk Classification + Whitelist
- `DEVICE_SIZE_ZERO` → Dangerous (يحتاج كتابة FORMAT)
- `NO_VOLUMES_DETECTED` → Medium (يحتاج كتابة CONFIRM)
- إضافتهما للـ whitelist كـ informational actions

### 11.4 — Knowledge Base Expansion
- مقالتان جديدتان: DEVICE_SIZE_ZERO, NO_VOLUMES_DETECTED
- 8 مقالات إجمالاً
- توصية "استشر مختص clean room" في DEVICE_SIZE_ZERO

### 📸 التحقق العملي (SSK UASP HDD)
- SSK يظهر في "الأجهزة"
- Size=0 → Critical ✅
- DEVICE_SIZE_ZERO diagnostic كامل ✅
- Dialog يعرض "خطر" + يطلب FORMAT ✅
- Whitelist يرفض الإجراء غير المُصرّح به ✅

### 📝 قصة الاختبار الحقيقي
- هارد SSK (JMicron JMS578) وصل بحالة "Click of Death"
- المستخدم نظّف PCB بنفسه → اختفى الصوت
- لكن Size=0 لا زال (عطل داخلي)
- **البرنامج شخّص الحالة بشكل صحيح تماماً**

---

## ⚠️ ملاحظة اختبارية
**الهارد SSK معطوب فيزيائياً** (Size=0 مع عدم وجود صوت).
- يحتاج مختص clean room
- لا يمكن لأي برنامج حل هذه الحالة
- UsbDiskDoctor سلّم التوصية الصحيحة

---

## Phase 12 — Final Package (قيد التخطيط)
- `dotnet publish` single-file win-x64
- README.md شامل
- docs/user-guide.md
- أيقونة (اختياري)
- Final commit + Tag v1.0.0