if (typeof SS === "undefined") {
  SS = {};
}
if (typeof SS.MSDYN === "undefined") {
  SS.MSDYN = {};
}
if (typeof SS.MSDYN.LGIntelliware === "undefined") {
  SS.MSDYN.LGIntelliware = {};
}
if (typeof SS.MSDYN.LGIntelliware.WR === "undefined") {
  SS.MSDYN.LGIntelliware.WR = {};
}

SS.MSDYN.LGIntelliware.WR.TaxiLicence = {
  onLoad: function (executionContext) {
    try {
      // Show/Hide columns...
      SS.MSDYN.LGIntelliware.WR.TaxiLicence.showHideTabBasedOnFieldValue(
        executionContext
      );
      SS.MSDYN.LGIntelliware.WR.TaxiLicence.serviceBasedBPF(executionContext);
      SS.MSDYN.LGIntelliware.WR.TaxiLicence.registerAddOnStageChangeEvent(executionContext);
      SS.MSDYN.LGIntelliware.WR.TaxiLicence.disableBpfFields(executionContext);
      SS.MSDYN.LGIntelliware.WR.TaxiLicence.bpfStatusChange(executionContext);
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  getMotHistoryButtonForm: function (primaryControl) {
    try {
      let formContext = primaryControl;
      let confirmStrings = {
        text: "Do you want to Get MOT History? You can't undo this action.",
        title: "Confirm Get MOT History",
      };
      let confirmOptions = { height: 200, width: 450 };
      Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
        function (success) {
          if (success.confirmed) {
            SS.MSDYN.LGIntelliware.WR.Common.showProgressIndicator();

            let entityId = formContext.data.entity
              .getId()
              .replace("{", "")
              .replace("}", "");
            let execute_ss_MOTGetMOTHistory_Request = {
              // Parameters
              entity: { entityType: "ss_taxilicence", id: entityId }, // entity

              getMetadata: function () {
                return {
                  boundParameter: "entity",
                  parameterTypes: {
                    entity: {
                      typeName: "mscrm.ss_taxilicence",
                      structuralProperty: 5,
                    },
                  },
                  operationType: 0,
                  operationName: "ss_MOTGetMOTHistory",
                };
              },
            };

            Xrm.WebApi.execute(execute_ss_MOTGetMOTHistory_Request)
              .then(function success(response) {
                if (response.ok) {
                  SS.MSDYN.LGIntelliware.WR.Common.hideProgressIndicator();
                }
              })
              .catch(function (e) {
                SS.MSDYN.LGIntelliware.WR.Common.hideProgressIndicator();
                SS.MSDYN.LGIntelliware.WR.Common.showError(e.message, false);
              });
          }
        }
      );
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  dvlaEnquireVehicleButtonForm: function (primaryControl) {
    try {
      let formContext = primaryControl;
      let confirmStrings = {
        text: "Do you want to Enquire Vehicle from DVLA? You can't undo this action.",
        title: "Confirm DVLA Enquire Vehicle",
      };
      let confirmOptions = { height: 200, width: 450 };
      Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
        function (success) {
          if (success.confirmed) {
            SS.MSDYN.LGIntelliware.WR.Common.showProgressIndicator();

            let entityId = formContext.data.entity
              .getId()
              .replace("{", "")
              .replace("}", "");
            let execute_ss_DVLAEnquireVehicle_Request = {
              // Parameters
              entity: { entityType: "ss_taxilicence", id: entityId }, // entity

              getMetadata: function () {
                return {
                  boundParameter: "entity",
                  parameterTypes: {
                    entity: {
                      typeName: "mscrm.ss_taxilicence",
                      structuralProperty: 5,
                    },
                  },
                  operationType: 0,
                  operationName: "ss_DVLAEnquireVehicle",
                };
              },
            };

            Xrm.WebApi.execute(execute_ss_DVLAEnquireVehicle_Request)
              .then(function success(response) {
                if (response.ok) {
                  SS.MSDYN.LGIntelliware.WR.Common.hideProgressIndicator();
                }
              })
              .catch(function (e) {
                SS.MSDYN.LGIntelliware.WR.Common.hideProgressIndicator();
                SS.MSDYN.LGIntelliware.WR.Common.showError(e.message, false);
              });
          }
        }
      );
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  dvlaGetDriverDataButtonForm: function (primaryControl) {
    try {
      let formContext = primaryControl;
      let confirmStrings = {
        text: "Do you want to Get Driver Data from DVLA? You can't undo this action.",
        title: "Confirm Get Driver Data from DVLA",
      };
      let confirmOptions = { height: 200, width: 450 };
      Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
        function (success) {
          if (success.confirmed) {
            SS.MSDYN.LGIntelliware.WR.Common.showProgressIndicator();

            let entityId = formContext.data.entity
              .getId()
              .replace("{", "")
              .replace("}", "");
            let execute_ss_DVLAGetDriverData_Request = {
              // Parameters
              entity: { entityType: "ss_taxilicence", id: entityId }, // entity

              getMetadata: function () {
                return {
                  boundParameter: "entity",
                  parameterTypes: {
                    entity: {
                      typeName: "mscrm.ss_taxilicence",
                      structuralProperty: 5,
                    },
                  },
                  operationType: 0,
                  operationName: "ss_DVLAGetDriverData",
                };
              },
            };

            Xrm.WebApi.execute(execute_ss_DVLAGetDriverData_Request)
              .then(function success(response) {
                if (response.ok) {
                  SS.MSDYN.LGIntelliware.WR.Common.hideProgressIndicator();
                }
              })
              .catch(function (e) {
                SS.MSDYN.LGIntelliware.WR.Common.hideProgressIndicator();
                SS.MSDYN.LGIntelliware.WR.Common.showError(e.message, false);
              });
          }
        }
      );
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  dbsCheckStatusButtonForm: function (primaryControl) {
    try {
      let formContext = primaryControl;
      let confirmStrings = {
        text: "Do you want to Check Status from DBS? You can't undo this action.",
        title: "Confirm DBS Check Status",
      };
      let confirmOptions = { height: 200, width: 450 };
      Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
        function (success) {
          if (success.confirmed) {
            SS.MSDYN.LGIntelliware.WR.Common.showProgressIndicator();

            let entityId = formContext.data.entity
              .getId()
              .replace("{", "")
              .replace("}", "");
            let execute_ss_DBSCheckStatus_Request = {
              // Parameters
              entity: { entityType: "ss_taxilicence", id: entityId }, // entity

              getMetadata: function () {
                return {
                  boundParameter: "entity",
                  parameterTypes: {
                    entity: {
                      typeName: "mscrm.ss_taxilicence",
                      structuralProperty: 5,
                    },
                  },
                  operationType: 0,
                  operationName: "ss_DBSCheckStatus",
                };
              },
            };

            Xrm.WebApi.execute(execute_ss_DBSCheckStatus_Request)
              .then(function success(response) {
                if (response.ok) {
                  SS.MSDYN.LGIntelliware.WR.Common.hideProgressIndicator();
                }
              })
              .catch(function (e) {
                SS.MSDYN.LGIntelliware.WR.Common.hideProgressIndicator();
                SS.MSDYN.LGIntelliware.WR.Common.showError(e.message, false);
              });
          }
        }
      );
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  isServiceSelectedDBS: function (primaryControl) {
    try {
      let formContext = primaryControl;
      let serviceConfigurationLookup = formContext
        .getControl("header_ss_serviceconfiguration")
        .getAttribute();

      if (
        serviceConfigurationLookup.getValue() &&
        serviceConfigurationLookup.getValue().length > 0 &&
        serviceConfigurationLookup.getValue()[0].name
      ) {
        let serviceConfigurationName = serviceConfigurationLookup
          .getValue()[0]
          .name.toLowerCase();

        if (
          serviceConfigurationName === "taxi licence - private hire operator"
        ) {
          return true;
        } else if (
          serviceConfigurationName ===
          "taxi licence - dual hackney carriage and private hire"
        ) {
          return true;
        } else {
          return true;
        }
      }

      return true;
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  isServiceSelectedMOT: function (primaryControl) {
    try {
      let formContext = primaryControl;
      let serviceConfigurationLookup = formContext
        .getControl("header_ss_serviceconfiguration")
        .getAttribute();

      // Check if the lookup field has a value
      if (
        serviceConfigurationLookup.getValue() &&
        serviceConfigurationLookup.getValue().length > 0 &&
        serviceConfigurationLookup.getValue()[0].name
      ) {
        let serviceConfigurationName = serviceConfigurationLookup
          .getValue()[0]
          .name.toLowerCase();

        if (
          serviceConfigurationName ===
          "taxi licence - hackney carriage vehicle" ||
          serviceConfigurationName === "taxi licence - private hire vehicle"
        ) {
          return true;
        } else {
          return true;
        }
      }

      // Default return if no value is set in the lookup field
      return true;
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  isServiceSelectedDVLA: function (primaryControl) {
    try {
      let formContext = primaryControl;
      let serviceConfigurationLookup = formContext
        .getControl("header_ss_serviceconfiguration")
        .getAttribute();
      const radioButtons = formContext.getAttribute(
        "ss_whatareyounotifyingusoff"
      );
      const selectedValue = radioButtons.getValue();
      // Check if the lookup field has a value
      if (
        serviceConfigurationLookup.getValue() &&
        serviceConfigurationLookup.getValue().length > 0 &&
        serviceConfigurationLookup.getValue()[0].name
      ) {
        let serviceConfigurationName = serviceConfigurationLookup
          .getValue()[0]
          .name.toLowerCase();

        if (
          serviceConfigurationName ===
          "taxi licence - notification of convictions and offences" &&
          selectedValue === 1
        ) {
          return true;
        } else if (
          serviceConfigurationName ===
          "taxi licence - dual hackney carriage and private hire"
        ) {
          return true;
        } else {
          return true;
        }
      }

      // Default return if no value is set in the lookup field
      return true;
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  showHideTabBasedOnFieldValue: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();

      // Check for the service configuration control
      if (formContext.getControl("header_ss_serviceconfiguration")) {
        let serviceConfigurationLookup = formContext
          .getControl("header_ss_serviceconfiguration")
          .getAttribute();
        if (
          serviceConfigurationLookup.getValue() &&
          serviceConfigurationLookup.getValue().length > 0 &&
          serviceConfigurationLookup.getValue()[0].name
        ) {
          let serviceConfigurationName = serviceConfigurationLookup
            .getValue()[0]
            .name.toLowerCase();

          // Show or hide tabs based on the service configuration
          if (
            serviceConfigurationName ===
            "taxi licence - dual hackney carriage and private hire"
          ) {
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "Dual_hackney_carriage_and_private_hire_details",
              true
            );
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_mot",
              false
            );
            SS.MSDYN.LGIntelliware.WR.TaxiLicence.hideEmptyFieldsInTabDHC(
              executionContext,
              "Dual_hackney_carriage_and_private_hire_details"
            );
            //SS.MSDYN.LGIntelliware.WR.TaxiLicence.hideEmptyFieldsOnReviewTab(executionContext);
          } else if (
            serviceConfigurationName ===
            "taxi licence - hackney carriage vehicle" ||
            serviceConfigurationName === "taxi licence - private hire vehicle"
          ) {
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_detail_sec_hackney_carriage_and_private_hire",
              true
            );
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_dvla_driver_data",
              false
            );
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_dvla_vehicle_details",
              false
            );
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_dbs",
              false
            );
            SS.MSDYN.LGIntelliware.WR.TaxiLicence.hideEmptyFieldsInTabHCV(
              executionContext,
              "tab_detail_sec_hackney_carriage_and_private_hire"
            );
          } else if (
            serviceConfigurationName === "taxi licence - private hire operator"
          ) {
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_detail_sec_private_hire_operator",
              true
            );
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_dvla_driver_data",
              false
            );
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_dvla_vehicle_details",
              false
            );
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_dbs",
              true
            );
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_mot",
              false
            );
            SS.MSDYN.LGIntelliware.WR.TaxiLicence.hideEmptyFieldsInTabPHO(
              executionContext,
              "tab_detail_sec_private_hire_operator"
            );
          } else if (
            serviceConfigurationName ===
            "taxi licence - notification of convictions and offences"
          ) {
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "Notification_of_convictions_details",
              true
            );
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_dvla_driver_data",
              false
            );
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_dvla_vehicle_details",
              false
            );
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_dbs",
              false
            );
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_mot",
              false
            );
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_pay360",
              false
            );

            // Get radio button field
            const radioButtons = formContext.getAttribute(
              "ss_whatareyounotifyingusoff"
            );

            // Get section controls by their names
            const motoringOffenceSection = formContext.ui.tabs
              .get("Notification_of_convictions_details")
              .sections.get("motorings_offence_details_section_4");
            const cautionWarningSection = formContext.ui.tabs
              .get("Notification_of_convictions_details")
              .sections.get("caution_or_warning_details_section_5");
            const policeInterviewSection = formContext.ui.tabs
              .get("Notification_of_convictions_details")
              .sections.get("questioned_or_interviewed_details_section_6");
            const convictionsSection = formContext.ui.tabs
              .get("Notification_of_convictions_details")
              .sections.get("convictions_details_section_7");
            const licenceDetailsSection = formContext.ui.tabs
              .get("Notification_of_convictions_details")
              .sections.get("licence_details_section_8");

            // Function to check and hide empty fields and sections
            function hideEmptyFields(section) {
              let allFieldsEmpty = true; // Track if all fields are empty

              section.controls.forEach((control) => {
                let attribute = control.getAttribute();
                if (attribute) {
                  let value = attribute.getValue();
                  if (!value) {
                    control.setVisible(false); // Hide control if empty
                  } else {
                    control.setVisible(true); // Show control if it has value
                    allFieldsEmpty = false; // Mark as not all fields empty
                  }
                }
              });

              // Hide entire section if all fields are empty
              section.setVisible(!allFieldsEmpty);
            }

            // Function to toggle sections based on the selected radio button value
            function toggleSections() {
              // Hide all sections first
              motoringOffenceSection.setVisible(false);
              cautionWarningSection.setVisible(false);
              policeInterviewSection.setVisible(false);
              convictionsSection.setVisible(false);
              licenceDetailsSection.setVisible(false); // Hide licence details section initially

              // Get the selected radio button value
              const selectedValue = radioButtons.getValue();

              // Show the corresponding section based on the selected radio button value
              if (selectedValue) {
                switch (selectedValue.toString()) {
                  case "1":
                    hideEmptyFields(motoringOffenceSection);
                    SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
                      executionContext,
                      "tab_dvla_driver_data",
                      true
                    );
                    SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
                      executionContext,
                      "tab_dvla_vehicle_details",
                      true
                    );
                    break;
                  case "2":
                    hideEmptyFields(cautionWarningSection);
                    break;
                  case "3":
                    hideEmptyFields(policeInterviewSection);
                    break;
                  case "4":
                    hideEmptyFields(convictionsSection);
                    break;
                  default:
                    break;
                }
              }

              // Check the licence details section for visibility
              hideEmptyFields(licenceDetailsSection); // Check if licence details should be visible

              // Hide the radio button column if it has no value
              if (!selectedValue) {
                formContext
                  .getControl("ss_whatareyounotifyingusoff")
                  .setVisible(false);
              } else {
                formContext
                  .getControl("ss_whatareyounotifyingusoff")
                  .setVisible(true);
              }
            }

            // Add event listener to the radio buttons to handle change events
            radioButtons.controls.forEach((control) => {
              control.addOnChange(() => {
                toggleSections(); // Call the function to show/hide sections
              });
            });

            // Initialize the visibility based on the selected radio button value on form load
            toggleSections();
          } else {
            // If no relevant configuration, hide both tabs
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "tab_applicant_details",
              false
            );
            SS.MSDYN.LGIntelliware.WR.Common.showHideTab(
              executionContext,
              "Notification_of_convictions_details",
              false
            );
          }
        }
      } else {
        SS.MSDYN.LGIntelliware.WR.Common.showError(
          "Unable to find relevant service to show application details. Please contact system administrator for more information.",
          false
        );
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  hideEmptyFieldsOnReviewTab: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext(); // Get the form context
      let attributes = formContext.data.entity.attributes.get(); // Get all attributes (fields)

      attributes.forEach(function (attribute) {
        let fieldName = attribute.getName();
        let control = formContext.getControl(fieldName);

        if (control && !attribute.getValue()) {
          // Check if the field is empty
          if (
            control.getControlType() === "standard" ||
            control.getControlType() === "boolean"
          ) {
            // Standard input or checkbox
            control.setVisible(false); // Hide the empty field or checkbox
          }
        }
      });
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  hideEmptyFieldsInTabHCV: function (executionContext, tabName) {
    let formContext = executionContext.getFormContext();
    let tab = formContext.ui.tabs.get(tabName);

    if (!tab) return;

    const areYouApplyingAs = formContext
      .getAttribute("ss_additionalquestionsareyouapplyingas")
      .getValue();
    const jointOwnershipSection = tab.sections.get(
      "tab_detail_sec_joint_ownership"
    );
    const companyDetailsSection = tab.sections.get(
      "tab_detail_sec_company_details"
    );
    const declaration = tab.sections.get("tab_detail_sec_declaration");

    // Generic function to update section visibility based on subgrid record count
    const updateSectionVisibility = function (gridControlName, section) {
      let gridControl = formContext.getControl(gridControlName);
      if (gridControl) {
        gridControl.addOnLoad(function () {
          const totalRecordCount = gridControl.getGrid().getTotalRecordCount();
          if (section !== companyDetailsSection) {
            section.setVisible(totalRecordCount > 0);
          } else if (section === companyDetailsSection) {
            gridControl.setVisible(totalRecordCount > 0);
          }
        });
      }
    };

    // Generic function to handle visibility logic based on conditions
    const handleSectionVisibility = function () {
      updateSectionVisibility("Subgrid_declaration", declaration);
      switch (areYouApplyingAs) {
        case 1:
          if (jointOwnershipSection) jointOwnershipSection.setVisible(false);
          if (companyDetailsSection) companyDetailsSection.setVisible(false);
          break;
        case 2:
          if (companyDetailsSection) companyDetailsSection.setVisible(false);
          updateSectionVisibility(
            "Subgrid_joint_ownership",
            jointOwnershipSection
          );
          break;
        case 3:
          if (jointOwnershipSection) jointOwnershipSection.setVisible(false);
          updateSectionVisibility("Subgrid_directors", companyDetailsSection);
          break;
        default:
          if (jointOwnershipSection) jointOwnershipSection.setVisible(false);
          if (companyDetailsSection) companyDetailsSection.setVisible(false);
      }
    };

    // Generic function to hide empty fields in a given tab
    const hideEmptyFields = function () {
      tab.sections.forEach(function (section) {
        section.controls.forEach(function (control) {
          let attribute = control.getAttribute();
          if (!attribute) return;

          let value = attribute.getValue();
          let attributeType = attribute.getAttributeType();

          if (["lookup", "boolean"].includes(attributeType)) return;

          control.setVisible(
            attributeType === "optionset"
              ? value !== null
              : value !== null && value !== undefined && value !== ""
          );
        });
      });
    };

    // Attach TabStateChange to handle subgrid and field visibility only when the tab is displayed
    tab.addTabStateChange(function () {
      if (tab.getDisplayState() === "expanded") {
        handleSectionVisibility();
        hideEmptyFields();
      }
    });
  },
  hideEmptyFieldsInTabDHC: function (executionContext, tabName) {
    try {
      let formContext = executionContext.getFormContext();
      let tab = formContext.ui.tabs.get(tabName);
      if (!tab) return;

      const hideEmptyFields = function () {
        tab.sections.forEach(function (section) {
          // Skip the section named 'tab_details_sec_office_use_only'
          if (section.getName() === "tab_details_sec_office_use_only") {
            return; // Skip this section
          }

          // Apply the hide logic to other sections
          section.controls.forEach(function (control) {
            let attribute = control.getAttribute();
            if (!attribute) return;

            let value = attribute.getValue();
            let attributeType = attribute.getAttributeType();

            // Exclude 'lookup' and 'boolean' types from the logic
            if (["lookup", "boolean"].includes(attributeType)) return;

            // Set visibility based on attribute type and value
            control.setVisible(
              attributeType === "optionset"
                ? value !== null
                : value !== null && value !== undefined && value !== ""
            );
          });
        });
      };

      // Function to update visibility of subgrids based on record count
      const hideshowSubgrid = function () {
        tab.sections.forEach(function (section) {
          section.controls.forEach(function (control) {
            if (control.getControlType() === "subgrid") {
              let subgridControl = control;
              subgridControl.addOnLoad(function () {
                let recordCount = subgridControl
                  .getGrid()
                  .getTotalRecordCount();
                // Hide subgrid if there are no records, show otherwise
                subgridControl.setVisible(recordCount > 0);
                subgridControl.setDisabled(true);
              });
            }
          });
        });
      };

      // Attach TabStateChange to handle subgrid and field visibility only when the tab is displayed
      tab.addTabStateChange(function () {
        if (tab.getDisplayState() === "expanded") {
          // Call hideEmptyFields when tab is expanded
          hideEmptyFields();
          hideshowSubgrid();
        }
      });
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  hideEmptyFieldsInTabPHO: function (executionContext, tabName) {
    let formContext = executionContext.getFormContext();
    let tab = formContext.ui.tabs.get(tabName);

    if (!tab) return;

    const areYouApplyingAs = formContext
      .getAttribute("ss_additionalquestionsareyouapplyingas")
      .getValue();
    const companyDetails = tab.sections.get("tab_details_sec_company_details");
    const detailsJointApplicants = tab.sections.get(
      "tab_detail_sec_details_of_join_applicants"
    );
    const individualApplicants = tab.sections.get(
      "tab_detail_section_individual_applicant_details"
    );
    const evidenceRightToWork = tab.sections.get(
      "tab_detail_sec_evidence_of_right_to_Work_in_the_uk"
    );
    const evidenceRightToWorkJointOwner = tab.sections.get(
      "tab_detail_sec_evidence_of_right_to_work_in_the_uk_joint"
    );

    // Function to update visibility of subgrids based on record count
    const hideshowSubgrid = function () {
      tab.sections.forEach(function (section) {
        section.controls.forEach(function (control) {
          if (control.getControlType() === "subgrid") {
            let subgridControl = control;
            subgridControl.addOnLoad(function () {
              let recordCount = subgridControl.getGrid().getTotalRecordCount();
              // Hide subgrid if there are no records, show otherwise
              subgridControl.setVisible(recordCount > 0);
              subgridControl.setDisabled(true);
            });
          }
        });
      });
    };

    const handleShowSectionVisibility = function () {
      switch (areYouApplyingAs) {
        case 1:
          if (companyDetails) companyDetails.setVisible(false);
          if (detailsJointApplicants) detailsJointApplicants.setVisible(false);
          if (evidenceRightToWorkJointOwner)
            evidenceRightToWorkJointOwner.setVisible(false);
          break;
        case 2:
          if (companyDetails) companyDetails.setVisible(false);
          if (individualApplicants) individualApplicants.setVisible(false);
          if (evidenceRightToWork) evidenceRightToWork.setVisible(false);
          break;
        case 3:
          if (detailsJointApplicants) detailsJointApplicants.setVisible(false);
          if (evidenceRightToWorkJointOwner)
            evidenceRightToWorkJointOwner.setVisible(false);
          if (evidenceRightToWork) evidenceRightToWork.setVisible(false);
          break;
      }
    };

    const hideEmptyFields = function () {
      tab.sections.forEach(function (section) {
        section.controls.forEach(function (control) {
          let attribute = control.getAttribute();
          if (!attribute) return;

          let value = attribute.getValue();
          let attributeType = attribute.getAttributeType();

          if (["lookup", "boolean"].includes(attributeType)) return;

          control.setVisible(
            attributeType === "optionset"
              ? value !== null
              : value !== null && value !== undefined && value !== ""
          );
        });
      });
    };

    // Attach TabStateChange to handle subgrid and field visibility only when the tab is displayed
    tab.addTabStateChange(function () {
      if (tab.getDisplayState() === "expanded") {
        hideshowSubgrid();
        hideEmptyFields();
        handleShowSectionVisibility();
      }
    });
  },
  getBPFIdByName: function (bpfName, callback) {
    try {
      let query = `?$select=workflowid&$filter=uniquename eq '${bpfName}'  and statecode eq 1`;
      Xrm.WebApi.retrieveMultipleRecords("workflow", query).then(
        function success(results) {
          if (results.entities.length > 0) {
            let workflowId = results.entities[0].workflowid;
            callback(workflowId);
          } else {
            callback(null);
          }
        },
        function (error) {
          callback(null);
          SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        }
      );
    }
    catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  serviceBasedBPF: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      if (formContext.getControl("header_ss_serviceconfiguration")) {
        let serviceConfigurationLookup = formContext.getControl("header_ss_serviceconfiguration").getAttribute();
        let lookupValue = serviceConfigurationLookup.getValue();
        if (lookupValue && lookupValue.length > 0 && lookupValue[0].name) {
          let serviceConfigurationName = lookupValue[0].name.toLowerCase();
          const bpfMapping = {
            "taxi licence - dual hackney carriage and private hire": "ss_bpf_dualhackneycarriagevehicle",
            "taxi licence - private hire operator": "ss_bpf_privatehireoperatorvehicle",
            "taxi licence - notification of convictions and offences": "ss_bpf_notificationofconvictionsandoffencesvehicle",
            "taxi licence - hackney carriage vehicle": "ss_bpf_hackneycarriagevehicle",
            "taxi licence - private hire vehicle": "ss_bpf_privatehirevehicle"
          };
          let bpfName = bpfMapping[serviceConfigurationName] || "ss_bpf_hackneycarriagevehicle";
          SS.MSDYN.LGIntelliware.WR.TaxiLicence.getBPFIdByName(bpfName, function (bpfId) {
            if (bpfId) {
              formContext.data.process.setActiveProcess(bpfId, function (status) {
                if (status === "success") {
                  SS.MSDYN.LGIntelliware.WR.TaxiLicence.tabFocusOnBPFStageChange(executionContext);
                }
              });
            }
          });
        }
      }
    }
    catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  setExpiryAndGrantDate: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      if (formContext.getControl("ss_newlicenceissuedate")) {
        let issueDate = formContext.getAttribute("ss_newlicenceissuedate").getValue();
        if (issueDate !== null) {
          if (!(issueDate instanceof Date)) {
            issueDate = new Date(issueDate);
          }
          let expiryDate = new Date(issueDate);
          expiryDate.setFullYear(expiryDate.getFullYear() + 1);
          expiryDate.setDate(expiryDate.getDate() - 1);
          let today = new Date();
          formContext.getAttribute("ss_newlicencegrantdate").setValue(today);
          formContext.getAttribute("ss_newlicenceexpirydate").setValue(expiryDate);
        }
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  registerAddOnStageChangeEvent: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      formContext.data.process.addOnStageChange(function (stageContext) {
        SS.MSDYN.LGIntelliware.WR.TaxiLicence.tabFocusOnBPFStageChange(stageContext);
      });
    }
    catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  tabFocusOnBPFStageChange: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      let activeProcess = formContext.data.process.getActiveProcess();
      let activeStage = formContext.data.process.getActiveStage();
      if (activeProcess && activeStage) {
        let processStageName = activeStage.getName();
        let applicationReview = '';
        if (formContext.getControl("header_ss_serviceconfiguration")) {
          let serviceConfigurationLookup = formContext
            .getControl("header_ss_serviceconfiguration")
            .getAttribute();
          if (
            serviceConfigurationLookup.getValue() &&
            serviceConfigurationLookup.getValue().length > 0 &&
            serviceConfigurationLookup.getValue()[0].name
          ) {
            let serviceConfigurationName = serviceConfigurationLookup
              .getValue()[0]
              .name.toLowerCase();
            const serviceConfigurationMap = {
              "taxi licence - dual hackney carriage and private hire":
                "Dual_hackney_carriage_and_private_hire_details",
              "taxi licence - hackney carriage vehicle":
                "tab_detail_sec_hackney_carriage_and_private_hire",
              "taxi licence - private hire vehicle":
                "tab_detail_sec_hackney_carriage_and_private_hire",
              "taxi licence - private hire operator":
                "tab_detail_sec_private_hire_operator",
              "taxi licence - notification of convictions and offences":
                "Notification_of_convictions_details"
            };
            applicationReview = serviceConfigurationMap[serviceConfigurationName] || '';
          }
        }
        let stageToTabMap = {
          "Review Application": applicationReview,
          "DVLA (Driver Data) Review": "tab_dvla_driver_data",
          "DVLA (Vehicle Details) Review": "tab_dvla_vehicle_details",
          "DBS Review": "tab_dbs",
          "Payment Review": "tab_pay360",
          "Review MOT History": "tab_mot",
          "Grant": "tab_summary",
          "Reject": "tab_summary"
        };
        if (activeStage.getName() == "Review MOT History") {
          formContext.getAttribute("statecode").setValue(0);
          formContext.getAttribute("statuscode").setValue(717800005);
        }
        else if (activeStage.getName() == "Payment Review") {
          formContext.getAttribute("statecode").setValue(0);
          formContext.getAttribute("statuscode").setValue(717800006);
        }
        else if (activeStage.getName() == "Grant/Reject") {
          formContext.getAttribute("statecode").setValue(0);
          formContext.getAttribute("statuscode").setValue(717800007);
        }
        formContext.data.save().then(() => {
          formContext.data.refresh();
        }).catch((e) => {
          SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
        });

        let tabName = stageToTabMap[processStageName.trim()];
        if (tabName !== undefined) {
          let tab = formContext.ui.tabs.get(tabName);
          if (tab) {
            tab.setFocus();
          }
        }
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  disableBpfFields: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      let bpfFieldNames = ['header_process_ss_paid', 'header_process_ss_whatareyounotifyingusoff'];
      bpfFieldNames.forEach(function (fieldName) {
        let bpfControl = formContext.getControl(fieldName);
        if (bpfControl) {
          bpfControl.setDisabled(true);
        }
      });
      let badgeNumber = formContext.getControl("header_process_ss_newbadgenumber");
      if (badgeNumber) {
        badgeNumber.getAttribute().addOnChange(function () {
          let value = badgeNumber.getAttribute().getValue();
          if (value && !/^\d*$/.test(value)) {
            badgeNumber.setNotification("Only numeric values are allowed.");
          } else {
            badgeNumber.clearNotification();
          }
        });
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  bpfStatusChange: function (executionContext) {
    var formContext = executionContext.getFormContext();
    var process = formContext.data.process;
    // Add an event listener for process status change
    process.addOnProcessStatusChange(function () {
      var bpfStatus = process.getStatus();
      if (bpfStatus === "finished") {
        if (formContext.getControl("header_ss_serviceconfiguration")) {
          let serviceConfigurationLookup = formContext
            .getControl("header_ss_serviceconfiguration")
            .getAttribute();
          if (
            serviceConfigurationLookup.getValue() &&
            serviceConfigurationLookup.getValue().length > 0 &&
            serviceConfigurationLookup.getValue()[0].name
          ) {
            let serviceConfigurationName = serviceConfigurationLookup
              .getValue()[0]
              .name.toLowerCase();
            if (
              serviceConfigurationName ===
              "taxi licence - notification of convictions and offences"
            ) {
              formContext.getAttribute("statecode").setValue(1);
              formContext.getAttribute("statuscode").setValue(717800004);
            } else {
              let grantReject = formContext.getAttribute("ss_grantreject").getValue();
              if (grantReject === 0) {
                formContext.getAttribute("statecode").setValue(1);
                formContext.getAttribute("statuscode").setValue(2);
              } else if (grantReject === 1) {
                formContext.getAttribute("statecode").setValue(1);
                formContext.getAttribute("statuscode").setValue(717800002);
              }
            }
            formContext.data.entity.save();
          }
        }
      }
    });
  }
};
