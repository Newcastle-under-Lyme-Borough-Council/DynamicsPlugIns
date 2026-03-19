using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SS.MSDYN.LGIntelliware.Actions.Common.QueryResponse
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class AuthDetails
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }
        public string authCode { get; set; }
        public string amountInMinorUnits { get; set; }
        public string maskedCardNumber { get; set; }
        public string cardDescription { get; set; }
        public string cardType { get; set; }
        public string merchantNumber { get; set; }
        public string expiryDate { get; set; }
        public string continuousAuditNumber { get; set; }
    }

    public class ContinuousAuditNumber
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }

        [JsonProperty("#text")]
        public string text { get; set; }
    }

    //public class Items
    //{
    //    [JsonConverter(typeof(SingleOrArrayCollectionConverter<List<ItemSummary>, ItemSummary>))]
    //    public List<ItemSummary> itemSummary { get; set; }
    //}

    public class ItemSummary
    {
        public LineId lineId { get; set; }
        public ContinuousAuditNumber continuousAuditNumber { get; set; }
    }

    public class LineId
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }

        [JsonProperty("#text")]
        public string text { get; set; }
    }

    public class PaymentDetails
    {
        public PaymentHeader paymentHeader { get; set; }
        public AuthDetails authDetails { get; set; }
        public SaleSummary saleSummary { get; set; }
    }

    public class PaymentHeader
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }
        public DateTime transactionDate { get; set; }
        public string machineCode { get; set; }
        public string uniqueTranId { get; set; }
    }

    public class PaymentResult
    {
        public Status status { get; set; }
        public PaymentDetails paymentDetails { get; set; }
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

    public class SaleSummary
    {
        //public Items items { get; set; }
    }

    public class ScpReference
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }

        [JsonProperty("#text")]
        public string text { get; set; }
    }

    public class ScpSimpleQueryResponse
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }
        public RequestId requestId { get; set; }
        public ScpReference scpReference { get; set; }
        public TransactionState transactionState { get; set; }
        public StoreCardResult storeCardResult { get; set; }
        public PaymentResult paymentResult { get; set; }
    }

    public class SOAPENVBody
    {
        public ScpSimpleQueryResponse scpSimpleQueryResponse { get; set; }
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

    public class Status
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }

        [JsonProperty("#text")]
        public string text { get; set; }
    }

    public class StoreCardResult
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }
        public string status { get; set; }
    }

    public class TransactionState
    {
        [JsonProperty("@xmlns")]
        public string xmlns { get; set; }

        [JsonProperty("#text")]
        public string text { get; set; }
    }
}
