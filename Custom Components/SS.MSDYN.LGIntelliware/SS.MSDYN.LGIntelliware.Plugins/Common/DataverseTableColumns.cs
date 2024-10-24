using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace SS.MSDYN.LGIntelliware.Plugins
{

    #region Sealed Class: MissedBinTableColumnNames
    /// <summary>
    /// Sealed class for service request table column names.
    /// </summary>
    public sealed class ServiceRequestTableColumnNames
    {
        public const string MissedBinTableName = "ss_missedbin";
        public const string MissedBinTableAlias = "ss_missedbin_alias";
        public const string MissedBin = "ss_missedbinid";
        public const string PlanningPermissionTableName = "ss_planningpermission";
        public const string PlanningPermissionTableAlias = "ss_planningpermission_alias";
        public const string PlanningPermission = "ss_planningpermissionid";
        public const string TaxiLicenceTableName = "ss_taxilicence";
        public const string TaxiLicenceTableAlias = "ss_taxilicence_alias";
        public const string TaxiLicence = "ss_taxilicenceid";
        public const string ReferenceNumber = "ss_referencenumber";
        public const string Description = "ss_description";
        public const string Owner = "ownerid";
        public const string SourceType = "ss_sourcetype";
        public const string Customer = "ss_customer";
        public const string ReportedBy = "ss_onbehalfofsomeone";
        public const string ServiceConfiguration = "ss_serviceconfiguration";
        public const string Case = "ss_incidentid";
    }
    #endregion

    #region Sealed Class: ServiceConfigurationColumnNames
    /// <summary>
    /// Sealed class for service configuration table column names.
    /// </summary>
    public sealed class ServiceConfigurationTableColumnNames
    {
        public const string TableName = "ss_serviceconfiguration";
        public const string TableAlias = "ss_serviceconfiguration_alias";
        public const string ServiceConfiguration = "ss_serviceconfigurationid";
        public const string Name = "ss_name";
        public const string Subject = "ss_subject";
        public const string Status = "statecode";
        public const string StatusReason = "statuscode";
    }
    #endregion

    #region Sealed Class: SubjectTableColumnNames
    /// <summary>
    /// Sealed class for subject table column names.
    /// </summary>
    public sealed class SubjectTableColumnNames
    {
        public const string TableName = "subject";
        public const string TableAlias = "ss_subject_alias";
        public const string Subject = "subjectid";
        public const string Title = "title";
    }
    #endregion

    #region Sealed Class: IncidentTableColumnNames
    /// <summary>
    /// Sealed class for incident table column names.
    /// </summary>
    public sealed class IncidentTableColumnNames
    {
        public const string TableName = "incident";
        public const string TableAlias = "ss_incident_alias";
        public const string Service = "ss_serviceconfigurationid";
        public const string CaseTitle = "title";
        public const string CaseType = "casetypecode";
        public const string Customer = "customerid";
        public const string Description = "description";
        public const string Owner = "ownerid";
        public const string Priority = "prioritycode";
        public const string ReportedBy = "ss_onbehalfofsomeoneid";
        public const string ServiceRequest = "ss_servicerequestid";
        public const string Subject = "subjectid";
        public const string CaseNumber = "ticketnumber";
        public const string Origin = "caseorigincode";
        public const string ServiceRequestStatus = "ss_externalsystemservicerequeststatus";
    }
    #endregion

    #region Sealed Class: SystemUserTableColumnNames
    /// <summary>
    /// Sealed class for system user table column names.
    /// </summary>
    public sealed class SystemUserTableColumnNames
    {
        public const string TableName = "systemuser";
        public const string TableAlias = "ss_systemuser_alias";
    }
    #endregion

    #region Sealed Class: ContactTableColumnNames
    /// <summary>
    /// Sealed class for contact table column names.
    /// </summary>
    public sealed class ContactTableColumnNames
    {
        public const string TableName = "contact";
        public const string TableAlias = "ss_contact_alias";
    }
    #endregion
}
