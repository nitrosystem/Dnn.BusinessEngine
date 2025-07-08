//using Microsoft.JScript;
//using Newtonsoft.Json;
//using NitroSystem.Dnn.BusinessEngine.Core.Security.Models;
//using System;
//using System.IO;
//using System.Security.Cryptography;
//using System.Text;

//namespace NitroSystem.Dnn.BusinessEngine.Core.Security
//{
//    public class BusinessEngineEncryptor
//    {
//        public static EncryptedModel Encrypt<T>(T model, RSAParameters publicKey)
//        {
//            var json = JsonSerializer.Serialize(model);
//            return EncryptInternal(Encoding.UTF8.GetBytes(json), publicKey);
//        }

//        public static EncryptedModel EncryptFile(byte[] fileBytes, RSAParameters publicKey)
//        {
//            return EncryptInternal(fileBytes, publicKey);
//        }

//        public static T Decrypt<T>(EncryptedModel encryptedModel, RSAParameters privateKey)
//        {
//            var decryptedBytes = DecryptInternal(encryptedModel, privateKey);
//            var json = Encoding.UTF8.GetString(decryptedBytes);
//            return JsonSerializer.Deserialize<T>(json);
//        }

//        public static byte[] DecryptFile(EncryptedModel encryptedModel, RSAParameters privateKey)
//        {
//            return DecryptInternal(encryptedModel, privateKey);
//        }

//        private static EncryptedModel EncryptInternal(byte[] data, RSAParameters publicKey)
//        {
//            using (var aes = Aes.Create())
//            {
//                aes.KeySize = 256;
//                aes.GenerateKey();
//                aes.GenerateIV();
//                aes.Mode = CipherMode.GCM;

//                using (var encryptor = aes.CreateEncryptor())
//                using (var ms = new MemoryStream())
//                {
//                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
//                    {
//                        cs.Write(data, 0, data.Length);
//                        cs.FlushFinalBlock();
//                    }

//                    var encryptedDataBytes = ms.ToArray();

//                    using (var rsa = RSA.Create())
//                    {
//                        rsa.ImportParameters(publicKey);
//                        var encryptedKeyBytes = rsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA256);

//                        return new EncryptedModel
//                        {
//                            EncryptedKey = Convert.ToBase64String(encryptedKeyBytes),
//                            EncryptedData = Convert.ToBase64String(encryptedDataBytes),
//                            IV = Convert.ToBase64String(aes.IV)
//                        };
//                    }
//                }
//            }
//        }

//        private static byte[] DecryptInternal(EncryptedModel model, RSAParameters privateKey)
//        {
//            using (var rsa = RSA.Create())
//            {
//                rsa.ImportParameters(privateKey);
//                var aesKey = rsa.Decrypt(Convert.FromBase64String(model.EncryptedKey), RSAEncryptionPadding.OaepSHA256);

//                using (var aes = Aes.Create())
//                {
//                    aes.Key = aesKey;
//                    aes.IV = Convert.FromBase64String(model.IV);
//                    aes.Mode = CipherMode.GCM;

//                    using (var decryptor = aes.CreateDecryptor())
//                    using (var ms = new MemoryStream(Convert.FromBase64String(model.EncryptedData)))
//                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
//                    using (var output = new MemoryStream())
//                    {
//                        cs.CopyTo(output);
//                        return output.ToArray();
//                    }
//                }
//            }
//        }
//    }
//}
