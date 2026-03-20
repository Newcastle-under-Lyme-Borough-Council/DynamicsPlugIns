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
    public sealed class ServiceRequest
    {
        public const string MissedBinTableName = "ss_missedbin";
        public const string MissedBinTableAlias = "ss_missedbin_alias";
        public const string MissedBin = "ss_missedbinid";
        public const string PlanningPermissionTableName = "ss_planningpermission";
        public const string PlanningPermissionTableAlias = "ss_planningpermission_alias";
        public const string PlanningPermission = "ss_planningpermissionid";
        public const string OrderRecyclingContainerTableName = "crb98_orderrecyclingcontainer";
        public const string OrderRecyclingContainerTableAlias = "crb98_orderrecyclingcontainer_alias";
        public const string TaxiLicenceTableName = "ss_taxilicence";
        public const string TaxiLicenceTableAlias = "ss_taxilicence_alias";
        public const string TaxiLicence = "ss_taxilicenceid";
        public const string GardenWasteTableName = "jcc_gardenwaste1";
        public const string GardenWasteTableAlias = "jcc_gardenwaste1_alias";
        public const string GardenWaste = "jcc_gardenwaste1id";
        public const string ReferenceNumber = "ss_referencenumber";
        public const string GardenWasteReferenceNumber = "jcc_gardenwasterequest";
        public const string GardenWasteContact = "jcc_contact";
        public const string Description = "ss_description";
        public const string Owner = "ownerid";
        public const string SourceType = "ss_sourcetype";
        public const string Customer = "ss_customer";
        public const string ReportedBy = "ss_onbehalfofsomeone";
        public const string ServiceConfiguration = "ss_serviceconfiguration";
        public const string Case = "ss_incidentid";
        public const string ContactProperty = "ss_contactproperty";
        public const string Property = "ss_property";
        public const string PropertyUprn = "ss_propertyuprn";

    }
    #endregion

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
    }
    #endregion

    #region Sealed Class: SubjectTableColumnNames
    /// <summary>
    /// Sealed class for subject table column names.
    /// </summary>
    public sealed class Subject
    {
        public const string TableName = "subject";
        public const string TableAlias = "ss_subject_alias";
        public const string SubjectId = "subjectid";
        public const string Title = "title";
    }
    #endregion

    #region Sealed Class: IncidentTableColumnNames
    /// <summary>
    /// Sealed class for incident table column names.
    /// </summary>
    public sealed class Incident
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
        public const string ContactProperty = "ss_contactproperty";
        public const string Property = "ss_property";
        public const string PropertyUprn = "ss_propertyuprn";

    }
    #endregion

    #region Sealed Class: SystemUserTableColumnNames
    /// <summary>
    /// Sealed class for system user table column names.
    /// </summary>
    public sealed class SystemUser
    {
        public const string TableName = "systemuser";
        public const string TableAlias = "ss_systemuser_alias";
    }
    #endregion

    #region Sealed Class: ContactTableColumnNames
    /// <summary>
    /// Sealed class for contact table column names.
    /// </summary>
    public sealed class Contact
    {
        public const string TableName = "contact";
        public const string TableAlias = "ss_contact_alias";
        public const string Uprn = "ss_uprn";
        public const string ContactId = "contactid";
        public const string Address = "address1_line1";
        public const string Address2 = "address1_line2";
        public const string City = "address1_city";
        public const string County = "address1_county";
        public const string PostCode = "address1_postalcode";
        public const string Address1_address = "ss_address1_address";
        public const string Address1_line3 = "address1_line3";
        public const string Stateorprovince = "address1_stateorprovince";
        public const string Country = "address1_country";
        public const string Latitude = "address1_latitude";
        public const string Longitude = "address1_longitude";
    }
    #endregion


    #region Sealed Class: PropertyTableColumnNames
    /// <summary>
    /// Sealed class for property table column names.
    /// </summary>
    public sealed class Property
    {
        public const string TableName = "ss_property";
        public const string PropertyId = "ss_propertyid";
        public const string Uprn = "ss_uprn";
        public const string Contact = "ss_contact";
        public const string Addresscs = "ss_addresscs";
        public const string Addressoscs = "ss_addressoscs";
        public const string TownName = "ss_townname";
        public const string County = "ss_county";
        public const string PostCode = "ss_postcode";
        public const string Localityname = "ss_localityname";
        public const string Posttown = "ss_posttown";
        public const string Region = "ss_region";
        public const string Latitude = "ss_latitude";
        public const string Longitude = "ss_longitude";
        public const string Streetname = "ss_streetname";
        public const string CreatedOn = "createdon";

    }
    #endregion


    #region Sealed Class: ContactPropertyTableColumnNames
    /// <summary>
    /// Sealed class for contactproperty table column names.
    /// </summary>
    public sealed class ContactProperty
    {
        public const string TableName = "ss_contactproperty";
        public const string Contact = "ss_contact";
        public const string Property = "ss_property";
        public const string IsDefault = "ss_isdefault";
        public const string ContactPropertyId = "ss_contactpropertyid";
    }
    #endregion

}
