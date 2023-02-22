using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Voupon.API.Util
{
    /// <summary>
    ///     Wrapper class for encapsulating claims parsing.
    /// </summary>
    public class Authentication
    {

        public static class Claim
        {
            public static readonly string MASTER_MEMBER_PROFILE_ID = "master_member_profile_id";
            public static readonly string MEMBER_PROFILE_ID = "member_profile_id";
            public static readonly string EMAIL = "email";
            public static readonly string FIRST_NAME = "first_name";
            public static readonly string LAST_NAME = "last_name";
            public static readonly string LOGGED_IN_AT = "logged_in_at";
            public static readonly string EXP = "exp";
            public static readonly string USER_ID = "user_id";
        }

        public bool IsValid { get; }
        public int MasterMemberProfileId { get; }
        public string Email { get; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime LoggedInAt { get; set; }
       

        public Authentication(HttpRequest request)
        {
            // Check if we have a header.
            if (!request.Headers.ContainsKey("Authorization"))
            {
                IsValid = false;
                return;
            }

            string authorizationHeader = request.Headers["Authorization"];

            // Check if the value is empty.
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                IsValid = false;
                return;
            }

            // Check if we can decode the header.
            IDictionary<string, object> claims = null;

            try
            {
                if (authorizationHeader.StartsWith("Bearer"))
                {
                    authorizationHeader = authorizationHeader.Substring(7);
                }

                // Validate the token and decode the claims.

                claims = new JwtBuilder()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(Environment.GetEnvironmentVariable(EnvironmentKey.JWT_SECRET))
                    .MustVerifySignature()
                    .Decode<IDictionary<string, object>>(authorizationHeader);
            }
            catch (Exception exception)
            {
                IsValid = false;
                return;
            }

            // Check if we have user claim.
            if (!claims.ContainsKey(Claim.MASTER_MEMBER_PROFILE_ID))
            {
                IsValid = false;
                return;
            }

            IsValid = true;
            MasterMemberProfileId = Convert.ToInt32(claims[Claim.MASTER_MEMBER_PROFILE_ID]);
            Email = Convert.ToString(claims[Claim.EMAIL]);
            FirstName = Convert.ToString(claims[Claim.FIRST_NAME]);
            LastName = Convert.ToString(claims[Claim.LAST_NAME]);
            LoggedInAt = Convert.ToDateTime(claims[Claim.LOGGED_IN_AT]);

        }
    }
}
