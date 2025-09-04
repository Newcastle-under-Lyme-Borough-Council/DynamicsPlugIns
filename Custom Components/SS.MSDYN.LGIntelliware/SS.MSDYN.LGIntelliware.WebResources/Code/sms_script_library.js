// Define namespace
if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

// Namespace for sms related functionality
SS.MSDYN.LGIntelliware.WR.SMS = {
    // Handles form onLoad event for sms entity
    onLoad: function (executionContext) {
        try {
            let formContext = executionContext.getFormContext();
            let formType = formContext.ui.getFormType();
            //Run this logic on create form
            if (formType == SS.MSDYN.LGIntelliware.WR.Constants.formType.create) {
                // Check if the scheduled end control is present in the header
                if (formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.smsHeaderFields.headerScheduledEnd)) {
                    // Set default value to current date/time
                    formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.smsHeaderFields.headerScheduledEnd).getAttribute().setValue(new Date());
                }
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    }
}

