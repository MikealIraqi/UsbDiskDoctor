# 🩺 UsbDiskDoctor

أداة سطح مكتب **Windows** لفحص وتشخيص وحدات التخزين الخارجية المتصلة عبر USB.

## ✨ الميزات

### 🔍 الاكتشاف والتشخيص
- كشف أجهزة USB عبر WMI (بما فيها UASP HDDs)
- تفاصيل كاملة: الموديل، الرقم التسلسلي، نوع الناقل، الحجم
- قراءة الفولومات: NTFS، FAT32، exFAT، ReFS، RAW

### 🏥 الفحص الصحي
- SMART Status (حيث يتوفر)
- فحص نظام الملفات (Dirty Bit, RAW)
- تقييم تلقائي: Healthy / Warning / Critical / Unknown
- 10+ قواعد تشخيص
### 📄 التقارير
- تقرير HTML كامل (RTL عربي) - يُعرض عبر WebView2
- تقرير JSON للاستخدام الآلي
- حفظ تلقائي في `reports/`

### 🛠️ الإصلاح الآمن
- 3 مستويات: Safe / Medium / Dangerous
- Whitelist صارم للأوامر
- Timeout إجباري + Audit log كامل

### 💾 الاستعادة
- Basic: نسخ آمن من mounted volume
- Advanced: File carving (JPEG/PNG/PDF)
- قيود صارمة: لا استعادة على نفس القرص المصدر
---

## 🚀 التشغيل

### من exe الجاهز (لا يحتاج .NET)
1. شغّل `publish/UsbDiskDoctor.App.exe`
2. متطلب واحد: WebView2 Runtime (مثبت عادة في Windows 10/11 الحديثة)

### من الكود
- dotnet build
- dotnet run --project src\UsbDiskDoctor.App
- dotnet test

**المتطلبات**: Windows 10/11 (64-bit) + .NET 8 SDK

---

## 📁 هيكل المشروع

- **src/UsbDiskDoctor.App** — WPF UI
- **src/UsbDiskDoctor.Core** — Models, Enums, Logging
- **src/UsbDiskDoctor.Diagnostics** — Discovery, Health, WMI
- **src/UsbDiskDoctor.Repair** — Planning, Safe Execution
- **src/UsbDiskDoctor.Recovery** — Scanning, Restoring, Carving
- **src/UsbDiskDoctor.Knowledge** — Knowledge Base (JSON)
- **src/UsbDiskDoctor.Reporting** — HTML/JSON Reports
- **tests/** — xUnit tests (81 اختبار)
- **docs/** — PHASE-LOG + user-guide
- **publish/** — Built .exe
---

## 🛡️ مستويات الأمان

| المستوى | الوصف | الموافقة |
|---------|-------|----------|
| 1 - Read-only | فحص، تقارير | لا يحتاج |
| 2 - Low-risk | chkdsk /scan | موافقة CONFIRM |
| 3 - Dangerous | format، wipe | كتابة FORMAT |

**قيود صارمة**:
- ❌ لا استعادة على نفس القرص المصدر
- ❌ لا تنفيذ أوامر من CommandPreview
- ❌ لا shell interpreters
- ✅ معالجة كل استثناء + Logging كامل

---

## 🧪 الاختبارات

- Core.Tests: 35 ✅
- Diagnostics.Tests: 46 ✅
- الإجمالي: 81 ✅

---

## 🏗️ التقنيات

- .NET 8 LTS
- WPF + MVVM (CommunityToolkit.Mvvm)
- System.Management (WMI)
- System.Text.Json + WebView2
- Serilog
- xUnit

---

## ⚠️ تنبيهات مهمة

1. أداة تشخيص واستعادة منطقية — لا تصلح أعطال هاردويرية فيزيائية
2. للحالات الحرجة (Click of Death, Size=0): استشر مختص
3. لا تستخدمه على أقراص سليمة بها بيانات مهمة
4. النسخة الاحتياطية أولاً

---

**صُنع بـ ❤️ في العراق**

---

## 🆕 v1.2.0 — فحص السعة الحقيقية

ميزة جديدة: كشف الفلاشات المزيفة التي تعلن سعة أكبر من الحقيقية.

- **وضعان:** Smart (سريع، دقيقة) / Full (كامل، ساعات)
- **يكتب أنماط اختبار فريدة** على كامل السعة المُعلنة ثم يقرأها للتحقق
- **كشف wraparound:** block index لا يطابق ما كُتب = السعة الحقيقية أقل من المُعلنة
- **3 طبقات أمان:**
  - TestFilePath لازم على نفس الوحدة
  - رفض الكتابة على وحدة غير فارغة (إلا بـ override صريح)
  - block size > 16 bytes
- **متاح من:** Tab التفاصيل → قسم "🔍 فحص السعة الحقيقية"

**الاختبارات الآن: 113** (كان 81)
- Core.Tests: 55
- Diagnostics.Tests: 46
- App.Tests: 12

---

## v1.3.0 - Polish + Accessibility

- Focus ring + Tab nav + AutomationProperties
- Loading overlay + Empty states
- MediaType inference (HDD/SSD/Flash)
- Release publish نظيف

الاختبارات الآن: 137 (كان 113)
