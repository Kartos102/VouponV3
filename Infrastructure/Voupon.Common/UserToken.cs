using System;
using System.Collections.Generic;
using System.Text;
using Voupon.Common.Encryption;
using Voupon.Common.Extensions;

namespace Voupon.Common
{
    public class UserToken
    {
        public long MemberProfileId { get; set; }
        public string Guid { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CurrentCommercialId { get; set; }
        public int LastAnsweredSurveyQuestionId { get; set; }
        public int NextQuestionId { get; set; }
        public int MemberMasterId { get; set; }

        public string ToTokenValue(bool isBase64Token = true)
        {
            string passPhrase = "Pas5pr@se";        // can be any string
            string saltValue = "s@1tValue";        // can be any string
            string hashAlgorithm = "SHA1";             // can be "MD5"
            int passwordIterations = 2;                // can be any number
            string initVector = "@1B2c3D4e5F6g7H8"; // must be 16 bytes
            int keySize = 256;                // can be 192 or 128

            this.NextQuestionId = 123456;
            this.LastAnsweredSurveyQuestionId = 123456;
            var token = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                this.MemberProfileId, this.Guid, this.Email, this.CurrentCommercialId, this.LastAnsweredSurveyQuestionId, this.NextQuestionId, this.CreatedAt.ToString("yyyy-MM-dd hh:mm:ss"), this.MemberMasterId);

            if (isBase64Token)
            {
                return Base64Url.Encode(Encoding.ASCII.GetBytes(RijndaelEncryption.Encrypt(token, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize)));
            }

            return RijndaelEncryption.Encrypt(token, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize);
        }

        public static UserToken FromTokenValue(string encryptedToken, bool isBase64Token = true)
        {
            if(string.IsNullOrEmpty(encryptedToken))
            {
                return null;
            }
            string passPhrase = "Pas5pr@se";        // can be any string
            string saltValue = "s@1tValue";        // can be any string
            string hashAlgorithm = "SHA1";             // can be "MD5"
            int passwordIterations = 2;                // can be any number
            string initVector = "@1B2c3D4e5F6g7H8"; // must be 16 bytes
            int keySize = 256;                // can be 192 or 128

            try
            {
                if (isBase64Token)
                {
                    encryptedToken = Encoding.Default.GetString(Base64Url.Decode(encryptedToken));
                }
                string[] decryptedToken = RijndaelEncryption.Decrypt(encryptedToken, passPhrase, saltValue, hashAlgorithm, passwordIterations, initVector, keySize).Split(new char[] { '|' });
                if (decryptedToken.Length < 7 || decryptedToken.Length > 8)
                    return null;


                //  todo: fix token created date
                var date = DateTime.Now;
                try
                {
                    date = DateTime.Parse(decryptedToken[6]);
                }
                catch (Exception e)
                {

                }

                if (decryptedToken.Length == 7)
                {
                    return new UserToken()
                    {
                        MemberProfileId = long.Parse(decryptedToken[0]),
                        Guid = decryptedToken[1],
                        Email = decryptedToken[2],
                        CurrentCommercialId = int.Parse(decryptedToken[3]),
                        LastAnsweredSurveyQuestionId = int.Parse(decryptedToken[4]),
                        NextQuestionId = int.Parse(decryptedToken[5]),
                        CreatedAt = date,
                        MemberMasterId = 0,
                    };
                }
                else
                {
                    
                    return new UserToken()
                    {
                        MemberProfileId = long.Parse(decryptedToken[0]),
                        Guid = decryptedToken[1],
                        Email = decryptedToken[2],
                        CurrentCommercialId = int.Parse(decryptedToken[3]),
                        LastAnsweredSurveyQuestionId = int.Parse(decryptedToken[4]),
                        NextQuestionId = int.Parse(decryptedToken[5]),
                        CreatedAt = date,
                        MemberMasterId = int.Parse(decryptedToken[7]),
                    };
                }


            }
            catch (Exception ex)
            {
                //  @todo: log
                return null;
            }

        }
    }


}
