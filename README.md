<div align="center">

# 💾 UsbDiskDoctor

أداة سطح مكتب **Windows** لفحص وتشخيص وحدات التخزين الخارجية المتصلة عبر USB.

![Version](https://img.shields.io/badge/version-1.4.1-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)
![Tests](https://img.shields.io/badge/tests-137%20passed-brightgreen)

[المميزات](#-المميزات) • [التشغيل](#-التشغيل) • [هيكل المشروع](#-هيكل-المشروع) • [الأمان](#-مستويات-الأمان) • [الاختبارات](#-الاختبارات) • [التقنيات](#-التقنيات)

</div>

---

## ✨ المميزات

### 🔍 الاكتشاف والتشخيص
* كشف أجهزة USB عبر WMI (بما فيها UASP HDDs).
* تفاصيل كاملة: الموديل، الرقم التسلسلي، نوع الناقل، الحجم.
* قراءة الفولومات: NTFS، FAT32، exFAT، ReFS، RAW.

### 🩺 الفحص الصحي
* **SMART Status** (حيث يتوفر).
* فحص نظام الملفات (Dirty Bit, RAW).
* تقييم تلقائي: **Healthy / Warning / Critical / Unknown**.
* أكثر من 10 قواعد تشخيص.

### 📄 التقارير
* تقرير **HTML** كامل (RTL عربي) يُعرض عبر WebView2.
* تقرير **JSON** للاستخدام الآلي.
* حفظ تلقائي في مجلد `reports/`.

### 🛠️ الإصلاح الآمن
* 3 مستويات: **Safe / Medium / Dangerous**.
* Whitelist صارم للأوامر.
* Timeout إجباري + Audit log كامل.

### 📦 الاستعادة
* **Basic**: نسخ آمن من mounted volume.
* **Advanced**: File carving (JPEG/PNG/PDF).
* قيود صارمة: لا استعادة على نفس القرص المصدر.

---

## 🚀 التشغيل

### من exe الجاهز (لا يحتاج .NET)
1. حمّل أحدث إصدار من صفحة [Releases](https://github.com/MikealIraqi/UsbDiskDoctor/releases).
2. شغّل `UsbDiskDoctor-Setup-v1.4.1.exe`.
3. **المتطلب الوحيد**: WebView2 Runtime (مثبت عادة في Windows 10/11 الحديثة).

### من الكود المصدري
```bash
dotnet build
dotnet run --project src\UsbDiskDoctor.App
dotnet test
