<div align="center">

<img src="assets/UsbDiskDoctor-Setup-v1.3.1.png" alt="Banner" width="100%">

# 🩺 UsbDiskDoctor 💾

**طبيب أقراص USB — تشخيص، إصلاح، استرداد**
**The USB Flash Drive Doctor — Diagnose, Repair, Recover**

[![Version](https://img.shields.io/badge/version-1.4.1-4F46E5?style=for-the-badge)](https://github.com/MikealIraqi/UsbDiskDoctor/releases)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-137%20passed-10B981?style=for-the-badge&logo=xunit&logoColor=white)](#-الاختبارات--tests)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-0078D6?style=for-the-badge&logo=windows&logoColor=white)](#-المتطلبات--requirements)

[![GitHub release](https://img.shields.io/github/v/release/MikealIraqi/UsbDiskDoctor?style=for-the-badge&color=4F46E5)](https://github.com/MikealIraqi/UsbDiskDoctor/releases/latest)
[![GitHub stars](https://img.shields.io/github/stars/MikealIraqi/UsbDiskDoctor?style=for-the-badge&color=F59E0B)](https://github.com/MikealIraqi/UsbDiskDoctor/stargazers)
[![Made in Iraq](https://img.shields.io/badge/made%20in-Iraq%20🇮🇶-EF4444?style=for-the-badge)](#)

### 📥 [**تحميل آخر إصدار — Download Latest Release**](https://github.com/MikealIraqi/UsbDiskDoctor/releases/latest) 🚀

</div>

---

<div align="center">

### 🌍 **اختر اللغة | Choose Language**

[🇸🇦 **العربية**](#-العربية) • [🇬🇧 **English**](#-english)

</div>

---
---

## 🇸🇦 العربية

<div dir="rtl">

### 🎯 ما هو UsbDiskDoctor؟

**UsbDiskDoctor** أداة سطح مكتب احترافية لفحص أقراص USB والتحقق من صحتها. تُشخّص الأعطال، تكتشف الأقراص المزيفة، تسترد الملفات المفقودة، وتقترح إصلاحات آمنة — كل ذلك بواجهة عربية أنيقة 🎨

> 🏗️ مبني بـ **WPF + .NET 8** • 🧪 **137 اختبار ناجح** • 🌐 **ثنائي اللغة**

### ✨ الميزات الرئيسية

#### 🔍 الاكتشاف والتشخيص
- 🔌 كشف أجهزة USB تلقائياً (بما فيها UASP HDDs)
- 📋 عرض تفاصيل كاملة: الموديل، الرقم التسلسلي، نوع الناقل، القسم
- 🧩 قراءة أنظمة الملفات: **FAT32 • exFAT • ReFS • RAW • NTFS**

#### 📊 الفحص الصحي
- 🧠 قراءة **SMART Status** (حيث تتوفر)
- 🩺 فحص نظام الملفات (Dirty Bit • RAW)
- 🎯 تصنيف الحالة: **Healthy ✅ • Warning ⚠️ • Critical 🔴 • Unknown ❓**
- 📈 محرّك تقييم بـ **12 قاعدة**

#### 🛠️ الإصلاح الآمن
- 🛡️ **7 طبقات أمان** (Whitelist • Risk Match • Token • Timeout)
- 🎚️ 3 مستويات خطورة: **Safe 🟢 • Medium 🟡 • Dangerous 🔴**
- 📜 Audit log لكل عملية
- ⏱️ Timeout + Kill Tree

#### 💾 الاسترداد
- 📂 **Basic**: نسخ آمن من mounted volume
- 🔬 **Advanced**: Sector-by-sector (JPEG • PNG • PDF carving)

#### 📚 قاعدة المعرفة
- 📖 **10 مقالات** عربية/إنجليزية
- 🔗 مرتبطة تلقائياً بالنتائج التشخيصية

#### 🎭 كشف السعة المزيفة
- ⚡ 3 أوضاع: **Quick (سريع) • Smart (ذكي) • Full (كامل)**
- 🧪 كتابة وقراءة أنماط اختبار
- 🎯 كشف الـ wraparound في الفلاشات المغشوشة

### 📥 التثبيت

1. 🔽 حمّل `UsbDiskDoctor-Setup-v1.4.1.exe` من [صفحة الإصدارات](https://github.com/MikealIraqi/UsbDiskDoctor/releases/latest)
2. 🖱️ شغّل المثبّت
3. ✅ اتبع التعليمات
4. 🚀 افتح البرنامج من Start Menu

### 🚀 طريقة الاستخدام

```
1️⃣ وصّل قرص USB
2️⃣ افتح البرنامج
3️⃣ اختر الجهاز من القائمة
4️⃣ اضغط "فحص" لبدء التشخيص
5️⃣ راجع التقرير (HTML / JSON)
6️⃣ اختر إصلاحاً مناسباً (إن وُجد)
```

### 🛠️ التقنيات المستخدمة

| الفئة | التقنية |
|-------|---------|
| 🎨 الواجهة | WPF + MVVM + CommunityToolkit.Mvvm |
| ⚙️ الإطار | .NET 8 LTS + C# 12 |
| 🔍 WMI | System.Management |
| 📝 التسجيل | Serilog |
| 🌐 التقارير | WebView2 + HTML + JSON |
| 🧪 الاختبارات | xUnit + 137 اختبار |

### 🧪 الاختبارات

| المشروع | عدد الاختبارات |
|---------|----------------|
| Core.Tests | **55** ✅ |
| Diagnostics.Tests | **70** ✅ |
| App.Tests | **12** ✅ |
| **المجموع** | **137** 🎯 |

```powershell
dotnet test
```

### 🗺️ خارطة الطريق

- ✅ **v1.0.0** — الإصدار الأول
- ✅ **v1.1.0** — تحديث الواجهة
- ✅ **v1.2.0** — كشف السعة المزيفة
- ✅ **v1.3.0** — تحسينات عامة
- ✅ **v1.3.1** — المثبّت
- ✅ **v1.4.0** — ثنائي اللغة
- ✅ **v1.4.1** — تنظيف + مقال جديد
- ⏳ **v1.5.0** — DI + Host Builder
- 🎯 **v2.0.0** — Fluent + .NET 9 + Dark Mode

### 🤝 المساهمة

المشروع مفتوح للمساهمات! 🎉

```powershell
git clone https://github.com/MikealIraqi/UsbDiskDoctor.git
cd UsbDiskDoctor
dotnet build
dotnet test
```

### 📜 الترخيص

هذا المشروع مرخّص تحت **MIT License** — استخدمه، عدّله، شاركه بحرية ✨

### 📞 التواصل

- 🐛 **مشكلة؟** [افتح Issue](https://github.com/MikealIraqi/UsbDiskDoctor/issues)
- 💡 **فكرة؟** [ابدأ Discussion](https://github.com/MikealIraqi/UsbDiskDoctor/discussions)
- ⭐ **أعجبك المشروع؟** أعطه نجمة!

</div>

---
---

## 🇬🇧 English

### 🎯 What is UsbDiskDoctor?

**UsbDiskDoctor** is a professional desktop tool for diagnosing USB flash drives. It detects failures, identifies fake-capacity drives, recovers lost files, and suggests safe repairs — all through a modern, clean UI 🎨

> 🏗️ Built with **WPF + .NET 8** • 🧪 **137 passing tests** • 🌐 **Bilingual**

### ✨ Key Features

#### 🔍 Discovery & Diagnostics
- 🔌 Automatic USB detection (including UASP HDDs)
- 📋 Full device details: Model, Serial, Bus Type, Partition
- 🧩 File system detection: **FAT32 • exFAT • ReFS • RAW • NTFS**

#### 📊 Health Check
- 🧠 **SMART Status** reading (where supported)
- 🩺 File system check (Dirty Bit • RAW)
- 🎯 Health classification: **Healthy ✅ • Warning ⚠️ • Critical 🔴 • Unknown ❓**
- 📈 Evaluation engine with **12 rules**

#### 🛠️ Safe Repair
- 🛡️ **7 safety layers** (Whitelist • Risk Match • Token • Timeout)
- 🎚️ 3 risk levels: **Safe 🟢 • Medium 🟡 • Dangerous 🔴**
- 📜 Audit log for every operation
- ⏱️ Timeout + Kill Tree

#### 💾 Recovery
- 📂 **Basic**: Safe copy from mounted volume
- 🔬 **Advanced**: Sector-by-sector (JPEG • PNG • PDF carving)

#### 📚 Knowledge Base
- 📖 **10 articles** in Arabic/English
- 🔗 Auto-linked to diagnostic results

#### 🎭 Fake Capacity Detection
- ⚡ 3 modes: **Quick • Smart • Full**
- 🧪 Pattern write & verify
- 🎯 Detects wraparound in counterfeit drives

### 📥 Installation

1. 🔽 Download `UsbDiskDoctor-Setup-v1.4.1.exe` from [Releases](https://github.com/MikealIraqi/UsbDiskDoctor/releases/latest)
2. 🖱️ Run the installer
3. ✅ Follow the wizard
4. 🚀 Launch from Start Menu

### 🚀 How to Use

```
1️⃣ Plug in a USB drive
2️⃣ Open the app
3️⃣ Select the device
4️⃣ Click "Scan" to start diagnostics
5️⃣ Review the report (HTML / JSON)
6️⃣ Apply a suggested repair (if any)
```

### 🛠️ Tech Stack

| Area | Technology |
|------|------------|
| 🎨 UI | WPF + MVVM + CommunityToolkit.Mvvm |
| ⚙️ Runtime | .NET 8 LTS + C# 12 |
| 🔍 WMI | System.Management |
| 📝 Logging | Serilog |
| 🌐 Reports | WebView2 + HTML + JSON |
| 🧪 Testing | xUnit + 137 tests |

### 🧪 Tests

| Project | Tests |
|---------|-------|
| Core.Tests | **55** ✅ |
| Diagnostics.Tests | **70** ✅ |
| App.Tests | **12** ✅ |
| **Total** | **137** 🎯 |

```powershell
dotnet test
```

### 🗺️ Roadmap

- ✅ **v1.0.0** — Initial release
- ✅ **v1.1.0** — UI modernization
- ✅ **v1.2.0** — Fake capacity detection
- ✅ **v1.3.0** — General polish
- ✅ **v1.3.1** — Installer
- ✅ **v1.4.0** — Bilingual
- ✅ **v1.4.1** — Cleanup + new article
- ⏳ **v1.5.0** — DI + Host Builder
- 🎯 **v2.0.0** — Fluent + .NET 9 + Dark Mode

### 🤝 Contributing

Contributions are welcome! 🎉

```powershell
git clone https://github.com/MikealIraqi/UsbDiskDoctor.git
cd UsbDiskDoctor
dotnet build
dotnet test
```

### 📜 License

This project is licensed under the **MIT License** — use it, modify it, share it freely ✨

### 📞 Contact

- 🐛 **Found a bug?** [Open an Issue](https://github.com/MikealIraqi/UsbDiskDoctor/issues)
- 💡 **Have an idea?** [Start a Discussion](https://github.com/MikealIraqi/UsbDiskDoctor/discussions)
- ⭐ **Like the project?** Give it a star!

---

<div align="center">

### 🩺💾 **UsbDiskDoctor** — Developed with ❤️ in **Basra, Iraq 🇮🇶**

**© 2026 Mahmoud (Abu Mahdi)**

⭐ **If this project helped you, consider giving it a star!** ⭐

</div>
