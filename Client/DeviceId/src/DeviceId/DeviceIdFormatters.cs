﻿using Client.DeviceId.src.DeviceId.Encoders;
using Client.DeviceId.src.DeviceId.Formatters;
using Client.DeviceId.src.DeviceId.Internal;

namespace Client.DeviceId.src.DeviceId;

/// <summary>
/// Provides access to some of the default formatters.
/// </summary>
public static class DeviceIdFormatters
{
    /// <summary>
    /// Returns the default formatter used in version 5 of the DeviceId library.
    /// </summary>
    public static IDeviceIdFormatter DefaultV5 { get; } = new HashDeviceIdFormatter(ByteArrayHashers.Sha256, new Base64UrlByteArrayEncoder());

    /// <summary>
    /// Returns the default formatter used in version 6 of the DeviceId library.
    /// </summary>
    public static IDeviceIdFormatter DefaultV6 { get; } = new HashDeviceIdFormatter(ByteArrayHashers.Sha256, ByteArrayEncoders.Base32Crockford);
}
