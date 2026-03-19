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

SS.MSDYN.LGIntelliware.WR.TaxiLicence = {
  // Handles form onLoad event
  onLoad: function (executionContext) {
    try {
      //Show/hide relevant tabs based on service configuration
      SS.MSDYN.LGIntelliware.WR.TaxiLicence.configureFormByServiceConfigurationValue(
        executionContext
      );
      //Set business process flow based on service configuration
      SS.MSDYN.LGIntelliware.WR.TaxiLicence.serviceBasedBPF(executionContext);
      //Register event on business process flow stage change
      SS.MSDYN.LGIntelliware.WR.TaxiLicence.registerAddOnStageChangeEvent(executionContext);
      //Badgenumber column validation 
      SS.MSDYN.LGIntelliware.WR.TaxiLicence.badgeNumberValidation(executionContext);
      //Meeting Date column validation 
      SS.MSDYN.LGIntelliware.WR.TaxiLicence.meetingDateValidation(executionContext);
      //Handle business process flow status change
      SS.MSDYN.LGIntelliware.WR.TaxiLicence.handleBpfCompletionStatusChange(executionContext);
      //Lock business process flow fields
      SS.MSDYN.LGIntelliware.WR.TaxiLicence.lockBPFFields(executionContext);
      //Set meeting time format
      const formContext = executionContext.getFormContext();
      const meetingTimeControl = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBpfFields.meetingTime);
      if (meetingTimeControl) {
        const meetingTimeAttribute = meetingTimeControl.getAttribute();
        // Run once on load
        SS.MSDYN.LGIntelliware.WR.TaxiLicence.SetMeetingTimeFormat(meetingTimeControl, meetingTimeAttribute);
        // Attach onchange handler correctly
        meetingTimeAttribute.addOnChange(function () {
          SS.MSDYN.LGIntelliware.WR.TaxiLicence.SetMeetingTimeFormat(meetingTimeControl, meetingTimeAttribute);
        });
      }

      //Show hide reason for decling meeting
      const availabilityOfMeetingControl = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.kindlyConfirmYourAvailabilityForTheMeeting);
      const reasonForDecliningMeetingControl = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.reasonForDecliningMeeting);
      if (availabilityOfMeetingControl && reasonForDecliningMeetingControl) {
        const availabilityOfMeetingAttribute = availabilityOfMeetingControl.getAttribute();
        // Run once on load
        SS.MSDYN.LGIntelliware.WR.TaxiLicence.showHideReasonForDecliningMeeting(availabilityOfMeetingAttribute, reasonForDecliningMeetingControl);
        // Attach onchange handler correctly
        availabilityOfMeetingAttribute.addOnChange(function () {
          SS.MSDYN.LGIntelliware.WR.TaxiLicence.showHideReasonForDecliningMeeting(availabilityOfMeetingAttribute, reasonForDecliningMeetingControl);
        });
      }
      //Show hide rppsc stage notification
      SS.MSDYN.LGIntelliware.WR.TaxiLicence.showHideRPPSCStageNotification(executionContext);
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  //Show form and fields based on service configuration value
  configureFormByServiceConfigurationValue: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      let configControl = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceHeaderFields.headerServiceConfiguration);

      if (!configControl) {
        // If service configuration field is missing, log error and exit
        SS.MSDYN.LGIntelliware.WR.Common.showError("Service configuration missing.", false);
        return;
      }
      //Get configControl value
      let value = configControl.getAttribute().getValue();
      if (value && value.length > 0 && value[0].name) {
        let serviceName = value[0].name.toLowerCase();
        // Handle taxi driver licence configuration
        if (serviceName == SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.taxiDriverLicence) {
          SS.MSDYN.LGIntelliware.WR.TaxiLicence.handleTaxiDriverLicence(executionContext);

        }
        // Handle private hire operator configuration
        if (serviceName == SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.privateHireOperator) {
          SS.MSDYN.LGIntelliware.WR.TaxiLicence.handlePrivateHireOperator(executionContext);

        }
        // Handle private hire vehicle configuration
        if (serviceName == SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.privateHireVehicle) {
          SS.MSDYN.LGIntelliware.WR.TaxiLicence.handleHackneyCarriageOrPrivateHire(executionContext);

        }
        // Handle hackney carriage vehicle configuration

        if (serviceName == SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.hackneyCarriageVehicle) {
          SS.MSDYN.LGIntelliware.WR.TaxiLicence.handleHackneyCarriageOrPrivateHire(executionContext);

        }
        // Handle notification of convictions configuration
        if (serviceName == SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.notificationOfConvictions) {
          SS.MSDYN.LGIntelliware.WR.TaxiLicence.handleNotificationOfConvictions(executionContext);

        }
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  //Handle taxi driver licence form
  handleTaxiDriverLicence: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      //Hide or show tabs 
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.taxiDriverLicence, true);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.mot, false);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.underReview, true);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.referredToPublicProtectionSubCommittee, true);

      // Disable fields that should not be editable by the user
      SS.MSDYN.LGIntelliware.WR.Common.disableFields(formContext, [
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.group2MedicalForm,
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.certificateOfGoodConduct,
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.dbsCertificate,
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.disabilityEqualityCertificate,
      ]);

      // Hide all empty fields in the taxi driver licence tab exclude the office use only section from hiding
      SS.MSDYN.LGIntelliware.WR.Common.hideEmptyFieldsInTab(formContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.taxiDriverLicence, [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableSections.officeUseOnly]);
      // Hide any subgrids in the taxi driver licence tab that have no records
      SS.MSDYN.LGIntelliware.WR.Common.hideEmptySubgridsInTab(formContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.taxiDriverLicence);
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  //Handle hackney carriage or private hire vehicle form
  handleHackneyCarriageOrPrivateHire: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      //Hide or show tabs 
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.hackneyCarriageAndPrivateHire, true);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.dvlaDriverData, false);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.dvlaVehicleDetails, false);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.underReview, true);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.referredToPublicProtectionSubCommittee, true);
      // Disable fields that should not be editable by the user
      SS.MSDYN.LGIntelliware.WR.Common.disableFields(formContext, [
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.group2MedicalForm,
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.garageTestPassCertificate,
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.taximeterReport,
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.publicHireInsuranceCertificate,
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.conversionCertificate,
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.v5Logbookv5Slip,
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.privateHireInsuranceCertificate,
      ]);

      // Hide all empty fields 
      SS.MSDYN.LGIntelliware.WR.Common.hideEmptyFieldsInTab(formContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.hackneyCarriageAndPrivateHire);
      // Hide any subgrids that have no records
      SS.MSDYN.LGIntelliware.WR.Common.hideEmptySubgridsInTab(formContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.hackneyCarriageAndPrivateHire);
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  //Handle private hire operator form
  handlePrivateHireOperator: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      //Hide or show tabs 
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.dvlaDriverData, false);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.dvlaVehicleDetails, false);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.privateHireOperator, true);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.dbs, true);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.mot, false);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.underReview, true);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.referredToPublicProtectionSubCommittee, true);
      // Disable fields that should not be editable by the user
      SS.MSDYN.LGIntelliware.WR.Common.disableFields(formContext, [
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.premisesPublicLiabilityInsurance,
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.basicDisclosureCertificate,
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.disabilityEqualitySafeguardingTraining,
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.fleetInsuranceWithScheduleOfVehicles,
      ]);
      // Hide any empty fields and subgrids 
      SS.MSDYN.LGIntelliware.WR.Common.hideEmptyFieldsInTab(formContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.privateHireOperator);
      SS.MSDYN.LGIntelliware.WR.Common.hideEmptySubgridsInTab(formContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.privateHireOperator);

      // Additional pho section logic based on are you applying as question
      let areYouApplyingAs = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.additionalQuestionsApplyingAs)?.getValue();
      // Get relevant sections for visibility control
      const companyDetails = formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.privateHireOperator).sections.get(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableSections.companyDetails);
      const detailsJointApplicants = formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.privateHireOperator).sections.get(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableSections.jointApplicants);
      const individualApplicants = formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.privateHireOperator).sections.get(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableSections.individualApplicants);
      const evidenceRightToWork = formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.privateHireOperator).sections.get(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableSections.evidenceRightToWork);
      const evidenceRightToWorkJointOwner = formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.privateHireOperator).sections.get(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableSections.evidenceRightToWorkJointOwner);
      // Control section visibility based on the applicant type
      switch (areYouApplyingAs) {
        case 1:
          companyDetails?.setVisible(false);
          detailsJointApplicants?.setVisible(false);
          evidenceRightToWorkJointOwner?.setVisible(false);
          break;
        case 2:
          companyDetails?.setVisible(false);
          individualApplicants?.setVisible(false);
          evidenceRightToWork?.setVisible(false);
          break;
        case 3:
          detailsJointApplicants?.setVisible(false);
          evidenceRightToWorkJointOwner?.setVisible(false);
          evidenceRightToWork?.setVisible(false);
          break;
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  //Handle notification of conviction form
  handleNotificationOfConvictions: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      // Show/hide tabs
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.notificationOfConvictions, true);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.dvlaDriverData, false);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.dvlaVehicleDetails, false);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.dbs, false);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.mot, false);
      SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.pay360, false);

      const radio = formContext.getAttribute(
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.whatAreYouNotifyingUsOf
      );
      // Get the radio button attribute for what are you notifying us of?

      if (radio) {
        // Attach change event handle what are you notifying us of
        radio.controls.forEach((c) => c.addOnChange(() => {
          SS.MSDYN.LGIntelliware.WR.TaxiLicence.handleWhatAreYoNotifyingUsOf(executionContext);
        }));

        // Run toggle once on form load to set initial visibility
        SS.MSDYN.LGIntelliware.WR.TaxiLicence.handleWhatAreYoNotifyingUsOf(executionContext);
      }
    }
    catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  // Toggle visibility of sections within the notification of convictions tab
  handleWhatAreYoNotifyingUsOf: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      const radio = formContext.getAttribute(
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.whatAreYouNotifyingUsOf
      );

      const tabName = SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.notificationOfConvictions;
      const sections = SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableSections;
      let selected = radio.getValue();
      // Hide sections initially
      SS.MSDYN.LGIntelliware.WR.Common.tabSectionVisible(formContext, tabName, sections.motoringOffenceDetailsSection, false);
      SS.MSDYN.LGIntelliware.WR.Common.tabSectionVisible(formContext, tabName, sections.cautionOrWarningDetails, false);
      SS.MSDYN.LGIntelliware.WR.Common.tabSectionVisible(formContext, tabName, sections.questionedORInterviewedDetails, false);
      SS.MSDYN.LGIntelliware.WR.Common.tabSectionVisible(formContext, tabName, sections.convictionsDetails, false);
      SS.MSDYN.LGIntelliware.WR.Common.tabSectionVisible(formContext, tabName, sections.licenceDetails, false);

      // Show relevant section based on radio button selection
      if (selected) {
        switch (selected.toString()) {
          case "1": // Motoring Offence
            SS.MSDYN.LGIntelliware.WR.Common.hideEmptyFieldsInTab(formContext, tabName, [
              sections.cautionOrWarningDetails,
              sections.questionedORInterviewedDetails,
              sections.convictionsDetails
            ]);
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.dvlaDriverData, true);
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.dvlaVehicleDetails, true);
            break;

          case "2": // Caution / Warning
            SS.MSDYN.LGIntelliware.WR.Common.hideEmptyFieldsInTab(formContext, tabName, [
              sections.motoringOffenceDetailsSection,
              sections.questionedORInterviewedDetails,
              sections.convictionsDetails
            ]);
            break;

          case "3": // Police Interview
            SS.MSDYN.LGIntelliware.WR.Common.hideEmptyFieldsInTab(formContext, tabName, [
              sections.motoringOffenceDetailsSection,
              sections.cautionOrWarningDetails,
              sections.convictionsDetails
            ]);
            break;

          case "4": // Convictions
            SS.MSDYN.LGIntelliware.WR.Common.hideEmptyFieldsInTab(formContext, tabName, [
              sections.motoringOffenceDetailsSection,
              sections.cautionOrWarningDetails,
              sections.questionedORInterviewedDetails
            ]);
            break;
        }
      }

      SS.MSDYN.LGIntelliware.WR.Common.hideEmptyFieldsInTab(formContext, tabName, [
        sections.motoringOffenceDetailsSection,
        sections.cautionOrWarningDetails,
        sections.questionedORInterviewedDetails,
        sections.convictionsDetails
      ]);

      // Hide empty subgrids in Notification of Convictions
      SS.MSDYN.LGIntelliware.WR.Common.hideEmptySubgridsInTab(formContext, tabName);

      // Show/hide radio itself
      formContext.getControl(
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.whatAreYouNotifyingUsOf
      ).setVisible(!!selected);
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  //Retrieves the Business Process Flow (BPF) ID by its unique name.
  getBPFIdByName: function (bpfName, callback) {
    try {
      // Build the web api query to fetch the workflow with the specified unique name and active state
      let query = `?$select=workflowid&$filter=uniquename eq '${bpfName}' and ${SS.MSDYN.LGIntelliware.WR.Constants.workFlowTableFields.stateCode} eq ${SS.MSDYN.LGIntelliware.WR.Constants.workFlowTableStateCode.active}`;
      // Execute the web api request to retrieve multiple records from the workflow entity
      Xrm.WebApi.retrieveMultipleRecords("workflow", query).then(
        function success(results) {
          // Check if any workflow records were returned
          if (results.entities.length > 0) {
            // Get the workflow ID of the first matching workflow
            let workflowId = results.entities[0].workflowid;
            callback(workflowId);
          } else {
            callback(null);
          }
        },
        function (e) {
          callback(null);
          SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
      );
    }
    catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  // //Activates the appropriate business process flow based on the selected service configuration.
  serviceBasedBPF: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      if (formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceHeaderFields.headerServiceConfiguration)) {
        let serviceConfigurationLookup = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceHeaderFields.headerServiceConfiguration).getAttribute();
        let lookupValue = serviceConfigurationLookup.getValue();
        // Proceed only if a service configuration is selected
        if (lookupValue && lookupValue.length > 0 && lookupValue[0].name) {
          let serviceConfigurationName = lookupValue[0].name.toLowerCase();
          // Map service configuration names to corresponding BPF names
          const bpfMapping = {
            [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.taxiDriverLicence]: SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFNames.taxiDriverLicence,
            [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.privateHireOperator]: SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFNames.privateHireOperator,
            [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.notificationOfConvictions]: SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFNames.notificationOfConvictions,
            [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.hackneyCarriageVehicle]: SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFNames.hackneyCarriageVehicle,
            [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.privateHireVehicle]: SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFNames.privateHireVehicle
          };
          let bpfName = bpfMapping[serviceConfigurationName] || SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFNames.hackneyCarriageVehicle;
          SS.MSDYN.LGIntelliware.WR.TaxiLicence.getBPFIdByName(bpfName, function (bpfId) {
            if (bpfId) {
              formContext.data.process.setActiveProcess(bpfId, function (status) {
                if (status === "success") {
                  SS.MSDYN.LGIntelliware.WR.TaxiLicence.tabFocusOnBPFStageChange(executionContext);
                }
              });
            }
          });
        }
      }
    }
    catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  // Register an event handler for business process flow  stage changes
  registerAddOnStageChangeEvent: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      // Attach the stage change event to the current bpf instance
      formContext.data.process.addOnStageChange(function (stageContext) {
        SS.MSDYN.LGIntelliware.WR.TaxiLicence.tabFocusOnBPFStageChange(stageContext);
      });

      formContext.data.process.addOnStageChange(function (stageContext) {
        SS.MSDYN.LGIntelliware.WR.TaxiLicence.showHideRPPSCStageNotification(executionContext);
      });
    }
    catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  // Handles tab focus and status updates when the business process flow stage changes
  tabFocusOnBPFStageChange: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      let activeProcess = formContext.data.process.getActiveProcess();
      let activeStage = formContext.data.process.getActiveStage();
      if (activeProcess && activeStage) {
        // Determine the corresponding tab for the active stage
        let processStageName = activeStage.getName();
        let applicationReview = '';
        // Get service configuration value to map Review Application" stage
        if (formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceHeaderFields.headerServiceConfiguration)) {
          let serviceConfigurationLookup = formContext
            .getControl(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceHeaderFields.headerServiceConfiguration)
            .getAttribute();
          if (
            serviceConfigurationLookup.getValue() &&
            serviceConfigurationLookup.getValue().length > 0 &&
            serviceConfigurationLookup.getValue()[0].name
          ) {
            let serviceConfigurationName = serviceConfigurationLookup
              .getValue()[0]
              .name.toLowerCase();
            const serviceConfigurationMap = {
              [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.taxiDriverLicence]:
                SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.taxiDriverLicence,
              [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.hackneyCarriageVehicle]:
                SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.hackneyCarriageAndPrivateHire,
              [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.privateHireVehicle]:
                SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.hackneyCarriageAndPrivateHire,
              [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.privateHireOperator]:
                SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.privateHireOperator,
              [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.notificationOfConvictions]:
                SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.notificationOfConvictions,
            };
            applicationReview = serviceConfigurationMap[serviceConfigurationName] || '';
          }
        }
        // Map all bpf stages to their corresponding tabs
        let stageToTabMap = {
          [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.reviewApplication]: applicationReview,
          [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.dvlaDriverDataReview]:
            SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.dvlaDriverData,
          [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.dvlaVehicleDetailsReview]:
            SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.dvlaVehicleDetails,
          [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.dbsReview]:
            SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.dbs,
          [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.paymentReview]:
            SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.pay360,
          [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.reviewMOTHistory]:
            SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.mot,
          [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.underReview]:
            SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.underReview,
          [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.referredToPublicProtectionSubCommittee]:
            SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.referredToPublicProtectionSubCommittee,
          [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.grant]:
            SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.summary,
          [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.reject]:
            SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableTabs.summary
        };
        // Update statusCode/stateCode for specific stages
        if (activeStage.getName() == SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.reviewMOTHistory) {
          formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.active);
          formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceStatusCode.motHistoryVerification);
        }
        else if (activeStage.getName() == SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.paymentReview) {
          formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.active);
          formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceStatusCode.paymentPending);
        }
        else if (activeStage.getName() == SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.grantOrReject) {
          formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.active);
          formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceStatusCode.paid);
        }
        else if (activeStage.getName() == SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.underReview) {
          formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.active);
          formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceStatusCode.underReview);
        }
        else if (activeStage.getName() == SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.referredToPublicProtectionSubCommittee) {
          formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.active);
          formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceStatusCode.referredToPublicProtectionSubCommittee);
        }
        // Save and refresh form to reflect updates
        formContext.data.save().then(() => {
          formContext.data.refresh();
        }).catch((e) => {
          SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        });
        // Focus the corresponding tab for the active stage
        let tabName = stageToTabMap[processStageName.trim()];
        if (tabName !== undefined) {
          let tab = formContext.ui.tabs.get(tabName);
          if (tab) {
            tab.setFocus();
          }
        }
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  //Validation on badge number column
  badgeNumberValidation: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      let badgeNumber = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBpfFields.headerBadgeNumber);
      if (badgeNumber) {
        badgeNumber.getAttribute().addOnChange(function () {
          let value = badgeNumber.getAttribute().getValue();
          if (value && !/^\d*$/.test(value)) {
            badgeNumber.setNotification("Only numeric values are allowed.");
          } else {
            badgeNumber.clearNotification();
          }
        });
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  // Validation on meeting date column
  meetingDateValidation: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      let meetingDateControl = formContext.getControl(
        SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBpfFields.meetingDate
      );

      if (meetingDateControl) {
        let meetingDateAttr = meetingDateControl.getAttribute();
        meetingDateAttr.addOnChange(function () {
          let value = meetingDateAttr.getValue();
          let today = new Date();
          today.setHours(0, 0, 0, 0);

          if (value) {
            let meetingDateOnly = new Date(value);
            meetingDateOnly.setHours(0, 0, 0, 0);

            if (meetingDateOnly < today) {
              meetingDateControl.setNotification(
                SS.MSDYN.LGIntelliware.WR.Constants.TaxiLicenceTableFieldErrorMessage.meetingDateNotificationMessage
              );
            } else {
              meetingDateControl.clearNotification();
            }
          } else {
            meetingDateControl.clearNotification();
          }
        });
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  //Lock bpf fields 
  lockBPFFields: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      let bpfFieldNames = [SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBpfFields.paid, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBpfFields.whatAreYouNotifying];
      bpfFieldNames.forEach(function (fieldName) {
        let bpfControl = formContext.getControl(fieldName);
        if (bpfControl) {
          bpfControl.setDisabled(true);
        }
      });
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  //Record status change on business process flow completion
  handleBpfCompletionStatusChange: function (executionContext) {
    try {
      var formContext = executionContext.getFormContext();
      var process = formContext.data.process;
      // Register event handler for process status changes
      process.addOnProcessStatusChange(function () {
        if (process.getStatus() === SS.MSDYN.LGIntelliware.WR.Constants.bpfStatus.finished) {
          let serviceConfigurationLookup = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.serviceConfiguration);

          if (serviceConfigurationLookup && serviceConfigurationLookup.getValue()?.length > 0) {
            let serviceConfigurationName = serviceConfigurationLookup.getValue()[0].name.toLowerCase();
            // Handle notification of convictions service 
            if (serviceConfigurationName === SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceServiceConfiguration.notificationOfConvictions) {
              formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.inactive);
              formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceStatusCode.ClosedOrCompleted);
              return;
            } else {
              // Handle other service configurations based on grant/reject decision
              let grantReject = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.grantReject)?.getValue();
              if (grantReject === 0) {
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.inactive);
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceStatusCode.closedORRejected);
                formContext.data.entity.save("save");
                return;
              } else if (grantReject === 1) {
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.inactive);
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceStatusCode.granted);
                formContext.data.entity.save("save");
                return;
              }
              let grantRejectRPPSC = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.grantRejectRPPSC)?.getValue();
              if (grantRejectRPPSC === 0) {
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.inactive);
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceStatusCode.granted);
                formContext.data.entity.save("save");
                return;
              }
              else if (grantRejectRPPSC === 1) {
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.inactive);
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceStatusCode.closedORRejected);
                formContext.data.entity.save("save");
                return;
              }
              let GranthWarningORRefuse = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.GrantWithWarningORRefuse)?.getValue();
              if (GranthWarningORRefuse === 0) {
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.inactive);
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceStatusCode.GrantWithWarning);
                formContext.data.entity.save("save");
                return;
              }
              else if (GranthWarningORRefuse === 1) {
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.inactive);
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceStatusCode.Refuse);
                formContext.data.entity.save("save");
                return;
              }
            }
            
          }
        }
      });
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  SetMeetingTimeFormat: function (meetingTimeControl, meetingTimeAttribute) {
    try {
      // Regular expression for 12-hour time format
      const regex12Hour = /^(0?[1-9]|1[0-2]):[0-5][0-9](am|pm)$/i;
      const meetingTimeValue = meetingTimeAttribute ? meetingTimeAttribute.getValue() : null;

      if (meetingTimeValue && !regex12Hour.test(meetingTimeValue)) {
        meetingTimeControl.setNotification(
          SS.MSDYN.LGIntelliware.WR.Constants.TaxiLicenceTableFieldErrorMessage.meetingTimeFormatText,
          SS.MSDYN.LGIntelliware.WR.Constants.TaxiLicenceTableFieldErrorMessage.meetingTimeNotificationControl
        );
      } else {
        meetingTimeControl.clearNotification(SS.MSDYN.LGIntelliware.WR.Constants.TaxiLicenceTableFieldErrorMessage.meetingTimeNotificationControl);
      }

    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  ShowHideRibbonLZ0Button: function (primaryControl) {
    try {
      const formContext = primaryControl;
      const process = formContext.data.process;
      if (!process) return false;
      const activeStage = process.getActiveStage();
      const meetingTimeControl = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBpfFields.meetingTime);
      const meetingDateControl = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBpfFields.meetingDate);

      if (activeStage && meetingTimeControl && meetingDateControl) {
        const activeStageName = activeStage.getName();
        const meetingTimeValue = meetingTimeControl.getAttribute()?.getValue();
        const meetingDateValue = meetingDateControl.getAttribute()?.getValue();

        if (
          activeStageName === SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.referredToPublicProtectionSubCommittee &&
          meetingTimeValue != null &&
          meetingDateValue != null
        ) {
          return true;
        }
        else {
          return false;
        }
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  sendLZ0LetterButtonForm: function (primaryControl) {
    try {
      let formContext = primaryControl;
      // Confirmation dialog text and configuration
      let confirmStrings = { text: SS.MSDYN.LGIntelliware.WR.Constants.TaxiLicenceTableDialogueConfiguration.sendLZ0LetterText, title: SS.MSDYN.LGIntelliware.WR.Constants.TaxiLicenceTableDialogueConfiguration.sendLZ0LetterTitle };
      let confirmOptions = { height: SS.MSDYN.LGIntelliware.WR.Constants.TaxiLicenceTableDialogueConfiguration.sendLZ0LetterHeight, width: SS.MSDYN.LGIntelliware.WR.Constants.TaxiLicenceTableDialogueConfiguration.sendLZ0LetterWidth };
      // Open confirmation dialog
      Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
        function (success) {
          if (success.confirmed) {
            // User confirmed Show progress indicator
            SS.MSDYN.LGIntelliware.WR.Common.showProgressIndicator();
            let entityId = formContext.data.entity.getId().replace("{", "").replace("}", "");
            // Execute bartec municipal: check service request status custom action
            let execute_ss_SendLZ0Letter_Request = {
              // Parameters
              entity: { entityType: SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.taxiLicenceLogicalName, id: entityId },
              // Metadata for custom action call
              getMetadata: function () {
                return {
                  boundParameter: SS.MSDYN.LGIntelliware.WR.Constants.customActionParameter.boundParameter,
                  parameterTypes: {
                    entity: { typeName: SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableCustomAction.typeName, structuralProperty: SS.MSDYN.LGIntelliware.WR.Constants.customActionParameter.structuralProperty }
                  },
                  operationType: SS.MSDYN.LGIntelliware.WR.Constants.customActionParameter.operationType, operationName: SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableCustomAction.taxiLicenceSendLZ0Letter
                };
              }
            };
            // Execute the custom action using Web API
            Xrm.WebApi.execute(execute_ss_SendLZ0Letter_Request).then(
              function success(response) {
                if (response.ok) {
                  SS.MSDYN.LGIntelliware.WR.Common.hideProgressIndicator();
                }
              }
            ).catch(function (e) {
              // Hide progress and show error if action fails
              SS.MSDYN.LGIntelliware.WR.Common.hideProgressIndicator();
              SS.MSDYN.LGIntelliware.WR.Common.showError(e.message, false);
            });
          }
        }
      );
    }
    catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  showHideReasonForDecliningMeeting: function (availabilityOfMeetingAttribute, reasonForDecliningMeetingControl) {
    try {

      const availabilityValue = availabilityOfMeetingAttribute.getValue();
      if (availabilityValue == SS.MSDYN.LGIntelliware.WR.Constants.radioOptionSet.yes || availabilityValue == null) {
        SS.MSDYN.LGIntelliware.WR.Common.showHideField(reasonForDecliningMeetingControl, false);
      } else {
        SS.MSDYN.LGIntelliware.WR.Common.showHideField(reasonForDecliningMeetingControl, true);
      }

    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  showHideRPPSCStageNotification: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      let process = formContext.data.process;
      if (!process) {
        formContext.ui.clearFormNotification(SS.MSDYN.LGIntelliware.WR.Constants.TaxiLicenceTableFormMessage.rppscNotificationId);
        return;
      }
      if (formContext.data.process.getActiveStage().getName() == SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceBPFStage.referredToPublicProtectionSubCommittee) {
        // Show notification only for this specific stage
        formContext.ui.setFormNotification(
          SS.MSDYN.LGIntelliware.WR.Constants.TaxiLicenceTableFormMessage.rppscStageMessage,
          SS.MSDYN.LGIntelliware.WR.Constants.formNotificationLevel.info,
          SS.MSDYN.LGIntelliware.WR.Constants.TaxiLicenceTableFormMessage.rppscNotificationId
        );
      } else {
        formContext.ui.clearFormNotification(SS.MSDYN.LGIntelliware.WR.Constants.TaxiLicenceTableFormMessage.rppscNotificationId);
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
};
