// Define namespace
if (typeof SS === "undefined") {
    SS = {};
}
if (typeof SS.MSDYN === "undefined") {
    SS.MSDYN = {};
}
if (typeof SS.MSDYN.LGIntelliware === "undefined") {
    SS.MSDYN.LGIntelliware = {};
}
if (typeof SS.MSDYN.LGIntelliware.WR === "undefined") {
    SS.MSDYN.LGIntelliware.WR = {};
}

// Namespace for constants
SS.MSDYN.LGIntelliware.WR.Constants = {

    //----------------------------------------------------------------Licence Service Table------------------------------------------------------
    //table fields
    licenseServiceTableFields: {
        licenseServiceSchemaName: "ss_LicenseService",
        licenseServiceId: "ss_licenseserviceid",
        licenseServiceLogicalName: "ss_licenseservice",
        areYouAnAgent: "ss_areyouanagentactingonbehalfoftheapplicant",
        agentActingOnBehalf: "ss_agentactingonbehalfoftheapplicant",
        contact: "ss_contact",
        newLicenseOrExisting: "ss_isthisanewlicenseorrenewinganexisting",
        placingFurnitureOnPavement: "ss_applicationforplacingfurnitureonpavement",
        licenseOfAlcohol: "ss_premiseshaveanalcohollicenseunderact2003",
        premisesLicenseNumber: "ss_pleasestatethepremiseslicencenumber",
        supplyOfAlcoholOnPavement: "ss_willalcoholbeusedintheareaofpavement",
        postalAddressType: "ss_postaladdresstype",
        premisesAddressOsMapReference: "ss_osmapreference",
        premisesAddressDescription: "ss_addressdescription",
        sellingOfRetailAlcohol: "ss_willyoubesellingalcoholbyoronbehalfofa",
        capacity: "ss_capacityareyouapplyingfor",
        confirmTheFollowing: "ss_confirmthefollowing",
        applicantApplyingAs: "ss_applyingas",
        applicantBusinessRegisteredInUK: "ss_isyourbusinessregistrdinukwithcompanyhouse",
        applicantBusinessRegisteredOutsideUK: "ss_isyourbusinessregisteredoutsidetheuk",
        applicantRegistrationNumber: "ss_registrationnumber",
        applicantCommercialRegister: "ss_commercialregister",
        agentApplyingAs: "ss_agentapplyingas",
        agentBusinessRegisteredInUK: "ss_agentisyourbusinessregisteredinuk",
        agentBusinessRegisteredOutsideUK: "ss_agentisyourbusinessregisteredoutsideuk",
        agentRegistrationNumber: "ss_agentregistrationnumber",
        agentCommercialRegister: "ss_agentcommercialregister",
        updateRequired: "ss_arethereanyupdates"
    },
    //table form sections
    licenseServiceTableSections: {
        //Pavement Form
        applicantContactDetails: "tab_AgentDetails_section_ApplicantContactDetails",
        //Club Premises Form
        applicantDetails: "tab_General_section_ApplicantDetails",
        applicantBusiness: "tab_General_section_ApplicantBusiness",
        agentDetails: "tab_General_section_AgentDetails",
        agentBusiness: "tab_General_section_AgentBusiness",
        agentBusinessAddress: "tab_General_section_AgentBusinessAddress",
        postalAddressOfClubPremises: "tab_ApplicationDetails_section_PostalAddressOfPremises",
        clubHoursMonday: "tab_ClubHours_section_Monday",
        clubHoursTuesday: "tab_ClubHours_section_Tuesday",
        clubHoursWednesday: "tab_ClubHours_section_Wednesday",
        clubHoursThursday: "tab_ClubHours_section_Thursday",
        clubHoursFriday: "tab_ClubHours_section_Friday",
        clubHoursSaturday: "tab_ClubHours_section_Saturday",
        clubHoursSunday: "tab_ClubHours_section_Sunday",
        clubHoursStandartDaysAndTimings: "tab_ClubHours_section_Standard Days And Timings",
        //premises form
        osMapReference: "tab_PremisesDetails_section_OSMapReference",
        addressDescription: "tab_PremisesDetails_section_AddressDescription",
        postalAddressOfPremises: "tab_PremisesDetails_section_PostalAddressOfPremises",
    },
    //table form tabs
    licenseServiceTableTabs: {
        //premises
        premisesDetails: "tab_PremisesDetails",
        nonIndividualApplicant: "tab_NonIndividualApplicants",
        individualApplicant: "tab_IndividualApplicantDetails",
        //pavement
        agentDetails: "tab_AgentDetails",
        //club premises
        general: "tab_General",
        applicationDetails: "tab_ApplicationDetails",
        clubHours: "tab_ClubHours"
    },
    //service configuration lookup values
    licenceServiceServiceConfiguration: {
        pavementLicense: "license service - pavement",
        clubPremises: "license service - club premises",
        personal: "license service - personal",
        premises: "license service - premises"
    },
    //header fields
    licenceServiceHeaderFields: {
        serviceConfiguration: "ss_serviceconfiguration"
    },
    //form names
    licenceServiceFormNames: {
        pavementLicense: "back office - pavement license service",
        clubPremises: "back office - club premises certificate",
        personal: "back office - personal license service",
        premises: "back office - premises license service"
    },
    //BPF names
    licenseServiceBPFNames: {
        pavementLicense: "ss_pavementlicensebpf",
        clubPremises: "ss_clubpremisescertificatebpf",
        personal: "ss_personallicenseservicebpf",
        premises: "ss_premiseslicenseservicebpf"
    },
    //----------------------------------------------------------------Taxi Licence Table------------------------------------------------------
    //Taxi licence table fields
    taxiLicenceTableFields: {
        taxiLicenceSchemaName: "ss_taxilicences",
        taxiLicenceId: "ss_taxilicenceid",
        taxiLicenceLogicalName: "ss_taxilicence",
        group2MedicalForm: "ss_group2medicalform",
        certificateOfGoodConduct: "ss_certificateofgoodconductfromembassy",
        dbsCertificate: "ss_dbscertificate",
        disabilityEqualityCertificate: "ss_disabilityequalitysafeguardingtrainingcertif",
        serviceConfiguration: "ss_serviceconfiguration",
        garageTestPassCertificate: "ss_uploadgaragetestpasscertificate",
        taximeterReport: "ss_taximeterreportupload",
        publicHireInsuranceCertificate: "ss_uploadpublichireinsurancecertificate",
        conversionCertificate: "ss_uploadconversioncertificate",
        v5Logbookv5Slip: "ss_uploadv5logbookv5slip",
        privateHireInsuranceCertificate: "ss_uploadprivatehireinsurancecertificate",
        premisesPublicLiabilityInsurance: "ss_premisespublicliabilityinsurance",
        basicDisclosureCertificate: "ss_uploadbasicdisclosurecertificateenhanceddbs",
        disabilityEqualitySafeguardingTraining: "ss_uploaddisabilityequalitysafeguardingtraining",
        fleetInsuranceWithScheduleOfVehicles: "ss_uploadfleetinsurancewithscheduleofvehicles",
        whatAreYouNotifyingUsOf: "ss_whatareyounotifyingusoff",
        additionalQuestionsApplyingAs: "ss_additionalquestionsareyouapplyingas",
        stateCode: "statecode",
        statusCode: "statuscode",
        grantReject: "ss_grantreject"
    },

     //Taxi licence table form tabs
    taxiLicenceTableTabs: {
        applicantDetails: "tab_applicant_details",
        notificationOfConvictions: "tab_notification_of_convictions_details",
        taxiDriverLicence: "tab_taxi_driver_licence_details",
        mot: "tab_mot",
        hackneyCarriageAndPrivateHire: "tab_hackney_carriage_and_private_hire_details",
        dvlaDriverData: "tab_dvla_driver_data",
        privateHireOperator: "tab_private_hire_operator_details",
        dvlaVehicleDetails: "tab_dvla_vehicle_details",
        dbs: "tab_dbs",
        pay360: "tab_pay360",
        summary: "tab_summary",
    },

    //Taxi licence table form sections
    taxiLicenceTableSections: {
        officeUseOnly: "tab_taxi_driver_licence_details_sec_office_use_only",
        motoringOffenceDetailsSection: "tab_notification_of_convictions_details_sec_motoring_offence",
        cautionOrWarningDetails: "tab_notification_of_convictions_details_sec_caution_or_warning",
        questionedORInterviewedDetails: "tab_notification_of_convictions_details_sec_question_or_interviewed_by_police",
        convictionsDetails: "tab_notification_of_convictions_details_sec_convictions",
        licenceDetails: "tab_notification_of_convictions_details_sec_licence_details",
        companyDetails: "tab_private_hire_operator_details_sec_company_details",
        jointApplicants: "tab_private_hire_operator_details_sec_joint_applicant",
        individualApplicants: "tab_private_hire_operator_details_sec_individual_applicant_details",
        evidenceRightToWork: "tab_private_hire_operator_details_evidence_of_right_to_work",
        evidenceRightToWorkJointOwner: "tab_private_hire_operator_details_sec_evidence_of_right_to_work_in_uk"
    },

    //Taxi licence table bpf ids
    taxiLicenceBPFNames: {
        taxiDriverLicence: "ss_bpf_dualhackneycarriagevehicle",
        privateHireOperator: "ss_bpf_privatehireoperatorvehicle",
        notificationOfConvictions: "ss_bpf_notificationofconvictionsandoffencesvehicle",
        hackneyCarriageVehicle: "ss_bpf_hackneycarriagevehicle",
        privateHireVehicle: "ss_bpf_privatehirevehicle"
    },

    //Taxi licence table bpf stages
    taxiLicenceBPFStage: {
        reviewApplication: "Review Application",
        dvlaDriverDataReview: "DVLA (Driver Data) Review",
        dvlaVehicleDetailsReview: "DVLA (Vehicle Details) Review",
        dbsReview: "DBS Review",
        paymentReview: "Payment Review",
        reviewMOTHistory: "Review MOT History",
        grant: "Grant",
        reject: "Reject",
        grantOrReject: "Grant/Reject"
    },

    //Taxi licence bpf fields
    taxiLicenceBpfFields: {
        headerBadgeNumber: "header_process_ss_newbadgenumber",
        paid: "header_process_ss_paid",
        whatAreYouNotifying: "header_process_ss_whatareyounotifyingusoff"
    },

    //Taxi licence form header fields
    taxiLicenceHeaderFields: {
        headerServiceConfiguration: "header_ss_serviceconfiguration",
    },

    //Taxi licence table statuscodes
    taxiLicenceStatusCode: {
        motHistoryVerification: 717800005,
        paymentPending: 717800006,
        paid: 717800007,
        closedORRejected: 2,
        granted: 717800002,
        ClosedOrCompleted: 717800004

    },

    //Taxi licence service configuration lookup values
    taxiLicenceServiceConfiguration: {
        taxiDriverLicence: "taxi licence - taxi driver licence",
        hackneyCarriageVehicle: "taxi licence - hackney carriage vehicle",
        privateHireVehicle: "taxi licence - private hire vehicle",
        privateHireOperator: "taxi licence - private hire operator",
        notificationOfConvictions: "taxi licence - notification of convictions and offences"
    },

    //----------------------------------------------------------------MissedBin Table------------------------------------------------------
    //Missedbin custom action
    missedBinTableCustomAction: {
        bartecMunicipalCheckServiceRequestStatus: "ss_BartecMunicipalCheckServiceRequestStatus",
        typeName: "mscrm.ss_missedbin"
    },

    //Missedbin table fields
    missedBinTableFields: {
        missedBinTableName: "ss_missedbin",
        missedBinSchemaName: "ss_missedbins",
        missedBinId: "ss_missedbinid",
        missedBinLogicalName: "ss_missedbin",

    },
    //Missedbin table dialogue configurations 
    missedBinTableDialogueConfiguration: {
        bartecMunicipalCheckServiceRequestStatusText: "Do you want to Check Status in Bartec Municipal? You can't undo this action.",
        bartecMunicipalCheckServiceRequestStatusTitle: "Confirm Bartec Municipal Check Status",
        bartecMunicipalCheckServiceRequestStatusHeight: 200,
        bartecMunicipalCheckServiceRequestStatusWidth: 450,
    },

    //---------------------------------------------------------------Holiday Table -------------------------------------------------------
    bankHolidayTableFields: {
        date: "ss_date"
    },

    //Bank holiday table field error message
    bankHolidayTableFieldErrorMessage: {
        restrictPastDatesText: "Past dates are not allowed. Please select today or a future date."
    },

    //---------------------------------------------------------------Contact Table -------------------------------------------------------
    contactTableFields: {
        mobilePhone: "mobilephone"
    },
    //Contact table field error message
    contactTableFieldErrorMessage: {
        validateMobilePhoneFormatText: "Should start with 00, should have the Country code, and then the Phone number. Maximum allowed length is 14 digits, no + sign, spaces or - are allowed."
    },

    //----------------------------------------------------------------Sms Table ------------------------------------------------------

    smsHeaderFields: {
        headerScheduledEnd: "header_scheduledend"
    },

    //----------------------------------------------------------------Planning Permission Table ------------------------------------------------------
    planningPermissionTableFields: {
        planningPermissionSchemaName: "ss_planningpermissions",
        planningPermissionId: "ss_planningpermissionid",
        planningPermissionLogicalName: "ss_planningpermission",
    },


    //----------------------------------------------------------------Customer Interaction Table ------------------------------------------------------

    customerInteractionTableFields: {
        regardingObject: "regardingobjectid",
        response: "ss_response",
        description: "description",
        subject: "ss_subject",
        customer: "ss_customer",
        serviceConfiguration: "ss_serviceconfigurationid",
        customerLookupValue: "_ss_customer_value",
        serviceConfigurationValue: "_ss_serviceconfiguration_value"
    },


    //----------------------------------------------------------------Custom Action ------------------------------------------------------
    customActionParameter: {
        boundParameter: "entity",
        operationType: 0,
        structuralProperty: 5
    },


    //---------------------------------------------------------------- Form Type ------------------------------------------------------

    formType: {
        create: 1
    },

    //---------------------------------------------------------------- Status column ------------------------------------------------------
    stateCode: {
        active: 0,
        inactive: 1
    },
    //---------------------------------------------------------------- Business Process Flow Status ------------------------------------------------------
    bpfStatus: {
        finished: "finished"
    },
    //---------------------------------------------------------------- Business Process Flow Status ------------------------------------------------------
    workFlowTableStateCode: {
        active: 1
    },

    //---------------------------------------------------------------- Work Flow Table ------------------------------------------------------
    workFlowTableFields: {
        stateCode: "statecode"
    }
};
