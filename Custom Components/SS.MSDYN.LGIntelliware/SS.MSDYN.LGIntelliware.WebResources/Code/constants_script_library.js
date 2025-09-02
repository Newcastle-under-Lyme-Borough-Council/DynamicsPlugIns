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
    //----------------------------------------------------------------Taxi Licence Table------------------------------------------------------
    //Taxi licence table fields
    taxiLicenceTableFields: {
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
        grantReject: "ss_grantreject",
    },
    //Taxi licence table form tabs
    taxiLicenceTableTabs: {
        applicantDetails: "tab_applicant_details",
        notificationOfConvictions: "Notification_of_convictions_details",
        taxiDriverLicence: "tab_detail_sec_taxi_driver_licence",
        mot: "tab_mot",
        hackneyCarriageAndPrivateHire: "tab_detail_sec_hackney_carriage_and_private_hire",
        dvlaDriverData: "tab_dvla_driver_data",
        privateHireOperator: "tab_detail_sec_private_hire_operator",
        dvlaVehicleDetails: "tab_dvla_vehicle_details",
        dbs: "tab_dbs",
        pay360: "tab_pay360",
        summary: "tab_summary",
    },

    //Taxi licence table form sections
    taxiLicenceTableSections: {
        officeUseOnly: "tab_details_sec_office_use_only",
        motoringOffenceDetailsSection: "motorings_offence_details_section_4",
        cautionOrWarningDetails: "caution_or_warning_details_section_5",
        questionedORInterviewedDetails: "questioned_or_interviewed_details_section_6",
        convictionsDetails: "convictions_details_section_7",
        licenceDetails: "licence_details_section_8",
        companyDetails: "tab_details_sec_company_details",
        jointApplicants: "tab_detail_sec_details_of_join_applicants",
        individualApplicants: "tab_detail_section_individual_applicant_details",
        evidenceRightToWork: "tab_detail_sec_evidence_of_right_to_Work_in_the_uk",
        evidenceRightToWorkJointOwner: "tab_detail_sec_evidence_of_right_to_work_in_the_uk_joint"
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
        paid:"header_process_ss_paid",
whatAreYouNotifying:"header_process_ss_whatareyounotifyingusoff"
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
 
    //Bank holiday table dialogue configurations 
    bankHolidayTableFieldErrorMessage: {
        restrictPastDatesText: "Past dates are not allowed. Please select today or a future date."
    },

    //----------------------------------------------------------------Custom Action ------------------------------------------------------
    customActionParameter: {
        boundParameter: "entity",
        operationType: 0,
        structuralProperty: 5
    },

   
    contactTableFields: {
        mobilePhone: "mobilephone"
    },
    customerInteractionTableFields: {
        regardingObject: "regardingobjectid",
        response: "ss_response",
        description: "description",
        subject: "ss_subject",
        customerLookup: "ss_customer",
        serviceConfigurationLookup: "ss_serviceconfigurationid",
        entities: {
            taxiLicence: { table: "ss_taxilicences", id: "ss_taxilicenceid" },
            missedBin: { table: "ss_missedbins", id: "ss_missedbinid" },
            planningPermission: { table: "ss_planningpermissions", id: "ss_planningpermissionid" },
            reportNoise: { table: "ss_reportnoises", id: "ss_reportnoiseid" },
            abandonBin: { table: "ss_abandonbinses", id: "ss_abandonbinsid" },
            abandonedVehicle: { table: "ss_abandonedvehicles", id: "ss_abandonedvehicleid" }
        }
    },

    smsTableFields: {
        scheduledEnd: "header_scheduledend"
    },

    actionName: {
        missedBinBartecStatus: "ss_BartecMunicipalCheckServiceRequestStatus",

    },

    stateCode: {
        active: 0,
        inactive: 1
    },
    bpfStatus: {
        finished: "finished"
    },
    workFlowTableStateCode: {
        active: 1
    },
    workFlowTableFields: {
        stateCode: "statecode"
    }
};
