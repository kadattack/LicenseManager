using System.ComponentModel;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;
using LicenseValidationServer.DTO;
using TehnolinkLicenseValidation.DTOs;
using Client.DeviceId.src.DeviceId;
using Client.DeviceId.src.DeviceId.Linux;
using Client.DeviceId.src.DeviceId.Mac;
using Client.DeviceId.src.DeviceId.Windows;
using Client.DeviceId.src.DeviceId.Windows.Mmi;

using File = System.IO.File;
using WindowsDeviceIdBuilderExtensions = Client.DeviceId.src.DeviceId.Windows.Wmi.WindowsDeviceIdBuilderExtensions;



namespace TehnolinkLicenseValidation;

public class TehnolinkAuthenticationClientLicense
{
   
    readonly License _license;

    public TehnolinkAuthenticationClientLicense(string licensePath)
    {
        _license = ReadLicenseAndKeys(licensePath);

    }

    public async Task<Boolean> ValidateLicense()
    {
        try
        {
            // Replace these values with your actual API endpoint, Bearer token, and request data
            var clientRequestDataDto = new ClientRequestDataDto {
                license = _license.license,
                client = _license.client,
                deviceId = GetDeviceId()
            };

            
            var clientRequestDto = EncryptAndSign(clientRequestDataDto, _license);



            // Send the POST request with Bearer token
            ServerResponseDto? response = await SendPostRequest(_license.apiUrl, _license.token, clientRequestDto);
            if (response == null)
            {
                Environment.Exit(100);
            }


            // Decrypt data response.data
            // validate signature of decrypted byte[] data
            // decryptedData gets converted to string
            // converted decryptedData is used in DeserializeObject to convert it to ServerResponseDataDto




            var (isSignatureValid, decryptedData) =DecryptAndValidateSignature(response, _license);
            
            if (isSignatureValid)
            {
                ServerResponseDataDto? dataDto = JsonConvert.DeserializeObject<ServerResponseDataDto>(Encoding.UTF8.GetString(decryptedData));
                if (dataDto != null)
                {
                    return dataDto.isValid;
                }
                Environment.Exit(100);
            }
            else
            {
                Console.WriteLine("Signature is invalid");
                Environment.Exit(100);
            }


            Console.WriteLine("Response:");
            Console.WriteLine(response);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e}");
            Environment.Exit(100);
        }
        return false;
    }

    private async Task<ServerResponseDto?> SendPostRequest(string apiUrl, string bearerToken, ClientRequestDto clientRequestDto)
    {

        // convert ServerResponseDataDto in to json string // not needed
        // convert string to bytearray with getBytes
        // create signature of this byte array json string
        // encrypt data of the byte array json string
        
        // TODO: Add authentication header from bearerToken
        

        // Send Https to server
        using (HttpClient client = new HttpClient())
        {

            int maxRetries = 5;

            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                // Send the POST request
                HttpResponseMessage response = await client.PostAsJsonAsync(apiUrl, clientRequestDto);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read and return the response content
                    string responseContent = await response.Content.ReadAsStringAsync();
                    try
                    {
                        ServerResponseDto? dto = JsonConvert.DeserializeObject<ServerResponseDto>(responseContent);
                        if (dto != null)
                        {
                            return dto;
                        }
                        throw new Exception("dto variable came out null");
                        Environment.Exit(100);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Environment.Exit(100);
                    }
                }
                else
                {
                    // Handle error cases here (e.g., log or throw an exception)
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");

                    // Introduce a delay before the next retry
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }


        return null;
    }


    public void CreateValidationTimer()
    {
        DateTime now = DateTime.Now;
        DateTime nextMonth = now.AddMonths(1);
        DateTime firstOfMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1);
        TimeSpan delay = firstOfMonth - now;

        // Create a timer with the calculated delay
        Timer timer = new Timer(async state => await ExecuteMonthlyTask(), null, delay, TimeSpan.FromDays(30));
    }

    private async Task ExecuteMonthlyTask()
    {
        try
        {
            await ValidateLicense();
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error executing monthly task: {ex.Message}");
            Environment.Exit(100);
        }
    }


    private (byte[], byte[]) EcdhEncryption(byte[] localPublicKeyFile, byte[] localPrivateKeyFile, byte[] remotePublicKey, byte[] dataToEncrypt)
    {
        using ECDiffieHellman ecdhLocal = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        ecdhLocal.ImportSubjectPublicKeyInfo(localPublicKeyFile, out _);
        ecdhLocal.ImportECPrivateKey(localPrivateKeyFile, out _);

        using ECDiffieHellman ecdhRemote = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        ecdhRemote.ImportSubjectPublicKeyInfo(remotePublicKey, out _);

        byte[] sharedSecret = ecdhLocal.DeriveKeyFromHash(ecdhRemote.PublicKey, HashAlgorithmName.SHA256, null, null);

        // Use the shared secret for symmetric-key encryption (e.g., using AES)
        using Aes aes = Aes.Create();
        aes.Key = sharedSecret;
        aes.GenerateIV(); // Initialization Vector
        aes.Padding = PaddingMode.PKCS7;
        aes.Mode = CipherMode.CFB; // Choose an appropriate mode

        // Encrypt the data
        using ICryptoTransform encryptor = aes.CreateEncryptor();
        byte[] encryptedData = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
        return (encryptedData, aes.IV);

    }

    private byte[] EcdhDecryption(byte[] localPublicEcdhKey, byte[] localPrivateEcdhKey, byte[] remotePublicEcdhKey, byte[] dataToDecrypt, byte[] iv)
    {
        using ECDiffieHellman ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        ecdh.ImportSubjectPublicKeyInfo(localPublicEcdhKey, out _);
        ecdh.ImportECPrivateKey(localPrivateEcdhKey, out _);

        using ECDiffieHellman ecdhRemote = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        ecdhRemote.ImportSubjectPublicKeyInfo(remotePublicEcdhKey, out _);

        byte[] sharedSecret = ecdh.DeriveKeyFromHash(ecdhRemote.PublicKey, HashAlgorithmName.SHA256, null, null);

        using Aes aes = Aes.Create();
        aes.Key = sharedSecret;
        aes.IV = iv;
        aes.Padding = PaddingMode.PKCS7;
        aes.Mode = CipherMode.CFB; // Choose an appropriate mode

        // Encrypt the data
        using ICryptoTransform decryptor = aes.CreateDecryptor();
        byte[] decryptedData = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
        return decryptedData;

    }


    private byte[] SignData(byte[] localPrivateKey, byte[] bdata)
    {
        using ECDsa serverEcdsa = ECDsa.Create();
        serverEcdsa.ImportECPrivateKey(localPrivateKey, out _);
        byte[] signature = serverEcdsa.SignData(bdata, HashAlgorithmName.SHA256);
        return signature;

    }


    private bool ValidateSignature(byte[] remotePublicKeyFile, byte[] signature, byte[] bdata)
    {
        using ECDsa serverEcdsa = ECDsa.Create();
        serverEcdsa.ImportSubjectPublicKeyInfo(remotePublicKeyFile, out _);
        bool isSignatureValid = serverEcdsa.VerifyData(bdata, signature, HashAlgorithmName.SHA256);
        return isSignatureValid;

    }

   
    
    
    public ClientRequestDto EncryptAndSign(ClientRequestDataDto serverDataDto, License license)
    {
        // convert ServerResponseDataDto in to json string // not needed
        // convert string to bytearray with getBytes
        // create signature of this byte array json string
        // encrypt data of the byte array json string
        
        byte[] dataDtoBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(serverDataDto));
        byte[] signature = SignData(license.clientPrivateEcdsaKey!, dataDtoBytes);
        (byte[] encryptedData, byte[] vi) = EcdhEncryption(license.clientPublicEcdhKey!, license.clientPrivateEcdhKey!, license.serverPublicEcdhKey!, dataDtoBytes);
      
        var clientRequestDto = new ClientRequestDto() {
            data = encryptedData,
            signature = signature,
            iv = vi,
            clientId = int.Parse(_license.clientId),
            licenseId = int.Parse(_license.licenseId)
        };
        return clientRequestDto;
    }
    
    
    public (bool, byte[]) DecryptAndValidateSignature(ServerResponseDto serverResponseDto, License license)
    {
        // Decrypt data response.data
        // validate signature of decrypted byte[] data
        // decryptedData gets converted to string
        // converted decryptedData is used in DeserializeObject to convert it to ServerResponseDataDto

        byte[] decryptedData = EcdhDecryption(license.clientPublicEcdhKey!,license.clientPrivateEcdhKey!, license.serverPublicEcdhKey!, serverResponseDto.data, serverResponseDto.iv);
        bool isSignatureValid = ValidateSignature(license.serverPublicEcdsaKey!, serverResponseDto.signature,decryptedData);
        return (isSignatureValid, decryptedData); }
    
    
    
    

    private byte[]? LoadKeyFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            string key = File.ReadAllText(filePath).Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "").Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "").Replace("-----BEGIN EC PRIVATE KEY-----", "").Replace("-----END EC PRIVATE KEY-----", "")
                .Replace("-----BEGIN EC PUBLIC KEY-----", "").Replace("-----END EC PUBLIC KEY-----", "")
                .Replace("\r\n", "").Replace("\n", "").Trim();
            var keyBytes = Convert.FromBase64String(key);
            return keyBytes;
        }
        else
        {
            Console.WriteLine($"File not found: {filePath}");
            return null;
        }
    }

    


    private License ReadLicenseAndKeys(string licensePath)
    {
        using (ECDiffieHellman ecdhLocal = ECDiffieHellman.Create())
        {
            using (ECDiffieHellman ecdhRemote = ECDiffieHellman.Create())
            {
                using (ECDsa ecdsaLocal = ECDsa.Create())
                {
                    try
                    {
                        using (FileStream fs = new FileStream(licensePath, FileMode.Open))
                        {
                            var clientPrivateEcdhKey = new byte[223];
                            fs.Read(clientPrivateEcdhKey, 0, 223);
                            ecdhLocal.ImportECPrivateKey(clientPrivateEcdhKey, out _);
                            var clientPublicEcdhKey = new byte[158];
                            fs.Read(clientPublicEcdhKey, 0, 158);
                            ecdhLocal.ImportSubjectPublicKeyInfo(clientPublicEcdhKey, out _);
                            var serverPublicEcdhKey = new byte[158];
                            fs.Read(serverPublicEcdhKey, 0, 158);
                            ecdhRemote.ImportSubjectPublicKeyInfo(serverPublicEcdhKey, out _);
                            var clientPrivateEcdsaKey = new byte[223];
                            fs.Read(clientPrivateEcdsaKey, 0, 223);
                            ecdsaLocal.ImportECPrivateKey(clientPrivateEcdsaKey, out _);
                            var serverPublicEcdsaKey = new byte[158];
                            fs.Read(serverPublicEcdsaKey, 0, 158);
                            ecdhRemote.ImportSubjectPublicKeyInfo(serverPublicEcdsaKey, out _);

                            // Calculate the number of bytes remaining in the file
                            int remainingBytes = (int)(fs.Length - fs.Position);
                            byte[] license = new byte[remainingBytes];
                            fs.Read(license, 0, remainingBytes);
                            var readLicense = Encoding.UTF8.GetString(license);
                            byte[] decodedBytes = Convert.FromBase64String(readLicense);
                            readLicense = Encoding.UTF8.GetString(decodedBytes);
                            License? lic = JsonConvert.DeserializeObject<License>(readLicense);
                            if (lic == null)
                            {
                                Environment.Exit(100);
                            }
                            lic.clientPrivateEcdhKey = clientPrivateEcdhKey;
                            lic.clientPublicEcdhKey = clientPublicEcdhKey;
                            lic.serverPublicEcdhKey = serverPublicEcdhKey;
                            lic.clientPrivateEcdsaKey = clientPrivateEcdsaKey;
                            lic.serverPublicEcdsaKey = serverPublicEcdsaKey;
                            return lic;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error: {e}");
                        Environment.Exit(100);
                    }
                }
            }
        }
        return null;
    }


    (byte[], byte[], byte[], byte[], byte[]) ReadServerKeysForServer(string serverkeys)
    {
        using (ECDiffieHellman ecdhLocal = ECDiffieHellman.Create())
        {
            using (ECDiffieHellman ecdhRemote = ECDiffieHellman.Create())
            {
                using (ECDsa ecdsaLocal = ECDsa.Create())
                {
                    using (FileStream fs = new FileStream(serverkeys, FileMode.Open))
                    {
                        byte[] serverPrivateEcdhKey = new byte[223];
                        fs.Read(serverPrivateEcdhKey, 0, 223);
                        ecdhLocal.ImportECPrivateKey(serverPrivateEcdhKey, out _);
                        byte[] serverPublicEcdhKey = new byte[158];
                        fs.Read(serverPublicEcdhKey, 0, 158);
                        ecdhLocal.ImportSubjectPublicKeyInfo(serverPublicEcdhKey, out _);
                        byte[] clientPublicEcdhKey = new byte[158];
                        fs.Read(clientPublicEcdhKey, 0, 158);
                        ecdhRemote.ImportSubjectPublicKeyInfo(clientPublicEcdhKey, out _);
                        byte[] serverPrivateEcdsaKey = new byte[223];
                        fs.Read(serverPrivateEcdsaKey, 0, 223);
                        ecdsaLocal.ImportECPrivateKey(serverPrivateEcdsaKey, out _);
                        byte[] clientPublicEcdsaKey = new byte[158];
                        fs.Read(clientPublicEcdsaKey, 0, 158);
                        ecdhRemote.ImportSubjectPublicKeyInfo(clientPublicEcdsaKey, out _);
                        return (serverPrivateEcdhKey, serverPublicEcdhKey, clientPublicEcdhKey, serverPrivateEcdsaKey, clientPublicEcdsaKey);
                    }
                }
            }
        }
    }


    private string GetDeviceId()
    {
        try
        {
            string deviceId = new DeviceIdBuilder()
                .AddMachineName()
                .AddOsVersion()
                .OnWindows(windows => windows
                    .AddProcessorId()
                    .AddMotherboardSerialNumber()
                    .AddSystemDriveSerialNumber())
                .OnLinux(linux => linux
                    .AddMotherboardSerialNumber()
                    .AddSystemDriveSerialNumber())
                .OnMac(mac => mac
                    .AddSystemDriveSerialNumber()
                    .AddPlatformSerialNumber())
                .ToString();
            return deviceId;
        }
        catch (Exception e)
        {
            //Console.WriteLine(e);
        }

        try
        {
            string deviceId = new DeviceIdBuilder()
                .AddMachineName()
                .AddOsVersion()
                .OnWindows(windows => WindowsDeviceIdBuilderExtensions.AddProcessorId(WindowsDeviceIdBuilderExtensions.AddMotherboardSerialNumber(WindowsDeviceIdBuilderExtensions.AddSystemDriveSerialNumber(windows))))
                .OnLinux(linux => linux
                    .AddMotherboardSerialNumber()
                    .AddSystemDriveSerialNumber())
                .OnMac(mac => mac
                    .AddSystemDriveSerialNumber()
                    .AddPlatformSerialNumber())
                .ToString();
            return deviceId;
        }
        catch (Exception e)
        {
            Console.WriteLine("Device Id generation failed");
            return "0";
        }

    }


    void GenerateLicenseAndServerKeys(string licensePath, string serverKeysPath)
    {
        var licensetext = "{\"licenseId\":\"1\",\"clientId\":\"1\",\"client\":\"tehnolink\",\"license\":\"blabla\",\"apiUrl\":\"http://localhost:5000/api/License/CheckLicense\",\"token\":\"bearertoken\"}";
        var licenseBytes = Encoding.UTF8.GetBytes(licensetext);
        string base64String = Convert.ToBase64String(licenseBytes);
        licenseBytes = Encoding.UTF8.GetBytes(base64String);
        using (ECDiffieHellman ecdhLocal = ECDiffieHellman.Create())
        {
            using (ECDiffieHellman ecdhRemote = ECDiffieHellman.Create())
            {
                using (ECDsa ecdsaLocal = ECDsa.Create())
                {
                    using (ECDsa ecdsaRemote = ECDsa.Create())
                    {
                        var remoteLicense = ecdhLocal.ExportECPrivateKey().Concat(ecdhLocal.ExportSubjectPublicKeyInfo()).Concat(ecdhRemote.ExportSubjectPublicKeyInfo()).Concat(ecdsaLocal.ExportECPrivateKey()).Concat(ecdsaRemote.ExportSubjectPublicKeyInfo()).Concat(licenseBytes).ToArray();
                        File.WriteAllBytes(licensePath, remoteLicense);

                        var serverKeysBtyes = ecdhRemote.ExportECPrivateKey().Concat(ecdhRemote.ExportSubjectPublicKeyInfo()).Concat(ecdhLocal.ExportSubjectPublicKeyInfo()).Concat(ecdsaRemote.ExportECPrivateKey()).Concat(ecdsaLocal.ExportSubjectPublicKeyInfo()).ToArray();
                        File.WriteAllBytes(serverKeysPath, serverKeysBtyes);

                    }
                }
            }
        }
    }
}
