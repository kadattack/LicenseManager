using System.Security.Cryptography;
using System.Text;
using LicenseValidationServer.Models;
using Newtonsoft.Json;
using TehnolinkLicenseValidation.DTOs;

namespace LicenseValidationServer.Utils;

public class Utils
{

   public (byte[], byte[]) EcdhEncryption(byte[] localPublicKeyFile, byte[] localPrivateKeyFile, byte[] remotePublicKey, byte[] dataToEncrypt)
    {
        using (ECDiffieHellman ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256))
        {
            ecdh.ImportSubjectPublicKeyInfo(localPublicKeyFile, out _);
            ecdh.ImportECPrivateKey(localPrivateKeyFile, out _);

            using (ECDiffieHellman ecdhRemote = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256))
            {
                ecdhRemote.ImportSubjectPublicKeyInfo(remotePublicKey, out _);

                byte[] sharedSecret = ecdh.DeriveKeyFromHash(ecdhRemote.PublicKey, HashAlgorithmName.SHA256, null, null);

                // Use the shared secret for symmetric-key encryption (e.g., using AES)
                using (Aes aes = Aes.Create())
                {
                    aes.Key = sharedSecret;
                    aes.GenerateIV(); // Initialization Vector
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CFB; // Choose an appropriate mode

                    // Encrypt the data
                    using (ICryptoTransform encryptor = aes.CreateEncryptor())
                    {
                        byte[] encryptedData = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
                        return (encryptedData, aes.IV);
                    }
                }
            }
        }
    }

    public byte[]  EcdhDecryption(byte[] localPublicKeyFile, byte[] localPrivateKeyFile, byte[] remotePublicKey, byte[] dataToDecrypt, byte[] iv)
    {
        using (ECDiffieHellman ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256))
        {
            ecdh.ImportSubjectPublicKeyInfo(localPublicKeyFile, out _);
            ecdh.ImportECPrivateKey(localPrivateKeyFile, out _);

            using (ECDiffieHellman ecdhRemote = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256))
            {
                ecdhRemote.ImportSubjectPublicKeyInfo(remotePublicKey, out _);

                byte[] sharedSecret = ecdh.DeriveKeyFromHash(ecdhRemote.PublicKey, HashAlgorithmName.SHA256, null, null);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = sharedSecret;
                    aes.IV = iv;
                    aes.Padding = PaddingMode.PKCS7;
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
    }


    public byte[] SignData(byte[] localPrivateKey, byte[] bdata)
    {
        using (ECDsa serverEcdsa = ECDsa.Create())
        {
            serverEcdsa.ImportECPrivateKey(localPrivateKey, out _);
            byte[] signature = serverEcdsa.SignData(bdata, HashAlgorithmName.SHA256);
            return signature;
        }
    }


    public bool ValidateSignature(byte[] remotePublicKeyFile,  byte[] signature, byte[] bdata)
    {
        using (ECDsa serverEcdsa = ECDsa.Create())
        {
            serverEcdsa.ImportSubjectPublicKeyInfo(remotePublicKeyFile, out _);
            bool isSignatureValid = serverEcdsa.VerifyData(bdata, signature, HashAlgorithmName.SHA256);
            return isSignatureValid;
        }
    }


    public void SaveECDsaKeyToFile(string pemKey, string filePath)
    {
        File.WriteAllText(filePath, pemKey);
        Console.WriteLine($"Key saved to {filePath}");
    }

    public byte[] LoadKeyFromFile(string filePath)
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

    public ServerResponseDto EncryptAndSign(ServerResponseDataDto serverDataDto, License license)
    {
        // convert ServerResponseDataDto in to json string // not needed
        // convert string to bytearray with getBytes
        // create signature of this byte array json string
        // encrypt data of the byte array json string
        
        byte[] dataDtoBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(serverDataDto));
        byte[] signature = SignData(license.server_private_ecdsa_key, dataDtoBytes);
        (byte[] encryptedData, byte[] vi) = EcdhEncryption(license.server_public_ecdh_key, license.server_private_ecdh_key, license.client_public_ecdh_key, dataDtoBytes);

        var serverResponseDto = new ServerResponseDto() {
            data = encryptedData,
            signature = signature,
            iv = vi
        };
        return serverResponseDto;
    }
    
    
    public (bool, byte[]) DecryptDataAndValidateSignature(ClientRequestDto clientRequestDto, License license)
    {
        // Decrypt data response.data
        // validate signature of decrypted byte[] data
        // decryptedData gets converted to string
        // converted decryptedData is used in DeserializeObject to convert it to ServerResponseDataDto

        byte[] decryptedData = EcdhDecryption(license.server_public_ecdh_key, license.server_private_ecdh_key, license.client_public_ecdh_key,clientRequestDto.data, clientRequestDto.iv);
        bool isSignatureValid = ValidateSignature(license.client_public_ecdsa_key, clientRequestDto.signature,decryptedData);
        return (isSignatureValid, decryptedData);

    }

    

    public (byte[], byte[], byte[], byte[], byte[], string) ReadLicenseAndKeys(string licensePath)
    {
        using (ECDiffieHellman ecdhLocal = ECDiffieHellman.Create())
        {
            using (ECDiffieHellman ecdhRemote = ECDiffieHellman.Create())
            {
                using (ECDsa ecdsaLocal = ECDsa.Create())
                {
                    using (FileStream fs = new FileStream(licensePath, FileMode.Open))
                    {
                        byte[] clientPrivateEcdhKey = new byte[223];
                        fs.Read(clientPrivateEcdhKey, 0, 223);
                        ecdhLocal.ImportECPrivateKey(clientPrivateEcdhKey, out _);
                        byte[] clientPublicEcdhKey = new byte[158];
                        fs.Read(clientPublicEcdhKey, 0, 158);
                        ecdhLocal.ImportSubjectPublicKeyInfo(clientPublicEcdhKey, out _);
                        byte[] serverPublicEcdhKey = new byte[158];
                        fs.Read(serverPublicEcdhKey, 0, 158);
                        ecdhRemote.ImportSubjectPublicKeyInfo(serverPublicEcdhKey, out _);
                        byte[] clientPrivateEcdsaKey = new byte[223];
                        fs.Read(clientPrivateEcdsaKey, 0, 223);
                        ecdsaLocal.ImportECPrivateKey(clientPrivateEcdsaKey, out _);
                        byte[] serverPublicEcdsaKey = new byte[158];
                        fs.Read(serverPublicEcdsaKey, 0, 158);
                        ecdhRemote.ImportSubjectPublicKeyInfo(serverPublicEcdsaKey, out _);

                        // Calculate the number of bytes remaining in the file
                        int remainingBytes = (int)(fs.Length - fs.Position);
                        byte[] license = new byte[remainingBytes];
                        fs.Read(license, 0, remainingBytes);
                        var readLicense = Encoding.UTF8.GetString(license);
                        byte[] decodedBytes = Convert.FromBase64String(readLicense);
                        readLicense = Encoding.UTF8.GetString(decodedBytes);

                        return (clientPrivateEcdhKey, clientPublicEcdhKey, serverPublicEcdhKey, clientPrivateEcdsaKey, serverPublicEcdsaKey, readLicense);
                    }

                }
            }
        }
    }


    public (byte[], byte[], byte[], byte[], byte[]) ReadServerKeysForServer(string serverkeys)
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

   
    public License GenerateLicense(string licensePath, AppDbContext context, Client client, Product product)
    {
        var licensetext = "{\"licenseId\":\"1\",\"clientId\":\"1\",\"client\":\"tehnolink\",\"license\":\"license\",\"apiUrl\":\"http://localhost:5000/api/License/CheckLicense\",\"token\":\"bearertoken\"}";
        var licenseBytes = Encoding.UTF8.GetBytes(licensetext);
        string base64String = Convert.ToBase64String(licenseBytes);
        licenseBytes = Encoding.UTF8.GetBytes(base64String);
        using (ECDiffieHellman ecdhServer = ECDiffieHellman.Create())
        {
            using (ECDiffieHellman ecdhClient = ECDiffieHellman.Create())
            {
                using (ECDsa ecdsaServer = ECDsa.Create())
                {
                    using (ECDsa ecdsaClient = ECDsa.Create())
                    {
                        var remoteLicense = ecdhClient.ExportECPrivateKey().Concat(ecdhClient.ExportSubjectPublicKeyInfo()).Concat(ecdhServer.ExportSubjectPublicKeyInfo()).Concat(ecdsaClient.ExportECPrivateKey()).Concat(ecdsaServer.ExportSubjectPublicKeyInfo()).Concat(licenseBytes).ToArray();
                        File.WriteAllBytes(licensePath, remoteLicense);

                        //var serverKeysBtyes = ecdhClient.ExportECPrivateKey().Concat(ecdhClient.ExportSubjectPublicKeyInfo()).Concat(ecdhServer.ExportSubjectPublicKeyInfo()).Concat(ecdsaClient.ExportECPrivateKey()).Concat(ecdsaServer.ExportSubjectPublicKeyInfo()).ToArray();
                        //File.WriteAllBytes(serverKeysPath, serverKeysBtyes);
                        
                        var lic1 = new License()
                        {
                            product = product,
                            activation_refreshed = DateTime.UtcNow,
                            active = true,
                            hardware_lock = false,
                            first_activation = DateTime.UtcNow,
                            hardware_id = null,
                            license = "license",
                            client = client,
                            server_private_ecdh_key = ecdhServer.ExportECPrivateKey(),
                            server_public_ecdh_key = ecdhServer.ExportSubjectPublicKeyInfo(),
                            server_private_ecdsa_key = ecdsaServer.ExportECPrivateKey(),
                            server_public_ecdsa_key = ecdsaServer.ExportSubjectPublicKeyInfo(),
                            client_private_ecdh_key = ecdhClient.ExportECPrivateKey(),
                            client_public_ecdh_key = ecdhClient.ExportSubjectPublicKeyInfo(),
                            client_private_ecdsa_key = ecdsaClient.ExportECPrivateKey(),
                            client_public_ecdsa_key = ecdsaClient.ExportSubjectPublicKeyInfo()
                        };
                        
                        client.licenses.Add(lic1);
                        context.Client.Update(client);
                        context.SaveChanges();
                        return lic1;
                    }
                }
            }
        }
    }


    
    
}
