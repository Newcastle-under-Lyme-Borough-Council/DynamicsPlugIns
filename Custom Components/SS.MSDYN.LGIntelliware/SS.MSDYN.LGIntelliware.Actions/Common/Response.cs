using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.MSDYN.LGIntelliware.Actions.Common
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class InvokeResult
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }
        public string status { get; set; }
        public string redirectUrl { get; set; }
    }

    public class RequestId
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }

        [JsonProperty("#text")]
        public string text { get; set; }
    }

    public class Root
    {
        [JsonProperty("SOAP-ENV:Envelope")]
        public SOAPENVEnvelope SOAPENVEnvelope { get; set; }
    }

    public class ScpReference
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }

        [JsonProperty("#text")]
        public string text { get; set; }
    }

    public class ScpSimpleInvokeResponse
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }
        public RequestId requestId { get; set; }
        public ScpReference scpReference { get; set; }
        public TransactionState transactionState { get; set; }
        public InvokeResult invokeResult { get; set; }
    }

    public class SOAPENVBody
    {
        public ScpSimpleInvokeResponse scpSimpleInvokeResponse { get; set; }
    }

    public class SOAPENVEnvelope
    {
        [JsonProperty("@xmlns:SOAP-ENV")]
        public string xmlnsSOAPENV { get; set; }

        [JsonProperty("SOAP-ENV:Header")]
        public object SOAPENVHeader { get; set; }

        [JsonProperty("SOAP-ENV:Body")]
        public SOAPENVBody SOAPENVBody { get; set; }
    }

    public class TransactionState
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }

        [JsonProperty("#text")]
        public string text { get; set; }
    }


}
