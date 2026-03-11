using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace SS.MSDYN.LGIntelliware.Actions
{
    #region Sealed Class: ServiceConfigurationColumnNames
    /// <summary>
    /// Sealed class for service configuration table column names.
    /// </summary>
    public sealed class ServiceConfiguration
    {
        public const string TableName = "ss_serviceconfiguration";
        public const string TableAlias = "ss_serviceconfiguration_alias";
        public const string ServiceConfigurationid = "ss_serviceconfigurationid";
        public const string Name = "ss_name";
        public const string Subject = "ss_subject";
        public const string Status = "statecode";
        public const string StatusReason = "statuscode";
        public const string BaseUrl = "ss_baseurl";
        public const string TokenUrl = "ss_tokenurl";
        public const string ClientId = "ss_clientid";
        public const string ClientSecret = "ss_clientsecret";
        public const string CreatePaymentLink = "ss_createpaymentlinkendpoint";
        public const string CheckPaymentStatus = "ss_paymentstatusendpoint";
        public const string GetProductEndpoint = "ss_getproductendpoint";
        public const string Environment = "ss_environment";
        public const string RecordName = "Payment Integration";
    }
    #endregion

    #region Sealed Class: Payment Transaction ColumnNames
    /// <summary>
    /// Sealed class for Payment Transaction table column names.
    /// </summary>
    public sealed class PaymentTransaction
    {
        public const string TableName = "ss_paymenttransaction";
        public const string PaymentTransactionId = "ss_paymenttransactionid";
        public const string Amount = "ss_amount";
        public const string Case = "ss_case";
        public const string ServiceRequestId = "ss_serviceid";
        public const string TransactionId = "ss_transactionid";
        public const string PaymentLink = "ss_paymentlink";
        public const string StatusReason = "statuscode";
        public const string Paid = "ss_paid";
    }
    #endregion

    #region Sealed Class: Case ColumnNames
    /// <summary>
    /// Sealed class for Payment Transaction table column names.
    /// </summary>
    public sealed class Case
    {
        public const string TableName = "incident";

    }
    #endregion

    #region Enum: Payment Transactions
    /// <summary>
    /// Enumeration for Status Code.
    /// </summary>

    public enum StatusCode
    {
        Pending = 1,
        Success = 717800001,
        Failed = 717800002
    }
    #endregion

    #region Sealed Class: Payment Transaction ColumnNames
    /// <summary>
    /// Sealed class for Taxi Licence  table column names.
    /// </summary>
    public sealed class TaxiLicence
    {
        public const string TableName = "ss_taxilicence";
        
    }
    #endregion

}
