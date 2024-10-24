if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

SS.MSDYN.LGIntelliware.WR.MissedBin = {
    checkBartecMunicipalStatusButtonForm: function (primaryControl) {
        try {
            let formContext = primaryControl;
            let confirmStrings = { text: "Do you want to Check Status in Bartec Municipal? You can't undo this action.", title: "Confirm Bartec Municipal Check Status" };
            let confirmOptions = { height: 200, width: 450 };
            Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                function (success) {
                    if (success.confirmed) {
                        SS.MSDYN.LGIntelliware.WR.Common.showProgressIndicator();

                        let entityId = formContext.data.entity.getId().replace("{", "").replace("}", "");
                        // Execute Bartec Municipal: Check service request status custom action...
                        let execute_ss_BartecMunicipalCheckServiceRequestStatus_Request = {
                            // Parameters
                            entity: { entityType: "ss_missedbin", id: entityId }, // entity

                            getMetadata: function () {
                                return {
                                    boundParameter: "entity",
                                    parameterTypes: {
                                        entity: { typeName: "mscrm.ss_missedbin", structuralProperty: 5 }
                                    },
                                    operationType: 0, operationName: "ss_BartecMunicipalCheckServiceRequestStatus"
                                };
                            }
                        };

                        Xrm.WebApi.execute(execute_ss_BartecMunicipalCheckServiceRequestStatus_Request).then(
                            function success(response) {
                                if (response.ok) {
                                    SS.MSDYN.LGIntelliware.WR.Common.hideProgressIndicator();
                                }
                            }
                        ).catch(function (e) {
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

