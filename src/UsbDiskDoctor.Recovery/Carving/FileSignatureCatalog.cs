using System.Collections.Generic;

namespace UsbDiskDoctor.Recovery.Carving
{
    /// <summary>
    /// Catalog of known file signatures for file carving operations.
    /// </summary>
    public static class FileSignatureCatalog
    {
        /// <summary>
        /// Read-only list of all known file signatures.
        /// </summary>
        public static IReadOnlyList<FileSignature> All { get; }

        static FileSignatureCatalog()
        {
            var jpeg = new FileSignature
            {
                Name = "JPEG",
                Extension = ".jpg",
                StartMarker = new byte[] { 0xFF, 0xD8, 0xFF },
                EndMarker = new byte[] { 0xFF, 0xD9 }
            };

            var png = new FileSignature
            {
                Name = "PNG",
                Extension = ".png",
                StartMarker = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
                EndMarker = new byte[] { 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 }
            };

            var pdf = new FileSignature
            {
                Name = "PDF",
                Extension = ".pdf",
                StartMarker = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D },
                EndMarker = new byte[] { 0x25, 0x25, 0x45, 0x4F, 0x46 }
            };

            All = new List<FileSignature> { jpeg, png, pdf }.AsReadOnly();
        }
    }
}