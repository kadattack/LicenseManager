
namespace TehnolinkLicenseValidation.DTOs;

public class ServerResponseDto
{
    public byte[] data { get; set; }
    public byte[] signature { get; set; }
    public byte[] iv { get; set; }

}
