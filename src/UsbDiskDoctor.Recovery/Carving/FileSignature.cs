using System;

namespace UsbDiskDoctor.Recovery.Carving
{
    /// <summary>
    /// Represents a file signature (magic bytes) used for file carving.
    /// Contains start and end markers to identify file boundaries.
    /// </summary>
    public sealed record FileSignature
    {
        /// <summary>
        /// Human-readable name of the file type (e.g., "JPEG", "PNG").
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// File extension including the leading dot (e.g., ".jpg").
        /// </summary>
        public string Extension { get; init; } = string.Empty;

        /// <summary>
        /// Byte sequence marking the start of the file.
        /// </summary>
        public byte[] StartMarker { get; init; } = Array.Empty<byte>();

        /// <summary>
        /// Byte sequence marking the end of the file.
        /// </summary>
        public byte[] EndMarker { get; init; } = Array.Empty<byte>();
    }
}