import React, { useState, useEffect } from "react";
import Table from "react-bootstrap/Table";

import { useServiceStore } from "../store";

//Notifications
const Step2 = (props: any) => {
  const { serviceDetails, set, clear } = useServiceStore();

  const [notifications, setNotifications] = useState([]);

  useEffect(() => {
    let serviceConfgiurationId = serviceDetails.ss_serviceconfigurationid;
    let query = `?$select=ss_notificationid,ss_enddateandtime,ss_livenotification,ss_notificationnumber,_ss_serviceconfiguration_value,ss_startdateandtime,ss_title&$filter=(statecode eq 0 and statuscode eq 1 and ss_livenotification eq true and _ss_serviceconfiguration_value eq ${serviceConfgiurationId})&$orderby=ss_title asc`;
    props.props.EContext.webAPI
      .retrieveMultipleRecords("ss_notification", query)
      .then(
        function success(results: any) {
          setNotifications(results.entities);
        },
        function (error: any) {
          console.log(error.message);
        }
      );
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
        <h2 className="h5 p-0">Active Notifications</h2>
      </div>
      <div className="row justify-content-center">
        <div className="col-md-12 col-12">
          <div className="table-style box-shadow">
            <Table hover responsive="md" className="m-0">
              <thead>
                <tr>
                  <th>Notification No.</th>
                  <th>Title</th>
                  <th>Start Date</th>
                  <th>End Date</th>
                </tr>
              </thead>
              <tbody>
                {notifications.map((curr, ind) => (
                  <tr key={ind}>
                    <td>{curr["ss_notificationnumber"]}</td>
                    <td>{curr["ss_title"]}</td>
                    <td>
                      {
                        curr[
                          "ss_startdateandtime@OData.Community.Display.V1.FormattedValue"
                        ]
                      }
                    </td>
                    <td>
                      {
                        curr[
                          "ss_enddateandtime@OData.Community.Display.V1.FormattedValue"
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

export default Step2;
