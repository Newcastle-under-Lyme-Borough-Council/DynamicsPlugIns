// Define namespace
if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

SS.MSDYN.LGIntelliware.WR.Email = {

    setEmailToFromContact: function (executionContext) {
        try {
            let formContext = executionContext.getFormContext();

            // Get the regarding object 
            let regarding = formContext.getAttribute(
                SS.MSDYN.LGIntelliware.WR.Constants.emailTableFields.regarding
            )?.getValue();

            if (!regarding || regarding.length === 0) {
                return;
            }

            let regardingId = regarding[0].id.replace("{", "").replace("}", "");
            let entityType = regarding[0].entityType;

            // Retrieve record
            Xrm.WebApi.retrieveRecord(
                entityType,
                regardingId,
                `?$select=${SS.MSDYN.LGIntelliware.WR.Constants.emailTableFields.contact}`
            )
            .then(function (result) {

                let contactField = SS.MSDYN.LGIntelliware.WR.Constants.emailTableFields.contact;

                if (!(contactField in result)) {
                    return;
                }

                let contactId = result[contactField];
                if (!contactId) {
                    return;
                }

                // Get formatted name
                let formattedName =
                    result[`${contactField}@OData.Community.Display.V1.FormattedValue`] || "";

                // Set TO
                formContext.getAttribute(
                    SS.MSDYN.LGIntelliware.WR.Constants.emailTableFields.to
                ).setValue([{
                    id: contactId,
                    entityType: SS.MSDYN.LGIntelliware.WR.Constants.contactTableFields.contactLogicalName,
                    name: formattedName
                }]);

            })
            .catch(function (error) {
                SS.MSDYN.LGIntelliware.WR.Common.showError(error, true);
            });

        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    setEmailFrom: function (executionContext) {
        try {
            var formContext = executionContext.getFormContext();
            if (formContext.ui.getFormType() !== SS.MSDYN.LGIntelliware.WR.Constants.formType.create) {
                return;
            }
            var userTable = SS.MSDYN.LGIntelliware.WR.Constants.userTableFields.userTablelogicalName;
            var userIdField = SS.MSDYN.LGIntelliware.WR.Constants.userTableFields.userId;
            var emailField = SS.MSDYN.LGIntelliware.WR.Constants.userTableFields.primaryEmail;
            var emailAddress = SS.MSDYN.LGIntelliware.WR.Constants.emailAddress.nulbcCustomerService;
            var fullNameField = SS.MSDYN.LGIntelliware.WR.Constants.userTableFields.fullName;
            var query =
                `?$select=${userIdField},${fullNameField}&$filter=${emailField} eq '${emailAddress}'`;
            Xrm.WebApi.retrieveMultipleRecords(userTable, query).then(
                function success(result) {
                    if (result.entities.length > 0) {
                        var user = result.entities[0];
                        var fromLookup = [{
                            id: user[userIdField],
                            name: user.fullname,
                            entityType: userTable
                        }];
                        formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.emailTableFields.from).setValue(fromLookup);
                    }
                }
            );
        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    }

};

