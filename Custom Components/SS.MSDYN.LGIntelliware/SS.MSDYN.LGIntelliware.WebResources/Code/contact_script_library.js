//Define namespace
if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

// Namespace for Contact-related functionality
SS.MSDYN.LGIntelliware.WR.Contact = {
    //Validate phone number
    validateMobilePhoneFormat: function (executionContext) {
        try {
            let formContext = executionContext.getFormContext();
            if (formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.contactTableFields.mobilePhone)) {
                SS.MSDYN.LGIntelliware.WR.Common.validateMobilePhoneFormat(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.contactTableFields.mobilePhone, SS.MSDYN.LGIntelliware.WR.Constants.contactTableFieldErrorMessage.validateMobilePhoneFormatText);
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    }
}

