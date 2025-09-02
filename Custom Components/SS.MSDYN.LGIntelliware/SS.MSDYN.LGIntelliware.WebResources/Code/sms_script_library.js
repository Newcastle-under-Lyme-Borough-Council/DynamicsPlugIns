// Define namespace
if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

let FORM_TYPE_CREATE = 1;
// Namespace for sms related functionality
SS.MSDYN.LGIntelliware.WR.SMS = {
    // Handles form onLoad event for SMS entity
    onLoad: function (executionContext) {
        try {
            let formContext = executionContext.getFormContext();
            let formType = formContext.ui.getFormType();
            //Run this logic on Create form
            if (formType == FORM_TYPE_CREATE) {
                // Check if the scheduled end control is present in the header
                if (formContext.getControl("header_scheduledend")) {
                    // Set default value to current date/time
                    formContext.getControl("header_scheduledend").getAttribute().setValue(new Date());
                }
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    }
}

