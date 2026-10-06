# HANDOFF: UsbDiskDoctor Project

**التاريخ**: 2026-10-06
**الإصدار الحالي**: v1.3.1 (مُصدر + installer)
**الحالة**: Phase 16.1 مكتملة - Distribution Ready
**الشراكة**: DeepSeek (مشرف) + Qwen3-Max (كاتب كود) + Hermes (RAG) + محمود (منفّذ)

---

## 1 - معلومات المستخدم

- **الاسم**: محمود (أبو مهدي) - بغداد، العراق
- **اللغة**: العربية العراقية المبسطة + English تقني داخل backticks
- **الجهاز**: Windows 10, 8GB RAM, i7-7600U
- **نمط العمل**: جلسات طويلة، سريع التعلم، دقيق في التوثيق

### بيئة التطوير
- VS 2026 Community (18.10.1)
- .NET SDK: 8.0.423 LTS
- Inno Setup 6.7.3 (للـ installer)
- مساحة العمل: `C:\LibreChat-App\projects\UsbDiskDoctor\`

---

## 2 - الأدوات والشراكة

### DeepSeek (المشرف/المراجع)
- Prompt Writer للـ Qwen
- Code Reviewer سطر بسطر
- **Writer مباشر** لـ XAML + docs (بسبب mojibake risk مع Qwen)
- PowerShell scripts جاهزة + قرارات هندسية

### Qwen3-Max (كاتب الكود)
- نموذج 1T، chat.qwen.ai
- **قوته**: كتابة كود C# من الصفر
- **ضعفه**:
  - Regression: عند "عدّل ملف X" -> الصق الملف الحالي كامل
  - Mojibake: يخرّب العربية -> لا تدعه يعدّل ملفات فيها عربي
  - Shadowing, int/long, Default params

### Hermes (RAG)
- لم يُستخدم في Phase 14/15/16 - docs/PHASE-LOG.md كذاكرة

### محمود (المنفّذ)
- يلصق Prompts في Qwen، ينفذ build/test، يعطي screenshots

---

## 3 - سير العمل

    [1] DeepSeek -> Prompt Qwen (أو كود مباشر)
    [2] محمود -> chat.qwen.ai
    [3] Qwen -> كود
    [4] محمود -> ينسخ رده لـ DeepSeek
    [5] DeepSeek -> مراجعة + PowerShell script
    [6] محمود -> يشغّل السكربت
    [7] dotnet build + test
    [8] commit
    [9] المرحلة التالية

**قاعدة ذهبية**: لا تنتقل لمرحلة قبل تأكيد نجاح الحالية.

---

## 4 - قواعد صارمة

1. لا تفترض - اسأل
2. PowerShell: مسارات absolute، كتابة واحدة في النهاية، index-based للـ XAML
3. NEVER `Get-Content -Encoding UTF8` مع ملفات عربية
4. NEVER `git add` بـ glob (CapacityCheck*.cs لا يطابق CapacityVerdict.cs)
5. سكربت > 150 سطر -> قسّمه لـ chunks < 80 سطر + `[string[]]$lines`
6. `Set-Location` لا يغيّر .NET CWD -> استخدم مسارات absolute
7. Exception catch order: subclass أولاً (EndOfStreamException قبل IOException)
8. عند إضافة مقالة Knowledge -> حدّث JsonKnowledgeServiceTests

---

## 5 - المعمارية

### 10 مشاريع

| # | المشروع | TFM | اختبارات |
|---|---------|-----|:--:|
| 1 | UsbDiskDoctor.App | net8.0-windows | - |
| 2 | UsbDiskDoctor.Core | net8.0 | - |
| 3 | UsbDiskDoctor.Diagnostics | net8.0-windows | - |
| 4 | UsbDiskDoctor.Repair | net8.0-windows | - |
| 5 | UsbDiskDoctor.Recovery | net8.0 | - |
| 6 | UsbDiskDoctor.Knowledge | net8.0 | - |
| 7 | UsbDiskDoctor.Reporting | net8.0-windows | - |
| 8 | Core.Tests | net8.0 | 55 |
| 9 | Diagnostics.Tests | net8.0-windows | 70 |
| 10 | App.Tests | net8.0-windows | 12 |

### الحزم NuGet (7)
- Serilog 4.2.0 + Sinks.File + Sinks.Debug
- System.Management 10.0.12
- CommunityToolkit.Mvvm 8.4.2
- Microsoft.Web.WebView2 1.0.4258.31

### Design System (Nebula Light)
- Primary Indigo #4F46E5 | Success #10B981 | Warning #F59E0B | Danger #EF4444
- Background #FAFAFA | Surface #FFFFFF | SurfaceAlt #F4F4F5
- Font: Cairo, Segoe UI Variable
- Converters: BoolToVis, HexToBrush
- Focus Visual: Indigo dashed 2px

### ملفات مهمة
- `src/UsbDiskDoctor.App/app.manifest` - requireAdministrator (UAC تلقائي)
- `src/UsbDiskDoctor.App/Assets/app.ico` - أيقونة التطبيق (48x48)
- `src/UsbDiskDoctor.App/Views/ContactWindow.xaml` - نافذة التواصل
- `src/UsbDiskDoctor.App/Views/AboutWindow.xaml` - نافذة حول البرنامج
- `installer/UsbDiskDoctor.iss` - Inno Setup script
- `installer/README.txt` - يُثبّت مع البرنامج

---

## 6 - ما تم إنجازه (Phase 0 -> 16.1)

### Phases 0-13 (v1.0.0 -> v1.1.0)
- Setup + Discovery + Diagnostics + Reports
- Knowledge Base (8 مقالات)
- Repair + Recovery (Basic + Advanced)
- Design System Nebula Light

### Phase 14 - Fake Capacity Check (v1.2.0)
8 sub-phases: ccde0cb, 8babe70, b253592, 2514f46, 6faf323, 311dcf7, ca492f3, cca1952

### Phase 15 - Polish + Accessibility (v1.3.0)
- 15.1 Accessibility (cb10cd8)
- 15.2 Loading + Empty States (903c2b7)
- 15.3a Publish cleanup (e98ef24)
- 15.3b MediaType inference (bb71f44)

### Phase 16.0 - Distribution (v1.3.1)
- App icon مدمج (32+48)
- ContactWindow (WhatsApp + Email + Copy phone)
- Admin manifest (requireAdministrator)
- Inno Setup installer
- Fixes: b8b7623 (Contact redesign), 1f50c12 (version sync), 96668f0 (heart Path), b7ac743 (shellexec)

### Phase 16.1 - About + Installer README (v1.3.1)
- AboutWindow (شرح 1/2/3 + مميزات)
- installer/README.txt (يُثبّت مع البرنامج + Start Menu link)
- Commit ae60769

---

## 7 - حالة Git

### آخر commits
- ae60769 Phase 16.1: About + installer README
- b7ac743 Phase 16.0-fix6: shellexec (fixes error 740)
- 96668f0 Phase 16.0-fix5: heart Path geometry
- 1f50c12 Phase 16.0-fix2: version sync
- b8b7623 Phase 16.0-fix: Contact cards layout
- bdf2505 Phase 16.0: icon + contact + manifest + installer
- 5eb136b Docs: 5 lessons
- 995b22b Phase 15.5: v1.3.0
- bb71f44 Phase 15.3b: MediaType
- e98ef24 Phase 15.3a: Publish cleanup

### Tags
- v1.0.0 - Initial Release (Phase 12)
- v1.1.0 - Nebula Light UI (Phase 13)
- v1.2.0 - Fake Capacity Check (Phase 14)
- v1.3.0 - Polish + Accessibility (Phase 15)
- v1.3.1 - Distribution Ready (Phase 16)

### الإجمالي
- ~70 commit
- 16 Phase مكتملة
- 137 اختبار
- 5 tags

---

## 8 - الدروس الحرجة

### أخطاء Qwen
| الخطأ | التصحيح |
|-------|---------|
| Regression on modify | الصق الملف الحالي كامل |
| Mojibake on Arabic | لا تدعه يعدّل ملفات فيها عربي |
| Shadowing | أسماء مميزة |
| int/long Math.Min | cast صريح |
| Default params | IProgress<T>? = null |
| Exception catch order | subclass أولاً |

### مشاكل PowerShell
| المشكلة | الحل |
|---------|------|
| $PWD خاطئ من Set-Location | مسارات absolute مع System.IO.* |
| Get-Content -Encoding UTF8 | لا تستخدمه مع عربي |
| glob patterns | استخدم wildcard واسع |
| Patch جزئي | كتابة واحدة في النهاية |
| XAML anchor عربي | استخدم anchor إنجليزي |
| سكربت > 150 سطر | قسّمه لـ chunks |
| here-string طويل | [string[]]$lines + AppendAllLines |

### قرارات هندسية
1. TFM = net8.0-windows لأي Windows APIs
2. Records + init
3. Fail-soft (قيمة آمنة عند الفشل)
4. OCE يُرفع دائماً
5. Sequential WMI
6. Whitelist للأوامر
7. WriteThrough في capacity test
8. requireAdministrator في manifest
9. shellexec في [Run] (لتفادي error 740)
10. Path geometry بدل emoji للحصول على fill color

### دروس مكتسبة (جلسة اليوم)
- Set-Location vs .NET CWD (مسار absolute ضروري)
- PowerShell script chunks < 80 سطر
- Git glob patterns (Capacity* بدل CapacityCheck*)
- Knowledge tests sync عند إضافة مقالة
- Exception catch order (subclass قبل superclass)
- Heart glyph: emoji لا يقبل Foreground - استخدم Path
- Inno [Run]: shellexec flag لتفادي error 740

---

## 9 - ما لم يُنجَز

### Priority High
- أيقونة 256x256 عالية الجودة (لـ Start Menu)
- Phase 16.2 - .NET 9 upgrade (16.2a DI, 16.2b .NET 9, 16.2c Fluent)

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

    # Publish + Installer
    dotnet publish src\UsbDiskDoctor.App -c Release -r win-x64 `
      --self-contained true -p:PublishSingleFile=true `
      -p:IncludeNativeLibrariesForSelfExtract=true `
      -p:EnableCompressionInSingleFile=true -o publish

    & "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "installer\UsbDiskDoctor.iss"

---

## 11 - أول رسالة للمحادثة الجديدة

    مرحباً، هذي محادثة جديدة وذاكرتك صفر. أنا محمود (أبو مهدي) من العراق.
    المشروع: UsbDiskDoctor - WPF + C# 12 + .NET 8.
    الإصدار: v1.3.1 - Production Ready مع Installer.

    أرفقت HANDOFF v1.3.1 + PHASE-LOG.md. اقرأهما وأخبرني:
    1. ملخص 10 نقاط
    2. حالة المشروع
    3. توصيتك للخطوة التالية

    الخيارات:
    - أيقونة 256x256 عالية الجودة
    - Phase 16.2 (DI + .NET 9 + Fluent Theme)
    - ميزات جديدة (USB-C PD، Command Palette)

    لا تبدأ قبل التأكيد.

---

## 12 - قائمة التحقق

- git status نظيف
- dotnet test = 137/137
- git tag -l يُظهر v1.3.1
- Setup.exe 64 MB موجود
- جهّز Qwen في tab

---

## الخلاصة

**UsbDiskDoctor v1.3.1** - Production Ready:
- 10 مشاريع - 137 اختبار - 5 tags
- Setup.exe 64.16 MB قابل للتوزيع
- Installer احترافي + Admin manifest + Icon
- About + Contact dialogs
- Fake Capacity Check + Accessibility + MediaType
- README مدمج في Installer
- توثيق عربي كامل

**الرحلة القادمة**:
- أيقونة 256x256 (تجميلي)
- Phase 16.2: DI + .NET 9 + Fluent (v2.0.0)

**صنع بـ قلب في العراق**

*HANDOFF v1.3.1 - آخر تحديث 2026-10-06*
