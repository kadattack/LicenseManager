using Client.ecdsa_dotnet.EcdsaDotNet.EcdsaDotNet;

namespace TehnolinkLicenseValidation.DTOs;

public class ServerResponseDto
{
    public byte[] data { get; set; }
    public byte[] signature { get; set; }
}
