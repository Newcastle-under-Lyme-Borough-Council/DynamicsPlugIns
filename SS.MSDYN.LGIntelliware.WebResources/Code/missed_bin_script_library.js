// Define namespace
if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }
// Namespace for MissedBin-related functionality
SS.MSDYN.LGIntelliware.WR.MissedBin = {
    onLoad: function (executionContext) {
        try {
            //Set business process flow based on dispute/escalate
            SS.MSDYN.LGIntelliware.WR.MissedBin.disputeEscalateBasedBPF(executionContext);
            //Handle business process flow status change
            SS.MSDYN.LGIntelliware.WR.MissedBin.handleBpfCompletionStatusChange(executionContext);
            //Register event on business process flow stage change
            SS.MSDYN.LGIntelliware.WR.MissedBin.registerAddOnStageChangeEvent(executionContext);

        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },
    // Activates the appropriate business process flow based on dispute/escalate value
    disputeEscalateBasedBPF: function (executionContext) {
        try {
            let formContext = executionContext.getFormContext();
            let disputeEscalateControl = formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.disputeEscalate);
            // Ensure control exists
            if (!disputeEscalateControl) return;

            let disputeEscalateValue = disputeEscalateControl.getAttribute().getValue();

            if (disputeEscalateValue != null) {
                // Compare to correct option set values
                if (disputeEscalateValue === SS.MSDYN.LGIntelliware.WR.Constants.missedBinChoiceFields.dispute) {

                    let bpfName = SS.MSDYN.LGIntelliware.WR.Constants.missedBinBPFNames.missedBinDispute;

                    SS.MSDYN.LGIntelliware.WR.Common.getBPFIdByName(bpfName, function (bpfId) {
                        if (bpfId) {
                            formContext.data.process.setActiveProcess(bpfId, function (status) {
                            });
                        }
                    });
                }
                else if (disputeEscalateValue === SS.MSDYN.LGIntelliware.WR.Constants.missedBinChoiceFields.escalate) {
                    let bpfName = SS.MSDYN.LGIntelliware.WR.Constants.missedBinBPFNames.missedBinEscalate;
                    SS.MSDYN.LGIntelliware.WR.Common.getBPFIdByName(bpfName, function (bpfId) {
                        if (bpfId) {

                            formContext.data.process.setActiveProcess(bpfId, function (status) {
                            });
                        }
                    });
                }
            }
            else {
                formContext.ui.process.setVisible(false)
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },
    // Handles check bartec status button click
    checkBartecMunicipalStatusButtonForm: function (primaryControl) {
        try {
            let formContext = primaryControl;
            // Confirmation dialog text and configuration
            let confirmStrings = { text: SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableDialogueConfiguration.bartecMunicipalCheckServiceRequestStatusText, title: SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableDialogueConfiguration.bartecMunicipalCheckServiceRequestStatusTitle };
            let confirmOptions = { height: SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableDialogueConfiguration.bartecMunicipalCheckServiceRequestStatusHeight, width: SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableDialogueConfiguration.bartecMunicipalCheckServiceRequestStatusWidth };
            // Open confirmation dialog
            Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                function (success) {
                    if (success.confirmed) {
                        // User confirmed Show progress indicator
                        SS.MSDYN.LGIntelliware.WR.Common.showProgressIndicator();
                        let entityId = formContext.data.entity.getId().replace("{", "").replace("}", "");
                        // Execute bartec municipal: check service request status custom action
                        let execute_ss_BartecMunicipalCheckServiceRequestStatus_Request = {
                            // Parameters
                            entity: { entityType: SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.missedBinTableName, id: entityId },
                            // Metadata for custom action call
                            getMetadata: function () {
                                return {
                                    boundParameter: SS.MSDYN.LGIntelliware.WR.Constants.customActionParameter.boundParameter,
                                    parameterTypes: {
                                        entity: { typeName: SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableCustomAction.typeName, structuralProperty: SS.MSDYN.LGIntelliware.WR.Constants.customActionParameter.structuralProperty }
                                    },
                                    operationType: SS.MSDYN.LGIntelliware.WR.Constants.customActionParameter.operationType, operationName: SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableCustomAction.bartecMunicipalCheckServiceRequestStatus
                                };
                            }
                        };
                        // Execute the custom action using Web API
                        Xrm.WebApi.execute(execute_ss_BartecMunicipalCheckServiceRequestStatus_Request).then(
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

    //Record status change on business process flow completion
    handleBpfCompletionStatusChange: function (executionContext) {
        try {
            let formContext = executionContext.getFormContext();
            let process = formContext.data.process;
            if (process == null) {
                return;
            }
            // Register event handler for process status changes
            process.addOnProcessStatusChange(function () {
                if (process.getStatus() === SS.MSDYN.LGIntelliware.WR.Constants.bpfStatus.finished) {
                    let newCollectionRequired = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinBpfFields.newCollectionRequired)?.getValue();
                    // Ensure control exists
                    if (newCollectionRequired === SS.MSDYN.LGIntelliware.WR.Constants.missedBinChoiceFields.newCollectionRequired) {
                        formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.inactive);
                        formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.missedBinStatusCode.newCollectionRequired);
                    } else if (newCollectionRequired === SS.MSDYN.LGIntelliware.WR.Constants.missedBinChoiceFields.newCollectionNoRequired) {
                        formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.inactive);
                        formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.missedBinStatusCode.noCollectionRequired);
                    }
                    let hasTheCaseBeenResolved = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinBpfFields.hasTheCaseBeenResolved)?.getValue();
                    if (hasTheCaseBeenResolved === SS.MSDYN.LGIntelliware.WR.Constants.missedBinChoiceFields.hasTheCaseBeenResolved) {
                        formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.inactive);
                        formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.missedBinStatusCode.resolved);
                    } else if (hasTheCaseBeenResolved === SS.MSDYN.LGIntelliware.WR.Constants.missedBinChoiceFields.hasTheCaseNotResolved) {
                        formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.inactive);
                        formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.missedBinStatusCode.closedCompleted);
                    }
                    // Save and refresh form to reflect updates
                    formContext.data.save().then(() => formContext.data.refresh(), () => { });

                }
            });
        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    // Register an event handler for business process flow  stage changes
    registerAddOnStageChangeEvent: function (executionContext) {
        try {
            let formContext = executionContext.getFormContext();
            let process = formContext.data.process;
            if (process == null) return;
            // Attach the stage change event to the current bpf instance
            process.addOnStageChange(function (stageContext) {
                SS.MSDYN.LGIntelliware.WR.MissedBin.statusChangeOnBPFStageChange(stageContext);
            });
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    // Handles status updates when the business process flow stage changes
    statusChangeOnBPFStageChange: function (executionContext) {
        try {
            let formContext = executionContext.getFormContext();
            let activeProcess = formContext.data.process.getActiveProcess();
            let activeStage = formContext.data.process.getActiveStage();
            if (activeProcess && activeStage) {
                let processStageName = activeStage.getName();
                // Update statusCode/stateCode for specific stages
                if (processStageName == SS.MSDYN.LGIntelliware.WR.Constants.missedBinBPFStage.assignAgent) {
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.active);
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.missedBinStatusCode.awaitingAssignment);
                }
                else if (processStageName == SS.MSDYN.LGIntelliware.WR.Constants.missedBinBPFStage.resolveCase) {
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.active);
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.missedBinStatusCode.investigatingDispute);
                }
                else if (processStageName == SS.MSDYN.LGIntelliware.WR.Constants.missedBinBPFStage.emailCustomer) {
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.active);
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.missedBinStatusCode.availableEscalation);
                }
                else if (processStageName == SS.MSDYN.LGIntelliware.WR.Constants.missedBinBPFStage.resolveEscalation) {
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.stateCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.stateCode.active);
                    formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.statusCode).setValue(SS.MSDYN.LGIntelliware.WR.Constants.missedBinStatusCode.awaitingResolution);
                }
                // Save and refresh form to reflect updates
                formContext.data.save().then(() => formContext.data.refresh(), () => { });


            }
        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    // Set crew lookup in case
    setCrewLookupInCase: function (executionContext) {
        try {
            let formContext = executionContext.getFormContext();

            // Get the selected crew 
            let crewAttribute = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.crew);
            if (!crewAttribute) return;

            let crewValue = crewAttribute.getValue();
            let crewId = crewValue && crewValue.length > 0 ? crewValue[0].id : null;

            // Get the related case lookup 
            let caseAttribute = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.case);
            if (!caseAttribute) return;

            let caseValue = caseAttribute.getValue();
            if (!caseValue || caseValue.length === 0) return; // no case 

            let caseId = caseValue[0].id;

            // Prepare the payload to update the case's crew lookup
            let data = {};
            if (crewId) {
                data[SS.MSDYN.LGIntelliware.WR.Constants.caseTableFields.crewSchemaName + "@odata.bind"] = `/${SS.MSDYN.LGIntelliware.WR.Constants.crewTableFields.crewEntitySetName}(${crewId.replace("{", "").replace("}", "")})`;
            } else {
                data[SS.MSDYN.LGIntelliware.WR.Constants.caseTableFields.crewSchemaName + "@odata.bind"] = null;
            }

            // Update the related case using Web API
            Xrm.WebApi.updateRecord(SS.MSDYN.LGIntelliware.WR.Constants.caseTableFields.caseLogicalName, caseId.replace("{", "").replace("}", ""), data)
                .then(function () {
                })
                .catch(function (e) {
                    SS.MSDYN.LGIntelliware.WR.Common.showError(e.message, true);
                });

        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    }

}

