if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

let FORM_TYPE_CREATE = 1;

SS.MSDYN.LGIntelliware.WR.SMS = {
    onLoad: function (executionContext) {
        try {
            let formContext = executionContext.getFormContext();
            let formType = formContext.ui.getFormType();

            if (formType == FORM_TYPE_CREATE) {
                if (formContext.getControl("header_scheduledend")) {
                    formContext.getControl("header_scheduledend").getAttribute().setValue(new Date());
                }
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    }
}

