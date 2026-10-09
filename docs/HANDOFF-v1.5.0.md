# HANDOFF: UsbDiskDoctor Project

**التاريخ**: 2026-10-09
**الإصدار**: v1.4.1 (Knowledge article + cleanup)
**الحالة**: Production Ready + 10 knowledge articles

---

## 1 - معلومات المستخدم

- **الاسم**: محمود (أبو مهدي) - البصرة، العراق
- **الجهاز**: Windows 10, 8GB RAM, i7-7600U
- **البيئة**: VS 2026, .NET 8.0.423 LTS, Inno Setup 6.7.3
- **المسار**: `C:\LibreChat-App\projects\UsbDiskDoctor\`

---

## 2 - الحالة

- **10 مشاريع .NET** (7 src + 3 tests)
- **137 اختبار ناجح** (Core: 55، Diagnostics: 70، App: 12)
- **10 مقالات** Knowledge Base
- **8 tags**: v1.0.0 → v1.4.1
- **Setup.exe** جاهز للتوزيع
- **ثنائي اللغة** (عربي + English)

---

## 3 - القاعدة الذهبية (درس 2026-10-09)

### ⚠️ أول خطوة في أي جلسة جديدة — قبل أي كود:

```powershell
cd C:\LibreChat-App\projects\UsbDiskDoctor
git log --oneline -10
Get-ChildItem docs\HANDOFF*.md | Sort LastWriteTime -Desc | Select -First 3
dotnet test --nologo 2>&1 | Select-String "Passed!|Failed!|Total:"
```

**لا تعتمد على HANDOFF قديم. تحقق أن الميزة غير موجودة قبل بنائها.**

**السبب**: في 2026-10-09، HANDOFF v1.1.0 (قديم 4 أيام) أدّى إلى بناء نظام Fake Capacity موازٍ لحل موجود منذ v1.2.0. النتيجة: 700+ سطر مكرر + 41 اختبار مكرر — كلها حُذفت.

---

## 4 - Phase 14 (Fake Capacity) — التصميم الحالي

**المصدر الوحيد:**
- `src\UsbDiskDoctor.Recovery\CapacityChecking\` — FakeCapacityChecker + IFakeCapacityChecker
- `src\UsbDiskDoctor.Recovery\Models\CapacityCheck*.cs` — 6 models
- `src\UsbDiskDoctor.App\ViewModels\CapacityCheckViewModel.cs` — ViewModel
- `tests\UsbDiskDoctor.Core.Tests\CapacityChecking\` — 20 اختبار
- `tests\UsbDiskDoctor.App.Tests\CapacityChecking\` — 12 اختبار

**الميزات:**
- 3 modes: Quick (100 samples) / Smart (1000 samples) / Full (كل الكتل)
- Progress: نسبة + سرعة MB/s + وقت متبقٍ + منقضٍ
- Detection: `SetLength` + Block-Index mismatch
- KB integration: عند Fake → `GetArticle("FAKE_CAPACITY_DETECTED")`

**⚠️ تحذير**: لا تبنِ نظام Fake Capacity موازياً. هذا النظام هو الوحيد.

---

## 5 - Knowledge Base (10 مقالات)

1. SMART_PREDICT_FAILURE
2. OPERATIONAL_STATUS_ERROR
3. OPERATIONAL_STATUS_DEGRADED
4. RAW_FILESYSTEM
5. FILESYSTEM_CHECK_FAILED
6. DIRTY_BIT_SET
7. DEVICE_SIZE_ZERO
8. NO_VOLUMES_DETECTED
9. (يُراجع)
10. FAKE_CAPACITY_DETECTED

**عند إضافة مقال**: حدّث `JsonKnowledgeServiceTests` (3 اختبارات تتحقق من العدد).

---

## 6 - Phase 17 (Bilingual) — مكتملة

- `Localization\Strings.ar.xaml` + `Strings.en.xaml`
- `Localization\LocalizationService.cs` (runtime + Registry)
- كل النصوص DynamicResource
- FlowDirection يتبدّل عند تغيير اللغة

---

## 7 - آخر commits

```
fe8dd7b chore: remove unused Diagnostics->Recovery ProjectReference
30d927a chore: remove knowledge.json.bak
7a2e180 Merge phase-14: knowledge article + cleanup
2428935 chore: finalize sln + csproj cleanup
c2fb224 feat(phase-14): add FAKE_CAPACITY_DETECTED knowledge article
```

## 8 - Tags

- v1.0.0 (Phase 12)
- v1.1.0 (Phase 13 - UI)
- v1.2.0 (Phase 14 - Fake Capacity)
- v1.3.0 (Phase 15 - Polish)
- v1.3.1 (Phase 16 - Installer)
- v1.4.0 (Phase 17 - Bilingual)
- **v1.4.1 (Knowledge article + Cleanup)** ← الحالي
- pre-option-d (شبكة أمان)

---

## 9 - دروس حرجة

### PowerShell
- `Set-Location` لا يغيّر `[System.IO.File]` CWD → استخدم مسارات مطلقة
- `git` يكتب warnings على stderr → `$ErrorActionPreference = "Continue"`
- `New-Item Directory` قبل `WriteAllText`
- multi-line: `@'...'@` (literal) أو `@"..."@` (interpolated)
- **here-string ضخم + عربي = PowerShell parse error** → استخدم Notepad/VS Code

### WPF / XAML
- `StaticResource` داخل ControlTemplate على مستوى النظام = crash عند swap → `DynamicResource`
- heart glyph: emoji بدون U+FE0F = outline → `Path`

### Qwen
- Regression on modify → الصق الملف الحالي كاملاً
- Mojibake على العربي → لا تدعه يعدّل ملفات فيها عربي

### Git
- قبل `checkout` — تأكد `git status` نظيف
- عند squash: `reset --soft` + `commit` (تأكد من staging)
- **tag يشير لـ HEAD دائماً** — إلا إذا كنت تريد وسم نقطة تاريخية

### معمارية
- قبل بناء ميزة: `git log --all --oneline | grep "feature"`
- HANDOFF قديم = خطر

---

## 10 - لم يُنجَز

**Priority High:**
- English GitHub README
- أيقونة 256x256

**Priority Medium:**
- Phase 18: DI + Host Builder
- .NET 9 upgrade
- Fluent Theme + Dark Mode

**Priority Low:**
- Command Palette (Ctrl+K)
- Live Monitoring

---

## 11 - أوامر أساسية

```powershell
cd C:\LibreChat-App\projects\UsbDiskDoctor
dotnet build
dotnet run --project src\UsbDiskDoctor.App
dotnet test

# Publish + Installer
dotnet publish src\UsbDiskDoctor.App -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true -o publish

& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "installer\UsbDiskDoctor.iss"
```

---

## 12 - أول رسالة للجلسة الجديدة

```
مرحباً. جلسة جديدة وذاكرتك صفر. أنا محمود (أبو مهدي) من البصرة، العراق.
المشروع: UsbDiskDoctor — WPF + C# 12 + .NET 8.
الإصدار: v1.4.1 — Bilingual (AR + EN) + Installer.

قبل أي شي، شغّل هذه الأوامر وأرسل الناتج:
1. git log --oneline -10
2. Get-ChildItem docs\HANDOFF*.md | Sort LastWriteTime -Desc | Select -First 3
3. dotnet test --nologo 2>&1 | Select-String "Passed!|Failed!|Total:"

ثم أخبرني:
- ما آخر commit؟
- كم اختبار ناجح؟
- ما أحدث HANDOFF؟

لا تبدأ أي كود قبل هذه الخطوة.

الخيارات:
- English GitHub README + نشر
- Phase 18: DI + .NET 9 + Fluent
- ميزات جديدة

اللغة: عربي عراقي + English تقني.
```

---

## الخلاصة

**UsbDiskDoctor v1.4.1** — Production Ready:
- 10 مشاريع - **137 اختبار** - 8 tags
- Setup.exe جاهز
- ثنائي اللغة
- 10 مقالات Knowledge Base
- Fake Capacity Check محسّن

**الدرس الأهم**: اقرأ أحدث HANDOFF أولاً — قبل أي كود.

*HANDOFF v1.5.0 — آخر تحديث 2026-10-09*