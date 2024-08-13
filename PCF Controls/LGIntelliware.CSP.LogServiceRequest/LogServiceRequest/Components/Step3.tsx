import React, { useState, useEffect } from "react";
import Table from "react-bootstrap/Table";

import { useServiceStore } from "../store";

const Step3 = (props: any) => {
  const { serviceDetails, set, clear } = useServiceStore();

  const [customerHistory, setCustomerHistory] = useState([]);

  useEffect(() => {
    let customerId = serviceDetails.customerId;
    let query = `?$select=incidentid,ticketnumber,title,createdon,description,caseorigincode,prioritycode,_ss_serviceconfigurationid_value,statecode,statuscode&$filter=(_customerid_value eq ${customerId} and _ss_serviceconfigurationid_value eq ${serviceDetails.ss_serviceconfigurationid})&$orderby=title asc`;
    props.props.EContext.webAPI.retrieveMultipleRecords("incident", query).then(
      function success(results: any) {
        setCustomerHistory(results.entities);
      },
      function (error: any) {
        console.log(error.message);
      }
    );
    // }
  }, []);

  return (
    <div className="">
      {serviceDetails.ss_service_name !== "" && (
        <div className="breadcrumb mb-4">
          <span className="breadcrumb-text">
            {serviceDetails.ss_service_name}
          </span>
        </div>
      )}

      <div className="mb-3">
        <h2 className="h5 p-0">Resident History</h2>
      </div>
      <div className="row justify-content-center">
        <div className="col-md-12 col-12">
          <div className="table-style box-shadow">
            <Table hover responsive="md" className="m-0">
              <thead>
                <tr>
                  <th>Case Title</th>
                  <th>Case Number</th>
                  <th>Priority</th>
                  <th>Origin</th>
                  <th>Status Reason</th>
                  <th>Created On</th>
                </tr>
              </thead>
              <tbody>
                {customerHistory &&
                  customerHistory.map((curr, ind) => (
                    <tr key={ind}>
                      <td>{curr["title"]}</td>
                      <td>{curr["ticketnumber"]}</td>
                      <td>
                        {
                          curr[
                            "caseorigincode@OData.Community.Display.V1.FormattedValue"
                          ]
                        }
                      </td>
                      <td>
                        {
                          curr[
                            "prioritycode@OData.Community.Display.V1.FormattedValue"
                          ]
                        }
                      </td>

                      <td>
                        {
                          curr[
                            "statuscode@OData.Community.Display.V1.FormattedValue"
                          ]
                        }
                      </td>
                      <td>
                        {
                          curr[
                            "createdon@OData.Community.Display.V1.FormattedValue"
                          ]
                        }
                      </td>
                    </tr>
                  ))}
              </tbody>
            </Table>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Step3;
