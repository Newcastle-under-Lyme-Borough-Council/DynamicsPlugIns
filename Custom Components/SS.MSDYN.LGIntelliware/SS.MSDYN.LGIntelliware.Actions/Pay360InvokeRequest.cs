using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Xml;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using SS.MSDYN.LGIntelliware.Actions.Common;

namespace SS.MSDYN.LGIntelliware.Actions
{
    public class Pay360InvokeRequest : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //Initializing Service Context.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            OrganizationServiceContext orgContext = new OrganizationServiceContext(service);
            try
            {
                var PaymentTransactionID = context.InputParameters["RecordID"].ToString();
                var PortalURL = context.InputParameters["PortalURL"].ToString();
                var FundCode = context.InputParameters["FundCode"].ToString();
                var VATCode = context.InputParameters["VATCode"].ToString();
                var Price = context.InputParameters["Price"].ToString();
                var ServiceName = context.InputParameters["ServiceName"].ToString();
                var RequestID = context.InputParameters["RequestID"].ToString();

                string Pay360serviceurl = context.InputParameters["Pay360serviceurl"].ToString();//GetEnvironmentVariableDefaultValue("ss_Pay360HMACSecretKeyWebserviceEndpoint", service).ToString() + "/scpClient";//"https://sbsctest.e-paycapita.com/scp/scpws/scpClient";
                string Pay360SiteID = context.InputParameters["Pay360SiteID"].ToString();//GetEnvironmentVariableDefaultValue("ss_Pay360SiteID", service);
                string Pay360SCPID = context.InputParameters["Pay360SCPID"].ToString();//GetEnvironmentVariableDefaultValue("ss_Pay360SCPID", service);
                string Pay360HMACID = context.InputParameters["Pay360HMACID"].ToString();//GetEnvironmentVariableDefaultValue("ss_Pay360HMACID", service);
                string Pay360HMACSecretKey = context.InputParameters["Pay360HMACSecretKey"].ToString();//GetEnvironmentVariableDefaultValue("ss_Pay360HMACSecretKey", service);

                try
                {
                    string uniqueID = Guid.NewGuid().ToString().Substring(0, 5);
                    string timmm = GenerateTimestamp();

                    string cre = "CapitaPortal!" + Pay360SCPID + "!" + uniqueID + "!" + timmm + "!Original!" + Pay360HMACID;
                    var tttt = CalculateDigest(Pay360HMACSecretKey, cre);
                    XmlDocument soapEnvelopeXml = CreateSoapEnvelope(uniqueID, timmm, tttt, Pay360SCPID, Pay360SiteID, Pay360HMACID, PortalURL, PaymentTransactionID, FundCode, VATCode, Price, ServiceName, RequestID);
                    HttpWebRequest webRequest = CreateWebRequest(Pay360serviceurl, "");
                    InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                    // begin async call to web request.
                    IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                    // suspend this thread until call is complete. You might want to
                    // do something usefull here like update your UI.
                    asyncResult.AsyncWaitHandle.WaitOne();

                    // get the response from the completed web request.
                    string soapResult;
                    using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
                    {
                        using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                        {
                            soapResult = rd.ReadToEnd();
                        }


                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(soapResult);
                        string str = xmlDoc.ChildNodes.ToString();

                        var jsonText = JsonConvert.SerializeXmlNode(xmlDoc);
                        Root Data = JsonConvert.DeserializeObject<Root>(jsonText);

                        if (Data.SOAPENVEnvelope != null &&
                            Data.SOAPENVEnvelope.SOAPENVBody != null &&
                            Data.SOAPENVEnvelope.SOAPENVBody.scpSimpleInvokeResponse != null &&
                            Data.SOAPENVEnvelope.SOAPENVBody.scpSimpleInvokeResponse.invokeResult != null &&
                            Data.SOAPENVEnvelope.SOAPENVBody.scpSimpleInvokeResponse.invokeResult.redirectUrl != null)
                        {
                            context.OutputParameters["SCPReference"] = Data.SOAPENVEnvelope.SOAPENVBody.scpSimpleInvokeResponse.scpReference.text.ToString();
                            context.OutputParameters["ReturnURL"] = Data.SOAPENVEnvelope.SOAPENVBody.scpSimpleInvokeResponse.invokeResult.redirectUrl.ToString();
                            context.OutputParameters["RequestStatus"] = Data.SOAPENVEnvelope.SOAPENVBody.scpSimpleInvokeResponse.invokeResult.status.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }


            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        private static string GenerateTimestamp()
        {
            DateTime now = DateTime.UtcNow;
            return string.Format("{0:d4}{1:d2}{2:d2}{3:d2}{4:d2}{5:d2}",
            now.Year,
            now.Month,
            now.Day,
            now.Hour,
            now.Minute,
            now.Second);
        }

        private static string CalculateDigest(string secretkey, string credentialsToHash)
        {
            byte[] keyBytes = Convert.FromBase64String(secretkey);
            byte[] bytesToHash = (new UTF8Encoding()).GetBytes(credentialsToHash);
            HMACSHA256 hmac = new HMACSHA256(keyBytes);
            byte[] hash = hmac.ComputeHash(bytesToHash);
            return Convert.ToBase64String(hash);
        }


        private static HttpWebRequest CreateWebRequest(string url, string action)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add("SOAPAction", "");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        private static XmlDocument CreateSoapEnvelope(string uniqueID, string timmm, string tttt, string pay360SCPID, string pay360SiteID, string pay360HMACID, string portalURL, string PaymentTransactionID, string fundCode, string vATCode, string price, string serviceName, string requestID)
        {



            string xmlPayload = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
<scpSimpleInvokeRequest xmlns=""http://www.capita-software-services.com/scp/simple"" xmlns:simple=""http://www.capita-software-services.com/scp/simple""
    xmlns:scpbase=""http://www.capita-software-services.com/scp/base"">
    <credentials xmlns=""https://support.capita-software.co.uk/selfservice/?commonFoundation"">
        <subject>
            <subjectType>CapitaPortal</subjectType>
            <identifier>" + pay360SCPID + @"</identifier>
            <systemCode>SCP</systemCode>
        </subject>
        <requestIdentification>
            <uniqueReference>" + uniqueID + @"</uniqueReference>
            <timeStamp>" + timmm + @"</timeStamp>
        </requestIdentification>
        <signature>
            <algorithm>Original</algorithm>
            <hmacKeyID>" + pay360HMACID + @"</hmacKeyID>
            <digest>" + tttt + @"</digest>
        </signature>
    </credentials>
    <scpbase:requestType>payOnly</scpbase:requestType>
    <scpbase:requestId>" + requestID + @"</scpbase:requestId>
    <scpbase:routing>
        <scpbase:returnUrl><![CDATA[" + portalURL + @"/PaymentSuccess?Id=" + PaymentTransactionID + @"]]></scpbase:returnUrl>
        <scpbase:backUrl><![CDATA[" + portalURL + @"/PaymentFailure?Id=" + PaymentTransactionID + @"]]></scpbase:backUrl>
        <scpbase:siteId>" + pay360SiteID + @"</scpbase:siteId>
        <scpbase:scpId>" + pay360SCPID + @"</scpbase:scpId>
    </scpbase:routing>
    <scpbase:panEntryMethod>ECOM</scpbase:panEntryMethod>
    <simple:sale>
        <scpbase:saleSummary>
            <scpbase:description>" + serviceName + @"</scpbase:description>
            <scpbase:amountInMinorUnits>" + Convert.ToDecimal(price) * 100 + @"</scpbase:amountInMinorUnits>
            <scpbase:reference>" + requestID + @"</scpbase:reference>
            <scpbase:displayableReference>" + serviceName + @"</scpbase:displayableReference>
        </scpbase:saleSummary>
        <simple:items>
            <simple:item>
                <scpbase:itemSummary>
                    <scpbase:description>" + serviceName + @"</scpbase:description>
                    <scpbase:amountInMinorUnits>" + Convert.ToDecimal(price) * 100 + @"</scpbase:amountInMinorUnits>
                    <scpbase:reference>" + requestID + @"</scpbase:reference>
                </scpbase:itemSummary>
                <scpbase:tax>
                    <scpbase:vat>
                        <scpbase:vatCode>" + vATCode + @"</scpbase:vatCode>
                        <scpbase:vatRate>0.00</scpbase:vatRate>
                        <scpbase:vatAmountInMinorUnits>0</scpbase:vatAmountInMinorUnits>
                    </scpbase:vat>
                </scpbase:tax>
                <scpbase:quantity>1</scpbase:quantity>
                <scpbase:lgItemDetails>
                    <scpbase:fundCode>" + fundCode + @"</scpbase:fundCode>
                </scpbase:lgItemDetails>
                <scpbase:lineId>Payment_1</scpbase:lineId>
            </simple:item>
        </simple:items>
    </simple:sale>
</scpSimpleInvokeRequest>
</soap:Body>
</soap:Envelope>";

            XmlDocument soapEnvelopeDocument = new XmlDocument();
            soapEnvelopeDocument.LoadXml(xmlPayload);
            return soapEnvelopeDocument;
        }

        private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }

        private static string GetEnvironmentVariableDefaultValue(string schemaName, IOrganizationService orgService)
        {
            var query = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet("defaultvalue", "schemaname", "environmentvariabledefinitionid")
            };

            query.Criteria.AddCondition("schemaname", ConditionOperator.Equal, schemaName);

            var result = orgService.RetrieveMultiple(query);

            if (result.Entities.Count > 0)
            {
                Entity entity = result.Entities[0];
                if (entity.Attributes.Contains("defaultvalue"))
                {
                    return entity.GetAttributeValue<string>("defaultvalue");
                }
            }

            // Return null if no matching record is found or if 'value' attribute is null.
            return null;
        }

    }
}
