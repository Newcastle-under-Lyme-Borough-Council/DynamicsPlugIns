// Define namespace
if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

// Namespace for bartec events related functionality
SS.MSDYN.LGIntelliware.WR.BartecEvents = {
    openSubEventsForm: async function (primaryControl) {
        try {
            const formContext = primaryControl;
            let parentId = null;
            let parentName = null;
            if (formContext?.data?.entity) {
                parentId = formContext.data.entity.getId();
                parentName = formContext.data.entity.getPrimaryAttributeValue();
            }
            //Get form ID dynamically using API
            const formId = await SS.MSDYN.LGIntelliware.WR.BartecEvents.getFormIdByName(
                SS.MSDYN.LGIntelliware.WR.Constants.bartecEventsFormName.subEventsForm,
                SS.MSDYN.LGIntelliware.WR.Constants.bartecEventsTableFields.bartecEventsSchemaName
            );

            if (!formId) {
                return;
            }
            const entityFormOptions = {
                entityName: SS.MSDYN.LGIntelliware.WR.Constants.bartecEventsTableFields.bartecEventsSchemaName,
                useQuickCreateForm: false,
                formId: formId,
                openInNewWindow: false
            };

            const formParameters = {};
            if (parentId) {
                const cleanId = parentId.replace(/[{}]/g, "");
                formParameters[SS.MSDYN.LGIntelliware.WR.Constants.bartecEventsTableFields.mainEvent] = cleanId;
                formParameters[SS.MSDYN.LGIntelliware.WR.Constants.bartecEventsTableFields.mainEventName] = parentName;
                formParameters[SS.MSDYN.LGIntelliware.WR.Constants.bartecEventsTableFields.associatedEventType] = SS.MSDYN.LGIntelliware.WR.Constants.bartecEventsTableFields.bartecEventsSchemaName;
            }

            // Open the form
            Xrm.Navigation.openForm(entityFormOptions, formParameters).then(
                function () {
                    return;
                },
                function (error) {
                    if (error?.message?.includes("Navigation was cancelled")) {
                        return;
                    }
                    SS.MSDYN.LGIntelliware.WR.Common.showError(error, true);
                }
            );

        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    //Fetch formId dynamically
    getFormIdByName: async function (formName, entityName) {
        const query = `?$select=${SS.MSDYN.LGIntelliware.WR.Constants.systemFormTableFields.formId},${SS.MSDYN.LGIntelliware.WR.Constants.systemFormTableFields.name}&$filter=${SS.MSDYN.LGIntelliware.WR.Constants.systemFormTableFields.name} eq '${formName}' and ${SS.MSDYN.LGIntelliware.WR.Constants.systemFormTableFields.objectTypeCode} eq '${entityName}' and ${SS.MSDYN.LGIntelliware.WR.Constants.systemFormTableFields.type} eq ${SS.MSDYN.LGIntelliware.WR.Constants.formlevel.information}`;
        return new Promise(function (resolve, reject) {
            Xrm.WebApi.retrieveMultipleRecords(SS.MSDYN.LGIntelliware.WR.Constants.systemFormTableFields.systemFormLogicalName, query).then(
                function (results) {
                    if (results.entities.length > 0) {
                        resolve(results.entities[0].formid);
                    } else {
                        resolve(null);
                    }
                },
                function (error) {
                    reject(error);
                }
            );
        });
    }
};