//Define namespace
if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

// Namespace for Contact-related functionality
SS.MSDYN.LGIntelliware.WR.Contact = {
    //Validate Phone Number
    validateMobilePhoneFormat: function (executionContext) {
        try {
            let formContext = executionContext.getFormContext();
            if (formContext.getControl("mobilephone")) {
                SS.MSDYN.LGIntelliware.WR.Common.validateMobilePhoneFormat(executionContext, "mobilephone", "Should start with 00, should have the Country code, and then the Phone number. Maximum allowed length is 14 digits, no + sign, spaces or - are allowed.");
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    }
}

