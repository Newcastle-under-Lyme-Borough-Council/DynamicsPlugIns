if (typeof (SS) === "undefined") { SS = {}; }
if (typeof (SS.MSDYN) === "undefined") { SS.MSDYN = {}; }
if (typeof (SS.MSDYN.LGIntelliware) === "undefined") { SS.MSDYN.LGIntelliware = {}; }
if (typeof (SS.MSDYN.LGIntelliware.WR) === "undefined") { SS.MSDYN.LGIntelliware.WR = {}; }

SS.MSDYN.LGIntelliware.WR.CustomerInteraction = {
  onLoad: function (executionContext) {
    try {
      SS.MSDYN.LGIntelliware.WR.CustomerInteraction.populateFields(executionContext);
      SS.MSDYN.LGIntelliware.WR.CustomerInteraction.showHideFieldsOnCustomerInteraction(executionContext);
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  populateFields: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      var regardingObject = formContext.getAttribute("regardingobjectid").getValue();
      if (regardingObject !== null) {
        var entityId = regardingObject[0].id.replace("{", "")
          .replace("}", "");
        var entityType = regardingObject[0].entityType;
        if (entityType === "ss_taxilicence") {
          SS.MSDYN.LGIntelliware.WR.CustomerInteraction.retrieveandsetfields("ss_taxilicences", "ss_taxilicenceid", entityId, formContext);
        }
        else if (entityType === "ss_missedbin") {
          SS.MSDYN.LGIntelliware.WR.CustomerInteraction.retrieveandsetfields("ss_missedbins", "ss_missedbinid", entityId, formContext);
        }
        else if (entityType === "ss_planningpermission") {
          SS.MSDYN.LGIntelliware.WR.CustomerInteraction.retrieveandsetfields("ss_planningpermissions", "ss_planningpermissionid", entityId, formContext);
        }
        else if (entityType === "ss_reportnoise") {
          SS.MSDYN.LGIntelliware.WR.CustomerInteraction.retrieveandsetfields("ss_reportnoises", "ss_reportnoiseid", entityId, formContext);
        }
        else if (entityType === "ss_abandonbin") {
          SS.MSDYN.LGIntelliware.WR.CustomerInteraction.retrieveandsetfields("ss_abandonbinses", "ss_abandonbinsid", entityId, formContext);
        }
        else if (entityType === "ss_abandonedvehicle") {
          SS.MSDYN.LGIntelliware.WR.CustomerInteraction.retrieveandsetfields("ss_abandonedvehicles", "ss_abandonedvehicleid", entityId, formContext);
        }
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  retrieveandsetfields: function (entitySchemaName, primarykey, entityId, formContext) {
    try {
      var req = new XMLHttpRequest();
      req.open(
        "GET",
        Xrm.Utility.getGlobalContext().getClientUrl() +
        `/api/data/v9.2/${entitySchemaName}?$select=_ss_customer_value,_ss_serviceconfiguration_value&$filter=${primarykey} eq '${entityId}'`,
        true
      );
      req.setRequestHeader("OData-MaxVersion", "4.0");
      req.setRequestHeader("OData-Version", "4.0");
      req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
      req.setRequestHeader("Accept", "application/json");
      req.setRequestHeader("Prefer", "odata.include-annotations=*");
      req.onreadystatechange = function () {
        if (this.readyState === 4) {
          req.onreadystatechange = null;
          if (this.status === 200) {
            var results = JSON.parse(this.response);
            if (results.value.length > 0) {
              var result = results.value[0];
              var ss_customer = result["_ss_customer_value"];
              var ss_customer_formatted = result["_ss_customer_value@OData.Community.Display.V1.FormattedValue"];
              var ss_customer_lookuplogicalname = result["_ss_customer_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
              var ss_serviceconfiguration = result["_ss_serviceconfiguration_value"];
              var ss_serviceconfiguration_formatted = result["_ss_serviceconfiguration_value@OData.Community.Display.V1.FormattedValue"];
              var ss_serviceconfiguration_lookuplogicalname = result["_ss_serviceconfiguration_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
              formContext.getAttribute("ss_serviceconfigurationid").setValue([
                {
                  id: ss_serviceconfiguration,
                  entityType: ss_serviceconfiguration_lookuplogicalname,
                  name: ss_serviceconfiguration_formatted,
                },
              ]);

              formContext.getAttribute("ss_customer").setValue([
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
      req.send();
    }
    catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  },
  showHideFieldsOnCustomerInteraction: function (executionContext) {
    try {
      let formContext = executionContext.getFormContext();
      if (formContext.ui.getFormType() === 1) {
        formContext.getControl("ss_response").setVisible(false);
        formContext.getControl("description").setDisabled(false);
        formContext.getControl("ss_subject").setDisabled(false);
      }
    } catch (e) {
      SS.MSDYN.LGIntelliware.WR.Common.showError(e, true);
    }
  }

};
