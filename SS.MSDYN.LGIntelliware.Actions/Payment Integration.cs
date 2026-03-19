using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Security.Principal; 
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace SS.MSDYN.LGIntelliware.Actions
{
    public class PaymentIntegration : IPlugin
    {

        // Prepare client for api
        private static readonly HttpClient client = new HttpClient();

        // Prepare endpoints and urls
        private static string tokenURL = "";
        private static string baseUrl = "";
        private static string paymentLinkEndpoint = "";
        private static string paymentStatusEndpoint = "";
        private static string GetProductEndpoint = "";
        private static string ClientId = "";
        private static string ClientSecret = "";
        private static string Environment = "";


        // Main represents where the plugin would call the main code
        // Expects 2 values
        // productInfo - a json object with information about what is being purchased - being represented by exampleInput
        // caseInfo - a json object storing the caseID and logged in user info - represented by caseInfo
        public void Execute(IServiceProvider serviceProvider)
        {
            //Initializing Service Context.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Prevent infinite loops by limiting depth
            if (context.Depth > 1)
            {
                return;
            }
            try
            {
                tracingService.Trace("Plugin Called");
                var inputString = context.InputParameters["inputString"].ToString();
                var caseInfo = context.InputParameters["caseInfo"].ToString();
                var api = context.InputParameters["api"].ToString();
                // This string represents the input from the javascript calling this function. These values will be set in the power page
                // In the real implementation this should be one of the inputs in the code and not hardcoded
                tracingService.Trace("Parameters passed!");
                OutputJson output = new OutputJson();
                output.products = new List<Product>();
                string outputString;

                GetServiceConfigurationRecord(service);

                // Convert the case information into json object
                CaseInfo caseJson = JsonConvert.DeserializeObject<CaseInfo>(caseInfo);
                tracingService.Trace("Case Json: " + caseJson.CaseID);
                // Convert payment info into json object 
                PaymentRequest paymentJson = JsonConvert.DeserializeObject<PaymentRequest>(inputString);
                paymentJson.environment = Environment;
                inputString = JsonConvert.SerializeObject(paymentJson);

                tracingService.Trace("Payment Json: " + paymentJson.dynamicsContactId);
                // Generate microsoft authentication
                string authResult = MakeAuthenticationRequest(tracingService).GetAwaiter().GetResult();

                // Convert into correct Json to process response
                AuthResponse authJson = JsonConvert.DeserializeObject<AuthResponse>(authResult);
                tracingService.Trace("auth json Json: " + authJson.token_type);
                foreach (var product in paymentJson.productList)
                {
                    tracingService.Trace("Entered foreach");
                    string productCost = GetProduct(product.code, authJson.access_token, tracingService).GetAwaiter().GetResult();
                    tracingService.Trace("product gets");
                    SingleProductResponse productJson = JsonConvert.DeserializeObject<SingleProductResponse>(productCost);
                    tracingService.Trace("sss" + productJson);
                    productJson.product.amount = productJson.product.amount * product.quantity;
                    productJson.product.quantity = product.quantity;

                    output.products.Add(productJson.product);
                }
                tracingService.Trace("out of foreach!");
                if (api == "get-payment")
                {
                    tracingService.Trace("Entered if");
                    // Check Transactions table
                    Entity transaction = CheckTransactionsTable(caseJson.CaseID, service);
                    tracingService.Trace("transacions checked!");
                    // Get payment status of transaction if one is found
                    if (transaction != null)
                    {
                        string result = CheckPaymentStatus(transaction[PaymentTransaction.TransactionId].ToString(), caseJson.ContactID, paymentJson.environment, authJson.access_token,tracingService).GetAwaiter().GetResult();

                        PaymentResponse response = JsonConvert.DeserializeObject<PaymentResponse>(result);
                        tracingService.Trace("Payment STATUS checked" + response.status);
                        if (response.status == "failed" || response.status == "cancelled" || response.status == "error")
                        {
                            output.status = "failed";
                            output.errorMessage = response.messages;

                            // for regenrating payment link
                            string newPaymentLink = GetPaymentLink(authJson.access_token, inputString,tracingService).GetAwaiter().GetResult();
                            // convert response to Json object
                            PaymentResponse newPayment = JsonConvert.DeserializeObject<PaymentResponse>(newPaymentLink);
                            // Set output values
                            output.paymentLink = newPayment.paymentLinkUrl;
                            //update the transaction id in the back office record
                            UpdateTransactionsRecord(newPayment.paymentGuidId, transaction[PaymentTransaction.PaymentTransactionId].ToString(), newPayment.paymentLinkUrl, StatusCode.Failed.GetHashCode(), service);
                            outputString = JsonConvert.SerializeObject(output);
                            context.OutputParameters["outputString"] = outputString;
                            return;
                        }
                        else if (response.status == "success")
                        {
                            output.status = response.status;
                            var transactionEntity = UpdateTransactionsRecord(" ", transaction[PaymentTransaction.PaymentTransactionId].ToString(), " ", StatusCode.Success.GetHashCode(), service);
                            //Update service record
                            UpdateServicePaidColumn(transactionEntity, service);
                            outputString = JsonConvert.SerializeObject(output);
                            context.OutputParameters["outputString"] = outputString;
                            return;
                        }
                        else
                        {
                            output.status = "pending";
                            output.paymentLink = transaction[PaymentTransaction.PaymentLink].ToString();
                            outputString = JsonConvert.SerializeObject(output);
                            context.OutputParameters["outputString"] = outputString;
                            return;
                        }
                    }
                    // Call payment result and pass authentication token
                    string paymentResult = GetPaymentLink(authJson.access_token, inputString,tracingService).GetAwaiter().GetResult();
                    tracingService.Trace("Payment Link created!");
                    // convert response to Json object
                    PaymentResponse payment = JsonConvert.DeserializeObject<PaymentResponse>(paymentResult);
                    tracingService.Trace("Payment Link deseralized!");
                    // Set output values
                    output.paymentLink = payment.paymentLinkUrl;
                    if (output.status == null)
                        output.status = "Payment Required";
                    //CreateTransactionsRecord(payment.amount*paymentJson.productList.First().quantity, caseJson.CaseID, paymentJson.identityObjectId, paymentJson.identityObjectEntity, payment.paymentGuidId, payment.paymentLinkUrl, service);
                    CreateTransactionsRecord(payment.amount, caseJson.CaseID, paymentJson.identityObjectId, paymentJson.identityObjectEntity, payment.paymentGuidId, payment.paymentLinkUrl, service);

                }

                outputString = JsonConvert.SerializeObject(output);
                context.OutputParameters["outputString"] = outputString;
                tracingService.Trace("Final!");
                return;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("An exception occured executing Payment Integration: " + ex + ".");
            }

        }

        static async Task<string> GetProduct(string product, string auth, ITracingService tracingService)
        {
            // Create URL
            string url = $"{baseUrl}{GetProductEndpoint}?productCode={product}";
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + auth);
            tracingService.Trace("get product URL is: " + url);
            // Send the request
            HttpResponseMessage response = await client.GetAsync(url);
            tracingService.Trace("response is: " + response.StatusCode);
            string result = await response.Content.ReadAsStringAsync();
            tracingService.Trace("result is: " + result);
            // Error handling
            if (!response.IsSuccessStatusCode)
            {
                tracingService.Trace($"PAYMENT ERROR ({(int)response.StatusCode}): {result}");
                return null;
            }

            // request received  
            return result;
        }

        // Function to generate a payment link using the request body as the criteria
        static async Task<string> GetPaymentLink(string authorization, string requestBody,ITracingService tracingService)
        {
            if (string.IsNullOrEmpty(authorization))
                return "No valid access token provided.";


            // Convert JSON string into HttpContent with proper headers
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            // Add authentication to headers
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorization);

          tracingService.Trace("Sending POST request...");

            // Send the authentication request
            string url = baseUrl + paymentLinkEndpoint;
            HttpResponseMessage response = await client.PostAsync(url, content);
            string result = await response.Content.ReadAsStringAsync();

            // error handling
            if (!response.IsSuccessStatusCode)
            {
                tracingService.Trace($"PAYMENT ERROR ({(int)response.StatusCode}): {result}");
                return null;
            }

            // request received
            // Add transaction to transaction table
            return result;
        }

        // function to check the transactions table for the case and try to get the most recent entry
        static Entity CheckTransactionsTable(string caseID, IOrganizationService service)
        {
            // Add the code to check the transactions table for the recent entry here
            var query = new QueryExpression(PaymentTransaction.TableName)
            {
                ColumnSet = new ColumnSet(PaymentTransaction.PaymentTransactionId, PaymentTransaction.TransactionId, PaymentTransaction.PaymentLink)
            };

            query.Criteria.AddCondition(PaymentTransaction.ServiceRequestId, ConditionOperator.Equal, new Guid(caseID));

            var result = service.RetrieveMultiple(query);

            if (result.Entities.Count > 0)
            {
                Entity transaction = result.Entities[0];
                if (transaction.Attributes.Contains(PaymentTransaction.TransactionId))
                {
                    return transaction;
                }
            }
            return null;
        }
        //function to get the service configuration for API endpoints
        static void GetServiceConfigurationRecord(IOrganizationService service)
        {
            var query = new QueryExpression(ServiceConfiguration.TableName)
            {
                ColumnSet = new ColumnSet(ServiceConfiguration.BaseUrl, ServiceConfiguration.TokenUrl, ServiceConfiguration.ClientId, ServiceConfiguration.ClientSecret, ServiceConfiguration.CreatePaymentLink, ServiceConfiguration.CheckPaymentStatus, ServiceConfiguration.GetProductEndpoint, ServiceConfiguration.Environment)
            };

            query.Criteria.AddCondition(ServiceConfiguration.Name, ConditionOperator.Equal, ServiceConfiguration.RecordName);

            var result = service.RetrieveMultiple(query);

            if (result.Entities.Count > 0)
            {
                Entity servicConfig = result.Entities[0];
                tokenURL = servicConfig[ServiceConfiguration.TokenUrl].ToString();
                baseUrl = servicConfig[ServiceConfiguration.BaseUrl].ToString();
                paymentLinkEndpoint = servicConfig[ServiceConfiguration.CreatePaymentLink].ToString();
                paymentStatusEndpoint = servicConfig[ServiceConfiguration.CheckPaymentStatus].ToString();
                GetProductEndpoint = servicConfig[ServiceConfiguration.GetProductEndpoint].ToString();
                ClientId = servicConfig[ServiceConfiguration.ClientId].ToString();
                ClientSecret = servicConfig[ServiceConfiguration.ClientSecret].ToString();
                Environment = servicConfig[ServiceConfiguration.Environment].ToString();
            }
            return;
        }
        static void CreateTransactionsRecord(double amount, string caseId, string identityObjectId, string identityObjectEntity, string transactionId, string paymentLink, IOrganizationService service)
        {
            Entity record = new Entity(PaymentTransaction.TableName);
            record[PaymentTransaction.Amount] = new Money((decimal)amount); // Currency
           // record[PaymentTransaction.Case] = new EntityReference(Case.TableName, new Guid(caseId)); // Lookup
            record[PaymentTransaction.ServiceRequestId] = new EntityReference(identityObjectEntity, new Guid(identityObjectId)); // Lookup
            record[PaymentTransaction.TransactionId] = transactionId; // Text
            record[PaymentTransaction.PaymentLink] = paymentLink; // Text
            service.Create(record);
            return;
        }
        static Entity UpdateTransactionsRecord(string transactionId, string recordGuid, string paymentLink, int statusReason, IOrganizationService service)
        {
            Entity transaction = service.Retrieve(PaymentTransaction.TableName, new Guid(recordGuid), new ColumnSet(PaymentTransaction.TransactionId, PaymentTransaction.ServiceRequestId));
            if (transactionId != " ")
                transaction[PaymentTransaction.TransactionId] = transactionId.ToString();
            if (paymentLink != " ")
                transaction[PaymentTransaction.PaymentLink] = paymentLink.ToString();
            transaction[PaymentTransaction.StatusReason] = new OptionSetValue(statusReason);

            // Udate the record
            service.Update(transaction);
            return transaction;
        }

        static void UpdateServicePaidColumn(Entity transaction, IOrganizationService service)
        {
            if (!transaction.Contains(PaymentTransaction.ServiceRequestId))
                return;

            EntityReference serviceRef = transaction[PaymentTransaction.ServiceRequestId] as EntityReference;
            if (serviceRef == null)
                return;

            // Check logical name
            if (serviceRef.LogicalName ==TaxiLicence.TableName)
            {
                Entity serviceEntity = new Entity(serviceRef.LogicalName)
                {
                    Id = serviceRef.Id
                };

                // Update toggle (Paid = Yes)
                serviceEntity[PaymentTransaction.Paid] = true;

                service.Update(serviceEntity);
            }
        }



        static async Task<string> CheckPaymentStatus(string paymentGUID, string contactID, string environment, string auth,ITracingService tracingService)
        {
            // Create URL
            string url = $"{baseUrl}{paymentStatusEndpoint}?dynamicsContactID={contactID}&paymentGuidId={paymentGUID}&environment={environment}";
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + auth);

            // Send the request
            HttpResponseMessage response = await client.GetAsync(url);
            string result = await response.Content.ReadAsStringAsync();

            // Error handling
            if (!response.IsSuccessStatusCode)
            {
                tracingService.Trace($"PAYMENT ERROR ({(int)response.StatusCode}): {result}");
                return null;
            }

            // request received  
            return result;
        }

        // Function to get authorisation from Microsoft for the other requests
        static async Task<string> MakeAuthenticationRequest(ITracingService tracingService)
        {
            // Content to send as x-www-form-urlencoded to Microsoft authentication
            var form = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", "api://c446406d-22cc-47f0-a980-16f9301db111/.default"),
                    new KeyValuePair<string, string>("client_id", ClientId), // ID shouldn't be hardcoded in
                    new KeyValuePair<string, string>("client_secret", ClientSecret) // Secret shouldn't be hardcoded in
                });

           tracingService.Trace("Sending POST request...");

            // Send the authentication request
            HttpResponseMessage response = await client.PostAsync(tokenURL, form);
            string result = await response.Content.ReadAsStringAsync();

            // Error handling
            if (!response.IsSuccessStatusCode)
            {
                tracingService.Trace($"PAYMENT ERROR ({(int)response.StatusCode}): {result}");
                return null;
            }

            // request received  
            return result;

        }
    }
    // Used to easily get the access token from the json response
    public class AuthResponse
    {
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public int ext_expires_in { get; set; }
        public string access_token { get; set; }
    }

    public class PaymentRequest
    {
        public string dynamicsContactId { get; set; }
        public bool guest { get; set; }
        public string identityObjectId { get; set; }
        public string identityObjectEntity { get; set; }
        public string dynamicsCaseId { get; set; }
        public string returnUrl { get; set; }
        public bool includePaymentGuid { get; set; }
        public List<Product> productList { get; set; }
        public bool createAgreement { get; set; }
        public string uprn { get; set; }
        public string environment { get; set; }
    }

    public class Product
    {
        public string code { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public double amount { get; set; }
        public int quantity { get; set; }
    }

    public class CaseInfo
    {
        public string ContactID { get; set; }
        public string CaseID { get; set; }
    }

    public class PaymentResponse
    {
        public bool success { get; set; }
        public List<string> messages { get; set; }
        public string status { get; set; }
        public bool finished { get; set; }
        public string paymentGuidId { get; set; }
        public string description { get; set; }
        public string returnUrl { get; set; }
        public int amountMinor { get; set; }
        public double amount { get; set; }
        public string paymentLinkUrl { get; set; }
        public string createdBy { get; set; }
        public string created { get; set; }
        public string updated { get; set; }
        public List<ProductResponse> products { get; set; }
        public string environment { get; set; }
    }

    public class SingleProductResponse
    {
        public bool success { get; set; }
        public List<string> messages { get; set; }
        public Product product { get; set; }
    }


    public class ProductResponse
    {
        public string code { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public double amount { get; set; }
        public int quantity { get; set; }
    }

    public class OutputJson
    {
        public string status { get; set; } // Payment Required, Payment Failed, Successful Payment
        public string paymentLink { get; set; } // Only if link required
        public List<string> errorMessage { get; set; } // Only if error
        public List<Product> products { get; set; } // Contains the price of each product
    }
}

