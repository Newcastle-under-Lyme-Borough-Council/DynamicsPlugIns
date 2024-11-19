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
using SS.MSDYN.LGIntelliware.Actions.Common.QueryResponse;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Policy;

namespace SS.MSDYN.LGIntelliware.Actions
{
    public class Pay360InvokeResponse : IPlugin
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
                //var PaymentTransactionID = context.InputParameters["RecordID"].ToString();
                var scpReference = context.InputParameters["SCPReference"].ToString();

                string Pay360serviceurl = context.InputParameters["Pay360serviceurl"].ToString();//GetEnvironmentVariableDefaultValue("ss_Pay360HMACSecretKeyWebserviceEndpoint", service).ToString() + "/scpClient";//"https://sbsctest.e-paycapita.com/scp/scpws/scpClient";
                string Pay360SiteID = context.InputParameters["Pay360SiteID"].ToString();//GetEnvironmentVariableDefaultValue("ss_Pay360SiteID", service);
                string Pay360SCPID = context.InputParameters["Pay360SCPID"].ToString();//GetEnvironmentVariableDefaultValue("ss_Pay360SCPID", service);
                string Pay360HMACID = context.InputParameters["Pay360HMACID"].ToString();//GetEnvironmentVariableDefaultValue("ss_Pay360HMACID", service);
                string Pay360HMACSecretKey = context.InputParameters["Pay360HMACSecretKey"].ToString();//GetEnvironmentVariableDefaultValue("ss_Pay360HMACSecretKey", service);

                try
                {
                    string uniqueID = Guid.NewGuid().ToString().Substring(0, 5);
                    string timmm = GenerateTimestamp();


                    string cre = "CapitaPortal!373224939!" + uniqueID + "!" + timmm + "!Original!456";
                    var tttt = CalculateDigest(Pay360HMACSecretKey, cre);

                    XmlDocument soapEnvelopeXml = CreateSoapEnvelope(uniqueID, timmm, tttt, Pay360HMACID, scpReference, Pay360SCPID);
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

                        Console.Write(soapResult);

                        if (Data.SOAPENVEnvelope != null &&
                            Data.SOAPENVEnvelope.SOAPENVBody != null &&
                            Data.SOAPENVEnvelope.SOAPENVBody.scpSimpleQueryResponse != null &&
                            Data.SOAPENVEnvelope.SOAPENVBody.scpSimpleQueryResponse.paymentResult != null &&
                            Data.SOAPENVEnvelope.SOAPENVBody.scpSimpleQueryResponse.paymentResult.status != null &&
                            Data.SOAPENVEnvelope.SOAPENVBody.scpSimpleQueryResponse.paymentResult.status != null)
                        {
                            context.OutputParameters["PaymentStatus"] = Data.SOAPENVEnvelope.SOAPENVBody.scpSimpleQueryResponse.paymentResult.status.text.ToString();
                            context.OutputParameters["ContinousAuditNumber"] = Data.SOAPENVEnvelope.SOAPENVBody.scpSimpleQueryResponse.paymentResult.paymentDetails.authDetails.continuousAuditNumber.ToString();
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

        private static XmlDocument CreateSoapEnvelope(string uniqueID, string requestTime, string digest, string Pay360HMACID, string scpReference, string pay360SCPID)
        {

            string credentialsXML = @"<credentials xmlns='https://support.capita-software.co.uk/selfservice/?commonFoundation'>
        <subject>
            <subjectType>CapitaPortal</subjectType>
            <identifier>"+ pay360SCPID + @"</identifier>
            <systemCode>SCP</systemCode>
        </subject>
        <requestIdentification>
            <uniqueReference>"+ uniqueID + @"</uniqueReference>
            <timeStamp>"+ requestTime + @"</timeStamp>
        </requestIdentification>
        <signature>
            <algorithm>Original</algorithm>
            <hmacKeyID>"+ Pay360HMACID + @"</hmacKeyID>
            <digest>"+ digest + @"</digest>
        </signature>
    </credentials>";

            string xmlPayload = @"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <scpSimpleQueryRequest xmlns=""http://www.capita-software-services.com/scp/simple"" xmlns:simple=""http://www.capita-software-services.com/scp/simple"" xmlns:scpbase=""http://www.capita-software-services.com/scp/base"">
            "+ credentialsXML + @"
            <scpbase:siteId>"+ Pay360HMACID + @"</scpbase:siteId>
            <scpbase:scpReference>"+ scpReference + @"</scpbase:scpReference>
        </scpSimpleQueryRequest>
    </soap:Body>
</soap:Envelope>"
            ;

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
