
namespace TehnolinkLicenseValidation.DTOs;

public class ClientRequestDto
{
    public byte[] data { get; set; }
    public byte[] signature { get; set; }
    public byte[] iv { get; set; }
    
    public int clientId { get; set; }
    public int licenseId { get; set; }
}
