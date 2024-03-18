using System.Security.Cryptography;
using Client.DeviceId.src.DeviceId.Encoders;

namespace Client.DeviceId.src.DeviceId.Internal;

/// <summary>
/// Static instances of the various byte array hashers.
/// </summary>
internal static class ByteArrayHashers
{
    /// <summary>
    /// Gets a byte array hasher that uses SHA256.
    /// </summary>
    public static ByteArrayHasher Sha256 { get; } = new ByteArrayHasher(() => SHA256.Create());
}
