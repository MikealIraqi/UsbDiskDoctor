# 📋 HANDOFF: UsbDiskDoctor Project

**التاريخ**: 2026-10-06
**الإصدار الحالي**: v1.3.0
**الحالة**: Phase 15 مكتملة

---

## 1️⃣ معلومات المستخدم

- الاسم: محمود (أبو مهدي) — بغداد
- Windows 10, 8GB RAM, i7-7600U
- VS 2026 Community + .NET 8.0.423 LTS
- مساحة العمل: `C:\LibreChat-App\projects\UsbDiskDoctor\`

---

## 2️⃣ الأدوات والشراكة

- **DeepSeek**: مشرف + مراجع + كاتب XAML/docs
- **Qwen3-Max**: كاتب C#/WPF — ضعيف في تعديل ملفات موجودة، خطر mojibake على العربي
- **Hermes**: RAG (لم يُستخدم في Phase 14/15)
- **محمود**: منفّذ + مختبر

---

## 3️⃣ سير العمل

DeepSeek → Prompt → Qwen → مراجعة → PowerShell → Build/Test → Commit

---

## 4️⃣ قواعد صارمة

1. لا تفترض — اسأل
2. PowerShell: مسارات absolute، كتابة واحدة في النهاية، index-based للـ XAML
3. NEVER `Get-Content -Encoding UTF8` مع عربي
4. NEVER `git add` بـ glob (`CapacityCheck*.cs` لا يطابق `CapacityVerdict.cs`)
5. سكربت > 150 سطر → قسّمه
---

## 5️⃣ المعمارية

### 📦 10 مشاريع

| # | المشروع | TFM | اختبارات |
|---|---------|-----|:--:|
| 1 | UsbDiskDoctor.App | net8.0-windows | — |
| 2 | UsbDiskDoctor.Core | net8.0 | — |
| 3 | UsbDiskDoctor.Diagnostics | net8.0-windows | — |
| 4 | UsbDiskDoctor.Repair | net8.0-windows | — |
| 5 | UsbDiskDoctor.Recovery | net8.0 | — |
| 6 | UsbDiskDoctor.Knowledge | net8.0 | — |
| 7 | UsbDiskDoctor.Reporting | net8.0-windows | — |
| 8 | Core.Tests | net8.0 | 55 |
| 9 | Diagnostics.Tests | net8.0-windows | 70 |
| 10 | App.Tests | net8.0-windows | 12 |

### 📦 NuGet (7)
- Serilog 4.2.0 + Sinks.File + Sinks.Debug
- System.Management 10.0.12
- CommunityToolkit.Mvvm 8.4.2
- Microsoft.Web.WebView2 1.0.4258.31

### 🎨 Design System
- Primary Indigo #4F46E5 | Success #10B981 | Warning #F59E0B | Danger #EF4444
- Background #FAFAFA | Surface #FFFFFF
- Font: Cairo, Segoe UI Variable
- Converters: BoolToVis, HexToBrush

---

## 6️⃣ ما تم إنجازه (0 → 15)

- **Phases 0-10**: Setup + Discovery + Diagnostics + Reports + Knowledge + Repair + Recovery
- **Phase 11**: USB filter fix + 2 rules (v1.0.0)
- **Phase 12**: Publish single-file (v1.0.0)
- **Phase 13**: UI Modernization — Nebula Light (v1.1.0)

### Phase 14 — Fake Capacity Check (v1.2.0)
8 sub-phases: contracts → implementation → tests → ViewModel → UI → docs

| Sub | Commit | الوصف |
|-----|--------|-------|
| 14.1 | ccde0cb | Contracts |
| 14.2 | 8babe70 | FakeCapacityChecker |
| 14.3a | b253592 | 3 internal static methods |
| 14.3b | 2514f46 | 20 اختبار |
| 14.4a | 6faf323 | ViewModel + 12 اختبار |
| 14.4b-1 | 311dcf7 | Plumbing |
| 14.4b-2 | ca492f3 | XAML + Converter |
| 14.4c | cca1952 | Docs + v1.2.0 |

### Phase 15 — Polish + Accessibility (v1.3.0)
4 sub-phases:

| Sub | Commit | الوصف |
|-----|--------|-------|
| 15.1 | cb10cd8 | Focus + TabNav + 7 AutomationProps |
| 15.2 | 903c2b7 | Loading overlay + 2 empty states |
| 15.3a | e98ef24 | Publish cleanup |
| 15.3b | bb71f44 | MediaType inference |
| 15.5 | 5bcc74e | Docs + version bump (هذا الـ commit) |
---

## 7️⃣ حالة Git

### آخر commits
- bb71f44 — Phase 15.3b: MediaType inference
- e98ef24 — Phase 15.3a: Publish cleanup
- 903c2b7 — Phase 15.2: Loading + Empty states
- cb10cd8 — Phase 15.1: Accessibility
- cca1952 — Phase 14.4c: v1.2.0
- ca492f3 — Phase 14.4b-2: UI section
- 311dcf7 — Phase 14.4b-1: Plumbing
- 6faf323 — Phase 14.4a: ViewModel + tests
- 2514f46 — Phase 14.3b: 20 tests
- b253592 — Phase 14.3a: Testability
- 8babe70 — Phase 14.2: Implementation
- ccde0cb — Phase 14.1: Contracts

### Tags
- v1.0.0 — Initial Release
- v1.1.0 — Nebula Light UI
- v1.2.0 — Fake Capacity Check
- v1.3.0 — Polish + Accessibility (الجديد)

### إجمالي
- ~62 commit
- 15 Phase مكتملة
- 137 اختبار ناجح

---

## 8️⃣ الدروس الحرجة

### ⚠️ أخطاء Qwen
| الخطأ | التصحيح |
|-------|---------|
| Regression on modify | الصق الملف الحالي كامل |
| Mojibake on Arabic | لا تدعه يعدّل ملفات فيها عربي |
| Shadowing | أسماء مميزة |
| int/long Math.Min | cast صريح |
| Default params | `IProgress<T>? = null` |
| Subfolder files | `New-Item -ItemType Directory` أولاً |
| Exception catch order | subclass أولاً (EndOfStream قبل IOException) |

### ⚠️ مشاكل PowerShell
| المشكلة | الحل |
|---------|------|
| $PWD خاطئ | مسارات absolute |
| Get-Content -Encoding UTF8 | لا تستخدمه مع عربي |
| glob patterns | `Capacity*` بدل `CapacityCheck*` |
| Patch فشل جزئياً | كتابة واحدة في النهاية |
| XAML regex | index-based (IndexOf) |
| سكربت > 150 سطر | قسّمه لـ chunks |

### 🎯 قرارات هندسية
1. TFM = net8.0-windows لأي Windows APIs
2. Records + init
3. Fail-soft
4. OCE يُرفع دائماً
5. Sequential WMI
6. Whitelist صارم
7. WriteThrough في capacity test
8. Knowledge tests: حدّث JsonKnowledgeServiceTests عند إضافة مقالة
---

## 9 - ما لم ينجز

### 14.4d - Polish + Real Test (مؤجل)
- UI Animations + Progress ring
- اختبار حقيقي على SSK HDD
- Screenshots للتحقق البصري

### Phase 16 - Foundation (v2.0.0)
- 16.1: DI + Host Builder
- 16.2: .NET 9 upgrade
- 16.3: Fluent + Dark Mode

### Priority Medium
- USB-C Power Delivery
- Command Palette (Ctrl+K)
- Live Monitoring (LiveCharts2)

### Priority Low
- Local AI (ONNX + Phi-3)
- Historical Analytics (ML.NET)
- Background Service

---

## 10 - أوامر أساسية

    cd C:\LibreChat-App\projects\UsbDiskDoctor
    dotnet build
    dotnet run --project src\UsbDiskDoctor.App
    dotnet test
    git log --oneline -20
    git tag -l

---

## 11 - أول رسالة للمحادثة الجديدة

    مرحباً، هذي محادثة جديدة وذاكرتك صفر.
    المشروع: UsbDiskDoctor - WPF + C# 12 + .NET 8.
    الإصدار: v1.3.0 - Phase 15 مكتملة.
    أرفقت HANDOFF v1.3.0 + PHASE-LOG. اقرأهما وأخبرني:
    1. ملخص 10 نقاط
    2. حالة المشروع
    3. توصيتك للخطوة التالية
    لا تبدأ قبل التأكيد.

---

## 12 - قائمة التحقق

- git status نظيف
- dotnet test = 137/137
- git tag -l يُظهر v1.3.0
- جهّز Qwen

---

## الخلاصة

UsbDiskDoctor v1.3.0 production-ready.
10 مشاريع - 137 اختبار - exe 70.64 MB.
صنع بـ قلب في العراق.

*HANDOFF v1.3.0 - آخر تحديث 2026-10-06*

### دروس إضافية (2026-10-06)

**Set-Location vs .NET CWD**:
- Set-Location يغير $PWD فقط
- [System.IO.File]::ReadAllText يستخدم .NET Process CWD (ثابت = C:\Windows\system32)
- الحل: مسارات absolute دائماً مع System.IO.*

**PowerShell script chunks**:
- سكربت PowerShell > 50 سطر بـ here-string = خطر paste truncation
- الحل: [string[]]$lines + AppendAllLines

**Git glob pattern**:
- استخدم wildcard واسع، تحقق من git status قبل commit
- CapacityCheck*.cs لا يطابق CapacityVerdict.cs

**Knowledge tests sync**:
- عند إضافة مقالة جديدة، حدّث JsonKnowledgeServiceTests
- 3 اختبارات تعتمد على عدد المقالات الثابت
