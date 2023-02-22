using System;
using System.Collections.Generic;
using System.Text;

namespace Voupon.Common.Encryption
{
    public class ChatToken
    {
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public string ToChatTokenValue(bool isBase64Token = true)
        {
            string passPhrase = "Azs5pr@t9";        // can be any string
            string saltValue = "z@1tV0dus";        // can be any string
            string hashAlgorithm = "SHA1";             // can be "MD5"
            int passwordIterations = 2;                // can be any number
            string initVector = "@1B2c3D4e5F6g7H8"; // must be 16 bytes
            int keySize = 256;                // can be 192 or 128

            var token = string.Format("{0}|{1}",
                this.Email, this.CreatedAt.ToString("yyyy-MM-dd hh:mm:ss"));


            if (isBase64Token)
            {
                return Base64Encode(RijndaelEncryption.Encrypt(token, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize));
            }

            return RijndaelEncryption.Encrypt(token, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);
        }

        public static ChatToken FromChatTokenValue(string encryptedToken, bool isBase64Token = true)
        {
            string passPhrase = "Azs5pr@t9";        // can be any string
            string saltValue = "z@1tV0dus";        // can be any string
            string hashAlgorithm = "SHA1";             // can be "MD5"
            int passwordIterations = 2;                // can be any number
            string initVector = "@1B2c3D4e5F6g7H8"; // must be 16 bytes
            int keySize = 256;                // can be 192 or 128

            try
            {
                if (isBase64Token)
                {
                    encryptedToken = Base64Decode(encryptedToken);
                }

                string[] decryptedToken = RijndaelEncryption.Decrypt(encryptedToken, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize).Split(new char[] { '|' });
                return new ChatToken()
                {
                    Email = decryptedToken[0].ToString(),
                    CreatedAt = DateTime.Parse(decryptedToken[1])
                };
            }
            catch (Exception ex)
            {
                //  @todo: log
                return null;
            }

        }
    }
}
