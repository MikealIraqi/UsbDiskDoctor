# HANDOFF: UsbDiskDoctor Project

**التاريخ**: 2026-10-09 (محدّث نهاية اليوم)
**الإصدار**: v1.4.1 — منشور على GitHub
**الحالة**: Production Ready + Published

---

## 1 - معلومات المستخدم

- **الاسم**: محمود (أبو مهدي) - البصرة، العراق 🇮🇶
- **الجهاز**: Windows 10, 8GB RAM, i7-7600U
- **البيئة**: VS 2026, .NET 8.0.423 LTS, Inno Setup 6.7.3
- **المسار**: `C:\LibreChat-App\projects\UsbDiskDoctor\`
- **GitHub**: https://github.com/MikealIraqi/UsbDiskDoctor

---

## 2 - الحالة الحالية

- **10 مشاريع .NET** (7 src + 3 tests)
- **137 اختبار ناجح** (Core: 55، Diagnostics: 70، App: 12)
- **10 مقالات** Knowledge Base
- **10 tags**: v1.0.0 → v1.4.1 (+ pre-option-d)
- **Setup.exe جاهز**: `publish-installer\UsbDiskDoctor-Setup-v1.4.1.exe` (64.17 MB)
- **منشور على GitHub**: الكود + tags + (Release pending)
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

**السبب**: اليوم بدأنا بـ HANDOFF v1.1.0 (قديم 4 أيام و4 إصدارات). أدّى إلى:
- بناء نظام Fake Capacity موازٍ (700+ سطر)
- 41 اختبار مكرر
- كلها حُذفت في النهاية

---

## 4 - Phase 14 (Fake Capacity) — التصميم النهائي

**المصدر الوحيد:**
- `src\UsbDiskDoctor.Recovery\CapacityChecking\`
  - `FakeCapacityChecker.cs` — SetLength + Block-Index + Sparse Sampling
  - `IFakeCapacityChecker.cs`
- `src\UsbDiskDoctor.Recovery\Models\CapacityCheck*.cs` — 6 models
- `src\UsbDiskDoctor.App\ViewModels\CapacityCheckViewModel.cs` — ViewModel (مُصلح mojibake)
- `tests\UsbDiskDoctor.Core.Tests\CapacityChecking\` — 20 اختبار
- `tests\UsbDiskDoctor.App.Tests\CapacityChecking\` — 12 اختبار

**الميزات:**
- 3 modes: Quick (100 samples) / Smart (1000 samples) / Full (كل الكتل)
- Progress: نسبة + سرعة MB/s + وقت متبقٍ + منقضٍ
- Detection: `SetLength(claimed)` + Block-Index mismatch
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
10. **FAKE_CAPACITY_DETECTED** ← أُضيف في v1.4.1

**عند إضافة مقال**: حدّث `JsonKnowledgeServiceTests` (3 اختبارات تتحقق من العدد: 9 → 10).

---

## 6 - GitHub

- **Repository**: https://github.com/MikealIraqi/UsbDiskDoctor
- **الفرع الرئيسي**: `master`
- **آخر commit**: `1286702 chore: bump version to 1.4.1`
- **Tags مرفوعة**: v1.0.0 → v1.4.1 + pre-option-d

### ⏳ مهمة معلّقة: GitHub Release
- **الرابط**: https://github.com/MikealIraqi/UsbDiskDoctor/releases/new
- **Tag**: `v1.4.1`
- **Title**: `UsbDiskDoctor v1.4.1`
- **Binary للرفع**: `publish-installer\UsbDiskDoctor-Setup-v1.4.1.exe`
- **Description**: انظر `docs\release-notes-v1.4.1.md` (إن وُجد)

### ⏳ مهمة معلّقة: README
- **الحالة**: صورة الغلاف تحتاج رفع إلى مجلد `assets\`
- **الخطوات**:
  1. أنشئ `assets/.gitkeep` عبر GitHub UI
  2. ارفع الصورة باسم `banner.png`
  3. حدّث `README.md` (نص جاهز في محادثة 2026-10-09)

---

## 7 - Version Management (درس مهم)

**النسخة في 3 أماكن — يجب تحديثها كلها:**

| # | الملف | السطر |
|---|-------|-------|
| 1 | `Directory.Build.props` | `<Version>1.4.1</Version>` |
| 2 | `installer\UsbDiskDoctor.iss` | `#define MyAppVersion "1.4.1"` |
| 3 | `UsbDiskDoctor.App.csproj` | (يقرأ من Directory.Build.props) |

**بعد كل version bump:**
```powershell
# 1. حدّث الملفين
# 2. أعد Publish
dotnet publish src\UsbDiskDoctor.App -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true -o publish

# 3. أعد بناء الـ installer
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "installer\UsbDiskDoctor.iss"

# 4. تحقق
Get-ChildItem publish-installer\
```

---

## 8 - آخر commits (مهم)

```
1286702 chore: bump version to 1.4.1 in Directory.Build.props + installer
0727de5 chore: ignore WebView2 runtime cache folders
9c071e4 chore: ignore *.exe release artifacts
db7afe0 docs: add HANDOFF v1.5.0 (current state + golden rule)
e09dc4f docs: remove outdated HANDOFFs (v1.3.0/v1.3.1/v1.4.0)
fe8dd7b chore: remove unused Diagnostics->Recovery ProjectReference
30d927a chore: remove knowledge.json.bak + unused Diagnostics→Recovery reference
7a2e180 Merge phase-14: knowledge article + cleanup
2428935 chore: finalize sln + csproj cleanup for phase-14
c2fb224 feat(phase-14): add FAKE_CAPACITY_DETECTED knowledge article
```

## 9 - Tags (10 total)

- v1.0.0 (Phase 12)
- v1.1.0 (Phase 13 - UI)
- v1.2.0 (Phase 14 - Fake Capacity)
- v1.3.0 (Phase 15 - Polish)
- v1.3.1 (Phase 16 - Installer)
- v1.4.0 (Phase 17 - Bilingual)
- **v1.4.1 (Knowledge article + Cleanup)** ← الحالي
- pre-option-d (شبكة أمان)

---

## 10 - دروس حرجة (محدّثة)

### PowerShell
- ⚠️ `Set-Location` **لا يغيّر** `[System.IO.File]` CWD → استخدم **مسارات مطلقة دائماً**
- `git` يكتب warnings على stderr → `$ErrorActionPreference = "Continue"`
- `New-Item Directory` قبل `WriteAllText`
- **here-string ضخم + عربي = parse error** → احفظ الملف بـ VS Code/Notepad
- عند الكتابة: استخدم `[System.Text.UTF8Encoding]::new($false)` (لا BOM)

### WPF / XAML
- `StaticResource` داخل ControlTemplate = crash عند swap → `DynamicResource`
- heart glyph: emoji بدون U+FE0F = outline → `Path`

### Qwen
- Regression on modify → الصق الملف الحالي كاملاً
- **Mojibake على العربي** → لا تدعه يعدّل ملفات فيها عربي

### Git
- قبل `checkout` — تأكد `git status` نظيف
- عند squash: `reset --soft` + `commit` (تأكد من staging)
- Tag يشير لـ HEAD — إلا إذا كنت تسم نقطة تاريخية
- PowerShell يفسر git stderr warnings كـ errors → تجاهلها إذا العملية نجحت

### معمارية
- **قبل بناء ميزة**: `git log --all --oneline | grep "feature_name"`
- **HANDOFF قديم = خطر**

---

## 11 - لم يُنجَز

### Priority High (فورية)
- ⏳ **GitHub Release v1.4.1** — رفع Setup.exe
- ⏳ **README redesign** — مع صورة الغلاف
- ⏳ **English GitHub README** (public-facing)

### Priority Medium
- Phase 18: DI + Host Builder (`Microsoft.Extensions.DependencyInjection`)
- .NET 9 upgrade
- Fluent Theme + Dark Mode

### Priority Low
- Command Palette (Ctrl+K)
- Live Monitoring
- USB-C PD

---

## 12 - أوامر أساسية

```powershell
cd C:\LibreChat-App\projects\UsbDiskDoctor

# Build + Test
dotnet build
dotnet test

# Run
dotnet run --project src\UsbDiskDoctor.App

# Publish exe
dotnet publish src\UsbDiskDoctor.App -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true -o publish

# Build installer
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "installer\UsbDiskDoctor.iss"
```

---

## 13 - أول رسالة للجلسة الجديدة

```
مرحباً. جلسة جديدة وذاكرتك صفر. أنا محمود (أبو مهدي) من البصرة، العراق.
المشروع: UsbDiskDoctor — WPF + C# 12 + .NET 8.
الإصدار: v1.4.1 — منشور على GitHub + Bilingual + Installer.

═══════════════════════════════════════════
⚠️ أول خطوة إلزامية — قبل أي كود:
═══════════════════════════════════════════

شغّل هذه الأوامر وأرسل الناتج:

1. cd C:\LibreChat-App\projects\UsbDiskDoctor
2. git log --oneline -10
3. Get-ChildItem docs\HANDOFF*.md | Sort LastWriteTime -Desc | Select -First 3
4. dotnet test --nologo 2>&1 | Select-String "Passed!|Failed!|Total:"

ثم أخبرني:
- ما آخر commit؟
- كم اختبار ناجح؟
- ما أحدث HANDOFF؟
- هل يوجد Release على GitHub؟

لا تبدأ أي كود قبل هذه الخطوة.

═══════════════════════════════════════════
المهام المعلّقة (من الجلسة السابقة):
═══════════════════════════════════════════

1. GitHub Release v1.4.1 (رفع Setup.exe)
2. README redesign + صورة الغلاف
3. English GitHub README

اللغة: عربي عراقي + English تقني.
```

---

## الخلاصة

**UsbDiskDoctor v1.4.1** — Production Ready + Published:
- 10 مشاريع - **137 اختبار** - 10 tags
- Setup.exe 64 MB جاهز
- ثنائي اللغة (عربي + English)
- 10 مقالات Knowledge Base
- **منشور على GitHub**

**الدرس الأهم من 2026-10-09**: اقرأ أحدث HANDOFF أولاً — قبل أي كود.

**الدرس الثاني**: النسخة في 3 ملفات — لا تنسَ واحداً.

*HANDOFF v1.5.1 — آخر تحديث 2026-10-09*