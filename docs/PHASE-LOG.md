# UsbDiskDoctor — Phase Log

سجل مختصر لكل مرحلة مكتملة. يُقرأ عند بدء محادثة جديدة.

---

## Phase 0 — Solution Scaffold
- **التاريخ**: 2026-10-05
- **Commit**: 65396da
- **النتيجة**: 9 مشاريع (7 src + 2 tests)، 14 مرجع، build نظيف

---

## Phase 1 — Enums + Models + Serilog
- **Commits**: 3b1839e → e639382
- **النتيجة**: 9 Enums + 7 Models + LoggingSetup + Serilog
- **قرارات**: sealed record, init, problem codes كـ string

---

## Phase 2 — USB Discovery
- **Commits**: 48bb193 → c5bcb0a
- **النتيجة**: IDeviceDiscoveryService + WmiDeviceDiscoveryService + MVVM يدوي + MainWindow
- **قرارات مهمة**:
  - System.Management 10.0.12
  - **TFM = net8.0-windows لأي مشروع Windows APIs**
  - MVVM يدوي (لاحقاً Toolkit)

---

## Phase 3 — Device Details + Volumes
- **Commits**: b7d7390 → 3107b4a
- **النتيجة**:
  - Migration لـ CommunityToolkit.Mvvm 8.4.2 (حذف 121 سطر boilerplate)
  - IVolumeReader + WmiVolumeReader (WMI associations)
  - WmiDeviceDiscoveryService يقرأ volumes تلقائياً
  - TabControl: تبويب "الأجهزة" + تبويب "التفاصيل"
  - DeviceDetailsViewModel
- **قرارات**:
  - with expression لتحديث DeviceSummary
  - Sequential volumes reading (وليس متوازي — أأمن WMI)
  - Null-safe fallbacks في DeviceDetailsViewModel
- **التحقق العملي**: التطبيق يفتح، TabControl يعمل، build نظيف

---

## ⚠️ ملاحظة اختبارية مهمة

**لم يُختبر التطبيق مع فلاشة USB حقيقية حتى 2026-10-05.**

اختبارات حالية:
- **SSK HDD خارجي** (JMicron JMS578): يظهر في Windows كـ InterfaceType='SCSI' (UASP)، لكن الهارد معطوب هاردويرياً (Size=0، Click of Death). لم يُختبر مع الكود بشكل كامل.
- **Android phone** (MTP): خارج نطاق المشروع (يستخدم WPD API، وليس Mass Storage).
- **لا يوجد فلاشة USB** متاحة للاختبار حالياً.

**التوصية**: اختبار التطبيق مع فلاشة USB (FAT32/exFAT) عند توفرها.

**ملاحظة معمارية**: الفلتر الحالي WHERE InterfaceType='USB' قد لا يلتقط بعض HDDs الخارجية (UASP). الحل المستقبلي: استخدام PNPDeviceID LIKE 'USB\%' أو MSFT_PhysicalDisk.BusType. مؤجل لـ Phase 12 (Polish).

---

## Phase 4 — Health Check (قيد التخطيط)
- **الهدف**: فحص صحي (SMART + filesystem + تقييم)
- **مخطط**:
  - ISmartReader + WmiSmartReader (MSStorageDriver_FailurePredictStatus)
  - IFileSystemChecker (chkdsk readonly)
  - IHealthEvaluator (HealthStatus rules)
  - عرض الحالة في TabDetails