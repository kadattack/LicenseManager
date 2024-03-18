using System.Security.Cryptography;
using Client.ecdsa_dotnet.EcdsaDotNet.EcdsaDotNet;

namespace TehnolinkLicenseValidation.DTOs;

public class ClientRequestDto
{
    public byte[] data { get; set; }
    public byte[] signature { get; set; }
    public byte[] iv { get; set; }
}
