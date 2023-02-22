using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.Infrastructures.Helpers
{
    public class RevenueMonsterSignature
    {
        public string GenerateSignature(string compactJson, string method, string nonceStr, string privateKey, string requestUrl, string signType, string timestamp)
        {
            string signedData = "";
            try
            {
                //GenerateSignatureResult result = new GenerateSignatureResult();
                string plainText = "";
                if (compactJson != "")
                {
                    string encodedData = Encode.Base64Encode(compactJson);
                    plainText = String.Format("data={0}&method={1}&nonceStr={2}&requestUrl={3}&signType={4}&timestamp={5}", encodedData, method, nonceStr, requestUrl, signType, timestamp);
                }
                else
                {
                    plainText = String.Format("method={0}&nonceStr={1}&requestUrl={2}&signType={3}&timestamp={4}", method, nonceStr, requestUrl, signType, timestamp);
                }
                byte[] plainTextByte = Encoding.UTF8.GetBytes(plainText);
                RSACryptoServiceProvider provider = PemKeyUtils.GetRSAProviderFromPemFile(privateKey);
                string prikey = provider.ToXmlString(true);
                byte[] signedBytes = provider.SignData(plainTextByte, CryptoConfig.MapNameToOID("SHA256"));
                signedData = Convert.ToBase64String(signedBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return signedData;
        }

        public static byte[] RSAEncrypt(byte[] plaintext, string destKey)
        {
            byte[] encryptedData;
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(destKey);
            encryptedData = rsa.Encrypt(plaintext, true);
            rsa.Dispose();
            return encryptedData;
        }
    }
}
