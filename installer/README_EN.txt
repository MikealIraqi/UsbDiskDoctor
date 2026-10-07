==============================================================
  UsbDiskDoctor v1.4.0 - USB Drive Diagnostic Tool
==============================================================

WHAT IS THIS TOOL?
--------------------------------------------------------------

UsbDiskDoctor is an Iraqi Windows tool that examines, diagnoses,
and repairs external storage devices (USB flash drives, external
HDDs, SD cards).

The tool helps you find out:
  - Is my flash drive healthy or failing?
  - Is it genuine or a fake with a spoofed capacity?
  - Are there damaged or lost files on it?
  - How can I recover them safely?

All this WITHOUT internet, in Arabic OR English, with a modern
interface.


1. DETECTS AND CHECKS DEVICES
--------------------------------------------------------------

Once you connect a flash drive or external HDD, the tool shows it
in the main list with full details: name, model, serial number,
real size, and current status.

It reads S.M.A.R.T. data (the heartbeat of any HDD), checks the
file system (NTFS, FAT32, exFAT), detects early signs of failure,
and gives you a clear verdict: Healthy / Warning / Critical.


2. DETECTS FAKE FLASH DRIVES
--------------------------------------------------------------

This is the strongest feature. Many counterfeit flash drives
advertise 128 GB while containing only 8 GB. The tool:

  - Writes test patterns across the full claimed capacity
  - Reads them back and verifies
  - Detects any wraparound
  - Reports the real capacity accurately


3. RECOVERS LOST FILES
--------------------------------------------------------------

If files were deleted or the drive is damaged, the tool can:

  - Copy existing files safely
  - Search for lost files using File Carving
  - Recover images (JPEG, PNG) and PDFs
  - WITHOUT harming the original data


4. SUGGESTS SAFE REPAIRS
--------------------------------------------------------------

The tool never executes anything dangerous without your consent.
It presents a categorized list of suggested actions:

  - Safe       : Runs directly
  - Medium     : Requires a confirmation keyword
  - Dangerous  : Requires typing a warning keyword

Every action must pass through a confirmation dialog first.


5. PROVIDES FULL REPORTS
--------------------------------------------------------------

After each scan you can:

  - View a professional HTML report (Arabic RTL or English)
  - Save a JSON copy for automation
  - Share the report with a specialist
  - Keep it for later review


WHY IS IT SAFE?
--------------------------------------------------------------

The tool is built with strict safety in mind:

  - Never writes without your permission
  - Command whitelist: only known commands can run
  - Blocks writing to the same source drive
  - Mandatory timeout on every operation
  - Full audit log of every action


WHO IS IT FOR?
--------------------------------------------------------------

  - Regular users: flash drive failed, want to know why
  - Technicians: hours spent diagnosing customer devices
  - Repair shops: need a fast, reliable tool
  - Careful buyers: want to verify a flash drive before purchase


HOW TO START?
--------------------------------------------------------------

  1. Launch UsbDiskDoctor (UAC prompt will appear)
  2. Connect your flash drive or external HDD
  3. Select the device from the list
  4. Click "Full Scan" and view the result

Expected time: 10 seconds for a quick scan, minutes for a full scan.


IMPORTANT NOTE
--------------------------------------------------------------

This is a LOGICAL diagnostic and recovery tool. It handles
software and firmware-level issues, but CANNOT fix physical
problems (such as a broken HDD head or impact damage). For such
cases, it recommends seeing a specialist in a clean room.


LANGUAGE / اللغة
--------------------------------------------------------------

The app supports both Arabic and English. Click the "Language"
button in the top toolbar to switch at any time. Your choice is
saved automatically.


==============================================================
  MADE WITH LOVE IN IRAQ
==============================================================

Developer: Mahmoud Al-Aboudi - Basra, Iraq

WhatsApp: 009647730393399
Email: tearscantstop@gmail.com

Version: v1.4.0
License: Free for personal use

==============================================================
