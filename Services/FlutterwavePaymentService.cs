using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using KogiExportHub.Models;

namespace KogiExportHub.Services
{
    public class FlutterwaveSettings
    {
        public string PublicKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string EncryptionKey { get; set; } = string.Empty;
    }

    public class FlutterwavePaymentService : IPaymentService
    {
        private readonly FlutterwaveSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://api.flutterwave.com/v3";
        private readonly string _callbackUrl; // Add this field

        public FlutterwavePaymentService(IOptions<FlutterwaveSettings> settings, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.SecretKey}");
            
            // Get the base URL dynamically
            var request = httpContextAccessor.HttpContext?.Request;
            _callbackUrl = $"{request?.Scheme}://{request?.Host}/Payment/Callback";
        }

        // Add the missing InitializePaymentAsync method
        public async Task<PaymentResponse> InitializePaymentAsync(decimal amount, string currency = "NGN", string customerEmail = "", string customerName = "")
        {
            try
            {
                var payload = new
                {
                    tx_ref = Guid.NewGuid().ToString(),
                    amount = amount,
                    currency = currency,
                    redirect_url = _callbackUrl, // Use dynamic URL instead of hardcoded one
                    customer = new
                    {
                        email = customerEmail,
                        name = customerName
                    },
                    customizations = new
                    {
                        title = "KogiExportHub Payment",
                        description = "Payment for products",
                        logo = $"{_callbackUrl.Substring(0, _callbackUrl.IndexOf("/Payment"))}/images/logo.png" // Fix logo URL too
                    }
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/payments", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                if (result?.status == "success")
                {
                    return new PaymentResponse
                    {
                        Status = "success",
                        Message = "Payment initialized successfully",
                        PaymentLink = result.data.link,
                        TransactionId = payload.tx_ref
                    };
                }

                return new PaymentResponse
                {
                    Status = "error",
                    Message = result?.message ?? "Payment initialization failed"
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponse
                {
                    Status = "error",
                    Message = ex.Message
                };
            }
        }

        // Create subaccount for sellers
        public async Task<SubaccountResponse> CreateSubaccountAsync(string email, string accountNumber, string bankCode, string businessName)
        {
            try
            {
                var payload = new
                {
                    account_bank = bankCode,
                    account_number = accountNumber,
                    business_name = businessName,
                    business_email = email,
                    business_contact = businessName,
                    business_contact_mobile = "",
                    business_mobile = "",
                    split_type = "percentage",
                    split_value = 0.95 // Seller gets 95%, platform keeps 5%
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/subaccounts", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                if (result?.status == "success")
                {
                    return new SubaccountResponse
                    {
                        Status = "success",
                        SubaccountId = result.data.subaccount_id,
                        Message = "Subaccount created successfully"
                    };
                }

                return new SubaccountResponse
                {
                    Status = "error",
                    Message = result?.message ?? "Subaccount creation failed"
                };
            }
            catch (Exception ex)
            {
                return new SubaccountResponse
                {
                    Status = "error",
                    Message = ex.Message
                };
            }
        }

        // Initialize payment with subaccount split
        public async Task<PaymentResponse> InitializePaymentWithSplitAsync(decimal amount, string customerEmail, string customerName, List<CartItem> items)
        {
            try
            {
                // Remove VAT and platform fee calculations as requested
                var subtotal = amount;
                // var vat = subtotal * 0.075m; // 7.5% - REMOVED
                // var platformFee = subtotal * 0.025m; // 2.5% - REMOVED
                var totalAmount = subtotal; // Use subtotal directly without additional fees

                // Group items by seller to create subaccounts array
                var subaccounts = items.GroupBy(item => item.SellerId)
                    .Where(group => !string.IsNullOrEmpty(group.First().SellerSubaccountId))
                    .Select(group => new
                    {
                        id = group.First().SellerSubaccountId,
                        transaction_split_ratio = Math.Round((group.Sum(i => i.TotalPrice) / subtotal) * 95, 2) // Platform keeps 5%
                    }).ToList();
                
                // If no subaccounts available, fall back to regular payment
                if (!subaccounts.Any())
                {
                    return await InitializePaymentAsync(totalAmount, "NGN", customerEmail, customerName);
                }
                // In the InitializePaymentWithSplitAsync method, find this section:
                var payload = new
                {
                    tx_ref = Guid.NewGuid().ToString(),
                    amount = totalAmount,
                    currency = "NGN",
                    redirect_url = _callbackUrl, // Replace hardcoded URL with dynamic one
                    customer = new
                    {
                        email = customerEmail,
                        name = customerName
                    },
                    subaccounts = subaccounts,
                    customizations = new
                    {
                        title = "KogiExportHub Payment",
                        description = "Payment for products",
                        logo = $"{_callbackUrl.Substring(0, _callbackUrl.IndexOf("/Payment"))}/images/logo.png" // Fix logo URL too
                    }
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/payments", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                if (result?.status == "success")
                {
                    return new PaymentResponse
                    {
                        Status = "success",
                        Message = "Payment initialized successfully",
                        PaymentLink = result.data.link,
                        TransactionId = payload.tx_ref
                    };
                }

                return new PaymentResponse
                {
                    Status = "error",
                    Message = result?.message ?? "Payment initialization failed"
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponse
                {
                    Status = "error",
                    Message = ex.Message
                };
            }
        }

        public async Task<PaymentVerificationResponse> VerifyPaymentAsync(string transactionId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/transactions/{transactionId}/verify");
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                if (result?.status == "success" && result?.data?.status == "successful")
                {
                    return new PaymentVerificationResponse
                    {
                        Status = "success",
                        Message = "Payment verified successfully",
                        Amount = result.data.amount,
                        Currency = result.data.currency,
                        IsSuccessful = true
                    };
                }

                return new PaymentVerificationResponse
                {
                    Status = "failed",
                    Message = "Payment verification failed",
                    IsSuccessful = false
                };
            }
            catch (Exception ex)
            {
                return new PaymentVerificationResponse
                {
                    Status = "error",
                    Message = ex.Message,
                    IsSuccessful = false
                };
            }
        }

        public async Task<TransferResponse> InitiateTransferAsync(decimal amount, string recipientAccount, string bankCode, string recipientName, string narration = "")
        {
            try
            {
                var payload = new
                {
                    account_bank = bankCode,
                    account_number = recipientAccount,
                    amount = amount,
                    narration = narration,
                    currency = "NGN",
                    reference = Guid.NewGuid().ToString(),
                    callback_url = $"{_callbackUrl.Replace("/Payment/Callback", "/Payment/TransferCallback")}",
                    debit_currency = "NGN"
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/transfers", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                if (result?.status == "success")
                {
                    return new TransferResponse
                    {
                        Status = "success",
                        Message = "Transfer initiated successfully",
                        TransferId = result.data.id.ToString(),
                        Amount = amount,
                        Reference = result.data.reference
                    };
                }

                return new TransferResponse
                {
                    Status = "error",
                    Message = result?.message ?? "Transfer failed"
                };
            }
            catch (Exception ex)
            {
                return new TransferResponse
                {
                    Status = "error",
                    Message = ex.Message
                };
            }
        }

        public async Task<TransferResponse> VerifyTransferAsync(string transferId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/transfers/{transferId}");
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                if (result?.status == "success")
                {
                    return new TransferResponse
                    {
                        Status = result.data.status,
                        Message = "Transfer verification successful",
                        TransferId = transferId,
                        Amount = result.data.amount,
                        Reference = result.data.reference
                    };
                }

                return new TransferResponse
                {
                    Status = "error",
                    Message = result?.message ?? "Transfer verification failed"
                };
            }
            catch (Exception ex)
            {
                return new TransferResponse
                {
                    Status = "error",
                    Message = ex.Message
                };
            }
        }

        public async Task<BankListResponse> GetBanksAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/banks/NG");
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                if (result?.status == "success")
                {
                    var banks = new List<BankInfo>();
                    foreach (var bank in result.data)
                    {
                        banks.Add(new BankInfo
                        {
                            Code = bank.code,
                            Name = bank.name
                        });
                    }

                    return new BankListResponse
                    {
                        Status = "success",
                        Banks = banks
                    };
                }

                return new BankListResponse
                {
                    Status = "error",
                    Banks = new List<BankInfo>()
                };
            }
            catch (Exception ex)
            {
                return new BankListResponse
                {
                    Status = "error",
                    Banks = new List<BankInfo>()
                };
            }
        }

        public async Task<AccountValidationResponse> ValidateAccountAsync(string accountNumber, string bankCode)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_baseUrl}/accounts/resolve", 
                    new StringContent(JsonConvert.SerializeObject(new { account_number = accountNumber, account_bank = bankCode }), 
                    Encoding.UTF8, "application/json"));
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                if (result?.status == "success")
                {
                    return new AccountValidationResponse
                    {
                        Status = "success",
                        AccountName = result.data.account_name,
                        AccountNumber = accountNumber
                    };
                }

                return new AccountValidationResponse { Status = "error" };
            }
            catch
            {
                return new AccountValidationResponse { Status = "error" };
            }
        }
    }
}