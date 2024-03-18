


using System.Security.Cryptography;
using System.Text;
using TehnolinkLicenseValidation;

/*
var valProtocol = new TehnolinkAuthenticationClientLicense("../../../License/serverPublicEcdhKey.pem", "../../../License/clientPrivateEcdhKey.pem", "../../../License/serverPublicEcdsaKey.pem", "../../../License/clientPrivateEcdsaKey.pem");
valProtocol.GenerateEcdhKeyPair("../../../License/serverPublicEcdhKey.pem", "../../../License/serverPrivateEcdhKey.pem");
valProtocol.GenerateEcdsaKeyPair("../../../License/serverPublicEcdsaKey.pem", "../../../License/serverPrivateEcdsaKey.pem");
valProtocol.GenerateEcdhKeyPair("../../../License/clientPublicEcdhKey.pem", "../../../License/clientPrivateEcdhKey.pem");
valProtocol.GenerateEcdsaKeyPair("../../../License/clientPublicEcdsaKey.pem", "../../../License/clientPrivateEcdsaKey.pem");



var valProtocol = new TehnolinkAuthenticationClientLicense();
//var isValid = valProtocol.ValidateLicense();
//Console.Write(isValid);
/*

var serverpublickey = valProtocol.LoadKeyFromFile("/home/newdev/RiderProjects/LicenseManager/LicenseValidationServer/License/serverPublicEcdhKey.pem");
var serverprivatekey = valProtocol.LoadKeyFromFile("/home/newdev/RiderProjects/LicenseManager/LicenseValidationServer/License/serverPrivateEcdhKey.pem");
var clientPublicEcdhKey = valProtocol.LoadKeyFromFile("/home/newdev/RiderProjects/LicenseManager/LicenseValidationServer/License/clientPublicEcdhKey.pem");
var clientPrivateEcdhKey = valProtocol.LoadKeyFromFile("/home/newdev/RiderProjects/LicenseManager/LicenseValidationServer/License/clientPrivateEcdhKey.pem");


var serverPublicEcdsaKey = valProtocol.LoadKeyFromFile("/home/newdev/RiderProjects/LicenseManager/LicenseValidationServer/License/serverPublicEcdsaKey.pem");
var serverPrivateEcdsaKey = valProtocol.LoadKeyFromFile("/home/newdev/RiderProjects/LicenseManager/LicenseValidationServer/License/serverPrivateEcdsaKey.pem");
var clientPublicEcdsaKey = valProtocol.LoadKeyFromFile("/home/newdev/RiderProjects/LicenseManager/LicenseValidationServer/License/clientPublicEcdsaKey.pem");
var clientPrivateEcdsaKey = valProtocol.LoadKeyFromFile("/home/newdev/RiderProjects/LicenseManager/LicenseValidationServer/License/clientPrivateEcdsaKey.pem");


var dataToEncrypt = Encoding.UTF8.GetBytes("abc");


//var enc = valProtocol.EcdhEncryption(clientpublickey,serverprivatekey, data);
//var dec = valProtocol.EcdhDecryption(serverpublickey,clientprivatekey, enc.Item1, enc.Item2, enc.Item3);




/*

//var clientPrivateEcdhKey = "MIHcAgEBBEIBc5YpSUGTfhG4Gf/BnUw55kNmanPlINNoVz03yO2nRp07eaJ+ssLbLkkW2Cpj0gNyzz0HZU0+M7pjUOThFdECRkygBwYFK4EEACOhgYkDgYYABAAgNqGM6eF9QmeF7Kj8BBKe67fM/ugoBAdivB0jvYE5h/pWcuE5tClGQNrPN27dHdmOS4deoGLlyM4aDmf2kryR5QDOvenRxwetjyGzkLsVjxmmmFuumpI2Ok5h7rHHjIrIiN9G6Z4kBoBSsb1+xrVexJE6N1xu1ILd4eWV7KnGPAg/Pw==";
//var clientPublicEcdhKey = "MIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQAIDahjOnhfUJnheyo/AQSnuu3zP7oKAQHYrwdI72BOYf6VnLhObQpRkDazzdu3R3ZjkuHXqBi5cjOGg5n9pK8keUAzr3p0ccHrY8hs5C7FY8ZpphbrpqSNjpOYe6xx4yKyIjfRumeJAaAUrG9fsa1XsSROjdcbtSC3eHlleypxjwIPz8=";
var serverPrivateEcdhKey = "MIHcAgEBBEIBy740irnQMJrM5rR8/Vi6ow8vm7BxgbDOdHkivxbxdOjLRZIarx1/C/XZ9/ZHUJHSagH8M9TkwFnA456AlgvvZjWgBwYFK4EEACOhgYkDgYYABAEHYhXs6RDvyzWw4yP6m1wj9j4/QoYeNsdrzvm4MyrwDe4c+8dGz/6Lnx1zyzq9sGQLsLjcsZVq2RQVaqvord9isgD3uXm0oGfgy5i1DmFAvcPYeO+FtGfnd3U+hWGsCESdwwI6it3bYp8+iyr0OgpKvoAudq6OZqxrXO8uifi6bt9k5w==";
var serverPublicEcdhKey = "MIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQBB2IV7OkQ78s1sOMj+ptcI/Y+P0KGHjbHa875uDMq8A3uHPvHRs/+i58dc8s6vbBkC7C43LGVatkUFWqr6K3fYrIA97l5tKBn4MuYtQ5hQL3D2HjvhbRn53d1PoVhrAhEncMCOord22KfPosq9DoKSr6ALnaujmasa1zvLon4um7fZOc=";


        using (ECDiffieHellman ecdhServer = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256))
        {

            using (ECDiffieHellman ecdhClient = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256))
            {

               
                byte[] sharedSecret = ecdhServer.DeriveKeyFromHash(ecdhClient.PublicKey, HashAlgorithmName.SHA256, null, null);
                Console.WriteLine(Convert.ToBase64String(sharedSecret));
                var tempiv = new byte[]{};
                byte[] encryptedData = new byte[] {};
                using (Aes aes = Aes.Create())
                {
                    aes.Key = sharedSecret;
                    aes.GenerateIV(); // Initialization Vector
                    aes.Mode = CipherMode.CFB; // Choose an appropriate mode
                    tempiv = aes.IV;

                    // Encrypt the data
                    using (ICryptoTransform encryptor = aes.CreateEncryptor())
                    {
                        encryptedData = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
                    }
                }

                byte[] sharedSecret2 = ecdhClient.DeriveKeyFromHash(ecdhServer.PublicKey, HashAlgorithmName.SHA256, null, null);
                Console.WriteLine(Convert.ToBase64String(sharedSecret2));
                using (Aes aes = Aes.Create())
                {
                    aes.Key = sharedSecret2;
                    aes.IV = tempiv;
                    //aes.GenerateIV(); // Initialization Vector
                    aes.Mode = CipherMode.CFB; // Choose an appropriate mode

                    // Encrypt the data
                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    {
                        byte[] decryptedData = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                        Console.WriteLine(Encoding.UTF8.GetString(decryptedData));
                    }
                }
            }
            
        }





var dataToSign = new byte[] { 1, 2, 3, 4, 5 }; // Data to sign and verify

using (ECDsa clientEcdsa = ECDsa.Create())
{
    // Import the client's private key
    clientEcdsa.ImportECPrivateKey(clientPrivateEcdsaKey, out _);

    // Sign the data with the client's private key
    byte[] signature = clientEcdsa.SignData(dataToSign, HashAlgorithmName.SHA256);

    using (ECDsa serverEcdsa = ECDsa.Create())
    {
        // Import the server's public key
        serverEcdsa.ImportSubjectPublicKeyInfo(clientPublicEcdsaKey, out _);

        // Verify the signature using the client's public key
        bool isSignatureValid = serverEcdsa.VerifyData(dataToSign, signature, HashAlgorithmName.SHA256);
        Console.WriteLine("Signature is valid: " + isSignatureValid);
    }
}*/