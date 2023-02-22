using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;
using Voupon.Rewards.WebApp.ViewModels.ThirdParty.IPay88;
using static Voupon.Rewards.WebApp.Infrastructures.Helpers.Crypto;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Voupon.Rewards.WebApp.ViewModels.ThirdParty.RevenueMonster;
using System.Dynamic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Voupon.Rewards.WebApp.Infrastructures.Helpers;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Voupon.Rewards.WebApp.Services.Logger;

namespace Voupon.Rewards.WebApp.Services.Checkout.Pages
{
    public class PaymentPage : IRequest<ApiResponseViewModel>
    {
        public Guid Id { get; set; }
        public int MasterMemberProfileId { get; set; }
        public bool IsTestPassPaymentGateway { get; set; }
        private class PaymentPageHandler : IRequestHandler<PaymentPage, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;
            public PaymentPageHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(PaymentPage request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                var order = await rewardsDBContext.Orders.Include(x => x.OrderItems).Include(x => x.OrderShopExternal).ThenInclude(x => x.OrderItemExternal).Where(x => x.Id == request.Id && x.MasterMemberProfileId == request.MasterMemberProfileId).FirstOrDefaultAsync();

                var viewModel = new OrderViewModel();
                if (order == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid request";
                    return apiResponseViewModel;
                }

                viewModel.Id = order.Id;
                viewModel.OrderStatus = order.OrderStatus;
                viewModel.TotalItems = order.TotalItems;
                viewModel.TotalPoints = order.TotalPoints;
                viewModel.TotalPrice = order.TotalPrice + order.TotalShippingCost;

                if (order.OrderStatus == 2)
                {
                    foreach (var item in order.OrderItems)
                    {
                        var product = await rewardsDBContext.Products.FirstOrDefaultAsync(x => x.Id == item.ProductId);
                        product.AvailableQuantity--;
                        product.TotalBought++;
                        await rewardsDBContext.SaveChangesAsync();
                    }
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = viewModel;
                    return apiResponseViewModel;
                }
                if (!request.IsTestPassPaymentGateway)
                {
                    return await GenerateRevenueMonsterPaymentUrl(order);
                }

                //order.TotalPrice = (decimal)1.00;

                var inputValue = string.Format("{0}{1}{2}{3}{4}",
                    appSettings.Value.PaymentGateways.Ipay88.MerchantKey,
                    appSettings.Value.PaymentGateways.Ipay88.MerchantCode,
                    order.Id.ToString(),
                     viewModel.TotalPrice.ToString("F").ToString().Replace(".", "").Replace(",", ""),
                    "MYR"
                    );

                /* string inputValue = appSettings.Value.PaymentGateways.Ipay88.MerchantKey
                    + appSettings.Value.PaymentGateways.Ipay88.MerchantCode
                    + order.Id.ToString()
                    + order.TotalPrice.ToString("F").ToString().Replace(".", "").Replace(",", "")
                    + "MYR";
                    */

                var productDesc = string.Join(",", order.OrderItems.GroupBy(x => x.ProductTitle).Select(x => x.Key));

                if (string.IsNullOrEmpty(productDesc))
                {
                    productDesc = string.Join(",", order.OrderShopExternal.SelectMany(x => x.OrderItemExternal).GroupBy(x => x.ProductTitle).Select(x => x.Key));
                }

                string signatureValue = SHA_256.GenerateSHA256String(inputValue);
                /*
                viewModel.Ipay88RequestViewModel = new IPay88RequestViewModel
                {
                    MerchantCode = appSettings.Value.PaymentGateways.Ipay88.MerchantCode,
                    RefNo = order.Id.ToString(),
                    Amount = "1.00", 
                    ProdDesc = productDesc,
                    UserName = "kok hong",
                    UserEmail = "kok.hong@vodus.my",
                    UserContact = "0164365644",
                    Remark = "",
                    Lang = "UTF-8",
                    SignatureType = "SHA256",
                    Signature = signatureValue,
                    Currency = "MYR",
                    PaymentResponseUrl = appSettings.Value.App.BaseUrl + "/checkout/response",
                    PaymentBackendUrl = appSettings.Value.App.BaseUrl + "/api/v1/thirdparty/ipay88/callback"
                };
                */
                viewModel.Ipay88RequestViewModel = new IPay88RequestViewModel
                {
                    MerchantCode = appSettings.Value.PaymentGateways.Ipay88.MerchantCode,
                    RefNo = order.Id.ToString(),
                    Amount = viewModel.TotalPrice.ToString("F"),
                    ProdDesc = productDesc,
                    UserName = order.BillingPersonFirstName + " " + order.BillingPersonLastName,
                    UserEmail = order.Email,
                    UserContact = order.BillingMobileCountryCode + order.BillingMobileNumber,
                    Remark = "",
                    Lang = "UTF-8",
                    SignatureType = "SHA256",
                    Signature = signatureValue,
                    Currency = "MYR",
                    PaymentResponseUrl = appSettings.Value.App.BaseUrl + "/checkout/response",
                    PaymentBackendUrl = appSettings.Value.App.BaseUrl + "/api/v1/thirdparty/ipay88/callback"
                };

                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Data = viewModel;
                return apiResponseViewModel;
            }

            public static string RemoveSpecialCharacters(string str)
            {
                return Regex.Replace(str, "[^a-zA-Z0-9_. ]+", "", RegexOptions.Compiled);
            }

            private async Task<ApiResponseViewModel> GenerateRevenueMonsterPaymentUrl(Orders order)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                //  Check if token exists
                var grantType = new RevenueMonsterGrantType
                {
                    GrantType = "client_credentials"
                };

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appSettings.Value.PaymentGateways.RevenueMonster.ClientId}:{appSettings.Value.PaymentGateways.RevenueMonster.ClientSecret}"))}");
                var tokenData = new StringContent(JsonConvert.SerializeObject(grantType), Encoding.UTF8, "application/json");
                var tokenResponse = await httpClient.PostAsync($"{appSettings.Value.PaymentGateways.RevenueMonster.AuthUrl}/v1/token", tokenData);
                string tokenResult = tokenResponse.Content.ReadAsStringAsync().Result;

                dynamic dynamicTokenResult = JsonConvert.DeserializeObject<ExpandoObject>(tokenResult);
                var accessToken = dynamicTokenResult.accessToken;

                var data = new RevenueMonsterRequestViewModel()
                {
                    Order = new ViewModels.ThirdParty.RevenueMonster.Order
                    {
                        Title = "Payment to Vodus",
                        // Temp fix remove products titles from the details 
                        Detail = ((order.OrderItems != null ? order.Email /*+ " - " + string.Join(" , ", order.OrderItems.Select(x => RemoveSpecialCharacters(x.ProductTitle))) : "") + (order.OrderShopExternal != null ? string.Join(" , ", order.OrderShopExternal.SelectMany(x => x.OrderItemExternal).Select(x => RemoveSpecialCharacters(x.ProductTitle)))*/ : order.Email)).Replace(" ", "_"),
                        Amount = (int)((order.TotalPrice + order.TotalShippingCost) * 100),
                        CurrencyType = "MYR",
                        Id = order.ShortId,
                        AdditionalData = ""
                    },
                    Customer = new Customer
                    {
                        CountryCode = order.BillingMobileCountryCode,
                        Email = order.Email,
                        PhoneNumber = order.BillingMobileNumber,
                        UserId = order.MasterMemberProfileId.ToString()
                    },
                    LayoutVersion = "v3",
                    Method = new string[] { },
                    Type = "WEB_PAYMENT",
                    NotifyUrl = $"{appSettings.Value.App.BaseUrl}/api/v1/thirdparty/revenuemonster/callback",
                    RedirectUrl = $"{appSettings.Value.App.BaseUrl}/checkout/response",
                    StoreId = "1630046014310667018"
                };

                //var len = data.Order.Detail.ToString().Length;
                var privateKey = appSettings.Value.PaymentGateways.RevenueMonster.PrivateKey;

                string compactJson = SignatureUtil.GenerateCompactJson(data);
                string method = "post";
                string nonceStr = RandomString.GenerateRandomString(32);
                var requestURL = $"{appSettings.Value.PaymentGateways.RevenueMonster.ApiUrl}/v3/payment/online";
                string signType = "sha256";
                string timestamp = Convert.ToString((Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                RevenueMonsterSignature signature = new RevenueMonsterSignature();
                string signatureResult = "";
                signatureResult = signature.GenerateSignature(compactJson, method, nonceStr, privateKey, requestURL, signType, timestamp);
                signatureResult = "sha256 " + signatureResult;

                var viewModel = new OrderViewModel();
                try
                {
                    var content = JsonConvert.SerializeObject(data);
                    var buffer = System.Text.Encoding.UTF8.GetBytes(compactJson);
                    var byteContent = new ByteArrayContent(buffer);
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    client.DefaultRequestHeaders.Add("X-Nonce-Str", nonceStr);
                    client.DefaultRequestHeaders.Add("X-Signature", signatureResult);
                    client.DefaultRequestHeaders.Add("X-Timestamp", timestamp);

                    var stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(requestURL, byteContent);
                    var responseStr = await response.Content.ReadAsStringAsync();

                    var jsonAsString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(jsonAsString);
                    dynamic paymentUrlGenerateResult = JsonConvert.DeserializeObject<ExpandoObject>(jsonAsString);


                    if (((IDictionary<string, object>)paymentUrlGenerateResult).ContainsKey("error"))
                    {
                        viewModel.ErrorMessage = paymentUrlGenerateResult.error.message;
                        Console.WriteLine(paymentUrlGenerateResult.error.message);
                        var debug = paymentUrlGenerateResult.error.debug;

                        await new Logs
                        {
                            Description = "Revenue Monster: " + paymentUrlGenerateResult.error.message,
                            Email = order.Email,
                            JsonData = debug,
                            ActionName = "PaymentPage",
                            TypeId = CreateErrorLogCommand.Type.Service,
                            SendgridAPIKey = appSettings.Value.Mailer.Sendgrid.APIKey,
                            RewardsDBContext = rewardsDBContext
                        }.Error();

                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Data = viewModel;
                        return apiResponseViewModel;
                    }

                    var paymentUrlGenerateResultStatus = paymentUrlGenerateResult.code;

                    if (string.IsNullOrEmpty(paymentUrlGenerateResultStatus))
                    {
                        apiResponseViewModel.Successful = false;
                    }

                    if (paymentUrlGenerateResultStatus == "SUCCESS")
                    {
                        viewModel.PaymentUrl = paymentUrlGenerateResult.item.url;

                        apiResponseViewModel.Successful = true;
                        apiResponseViewModel.Data = viewModel;
                    }
                    else
                    {
                        viewModel.ErrorMessage = "Hmm.. we are busy at the moment. Please try again later";
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Data = viewModel;
                    }

                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = ex.ToString() + " " + viewModel.ErrorMessage;
                    Console.WriteLine("Error", ex.Message);
                }
                return apiResponseViewModel;
            }

            private class RevenueMonsterGrantType
            {
                public string GrantType { get; set; }
            }


            public static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
            {
                byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

                // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
                MemoryStream mem = new MemoryStream(privkey);
                BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
                byte bt = 0;
                ushort twobytes = 0;
                int elems = 0;
                try
                {
                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();        //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();       //advance 2 bytes
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes != 0x0102) //version number
                        return null;
                    bt = binr.ReadByte();
                    if (bt != 0x00)
                        return null;


                    //------  all private key components are Integer sequences ----
                    elems = GetIntegerSize(binr);
                    MODULUS = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    E = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    D = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    P = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    Q = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    DP = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    DQ = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    IQ = binr.ReadBytes(elems);

                    // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                    RSAParameters RSAparams = new RSAParameters();
                    RSAparams.Modulus = MODULUS;
                    RSAparams.Exponent = E;
                    RSAparams.D = D;
                    RSAparams.P = P;
                    RSAparams.Q = Q;
                    RSAparams.DP = DP;
                    RSAparams.DQ = DQ;
                    RSAparams.InverseQ = IQ;
                    RSA.ImportParameters(RSAparams);
                    return RSA;
                }
                catch (Exception)
                {
                    return null;
                }
                finally
                {
                    binr.Close();
                }
            }

            public static byte[] DecodeOpenSSLPrivateKey(String instr)
            {
                const String pemprivheader = "-----BEGIN RSA PRIVATE KEY-----";
                const String pemprivfooter = "-----END RSA PRIVATE KEY-----";
                String pemstr = instr.Trim();
                byte[] binkey;
                if (!pemstr.StartsWith(pemprivheader) || !pemstr.EndsWith(pemprivfooter))
                    return null;

                StringBuilder sb = new StringBuilder(pemstr);
                sb.Replace(pemprivheader, "");  //remove headers/footers, if present
                sb.Replace(pemprivfooter, "");

                String pvkstr = sb.ToString().Trim();   //get string after removing leading/trailing whitespace

                try
                {        // if there are no PEM encryption info lines, this is an UNencrypted PEM private key
                    binkey = Convert.FromBase64String(pvkstr.Replace("\n", ""));
                    return binkey;
                }
                catch (System.FormatException)
                {       //if can't b64 decode, it must be an encrypted private key
                        //Console.WriteLine("Not an unencrypted OpenSSL PEM private key");  
                }

                StringReader str = new StringReader(pvkstr);

                //-------- read PEM encryption info. lines and extract salt -----
                if (!str.ReadLine().StartsWith("Proc-Type: 4,ENCRYPTED"))
                    return null;
                String saltline = str.ReadLine();
                if (!saltline.StartsWith("DEK-Info: DES-EDE3-CBC,"))
                    return null;
                String saltstr = saltline.Substring(saltline.IndexOf(",") + 1).Trim();
                byte[] salt = new byte[saltstr.Length / 2];
                for (int i = 0; i < salt.Length; i++)
                    salt[i] = Convert.ToByte(saltstr.Substring(i * 2, 2), 16);
                if (!(str.ReadLine() == ""))
                    return null;

                //------ remaining b64 data is encrypted RSA key ----
                String encryptedstr = str.ReadToEnd();

                try
                {   //should have b64 encrypted RSA key now
                    binkey = Convert.FromBase64String(encryptedstr);
                }
                catch (System.FormatException)
                {  // bad b64 data.
                    return null;
                }

                //------ Get the 3DES 24 byte key using PDK used by OpenSSL ----

                SecureString despswd = GetSecPswd("Enter password to derive 3DES key==>");
                //Console.Write("\nEnter password to derive 3DES key: ");
                //String pswd = Console.ReadLine();
                byte[] deskey = GetOpenSSL3deskey(salt, despswd, 1, 2);    // count=1 (for OpenSSL implementation); 2 iterations to get at least 24 bytes
                if (deskey == null)
                    return null;
                //showBytes("3DES key", deskey) ;

                //------ Decrypt the encrypted 3des-encrypted RSA private key ------
                byte[] rsakey = DecryptKey(binkey, deskey, salt);   //OpenSSL uses salt value in PEM header also as 3DES IV
                if (rsakey != null)
                    return rsakey;  //we have a decrypted RSA private key
                else
                {
                    Console.WriteLine("Failed to decrypt RSA private key; probably wrong password.");
                    return null;
                }
            }

            private static byte[] GetOpenSSL3deskey(byte[] salt, SecureString secpswd, int count, int miter)
            {
                IntPtr unmanagedPswd = IntPtr.Zero;
                int HASHLENGTH = 16;    //MD5 bytes
                byte[] keymaterial = new byte[HASHLENGTH * miter];     //to store contatenated Mi hashed results


                byte[] psbytes = new byte[secpswd.Length];
                unmanagedPswd = Marshal.SecureStringToGlobalAllocAnsi(secpswd);
                Marshal.Copy(unmanagedPswd, psbytes, 0, psbytes.Length);
                Marshal.ZeroFreeGlobalAllocAnsi(unmanagedPswd);

                //UTF8Encoding utf8 = new UTF8Encoding();
                //byte[] psbytes = utf8.GetBytes(pswd);

                // --- contatenate salt and pswd bytes into fixed data array ---
                byte[] data00 = new byte[psbytes.Length + salt.Length];
                Array.Copy(psbytes, data00, psbytes.Length);        //copy the pswd bytes
                Array.Copy(salt, 0, data00, psbytes.Length, salt.Length);   //concatenate the salt bytes

                // ---- do multi-hashing and contatenate results  D1, D2 ...  into keymaterial bytes ----
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] result = null;
                byte[] hashtarget = new byte[HASHLENGTH + data00.Length];   //fixed length initial hashtarget

                for (int j = 0; j < miter; j++)
                {
                    // ----  Now hash consecutively for count times ------
                    if (j == 0)
                        result = data00;    //initialize 
                    else
                    {
                        Array.Copy(result, hashtarget, result.Length);
                        Array.Copy(data00, 0, hashtarget, result.Length, data00.Length);
                        result = hashtarget;
                        //Console.WriteLine("Updated new initial hash target:") ;
                        //showBytes(result) ;
                    }

                    for (int i = 0; i < count; i++)
                        result = md5.ComputeHash(result);
                    Array.Copy(result, 0, keymaterial, j * HASHLENGTH, result.Length);  //contatenate to keymaterial
                }
                //showBytes("Final key material", keymaterial);
                byte[] deskey = new byte[24];
                Array.Copy(keymaterial, deskey, deskey.Length);

                Array.Clear(psbytes, 0, psbytes.Length);
                Array.Clear(data00, 0, data00.Length);
                Array.Clear(result, 0, result.Length);
                Array.Clear(hashtarget, 0, hashtarget.Length);
                Array.Clear(keymaterial, 0, keymaterial.Length);

                return deskey;
            }
            public static byte[] DecryptKey(byte[] cipherData, byte[] desKey, byte[] IV)
            {
                MemoryStream memst = new MemoryStream();
                TripleDES alg = TripleDES.Create();
                alg.Key = desKey;
                alg.IV = IV;
                try
                {
                    CryptoStream cs = new CryptoStream(memst, alg.CreateDecryptor(), CryptoStreamMode.Write);
                    cs.Write(cipherData, 0, cipherData.Length);
                    cs.Close();
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                    return null;
                }
                byte[] decryptedData = memst.ToArray();
                return decryptedData;
            }
            private static SecureString GetSecPswd(String prompt)
            {
                SecureString password = new SecureString();

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(prompt);
                Console.ForegroundColor = ConsoleColor.Magenta;

                while (true)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    if (cki.Key == ConsoleKey.Enter)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine();
                        return password;
                    }
                    else if (cki.Key == ConsoleKey.Backspace)
                    {
                        // remove the last asterisk from the screen...
                        if (password.Length > 0)
                        {
                            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                            Console.Write(" ");
                            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                            password.RemoveAt(password.Length - 1);
                        }
                    }
                    else if (cki.Key == ConsoleKey.Escape)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine();
                        return password;
                    }
                    else if (Char.IsLetterOrDigit(cki.KeyChar) || Char.IsSymbol(cki.KeyChar))
                    {
                        if (password.Length < 20)
                        {
                            password.AppendChar(cki.KeyChar);
                            Console.Write("*");
                        }
                        else
                        {
                            Console.Beep();
                        }
                    }
                    else
                    {
                        Console.Beep();
                    }
                }
            }
            private static int GetIntegerSize(BinaryReader binr)
            {
                byte bt = 0;
                byte lowbyte = 0x00;
                byte highbyte = 0x00;
                int count = 0;
                bt = binr.ReadByte();
                if (bt != 0x02)     //expect integer
                    return 0;
                bt = binr.ReadByte();

                if (bt == 0x81)
                    count = binr.ReadByte();    // data size in next byte
                else
                if (bt == 0x82)
                {
                    highbyte = binr.ReadByte(); // data size in next 2 bytes
                    lowbyte = binr.ReadByte();
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    count = BitConverter.ToInt32(modint, 0);
                }
                else
                {
                    count = bt;     // we already have the data size
                }



                while (binr.ReadByte() == 0x00)
                {   //remove high order zeros in data
                    count -= 1;
                }
                binr.BaseStream.Seek(-1, SeekOrigin.Current);       //last ReadByte wasn't a removed zero, so back up a byte
                return count;
            }


        }


        public class OrderViewModel
        {
            public Guid Id { get; set; }
            public int MasterMemberProfileId { get; set; }
            public string Email { get; set; }
            public decimal TotalPrice { get; set; }
            public int TotalPoints { get; set; }
            public int TotalItems { get; set; }
            public short OrderStatus { get; set; }
            public DateTime CreatedAt { get; set; }
            public string BillingPersonFirstName { get; set; }
            public string BillingPersonLastName { get; set; }
            public string BillingEmail { get; set; }
            public string BillingMobileNumber { get; set; }
            public string BillingMobileCountryCode { get; set; }
            public string BillingAddressLine1 { get; set; }
            public string BillingAddressLine2 { get; set; }
            public string BillingPostcode { get; set; }
            public string BillingCity { get; set; }
            public string BillingState { get; set; }
            public string BillingCountry { get; set; }
            public string ShippingPersonFirstName { get; set; }
            public string ShippingPersonLastName { get; set; }
            public string ShippingEmail { get; set; }
            public string ShippingMobileNumber { get; set; }
            public string ShippingMobileCountryCode { get; set; }
            public string ShippingAddressLine1 { get; set; }
            public string ShippingAddressLine2 { get; set; }
            public string ShippingPostcode { get; set; }
            public string ShippingCity { get; set; }
            public string ShippingState { get; set; }
            public string ShippingCountry { get; set; }

            public string PaymentUrl { get; set; }
            public string ErrorMessage { get; set; }

            public IPay88RequestViewModel Ipay88RequestViewModel { get; set; }
        }

    }

}
