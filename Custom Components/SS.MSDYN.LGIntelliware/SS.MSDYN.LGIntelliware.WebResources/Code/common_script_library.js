if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

let COMPARE_DATE_UNIQUE_ID = "COMPARE_DATE_UNIQUE_ID";
let COMPARE_YEAR_UNIQUE_ID = "COMPARE_YEAR_UNIQUE_ID";
let VALIDATE_MOBILE_PHONE_UNIQUE_ID = "VALIDATE_MOBILE_PHONE_UNIQUE_ID";
let PROGRESS_INDICATOR_TEXT = "Processing...";

SS.MSDYN.LGIntelliware.WR.Common = {
    showError: function (e, validate) {
        if (validate) {
            let exmsg = "";
            if (e.message) {
                exmsg += e.message;
            }
            if (e.stack) {
                exmsg += " | stack: " + e.stack;
            }
            Xrm.Navigation.openErrorDialog({ message: exmsg }).then(
                function (success) { },
                function (error) {
                    SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
                }
            );
        }
        else {
            Xrm.Navigation.openErrorDialog({ message: e }).then(
                function (success) { },
                function (error) {
                    SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
                }
            );
        }
    },
    showHideTab: function (executionContext, tabId, visible) {
        try {
            let formContext = executionContext.getFormContext();
            let tabToShowHide = formContext.ui.tabs.get(tabId);
            if (typeof (tabToShowHide) != "undefined" && tabToShowHide != null) {
                tabToShowHide.setVisible(visible);
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },
    showHideSection: function (executionContext, tabId, sectionId, visible) {
        try {
            let formContext = executionContext.getFormContext();
            let sectionTab = formContext.ui.tabs.get(tabId);
            if (typeof (sectionTab) != "undefined" && sectionTab != null) {
                let sectionToShowHide = sectionTab.sections.get(sectionId);
                if (typeof (sectionToShowHide) != "undefined" && sectionToShowHide != null) {
                    sectionToShowHide.setVisible(visible);
                }
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },
    compareDateValues: function (executionContext, sourceDateAttributeKey, targetDateAttributeKey, errorMessage) {
        try {
            let formContext = executionContext.getFormContext();
            let sourceDate = formContext.getAttribute(sourceDateAttributeKey);
            let targetDate;
            if (targetDateAttributeKey.length > 0) {
                let targetDate = formContext.getAttribute(targetDateAttributeKey);
                if (typeof (sourceDate) != "undefined" && sourceDate != null && typeof (targetDate) != "undefined" && targetDate != null) {
                    if (sourceDate.getValue() != null && targetDate.getValue() != null) {
                        // if the target date is before the source date, show error message...
                        if (targetDate.getValue().setHours(0, 0, 0, 0) < sourceDate.getValue().setHours(0, 0, 0, 0)) {
                            formContext.getControl(targetDateAttributeKey).setNotification(errorMessage, COMPARE_DATE_UNIQUE_ID);
                        }
                        else {
                            formContext.getControl(targetDateAttributeKey).clearNotification(COMPARE_DATE_UNIQUE_ID);
                        }
                    }
                }
            }
            else {
                targetDate = new Date();
                if (typeof (sourceDate) != "undefined" && sourceDate != null) {
                    if (sourceDate.getValue() != null) {
                        // if the target date is before the source date, show error message...
                        if (sourceDate.getValue().setHours(0, 0, 0, 0) < targetDate.setHours(0, 0, 0, 0)) {
                            formContext.getControl(sourceDateAttributeKey).setNotification(errorMessage, COMPARE_DATE_UNIQUE_ID);
                        }
                        else {
                            formContext.getControl(sourceDateAttributeKey).clearNotification(COMPARE_DATE_UNIQUE_ID);
                        }
                    }
                }
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },
    compareYearValues: function (executionContext, sourceDateAttributeKey, targetDateAttributeKey, errorMessage) {
        try {
            let formContext = executionContext.getFormContext();
            let sourceDate = formContext.getAttribute(sourceDateAttributeKey);
            let targetDate;
            if (targetDateAttributeKey.length > 0) {
                let targetDate = formContext.getAttribute(targetDateAttributeKey);
                if (typeof (sourceDate) != "undefined" && sourceDate != null && typeof (targetDate) != "undefined" && targetDate != null) {
                    if (sourceDate.getValue() != null && targetDate.getValue() != null) {
                        let sourceDateYear = sourceDate.getValue().getFullYear();
                        let targetDateYear = targetDate.getFullYear();
                        // if the target date is before the source date, show error message...
                        if (sourceDateYear > targetDateYear) {
                            formContext.getControl(sourceDateAttributeKey).setNotification(errorMessage, COMPARE_YEAR_UNIQUE_ID);
                        }
                        else {
                            formContext.getControl(sourceDateAttributeKey).clearNotification(COMPARE_YEAR_UNIQUE_ID);
                        }
                    }
                }
            }
            else {
                targetDate = new Date();
                if (typeof (sourceDate) != "undefined" && sourceDate != null) {
                    if (sourceDate.getValue() != null) {
                        // if the target date is before the source date, show error message...
                        let sourceDateYear = sourceDate.getValue().getFullYear();
                        let targetDateYear = targetDate.getFullYear();
                        if (sourceDateYear > targetDateYear) {
                            formContext.getControl(sourceDateAttributeKey).setNotification(errorMessage, COMPARE_YEAR_UNIQUE_ID);
                        }
                        else {
                            formContext.getControl(sourceDateAttributeKey).clearNotification(COMPARE_YEAR_UNIQUE_ID);
                        }
                    }
                }
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },
    validateMobilePhoneFormat: function (executionContext, sourceAttributeKey, errorMessage) {
        try {
            debugger;
            let regexMobilePhoneFormat = /^00\d{12}$/;

            let formContext = executionContext.getFormContext();
            let sourceAttribute = formContext.getAttribute(sourceAttributeKey);
            if (typeof (sourceAttribute) != "undefined" && sourceAttribute != null) {
                let sourceAttributeValue = sourceAttribute.getValue();
                if (typeof (sourceAttributeValue) != 'undefined' && sourceAttributeValue != null) {
                    let formattedValue = sourceAttributeValue.replace(/\s+/g, "");
                    if (!formattedValue.match(regexMobilePhoneFormat)) {
                        formContext.getControl(sourceAttributeKey).setNotification(errorMessage, VALIDATE_MOBILE_PHONE_UNIQUE_ID);
                    }
                    else {
                        formContext.getControl(sourceAttributeKey).clearNotification(VALIDATE_MOBILE_PHONE_UNIQUE_ID);
                    }
                }
            }
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },
    showProgressIndicator: function () {
        Xrm.Utility.showProgressIndicator(PROGRESS_INDICATOR_TEXT);
    },
    hideProgressIndicator: function () {
        Xrm.Utility.closeProgressIndicator();
    }
}

