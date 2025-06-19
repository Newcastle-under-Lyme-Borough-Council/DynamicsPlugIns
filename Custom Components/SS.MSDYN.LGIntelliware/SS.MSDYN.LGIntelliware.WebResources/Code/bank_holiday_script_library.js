if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

SS.MSDYN.LGIntelliware.WR.BankHoliday = {
    restrictPastDates: function (executionContext) {
        try {
             let formContext = executionContext.getFormContext();
            if (formContext.getControl("ss_date")) {
            SS.MSDYN.LGIntelliware.WR.Common.compareDateValues(executionContext, "ss_date","", "Past dates are not allowed. Please select today or a future date.");
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    }
}

