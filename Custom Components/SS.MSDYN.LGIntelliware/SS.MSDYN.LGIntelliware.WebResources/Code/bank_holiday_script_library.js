// Define namespace
if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

// Namespace for bank holiday related functionality
SS.MSDYN.LGIntelliware.WR.BankHoliday = {
    //Restricts the user from entering past dates
    restrictPastDates: function (executionContext) {
        try {
            let formContext = executionContext.getFormContext();
            // Check if the ss_date control exists on the form
            if (formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.bankHolidayTableFields.date)) {
                SS.MSDYN.LGIntelliware.WR.Common.compareDateValues(executionContext, SS.MSDYN.LGIntelliware.WR.Constants.bankHolidayTableFields.date, "", SS.MSDYN.LGIntelliware.WR.Constants.bankHolidayTableFieldErrorMessage.restrictPastDatesText);
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    }
}

