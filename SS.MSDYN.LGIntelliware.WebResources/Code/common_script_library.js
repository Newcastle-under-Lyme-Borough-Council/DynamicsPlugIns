// Define namespace
if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

// Unique ids for notifications
let COMPARE_DATE_UNIQUE_ID = "COMPARE_DATE_UNIQUE_ID";
let COMPARE_YEAR_UNIQUE_ID = "COMPARE_YEAR_UNIQUE_ID";
let VALIDATE_MOBILE_PHONE_UNIQUE_ID = "VALIDATE_MOBILE_PHONE_UNIQUE_ID";
// Text to show in progress indicator
let PROGRESS_INDICATOR_TEXT = "Processing...";

// Common helper methods for form logic
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
    //Show or hide a tab on the form
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
    //Show or hide a section within a tab
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
    //Compare two date fields
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
    //Compare year values between two date fields
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
    //Validate mobile phone format (must start with '00' and contain 12 digits)
    validateMobilePhoneFormat: function (executionContext, sourceAttributeKey, errorMessage) {
        try {
            let regexMobilePhoneFormat = /^00\d{12}$/;
            let formContext = executionContext.getFormContext();
            let sourceAttribute = formContext.getAttribute(sourceAttributeKey);
            if (typeof (sourceAttribute) != "undefined" && sourceAttribute != null) {
                let sourceAttributeValue = sourceAttribute.getValue();
                if (typeof (sourceAttributeValue) != 'undefined' && sourceAttributeValue != null) {
                    let formattedValue = sourceAttributeValue.replace(/\s+/g, "");  //remove spaces
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
    //Show progress indicator with a standard message
    showProgressIndicator: function () {
        try {
            Xrm.Utility.showProgressIndicator(PROGRESS_INDICATOR_TEXT);
        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },
    // Hide progress indicator
    hideProgressIndicator: function () {
        try {
            Xrm.Utility.closeProgressIndicator();
        }
        catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    //Hie empty fields in tab
    hideEmptyFieldsInTab: function (formContext, tabName, excludedSections = []) {
        try {
            let tab = formContext.ui.tabs.get(tabName);
            if (!tab) return;

            tab.sections.forEach((section) => {
                if (excludedSections.includes(section.getName())) return;

                section.controls.forEach((control) => {
                    let attribute = control.getAttribute();
                    if (!attribute) return;

                    let value = attribute.getValue();
                    let type = attribute.getAttributeType();

                    if (["lookup", "boolean"].includes(type)) return;

                    control.setVisible(type === "optionset" ? value !== null : !!value);
                });
            });
        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    //Hide empty subgrids in tab
    hideEmptySubgridsInTab: function (formContext, tabName) {
        try {
            let tab = formContext.ui.tabs.get(tabName);
            if (!tab) return;

            tab.sections.forEach((section) => {
                section.controls.forEach((control) => {
                    if (control.getControlType() === "subgrid") {
                        control.addOnLoad(() => {
                            let recordCount = control.getGrid().getTotalRecordCount();
                            control.setVisible(recordCount > 0);
                            control.setDisabled(true);
                        });
                    }
                });
            });
        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    //Disable control in form
    disableFields: function (formContext, fieldNames = []) {
        try {
            fieldNames.forEach((field) => {
                let ctrl = formContext.getControl(field);
                if (ctrl) ctrl.setDisabled(true);
            });
        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    //Show or hide a section inside a specific tab
    tabSectionVisible: function (formContext, tabName, sectionName, visible) {
        try {
            // Get the tab by name
            let tab = formContext.ui.tabs.get(tabName);
            if (tab) {
                //Get the section by name
                let section = tab.sections.get(sectionName);
                if (section) {
                    section.setVisible(visible);
                }
                else {
                    return;
                }
            }
        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },
    //Show Hide field control in form
    showHideField: function (fieldControl, visible) {
        try {
            fieldControl.setVisible(visible);
        } catch (e) {
            SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
    },

    
  //Retrieves the Business Process Flow (BPF) ID by its unique name.
  getBPFIdByName: function (bpfName, callback) {
    try {
      // Build the web api query to fetch the workflow with the specified unique name and active state
      let query = `?$select=workflowid&$filter=uniquename eq '${bpfName}' and ${SS.MSDYN.LGIntelliware.WR.Constants.workFlowTableFields.stateCode} eq ${SS.MSDYN.LGIntelliware.WR.Constants.workFlowTableStateCode.active}`;
      // Execute the web api request to retrieve multiple records from the workflow entity
      Xrm.WebApi.retrieveMultipleRecords("workflow", query).then(
        function success(results) {
          // Check if any workflow records were returned
          if (results.entities.length > 0) {
            // Get the workflow ID of the first matching workflow
            let workflowId = results.entities[0].workflowid;
            callback(workflowId);
          } else {
            callback(null);
          }
        },
        function (e) {
          callback(null);
          SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
      );
    }
    catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  }

}

