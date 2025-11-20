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

SS.MSDYN.LGIntelliware.WR.LicenseService = {
    // Handles form onLoad event
    onLoad: function (executionContext) {
        try {
            //Select Form based on service configuration
            SS.MSDYN.LGIntelliware.WR.LicenseService.selectFormByServiceConfigurationValue(executionContext);
            //Set business process flow based on service configuration
            SS.MSDYN.LGIntelliware.WR.LicenseService.serviceBasedBPF(executionContext);

            //register a process status change event
            SS.MSDYN.LGIntelliware.WR.LicenseService.registerAddOnProcessStatusChangeEvent(executionContext);


        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    //Select Form of License Service based on service Configuration
    selectFormByServiceConfigurationValue: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext();
            var configControl = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceHeaderFields.serviceConfiguration);

            if (!configControl) {
                // If service configuration field is missing, log error and exit
                SS.MSDYN.LGIntelliware.WR.Common.showError("Service configuration missing.", false);
                return;
            }
            //Get configControl value
            var configValue = configControl.getAttribute().getValue();
            if (configValue && configValue.length > 0 && configValue[0].name) {
                var serviceName = configValue[0].name.toLowerCase();
                // Handle Pavement License 
                if (serviceName == SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceServiceConfiguration.pavement) {
                    SS.MSDYN.LGIntelliware.WR.LicenseService.formSelector(formContext, SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceFormNames.pavement);
                }
                // Handle Club Premises
                if (serviceName == SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceServiceConfiguration.clubPremises) {
                    SS.MSDYN.LGIntelliware.WR.LicenseService.formSelector(formContext, SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceFormNames.clubPremises);
                }
                // Handle Premises License
                if (serviceName == SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceServiceConfiguration.premises) {
                    SS.MSDYN.LGIntelliware.WR.LicenseService.formSelector(formContext, SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceFormNames.premises);
                }
                // Handle Personal License

                if (serviceName == SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceServiceConfiguration.personal) {
                    SS.MSDYN.LGIntelliware.WR.LicenseService.formSelector(formContext, SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceFormNames.personal);
                }
            }
        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },
    formSelector: function (formContext, formToSelect) {
        try {
            //Label of the current form.
            var currentFormLabel = formContext.ui.formSelector.getCurrentItem().getLabel().toLowerCase();
            //Check if already form is selected
            if (currentFormLabel !== formToSelect) {
                var listOfAvailableForms = formContext.ui.formSelector.items.get();
                listOfAvailableForms.forEach(form => {
                    if (form.getLabel().toLowerCase() === formToSelect.toLowerCase()) {
                        form.navigate();
                    }
                });
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    //Activates the appropriate business process flow based on the selected service configuration.
    serviceBasedBPF: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext();
            if (formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceHeaderFields.serviceConfiguration)) {
                var serviceConfigurationLookup = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceHeaderFields.serviceConfiguration).getAttribute();
                var serviceConfigurationLookupValue = serviceConfigurationLookup.getValue();
                // Proceed only if a service configuration is selected
                if (serviceConfigurationLookupValue && serviceConfigurationLookupValue.length > 0 && serviceConfigurationLookupValue[0].name) {
                    var serviceConfigurationName = serviceConfigurationLookupValue[0].name.toLowerCase();
                    // Map service configuration names to corresponding BPF names
                    const bpfMapping = {
                        [SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceServiceConfiguration.pavement]: SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceBPFNames.pavement,
                        [SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceServiceConfiguration.clubPremises]: SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceBPFNames.clubPremises,
                        [SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceServiceConfiguration.personal]: SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceBPFNames.personal,
                        [SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceServiceConfiguration.premises]: SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceBPFNames.premises
                    };
                    var bpfName = bpfMapping[serviceConfigurationName];
                    SS.MSDYN.LGIntelliware.WR.LicenseService.getBPFIdByName(bpfName, function (bpfId) {
                        if (bpfId) {
                            formContext.data.process.setActiveProcess(bpfId, function (status) {
                                return;
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

    //temp will be moved in the common library later
    getBPFIdByName: function (bpfName, callback) {
        try {
            // Build the web api query to fetch the workflow with the specified unique name and active state
            var query = `?$select=workflowid&$filter=uniquename eq '${bpfName}' and ${SS.MSDYN.LGIntelliware.WR.Constants.workFlowTableFields.stateCode} eq ${SS.MSDYN.LGIntelliware.WR.Constants.workFlowTableStateCode.active}`;
            // Execute the web api request to retrieve multiple records from the workflow entity
            Xrm.WebApi.retrieveMultipleRecords("workflow", query).then(
                function success(results) {
                    // Check if any workflow records were returned
                    if (results.entities.length > 0) {
                        // Get the workflow ID of the first matching workflow
                        var workflowId = results.entities[0].workflowid;
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

    //Show hide fields on Forms
    hideExtraFieldsOnPavementForm: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext();
            //BPF Stage Events
            SS.MSDYN.LGIntelliware.WR.LicenseService.registerPavementAddPreOnStageChangeEvent(formContext);
            //lock bpf field
            var licenseNumberFieldControl = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBpfFields.pavementLicenceNumber);
            var licenseNumberFieldControl1 = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBpfFields.pavementLicenceNumber1);
            if (licenseNumberFieldControl)
                licenseNumberFieldControl.setDisabled(true);
            if (licenseNumberFieldControl1)
                licenseNumberFieldControl1.setDisabled(true);
            //remove extra status values
            formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.statusReason).removeOption(SS.MSDYN.LGIntelliware.WR.Constants.LicenceServiceStatusCode.refferedToCommittee);

            //field check on Agent Detail Form
            var agent = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentActingOnBehalf).getValue();
            if (agent) {
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.contact).setVisible(false);
            }
            else {
                formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableTabs.agentDetails).sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantContactDetails).setVisible(false);
            }

            //field checks on Details Of Application
            var newLicenseOrExisting = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.newLicenseOrExisting).getValue();
            if (newLicenseOrExisting === 717800001) { //Renewing License
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.placingFurnitureOnPavement).setVisible(false);
            }
            var licenseOfAlcohol = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.licenseOfAlcohol).getValue();
            if (!licenseOfAlcohol) {
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.premisesLicenseNumber).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.supplyOfAlcoholOnPavement).setVisible(false);
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    hideExtraFieldsOnClubPremisesForm: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext();

            //BPF Stage Events
            SS.MSDYN.LGIntelliware.WR.LicenseService.registerAddOnPreStageChangeEvent(formContext);
            //lock bpf field
            var licenseNumberFieldControl = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBpfFields.clubLicenceNumber);
            var licenseNumberFieldControl1 = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBpfFields.clubLicenceNumber1);
            if (licenseNumberFieldControl)
                licenseNumberFieldControl.setDisabled(true);
            if (licenseNumberFieldControl1)
                licenseNumberFieldControl1.setDisabled(true);
            //General tab
            var agent = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentActingOnBehalf).getValue();
            var generalTab = formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableTabs.general);
            if (!agent) {
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentDetails).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusiness).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddress).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddressLookup).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantFirstName).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantLastName).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantEmail).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantMainPhone).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantOtherPhone).setVisible(false);
            } else {
                var agentApplyingAs = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentApplyingAs).getValue();
                if (agentApplyingAs === 717800001) { //Individual
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusiness).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddress).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddressLookup).setVisible(true);
                } else {
                    var agentBusinessInUk = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentBusinessRegisteredInUK).getValue();
                    if (agentBusinessInUk) {
                        formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentCommercialRegister).setVisible(false);
                        generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddress).setVisible(false);
                        generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddressLookup).setVisible(true);
                    } else {
                        formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentCommercialRegister).setVisible(false);
                        generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddress).setVisible(true);
                        generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddressLookup).setVisible(false);
                    }
                }

            }
            var applicantApplyingAs = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantApplyingAs).getValue();
            if (applicantApplyingAs === 1) { //Individual
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusiness).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddress).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddressLookup).setVisible(true);
            } else {
                var applicantBusinessInUk = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantBusinessRegisteredInUK).getValue();
                if (applicantBusinessInUk) {
                    formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantCommercialRegister).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddress).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddressLookup).setVisible(true);
                } else {
                    formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantCommercialRegister).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddress).setVisible(true);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddressLookup).setVisible(false);
                }
            }
            

            //Application details tab
            var postalAddressType = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.postalAddressType).getValue();
            var applicantDetailsTab = formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableTabs.applicationDetails);
            switch (postalAddressType) {
                case 0: //address
                    formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.premisesAddressOsMapReference).setVisible(false);
                    formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.premisesAddressDescription).setVisible(false);
                    break;
                case 1: //OS Map
                    formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.premisesAddressDescription).setVisible(false);
                    applicantDetailsTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.postalAddressOfClubPremises).setVisible(false);
                    break;
                case 2: //Description
                    formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.premisesAddressOsMapReference).setVisible(false);
                    applicantDetailsTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.postalAddressOfClubPremises).setVisible(false);
                    break;
            }

            //Club hours tab
            var sellingOfRetailAlcohol = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.sellingOfRetailAlcohol).getValue();
            if (!sellingOfRetailAlcohol) {
                var clubHoursTab = formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableTabs.clubHours);
                clubHoursTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.clubHoursMonday).setVisible(false);
                clubHoursTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.clubHoursTuesday).setVisible(false);
                clubHoursTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.clubHoursWednesday).setVisible(false);
                clubHoursTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.clubHoursThursday).setVisible(false);
                clubHoursTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.clubHoursFriday).setVisible(false);
                clubHoursTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.clubHoursSaturday).setVisible(false);
                clubHoursTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.clubHoursSunday).setVisible(false);
                clubHoursTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.clubHoursStandartDaysAndTimings).setVisible(false);
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    hideExtraFieldsOnPremisesForm: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext();
            //BPF Stage Events
            SS.MSDYN.LGIntelliware.WR.LicenseService.registerAddOnPreStageChangeEvent(formContext);

            //lock bpf field
            var capacityFieldControl = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBpfFields.capacity);
            var licenseNumberFieldControl = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBpfFields.premisesLicenceNumber);
            var licenseNumberFieldControl1 = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBpfFields.premisesLicenceNumber1);
            if (capacityFieldControl)
                capacityFieldControl.setDisabled(true);
            if (licenseNumberFieldControl)
                licenseNumberFieldControl.setDisabled(true);
            if (licenseNumberFieldControl1)
                licenseNumberFieldControl1.setDisabled(true);
            //General tab
            var agent = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentActingOnBehalf).getValue();
            var generalTab = formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableTabs.general);
            if (!agent) {
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentDetails).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusiness).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddress).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddressLookup).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantFirstName).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantLastName).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantEmail).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantMainPhone).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantOtherPhone).setVisible(false);
            } else {
                var agentApplyingAs = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentApplyingAs).getValue();
                if (agentApplyingAs === 717800001) { //Individual
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusiness).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddress).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddressLookup).setVisible(true);
                } else {
                    var agentBusinessInUk = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentBusinessRegisteredInUK).getValue();
                    if (agentBusinessInUk) {
                        formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentCommercialRegister).setVisible(false);
                        generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddress).setVisible(false);
                        generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddressLookup).setVisible(true);
                    } else {
                        formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentCommercialRegister).setVisible(false);
                        generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddress).setVisible(true);
                        generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddressLookup).setVisible(false);
                    }
                }
            }
            var applicantApplyingAs = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantApplyingAs).getValue();
            if (applicantApplyingAs === 1) { //Individual
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusiness).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddress).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddressLookup).setVisible(true);
            } else {
                var applicantBusinessInUk = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantBusinessRegisteredInUK).getValue();
                if (applicantBusinessInUk) {
                    formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantCommercialRegister).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddress).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddressLookup).setVisible(true);
                } else {
                    formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantCommercialRegister).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddress).setVisible(true);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddressLookup).setVisible(false);
                }
            }

            //Premises details tab
            var postalAddressType = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.postalAddressType).getValue();
            var premisesDetailsTab = formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableTabs.premisesDetails);
            switch (postalAddressType) {
                case 0: //address
                    premisesDetailsTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.osMapReference).setVisible(false);
                    premisesDetailsTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.addressDescription).setVisible(false);
                    break;
                case 1: //OS Map
                    premisesDetailsTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.postalAddressOfPremises).setVisible(false);
                    premisesDetailsTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.addressDescription).setVisible(false);
                    break;
                case 2: //Description
                    premisesDetailsTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.postalAddressOfPremises).setVisible(false);
                    premisesDetailsTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.osMapReference).setVisible(false);
                    break;
            }

            //Application Detail tab
            var capacity = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.capacity).getValue();
            if (capacity === 717800005 || capacity === 717800006 || capacity === 717800007 || capacity === 717800008 || capacity === 717800009 || capacity === 717800010 || capacity === 717800011) {
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.confirmTheFollowing).setVisible(false);
            }
            if (capacity === 717800000) { //Individuals
                formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableTabs.nonIndividualApplicant).setVisible(false);
            }
            else {
                formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableTabs.individualApplicant).setVisible(false);
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    hideExtraFieldsOnPersonalForm: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext();

            //BPF Stage Events
            SS.MSDYN.LGIntelliware.WR.LicenseService.registerAddOnPersonalPreStageChangeEvent(formContext);
            //lock bpf field
            var licenseNumberFieldControl = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBpfFields.personalLicenceNumber);
            var licenseNumberFieldControl1 = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBpfFields.personalLicenceNumber1);
            if (licenseNumberFieldControl)
                licenseNumberFieldControl.setDisabled(true);
            if (licenseNumberFieldControl1)
                licenseNumberFieldControl1.setDisabled(true);
            //General tab
            var agent = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentActingOnBehalf).getValue();
            var generalTab = formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableTabs.general);
            if (!agent) {
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentDetails).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusiness).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddress).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddressLookup).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantFirstName).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantLastName).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantEmail).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantMainPhone).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantOtherPhone).setVisible(false);
            } else {
                var agentApplyingAs = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentApplyingAs).getValue();
                if (agentApplyingAs === 717800001) { //Individual
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusiness).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddress).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddressLookup).setVisible(true);
                } else {
                    var agentBusinessInUk = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentBusinessRegisteredInUK).getValue();
                    if (agentBusinessInUk) {
                        formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentCommercialRegister).setVisible(false);
                        generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddress).setVisible(false);
                        generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddressLookup).setVisible(true);
                    } else {
                        formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.agentCommercialRegister).setVisible(false);
                        generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddress).setVisible(true);
                        generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.agentBusinessAddressLookup).setVisible(false);
                    }
                }
            }
            var applicantApplyingAs = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantApplyingAs).getValue();
            if (applicantApplyingAs === 1) { //Individual
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusiness).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddress).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddressLookup).setVisible(true);
            } else {
                var applicantBusinessInUk = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantBusinessRegisteredInUK).getValue();
                if (applicantBusinessInUk) {
                    formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantCommercialRegister).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddress).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddressLookup).setVisible(true);
                } else {
                    formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.applicantCommercialRegister).setVisible(false);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddress).setVisible(true);
                    generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantBusinessAddressLookup).setVisible(false);
                }
            }
            //Personal Details
            var personalDetailsTab = formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableTabs.personalDetails);
            if (!personalDetailsTab) return;
            var previousName = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.previousName).getValue();
            if (!previousName) {
                personalDetailsTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.applicantNameHistory).setVisible(false);
            }
            var ordinarilyResident = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.ordinarilyResident).getValue();
            if (!ordinarilyResident) {
                personalDetailsTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.correspondenceAddress).setVisible(false);
            }
            //Forfeiture
            var forfeitureTab = formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableTabs.forfeiture);
            if (!forfeitureTab) return;
            var forfeitedPersonalLicense = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.forfeitedPersonalLicense).getValue();
            if (!forfeitedPersonalLicense) {
                forfeitureTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.forfeitureCases).setVisible(false);
            }
            var convictionOfOffence = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.convictionOfOffence).getValue();
            if (!convictionOfOffence) {
                forfeitureTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableSections.releventOffences).setVisible(false);
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    hideExtraFieldsOnLicenseProvisionForm: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext();

            var provisionTypeValue = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenceProvisionTableFields.provisionType).getValue();
            var generalTab = formContext.ui.tabs.get(SS.MSDYN.LGIntelliware.WR.Constants.licenceProvisionFormTabs.general);

            if (provisionTypeValue !== 9 && provisionTypeValue !== 7) {//other than alcohol or similar activity provision
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenceProvisionFormSections.supervisor).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenceProvisionFormSections.supervisorConsent).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceProvisionTableFields.saleOfAlcohol).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceProvisionTableFields.descriptionOfEntertainment).setVisible(false);

            } else if (provisionTypeValue === 9) { //Provision of Alcohol
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceProvisionTableFields.stateOfActivity).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceProvisionTableFields.descriptionOfEntertainment).setVisible(false);
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceProvisionTableFields.provisionPlace).setVisible(false);


            } else if (provisionTypeValue === 7) {// Provision of similar activity
                formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceProvisionTableFields.saleOfAlcohol).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenceProvisionFormSections.supervisor).setVisible(false);
                generalTab.sections.get(SS.MSDYN.LGIntelliware.WR.Constants.licenceProvisionFormSections.supervisorConsent).setVisible(false);
            }
            SS.MSDYN.LGIntelliware.WR.LicenseService.lockAllFields(formContext);

        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    //Method to validate expiry date according to issue date
    validateExpiryDate: function (executionContext) {
        var formContext = executionContext.getFormContext();

        // Get the Issue Date and Expiry Date attributes
        var issueDateAttr = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.licenceIssueDate);
        var expiryDateAttr = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.licenceExpiryDate);
        var expiryDateBpfAttr = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBpfFields.licenceExpiryDate);
        var expiryDateBpfAttr1 = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBpfFields.licenceExpiryDate1);

        // Ensure both fields exist
        if (!issueDateAttr || !expiryDateAttr) return;

        var issueDate = issueDateAttr.getValue();
        var expiryDate = expiryDateAttr.getValue();

        // If either date is empty, skip validation
        if (!issueDate || !expiryDate) return;

        // Compare the dates
        if (expiryDate <= issueDate) {
            expiryDateBpfAttr.setNotification("Expiry date must be after the Issue date.");
            expiryDateBpfAttr1.setNotification("Expiry date must be after the Issue date.");
        }
        else {
            expiryDateBpfAttr.clearNotification();
            expiryDateBpfAttr1.clearNotification();
        }
    },

    //back office events through BPF
    lockUnlockFields: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var updateRequired = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.updateRequired).getValue();
        if (updateRequired) {
            SS.MSDYN.LGIntelliware.WR.LicenseService.unlockAllFields(formContext);
        } else {
            SS.MSDYN.LGIntelliware.WR.LicenseService.lockAllFields(formContext);
        }

    },

    lockAllFields: function (formContext) {
        formContext.data.entity.attributes.forEach(function (attribute, index) {
            var attributeControl = formContext.getControl(attribute.getName());
            if (attributeControl) {
                attributeControl.setDisabled(true);
            }
        });
    },

    unlockAllFields: function (formContext) {
        formContext.data.entity.attributes.forEach(function (attribute, index) {
            var attributeControl = formContext.getControl(attribute.getName());
            if (attributeControl) {
                attributeControl.setDisabled(false);
            }
        });
    },

    //bpf completion event
    registerAddOnProcessStatusChangeEvent: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var process = formContext.data.process;
        // Register event handler for process status changes
        process.addOnProcessStatusChange(function () {
            if (process.getStatus() === SS.MSDYN.LGIntelliware.WR.Constants.bpfStatus.finished) {
                var grantReject = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.grantReject).getValue();
                if (grantReject === 0) {
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.status).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.inactive);
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.statusReason).setValue(SS.MSDYN.LGIntelliware.WR.Constants.LicenceServiceStatusCode.rejected);
                }
                else if (grantReject === 1) {
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.status).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.inactive);
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.statusReason).setValue(SS.MSDYN.LGIntelliware.WR.Constants.LicenceServiceStatusCode.granted);
                }
            }
            formContext.data.entity.save("save");
        });
    },

    //Club and Premises License Bpf Event
    registerAddOnPreStageChangeEvent: function (formContext) {
        try {
            // Attach the pre stage change event to the current bpf instance
            formContext.data.process.addOnPreStageChange(function (stageContext) {
                SS.MSDYN.LGIntelliware.WR.LicenseService.onPreStageChangeBpf(stageContext);
            });
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    onPreStageChangeBpf: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext();
            var activeStage = formContext.data.process.getActiveStage();
            var processStageName = activeStage.getName();
            if (processStageName === SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBPFStage.verification) {
                var representations = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.hastheRepresentationsRecieved).getValue();
                if (!representations) {
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.grantReject).setValue(1); //Grant
                }
                else {
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.grantReject).setValue(null);
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.statusReason).setValue(SS.MSDYN.LGIntelliware.WR.Constants.LicenceServiceStatusCode.refferedToCommittee);
                }
            }
            if (processStageName === SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBPFStage.review) {
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.statusReason).setValue(SS.MSDYN.LGIntelliware.WR.Constants.LicenceServiceStatusCode.inProgress);
            }
            if (processStageName === SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBPFStage.meeting) {
                var meetingCompleted = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.meetingCompleted).getValue();
                if (!meetingCompleted) {
                    executionContext.getEventArgs().preventDefault();
                    formContext.data.process.movePrevious();
                }
            }
            if (processStageName === SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBPFStage.updatesRequired) {
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.updateRequired).setValue(false);
                SS.MSDYN.LGIntelliware.WR.LicenseService.lockAllFields(formContext);
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },



    //Pavement License Bpf Event
    registerPavementAddPreOnStageChangeEvent: function (formContext) {
        try {
            // Attach the pre stage change event to the current bpf instance
            formContext.data.process.addOnPreStageChange(function (stageContext) {
                SS.MSDYN.LGIntelliware.WR.LicenseService.onPreStageChangePavementBpf(stageContext);
            });
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    onPreStageChangePavementBpf: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext();
            var activeStage = formContext.data.process.getActiveStage();
            var processStageName = activeStage.getName();
            if (processStageName === SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBPFStage.consultationPeriod) {
                var representations = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.hastheRepresentationsRecieved).getValue();
                var unconditionalGrant = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.unconditionalGrant).getValue();
                if (representations || !(unconditionalGrant)) {
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.grantReject).setValue(null);
                }
                else {
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.grantReject).setValue(1); //Grant
                }
            }
            if (processStageName === SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBPFStage.review) {
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.statusReason).setValue(SS.MSDYN.LGIntelliware.WR.Constants.LicenceServiceStatusCode.inProgress);
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    //Personal License Bpf Event
    registerAddOnPersonalPreStageChangeEvent: function (formContext) {
        try {
            // Attach the pre stage change event to the current bpf instance
            formContext.data.process.addOnPreStageChange(function (stageContext) {
                SS.MSDYN.LGIntelliware.WR.LicenseService.onPreStageChangePersonalBpf(stageContext);
            });
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    onPreStageChangePersonalBpf: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext();
            var activeStage = formContext.data.process.getActiveStage();
            var processStageName = activeStage.getName();
            if (processStageName === SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBPFStage.reviewConvictions) {
                var refferedToCommittee = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.refferedToCommittee).getValue();
                if (refferedToCommittee) {
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.statusReason).setValue(SS.MSDYN.LGIntelliware.WR.Constants.LicenceServiceStatusCode.refferedToCommittee);
                }
            }
            if (processStageName === SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBPFStage.review) {
                formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.statusReason).setValue(SS.MSDYN.LGIntelliware.WR.Constants.LicenceServiceStatusCode.inProgress);
            }
            if (processStageName === SS.MSDYN.LGIntelliware.WR.Constants.licenceServiceBPFStage.meeting) {
                var meetingCompleted = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.licenseServiceTableFields.meetingCompleted).getValue();
                if (!meetingCompleted) {
                    executionContext.getEventArgs().preventDefault();
                    formContext.data.process.movePrevious();
                }
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },
};

