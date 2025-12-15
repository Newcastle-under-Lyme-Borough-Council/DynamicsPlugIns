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

    //----------------------------------------------------------------Licence Service------------------------------------------------------
    //table fields
    licenseServiceTableFields: {
        licenseServiceSchemaName: "ss_LicenseService",
        licenseServiceId: "ss_licenseserviceid",
        licenseServiceLogicalName: "ss_licenseservice",
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
        updateRequired: "ss_arethereanyupdates",
        hastheRepresentationsRecieved: "ss_hastherepresentationbeenreceived",
        grantReject: "ss_grantreject",
        status: "statecode",
        statusReason: "statuscode",
        unconditionalGrant: "ss_unconditionalgrant",
        meetingCompleted: "ss_meetingcompleted",
        applicantFirstName: "ss_applicantfirstname",
        applicantLastName: "ss_applicantlastname",
        applicantEmail: "ss_applicantemail",
        applicantMainPhone: "ss_applicanttelephonenumber",
        applicantOtherPhone: "ss_othertelephonenumber",
        licenceIssueDate: "ss_issuedate",
        licenceExpiryDate: "ss_expirydate",
        previousName: "ss_haveyouhadanypreviousormaidennames",
        ordinarilyResident: "ss_isyourordinaryaddressyourcorrespondaddress",
        forfeitedPersonalLicense: "ss_wasanypersonallicenceforfeitedinpast5years",
        convictionOfOffence: "ss_convictedofanyrelevantorforeignoffence",
        refferedToCommittee: "ss_doesthecaseneedtobereferredtothecommittee"
    },
    //table form sections
    licenseServiceTableSections: {
        //Pavement Form
        applicantContactDetails: "tab_AgentDetails_section_ApplicantContactDetails",
        //Club Premises Form
        applicantDetails: "tab_General_section_ApplicantDetails",
        applicantBusiness: "tab_General_section_ApplicantBusiness",
        applicantBusinessAddress: "tab_General_section_BusinessAddress",
        applicantBusinessAddressLookup: "tab_general_section_AddressLookup",
        agentDetails: "tab_General_section_AgentDetails",
        agentBusiness: "tab_General_section_AgentBusiness",
        agentBusinessAddress: "tab_General_section_AgentBusinessAddress",
        agentBusinessAddressLookup: "tab_general_section_BusinessAddressLookup",
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
        //personal
        applicantNameHistory: "tab_PersonalDetail_section_applicantnamehistory",
        correspondenceAddress: "tab_personaldetail_section_CorrespondenceAddress",
        forfeitureCases: "tab_ForfeitureOfPersonalLicense_section_ForfeitureCases",
        releventOffences: "tab_ForfeitureOfPersonalLicense_section_RelevantorForeignOffences"
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
        clubHours: "tab_ClubHours",
        //personal
        personalDetails: "tab_PersonalDetail",
        forfeiture: "tab_ForfeitureOfPersonalLicense"
    },
    //service configuration lookup values
    licenceServiceServiceConfiguration: {
        pavement: "licence service - pavement",
        clubPremises: "licence service - club premises",
        personal: "licence service - personal",
        premises: "licence service - premises"
    },
    //header fields
    licenceServiceHeaderFields: {
        serviceConfiguration: "ss_serviceconfiguration"
    },
    //form names
    licenceServiceFormNames: {
        pavement: "back office - pavement licence service",
        clubPremises: "back office - club premises certificate",
        personal: "back office - personal licence service",
        premises: "back office - premises licence service"
    },
    //BPF names
    licenseServiceBPFNames: {
        pavement: "ss_pavementlicensebpf",
        clubPremises: "ss_clubpremisescertificatebpf",
        personal: "ss_personallicenseservicebpf",
        premises: "ss_premiseslicenseservicebpf"
    },

    //Licence service table bpf stages
    licenceServiceBPFStage: {
        consultationPeriod: "Consultation Period",
        verification: "Verification And Confirmation",
        review: "Review",
        meeting: "Meeting",
        grantReject: "Grant/Rejected",
        passApplicationToCommittee: "Pass Application To Committee",
        reviewConvictions: "Review Convictions",
        updatesRequired: "Updates Required"
    },
    //licnece service bpf fields
    licenceServiceBpfFields: {
        capacity: "header_process_ss_capacityareyouapplyingfor",
        pavementLicenceNumber: "header_process_ss_pavementlicensenumber",
        pavementLicenceNumber1: "header_process_ss_pavementlicensenumber_1",
        premisesLicenceNumber: "header_process_ss_premiseslicensenumber",
        premisesLicenceNumber1: "header_process_ss_premiseslicensenumber_1",
        personalLicenceNumber: "header_process_ss_personallicensenumber",
        clubLicenceNumber: "header_process_ss_clublicensenumber",
        clubLicenceNumber1: "header_process_ss_clublicensenumber_1",
        personalLicenceNumber: "header_process_ss_personallicensenumber",
        personalLicenceNumber1: "header_process_ss_personallicensenumber_1",
        licenceIssueDate: "header_process_ss_issuedate",
        licenceExpiryDate: "header_process_ss_expirydate",
        licenceIssueDate1: "header_process_ss_issuedate_1",
        licenceExpiryDate1: "header_process_ss_expirydate_1",
        refferedToCommittee: "header_process_ss_doesthecaseneedtobereferredtothecommittee",
        meetingCompleted: "header_process_ss_meetingcompleted"

    },
    //Taxi licence table statuscodes
    LicenceServiceStatusCode: {
        submitted: 717800005,
        inProgress: 1,
        awaitingInformation: 717800003,
        rejected: 717800008,
        granted: 717800007,
        refferedToCommittee: 717800006

    },

    //License Provision table fields
    licenceProvisionTableFields: {
        provisionType: "ss_provisiontype",
        saleOfAlcohol: "ss_willthesaleofalcoholbeforconsumption",
        descriptionOfEntertainment: "ss_descriptionofentertainment",
        stateOfActivity: "ss_statetypeofactivity",
        provisionPlace: "ss_provisionplaces",
    },
    //license provision form sections
    licenceProvisionFormSections: {
        supervisor: "tab_General_section_Supervisor",
        supervisorConsent: "tab_General_section_SupervisorConsent"
    },
    //license provision form tabs
    licenceProvisionFormTabs: {
        general: "tab_General"
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
        grantReject: "ss_grantreject",
        grantRejectRPPSC: "ss_grantrejectrppsc",
        GrantWithWarningORRefuse: "ss_grantwithwarningrefuse",
        kindlyConfirmYourAvailabilityForTheMeeting: "ss_kindlyconfirmyouravailabilityforthemeeting",
        reasonForDecliningMeeting: "ss_reasonfordecliningmeeting"
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
        underReview: "tab_under_review",
        referredToPublicProtectionSubCommittee: "tab_referred_to_public_protection_sub_committee",
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
        underReview: "Under Review",
        referredToPublicProtectionSubCommittee: "Referred To Public Protection Sub-Committee",
        grant: "Grant",
        reject: "Reject",
        grantOrReject: "Grant / Reject / Under Review",
    },

    //Taxi licence bpf fields
    taxiLicenceBpfFields: {
        headerBadgeNumber: "header_process_ss_newbadgenumber",
        paid: "header_process_ss_paid",
        whatAreYouNotifying: "header_process_ss_whatareyounotifyingusoff",
        meetingTime: "header_process_ss_meetingtime",
        meetingDate: "header_process_ss_meetingdate",
        reasonForGrant: "header_process_ss_reasonforgrantlicence",
        grantWithWarningOrRefuse: "header_process_ss_grantwithwarningrefuse"
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
        underReview: 717800008,
        referredToPublicProtectionSubCommittee: 717800009,
        closedORRejected: 2,
        granted: 717800002,
        ClosedOrCompleted: 717800004,
        grantWithWarning: 717800012,
        GrantWithWarning: 717800012,
        Refuse: 717800011

    },

    //Taxi licence service configuration lookup values
    taxiLicenceServiceConfiguration: {
        taxiDriverLicence: "taxi licence - taxi driver licence",
        hackneyCarriageVehicle: "taxi licence - hackney carriage vehicle",
        privateHireVehicle: "taxi licence - private hire vehicle",
        privateHireOperator: "taxi licence - private hire operator",
        notificationOfConvictions: "taxi licence - notification of convictions and offences"
    },

    //Taxi licence table field message
    TaxiLicenceTableFieldErrorMessage: {
        meetingTimeFormatText: "Please enter time in HH:MMam/pm format (12-hour)",
        meetingTimeNotificationControl: "meeting_time_format_error",
        meetingDateNotificationMessage: "Meeting date should be today or a future date.",
    },
    //TaxiLicence table dialogue configurations 
    TaxiLicenceTableDialogueConfiguration: {
        sendLZ0LetterText: "Do you want to send LZ0 letter? You can't undo this action.",
        sendLZ0LetterTitle: "Confirm Send LZ0 Letter",
        sendLZ0LetterHeight: 200,
        sendLZ0LetterWidth: 450,
    },

    //TaxiLicence table form level messages  
    TaxiLicenceTableFormMessage: {
        rppscStageMessage: "Please add meeting date and time and save to send LZ0 letter using the Send LZ0 Letter button.",
        rppscNotificationId: "rppscStageNotifificationId"
    },

    //TaxiLicence custom action
    taxiLicenceTableCustomAction: {
        taxiLicenceSendLZ0Letter: "ss_TaxiLicenceSendLZ0Letter",
        typeName: "mscrm.ss_taxilicence"
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
        disputeEscalate: "ss_disputeescalate",
        stateCode: "statecode",
        statusCode: "statuscode",
        crew:"ss_crew",
        case:"ss_incidentid"

    },

    //Missedbin choice fields
    missedBinChoiceFields: {
        dispute: 0,
        escalate: 1,
        newCollectionRequired:1,
        newCollectionNoRequired:0,
        hasTheCaseBeenResolved:1,
        hasTheCaseNotResolved:0
    },
    //Missedbin table dialogue configurations 
    missedBinTableDialogueConfiguration: {
        bartecMunicipalCheckServiceRequestStatusText: "Do you want to Check Status in Bartec Municipal? You can't undo this action.",
        bartecMunicipalCheckServiceRequestStatusTitle: "Confirm Bartec Municipal Check Status",
        bartecMunicipalCheckServiceRequestStatusHeight: 200,
        bartecMunicipalCheckServiceRequestStatusWidth: 450,
    },

     //Missed bin table bpf names
    missedBinBPFNames: {
        missedBinDispute: "ss_missedbindisputeprocess",
        missedBinEscalate: "ss_missedbinescalationprocess",
       
    },
      //Missed bin bpf fields
    missedBinBpfFields: {
        newCollectionRequired: "ss_newcollectionrequired",
        hasTheCaseBeenResolved:"ss_hasthecasebeenresolved"
     
    },

    //Missed bin table statuscodes
    missedBinStatusCode: {
         noCollectionRequired: 717800008,
         newCollectionRequired:717800009,
         awaitingAssignment:717800007,
         availableEscalation:717800001,
         investigatingDispute:717800004,
         awaitingResolution:717800010,
         closedCompleted:717800000,
         resolved:717800006
    },
    //Missed bin table bpf stages
    missedBinBPFStage: {
        assignAgent: "Assign Agent",
        resolveCase: "Resolve Case",
        emailCustomer:"Email Customer",
        resolveEscalation:"Resolve Escalation"
    },

    //---------------------------------------------------------------Case Table --------------------------------------------------------
   caseTableFields: {
        caseLogicalName: "incident",
        crewSchemaName:"ss_Crew",
        crewLogicalName:"ss_crew",
    },

    //---------------------------------------------------------------Crew Table --------------------------------------------------------
   crewTableFields: {
     crewEntitySetName: "ss_crews"
    },
    //---------------------------------------------------------------Holiday Table -------------------------------------------------------
    bankHolidayTableFields: {
        date: "ss_date"
    },
 //---------------------------------------------------------------Email Table -------------------------------------------------------
    emailTableFields: {
        regarding: "regardingobjectid",
        contact:"_ss_customer_value",
        to:"to"
    },

    //Bank holiday table field error message
    bankHolidayTableFieldErrorMessage: {
        restrictPastDatesText: "Past dates are not allowed. Please select today or a future date."
    },

    //---------------------------------------------------------------Contact Table -------------------------------------------------------
    contactTableFields: {
        contactLogicalName:"contact",
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

    //----------------------------------------------------------------Bartec Events  Table------------------------------------------------------
    //Taxi licence table fields
    bartecEventsTableFields: {
        bartecEventsSchemaName: "ss_bartecevents",
        mainEvent: "ss_associatedevent",
        mainEventName: "ss_associatedeventname",
        associatedEventType: "ss_associatedeventtype"
    },


    //Bartec events form name
    bartecEventsFormName: {
        subEventsForm: "Bartec Sub Events"
    },

    //----------------------------------------------------------------System Form Table------------------------------------------------------
    systemFormTableFields: {
        systemFormLogicalName: "systemform",
        formId: "formid",
        name: "name",
        objectTypeCode: "objecttypecode",
        type: "type"
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

    formlevel: {
        information: 2
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
    },

    //---------------------------------------------------------------- Form Notification Level ------------------------------------------------------
    formNotificationLevel: {
        info: "INFO"
    },

    //---------------------------------------------------------------- Radio options set values ------------------------------------------------------
    radioOptionSet: {
        yes: 1,
        no: 0
    }

};
