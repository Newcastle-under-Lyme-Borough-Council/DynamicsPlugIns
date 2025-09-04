// Define namespace
if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

// Namespace for customer interaction-related functionality
SS.MSDYN.LGIntelliware.WR.CustomerInteraction = {
  //OnLoad event handler for the customer interaction form.
  onLoad: function (executionContext) {
    try {
      SS.MSDYN.LGIntelliware.WR.CustomerInteraction.populateFields(executionContext);
      SS.MSDYN.LGIntelliware.WR.CustomerInteraction.showHideFieldsOnCustomerInteraction(executionContext);
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  //Populate fields on the customer interaction form based on the regarding record.
  populateFields: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      // Get the value of the regarding field (lookup)
      var regardingObject = formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.customerInteractionTableFields.regardingObject).getValue();
      if (regardingObject !== null) {
        var entityId = regardingObject[0].id.replace("{", "")
          .replace("}", "");
        var entityType = regardingObject[0].entityType;
        // Based on the entity type retrieve fields and populate the form
        if (entityType === SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.taxiLicenceLogicalName) {
          SS.MSDYN.LGIntelliware.WR.CustomerInteraction.retrieveandsetfields(SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.taxiLicenceSchemaName, SS.MSDYN.LGIntelliware.WR.Constants.taxiLicenceTableFields.taxiLicenceId, entityId, formContext);
        }
        else if (entityType === SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.missedBinLogicalName) {
          SS.MSDYN.LGIntelliware.WR.CustomerInteraction.retrieveandsetfields(SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.missedBinSchemaName, SS.MSDYN.LGIntelliware.WR.Constants.missedBinTableFields.missedBinId, entityId, formContext);
        }
        else if (entityType === SS.MSDYN.LGIntelliware.WR.Constants.planningPermissionTableFields.planningPermissionLogicalName) {
        SS.MSDYN.LGIntelliware.WR.CustomerInteraction.retrieveandsetfields(SS.MSDYN.LGIntelliware.WR.Constants.planningPermissionTableFields.planningPermissionSchemaName, SS.MSDYN.LGIntelliware.WR.Constants.planningPermissionTableFields.planningPermissionId, entityId, formContext);
        }
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  //Retrieve related record fields and populate them on the customer interaction form.
  retrieveandsetfields: function (entitySchemaName, primarykey, entityId, formContext) {
    try {
      // Create XMLHttpRequest
      var req = new XMLHttpRequest();
      req.open(
        "GET",
        Xrm.Utility.getGlobalContext().getClientUrl() +
        `/api/data/v9.2/${entitySchemaName}?$select=${SS.MSDYN.LGIntelliware.WR.Constants.customerInteractionTableFields.customerLookupValue},${SS.MSDYN.LGIntelliware.WR.Constants.customerInteractionTableFields.serviceConfigurationValue}&$filter=${primarykey} eq '${entityId}'`,
        true
      );
      //Set request headers for OData
      req.setRequestHeader("OData-MaxVersion", "4.0");
      req.setRequestHeader("OData-Version", "4.0");
      req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
      req.setRequestHeader("Accept", "application/json");
      req.setRequestHeader("Prefer", "odata.include-annotations=*");
      //Handle response
      req.onreadystatechange = function () {
        if (this.readyState === 4) {
          req.onreadystatechange = null;
          if (this.status === 200) {
            var results = JSON.parse(this.response);
            if (results.value.length > 0) {
              var result = results.value[0];
              //Retrieve customer lookup field
              var ss_customer = result[SS.MSDYN.LGIntelliware.WR.Constants.customerInteractionTableFields.customerLookupValue];
              var ss_customer_formatted = result[SS.MSDYN.LGIntelliware.WR.Constants.customerInteractionTableFields.customerLookupValue+"@OData.Community.Display.V1.FormattedValue"];
              var ss_customer_lookuplogicalname = result[SS.MSDYN.LGIntelliware.WR.Constants.customerInteractionTableFields.customerLookupValue+"@Microsoft.Dynamics.CRM.lookuplogicalname"];
              //Retrieve service configuration lookup field
              var ss_serviceconfiguration = result[SS.MSDYN.LGIntelliware.WR.Constants.customerInteractionTableFields.serviceConfigurationValue];
              var ss_serviceconfiguration_formatted = result[SS.MSDYN.LGIntelliware.WR.Constants.customerInteractionTableFields.serviceConfigurationValue+"@OData.Community.Display.V1.FormattedValue"];
              var ss_serviceconfiguration_lookuplogicalname = result[SS.MSDYN.LGIntelliware.WR.Constants.customerInteractionTableFields.serviceConfigurationValue+"@Microsoft.Dynamics.CRM.lookuplogicalname"];
              // Set service configuration lookup on the form
              formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.customerInteractionTableFields.serviceConfiguration).setValue([
                {
                  id: ss_serviceconfiguration,
                  entityType: ss_serviceconfiguration_lookuplogicalname,
                  name: ss_serviceconfiguration_formatted,
                },
              ]);
              // Set customer lookup on the form
              formContext.getAttribute(SS.MSDYN.LGIntelliware.WR.Constants.customerInteractionTableFields.customer).setValue([
                {
                  id: ss_customer,
                  entityType: ss_customer_lookuplogicalname,
                  name: ss_customer_formatted,
                },
              ]);
            }
          }
        }
      };
      // Send the request
      req.send();
    }
    catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },

  //Show or hide fields on the customer interaction form
  showHideFieldsOnCustomerInteraction: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      if (formContext.ui.getFormType() ===SS.MSDYN.LGIntelliware.WR.Constants.formType.create) {
        // Hide/Enable fields
        formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.customerInteractionTableFields.response).setVisible(false);
        formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.customerInteractionTableFields.description).setDisabled(false);
        formContext.getControl(SS.MSDYN.LGIntelliware.WR.Constants.customerInteractionTableFields.subject).setDisabled(false);
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  }
};
