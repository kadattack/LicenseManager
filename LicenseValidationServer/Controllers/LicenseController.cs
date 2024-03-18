using System.Text;
using LicenseValidationServer.DTO;
using LicenseValidationServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TehnolinkLicenseValidation.DTOs;

namespace LicenseValidationServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LicenseController : ControllerBase
{
    private AppDbContext _dbContext;

    public LicenseController(AppDbContext context)
    {
        _dbContext = context;
    }

    [HttpGet]
    [Route("/api/License/CheckServer")]
    public ActionResult CheckServer()
    {
        return Ok();
    }


    [HttpPost]
    [Route("/api/License/CheckLicense")]
    public async Task<ActionResult<ServerResponseDto>> CheckLicense(ClientRequestDto clientRequestDto)
    {
        try
        {
            var utils = new Utils.Utils();

            License? license = await _dbContext.License.FirstOrDefaultAsync(x => x.license_id == clientRequestDto.licenseId);
            if (license == null)
            {
                return Problem();
            }


            var (isSignatureValid, decryptedData) = utils.DecryptDataAndValidateSignature(clientRequestDto, license);

            if (isSignatureValid)
            {
                try
                {
                    ClientRequestDataDto? clientDataDto = JsonConvert.DeserializeObject<ClientRequestDataDto>(Encoding.UTF8.GetString(decryptedData));
                    Console.WriteLine(clientDataDto.client);
                    Console.WriteLine(clientDataDto.license);
                    Console.WriteLine(clientDataDto.deviceId);
                    ServerResponseDataDto? serverDataDto = null;
                    if (license.license == clientDataDto.license && license.active)
                    {
                        serverDataDto = new ServerResponseDataDto() {
                            isValid = true
                        };
                    }
                    else
                    {
                        serverDataDto = new ServerResponseDataDto() {
                            isValid = false
                        };
                    }

                    var serverResponseDto = utils.EncryptAndSign(serverDataDto, license);

                    return serverResponseDto;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Problem("Corrupted decrypted data");
                }






            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }


        return Problem();

    }

}
