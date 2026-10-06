# UsbDiskDoctor Installer

## البناء

```powershell
# 1) تأكد من publish/ محدّث
dotnet publish src\UsbDiskDoctor.App -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true -o publish

# 2) build الـ installer
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\UsbDiskDoctor.iss
```

## الناتج

`publish-installer\UsbDiskDoctor-Setup-v1.3.1.exe`

## الميزات

- تثبيت على C:\Program Files\UsbDiskDoctor
- Desktop shortcut مع أيقونة
- Start Menu entry
- Uninstaller كامل
- عربي + إنجليزي
