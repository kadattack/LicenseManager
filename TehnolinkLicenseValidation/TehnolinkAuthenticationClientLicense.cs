using System.Text;
using Client.ecdsa_dotnet.EcdsaDotNet.EcdsaDotNet;
using Newtonsoft.Json;
using TehnolinkLicenseValidation.DTOs;
using System.Security.Cryptography;
using LicenseValidationServer.DTO;


namespace TehnolinkLicenseValidation;

public class TehnolinkAuthenticationClientLicense
{
    byte[] _publicKey;
    byte[] _publicEcdsaKey;
    byte[] _privateKey;
    byte[] _privateEcdsaKey;

    public TehnolinkAuthenticationClientLicense(string publicKeyPath, string privateKeyPath, string publicEcdsaKeyPath, string privateEcdsaKeyPath)
    {
        _publicKey = LoadKeyFromFile(publicKeyPath);
        _publicEcdsaKey = LoadKeyFromFile(publicEcdsaKeyPath);
        _privateKey = LoadKeyFromFile(privateKeyPath);
        _privateEcdsaKey = LoadKeyFromFile(privateEcdsaKeyPath);
    }

    public async Task<Boolean> ValidateLicense()
    {
        try
        {
            // Replace these values with your actual API endpoint, Bearer token, and request data
            string apiUrl = "https://your-api-endpoint.com/api/resource";
            string bearerToken = "your-bearer-token";
            string requestData = "{\"license\":\"blablabla\",\"client\":\"someclient\"}";




            // Send the POST request with Bearer token
            ServerResponseDto? response = await SendPostRequest(apiUrl, bearerToken, requestData);
            if (response == null)
            {
                Environment.Exit(100);
            }


            // Decrypt data response.data
            // validate signature of decrypted byte[] data
            // decryptedData gets converted to string
            // converted decryptedData is used in DeserializeObject to convert it to ServerResponseDataDto



            var decryptedData = EcdhDecryption(_publicKey, _privateKey, response.data);
            bool isSignatureValid = ValidateSignature(_publicEcdsaKey, _privateEcdsaKey, response.signature, decryptedData);

            if (isSignatureValid)
            {
                ServerResponseDataDto dataDto = JsonConvert.DeserializeObject<ServerResponseDataDto>(Encoding.UTF8.GetString(decryptedData));
                if (dataDto.isValid == true)
                {
                    return true;
                }
                Environment.Exit(100);
            }
            else
            {
                Environment.Exit(100);
            }


            Console.WriteLine("Response:");
            Console.WriteLine(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Environment.Exit(100);
        }
        return false;
    }

    private async Task<ServerResponseDto?> SendPostRequest(string apiUrl, string bearerToken, string requestData)
    {

        // convert ServerResponseDataDto in to json string // not needed
        // convert string to bytearray with getBytes
        // create signature of this byte array json string
        // encrypt data of the byte array json string


        var requestDataBytes = Encoding.UTF8.GetBytes(requestData);
        var signature = SignData(_publicEcdsaKey, _privateEcdsaKey, requestDataBytes);
        var encryptedData = EcdhEncryption(_publicKey, _privateKey, requestDataBytes);



        // Send Https to server
        using (HttpClient client = new HttpClient())
        {

            var clientRequestDataDto = new ClientRequestDto() {
                data = encryptedData,
                signature = signature
            };

            // Transform DTO in to string
            string jsonContent = JsonConvert.SerializeObject(clientRequestDataDto);

            // Set Bearer token in the Authorization header
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

            // Prepare the request content
            StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Number of retries
            int maxRetries = 5;

            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                // Send the POST request
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read and return the response content
                    string responseContent = await response.Content.ReadAsStringAsync();
                    ServerResponseDto dto = JsonConvert.DeserializeObject<ServerResponseDto>(responseContent);
                    return dto;

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


    private byte[] EcdhEncryption(byte[] publicKeyFile, byte[] privateKeyFile, byte[] dataToEncrypt)
    {
        using (ECDiffieHellman ecdh = ECDiffieHellman.Create())
        {
            ecdh.ImportSubjectPublicKeyInfo(publicKeyFile, out _);
            ecdh.ImportECPrivateKey(privateKeyFile, out _);


            // Perform key exchange
            byte[] sharedSecret = ecdh.DeriveKeyFromHash(ecdh.PublicKey, HashAlgorithmName.SHA256, null, null);

            // Use the shared secret for symmetric-key encryption (e.g., using AES)
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = sharedSecret;
                aes.GenerateIV(); // Initialization Vector
                aes.Mode = CipherMode.CFB; // Choose an appropriate mode

                // Encrypt the data
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] encryptedData = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
                    return encryptedData;
                }
            }
        }
    }

    private byte[] EcdhDecryption(byte[] publicKeyFile, byte[] privateKeyFile, byte[] dataToDecrypt)
    {
        using (ECDiffieHellman ecdh = ECDiffieHellman.Create())
        {
            ecdh.ImportSubjectPublicKeyInfo(publicKeyFile, out _);
            ecdh.ImportECPrivateKey(privateKeyFile, out _);

            byte[] sharedSecret = ecdh.DeriveKeyFromHash(ecdh.PublicKey, HashAlgorithmName.SHA256, null, null);

            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = sharedSecret;
                aes.GenerateIV(); // Initialization Vector
                aes.Mode = CipherMode.CFB; // Choose an appropriate mode

                // Encrypt the data
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] decryptedData = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
                    return decryptedData;
                }
            }
        }
    }


    private byte[] SignData(byte[] publicEcdsaKeyFile, byte[] privateEcdsaKeyFile, byte[] bdata)
    {
        using (ECDsa serverEcdsa = ECDsa.Create())
        {
            serverEcdsa.ImportECPrivateKey(privateEcdsaKeyFile, out _);
            serverEcdsa.ImportSubjectPublicKeyInfo(publicEcdsaKeyFile, out _);
            byte[] signature = serverEcdsa.SignData(bdata, HashAlgorithmName.SHA256);
            return signature;
        }
    }


    public bool ValidateSignature(byte[] publicEcdsaKeyFile, byte[] privateEcdsaKeyFile, byte[] signature, byte[] data)
    {
        using (ECDsa serverEcdsa = ECDsa.Create())
        {
            serverEcdsa.ImportECPrivateKey(privateEcdsaKeyFile, out _);
            serverEcdsa.ImportSubjectPublicKeyInfo(publicEcdsaKeyFile, out _);
            bool isSignatureValid = serverEcdsa.VerifyData(data, signature, HashAlgorithmName.SHA256);
            return isSignatureValid;
        }
    }


    private void SaveECDsaKeyToFile(string pemKey, string filePath)
    {
        File.WriteAllText(filePath, pemKey);
        Console.WriteLine($"Key saved to {filePath}");
    }

    private byte[] LoadKeyFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            string key = File.ReadAllText(filePath).Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "").Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "").Replace("-----BEGIN EC PRIVATE KEY-----", "").Replace("-----END EC PRIVATE KEY-----", "")
                .Replace("-----BEGIN EC PUBLIC KEY-----", "").Replace("-----END EC PUBLIC KEY-----", "")
                .Replace("\r\n", "").Replace("\n", "").Trim();
            Console.WriteLine($"Private key loaded from {filePath}");
            var keyBytes = Convert.FromBase64String(key);

            return keyBytes;
        }
        else
        {
            Console.WriteLine($"File not found: {filePath}");
            return null;
        }
    }


    public void GenerateEcdhKeyPair(string publicKeyPath, string privateKeyPath)
    {
        using (ECDiffieHellman ecdh = ECDiffieHellman.Create())
        {
            SaveECDsaKeyToFile(ecdh.ExportECPrivateKeyPem(), privateKeyPath);
            SaveECDsaKeyToFile(ecdh.ExportSubjectPublicKeyInfoPem(), publicKeyPath);
        }
    }
    
    public void GenerateEcdsaKeyPair(string publicKeyPath, string privateKeyPath)
    {
        using (ECDsa ecdsa = ECDsa.Create())
        {
            SaveECDsaKeyToFile(ecdsa.ExportECPrivateKeyPem(), privateKeyPath);
            SaveECDsaKeyToFile(ecdsa.ExportSubjectPublicKeyInfoPem(), publicKeyPath);
        }
    }


}
