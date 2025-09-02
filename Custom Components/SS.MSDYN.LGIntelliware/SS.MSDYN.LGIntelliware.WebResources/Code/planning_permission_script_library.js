// Define namespace
if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

// Namespace for plannig permission-related functionality
SS.MSDYN.LGIntelliware.WR.PlanningPermission = {
	// Handles planning permission button click
	downloadPlanningPermissionApplicationButtonForm: function (primaryControl) {
		try {
			let formContext = primaryControl;
			// Confirmation dialog text and configuration
			let confirmStrings = { text: "Do you want to Download Application from Planning Portal? You can't undo this action.", title: "Confirm Download Application" };
			let confirmOptions = { height: 200, width: 450 };
			// Open confirmation dialog
			Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
				function (success) {
					if (success.confirmed) {
						// User confirmed Show progress indicator
						SS.MSDYN.LGIntelliware.WR.Common.showProgressIndicator();

						let entityId = formContext.data.entity.getId().replace("{", "").replace("}", "");
						// Execute planning portal: download application custom action
						let execute_ss_PlanningPortalDownloadApplication_Request = {
							// Parameters
							entity: { entityType: "ss_planningpermission", id: entityId }, // entity
							// Metadata for custom action call
							getMetadata: function () {
								return {
									boundParameter: "entity",
									parameterTypes: {
										entity: { typeName: "mscrm.ss_planningpermission", structuralProperty: 5 }
									},
									operationType: 0, operationName: "ss_PlanningPortalDownloadApplication"
								};
							}
						};
						// Execute the custom action using Web API
						Xrm.WebApi.execute(execute_ss_PlanningPortalDownloadApplication_Request).then(
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

