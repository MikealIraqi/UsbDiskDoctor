using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UsbDiskDoctor.Recovery.Carving;
using UsbDiskDoctor.Recovery.SectorReading;
using Xunit;

namespace UsbDiskDoctor.Core.Tests.Carving
{
    /// <summary>
    /// Unit tests for <see cref="FileCarver"/>.
    /// Uses MemorySectorReader — no real disk access.
    /// </summary>
    public class FileCarverTests
    {
        // ---------- Helpers to build synthetic data ----------

        private static readonly byte[] JpegStart = { 0xFF, 0xD8, 0xFF };
        private static readonly byte[] JpegEnd   = { 0xFF, 0xD9 };
        private static readonly byte[] PngStart  = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] PngEnd    = { 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 };
        private static readonly byte[] PdfStart  = { 0x25, 0x50, 0x44, 0x46, 0x2D };
        private static readonly byte[] PdfEnd    = { 0x25, 0x25, 0x45, 0x4F, 0x46 };

        private static byte[] BuildJpeg(int paddingBytes)
        {
            var data = new List<byte>();
            data.AddRange(JpegStart);
            for (int i = 0; i < paddingBytes; i++) data.Add(0x41);
            data.AddRange(JpegEnd);
            return data.ToArray();
        }

        private static byte[] BuildPng(int paddingBytes)
        {
            var data = new List<byte>();
            data.AddRange(PngStart);
            for (int i = 0; i < paddingBytes; i++) data.Add(0x42);
            data.AddRange(PngEnd);
            return data.ToArray();
        }

        private static byte[] BuildPdf(int paddingBytes)
        {
            var data = new List<byte>();
            data.AddRange(PdfStart);
            for (int i = 0; i < paddingBytes; i++) data.Add(0x43);
            data.AddRange(PdfEnd);
            return data.ToArray();
        }

        // ---------- Tests ----------

        [Fact]
        public void Scan_ThrowsArgumentNullException_OnNullReader()
        {
            var sut = new FileCarver();
            Assert.Throws<ArgumentNullException>(() =>
                sut.Scan(null!, 1024L * 1024));
        }

        [Fact]
        public void Scan_EmptySource_ReturnsEmpty()
        {
            var sut = new FileCarver();
            using var reader = new MemorySectorReader(Array.Empty<byte>());

            var results = sut.Scan(reader, 10L * 1024 * 1024);

            Assert.Empty(results);
        }

        [Fact]
        public void Scan_SingleJpeg_IsDetected()
        {
            var jpeg = BuildJpeg(paddingBytes: 1000);
            var sut = new FileCarver();
            using var reader = new MemorySectorReader(jpeg);

            var results = sut.Scan(reader, 10L * 1024 * 1024);

            Assert.Single(results);
            Assert.Equal(".jpg", results[0].Extension);
            Assert.Equal(0, results[0].SourceOffset);
            Assert.Equal(jpeg.Length, results[0].SizeBytes);
        }

        [Fact]
        public void Scan_SinglePng_IsDetected()
        {
            var png = BuildPng(paddingBytes: 2000);
            var sut = new FileCarver();
            using var reader = new MemorySectorReader(png);

            var results = sut.Scan(reader, 10L * 1024 * 1024);

            Assert.Single(results);
            Assert.Equal(".png", results[0].Extension);
        }

        [Fact]
        public void Scan_SinglePdf_IsDetected()
        {
            var pdf = BuildPdf(paddingBytes: 800);
            var sut = new FileCarver();
            using var reader = new MemorySectorReader(pdf);

            var results = sut.Scan(reader, 10L * 1024 * 1024);

            Assert.Single(results);
            Assert.Equal(".pdf", results[0].Extension);
        }

        [Fact]
        public void Scan_NoSignatures_ReturnsEmpty()
        {
            var noise = new byte[4096];
            for (int i = 0; i < noise.Length; i++) noise[i] = (byte)(i % 200);
            var sut = new FileCarver();
            using var reader = new MemorySectorReader(noise);

            var results = sut.Scan(reader, 10L * 1024 * 1024);

            Assert.Empty(results);
        }

        [Fact]
        public void Scan_FileTooSmall_IsIgnored()
        {
            // 10 bytes payload → total well under 512 min
            var tinyJpeg = BuildJpeg(paddingBytes: 10);
            var sut = new FileCarver();
            using var reader = new MemorySectorReader(tinyJpeg);

            var results = sut.Scan(reader, 10L * 1024 * 1024);

            Assert.Empty(results);
        }

        [Fact]
        public void Scan_MultipleFilesBackToBack_AllDetected()
        {
            var data = new List<byte>();
            data.AddRange(BuildJpeg(paddingBytes: 600));
            data.AddRange(BuildPng(paddingBytes: 600));
            data.AddRange(BuildPdf(paddingBytes: 600));

            var sut = new FileCarver();
            using var reader = new MemorySectorReader(data.ToArray());

            var results = sut.Scan(reader, 10L * 1024 * 1024);

            Assert.Equal(3, results.Count);
            Assert.Contains(results, r => r.Extension == ".jpg");
            Assert.Contains(results, r => r.Extension == ".png");
            Assert.Contains(results, r => r.Extension == ".pdf");
        }

        [Fact]
        public void Scan_FileTooLarge_IsAbandoned()
        {
            // JPEG payload 2 KB, but maxFileSizeBytes = 1000 → abandoned
            var jpeg = BuildJpeg(paddingBytes: 2048);
            var sut = new FileCarver();
            using var reader = new MemorySectorReader(jpeg);

            var results = sut.Scan(reader, maxFileSizeBytes: 1000L);

            Assert.Empty(results);
        }

        [Fact]
        public void Scan_CancelledToken_Throws()
        {
            var jpeg = BuildJpeg(paddingBytes: 1000);
            var sut = new FileCarver();
            using var reader = new MemorySectorReader(jpeg);

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.Throws<OperationCanceledException>(() =>
                sut.Scan(reader, 10L * 1024 * 1024, null, cts.Token));
        }
    }
}