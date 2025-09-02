// Define namespace
if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }
// Namespace for MissedBin-related functionality
SS.MSDYN.LGIntelliware.WR.MissedBin = {
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
    }
}

