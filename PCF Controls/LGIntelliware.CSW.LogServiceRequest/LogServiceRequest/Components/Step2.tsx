import React, { useState, useEffect } from "react";
import Table from "react-bootstrap/Table";

import { useServiceStore } from "../store";
import { Button, Form } from "react-bootstrap";

import { FaHome, FaChevronRight } from "react-icons/fa";
import moment from "moment";

//Notifications
const Step2 = (props: any) => {
  const { serviceDetails, set, clear } = useServiceStore();

  const [notifications, setNotifications] = useState([]);
  // const [selectedNotification, setSelectedNotification]: any = useState({
  //   ss_notificationid: serviceDetails.ss_notificationid,
  // });

  useEffect(() => {
    let serviceConfgiurationId = serviceDetails.ss_serviceconfigurationid;
    let query = `?$select=ss_notificationid,ss_description,ss_enddateandtime,ss_livenotification,ss_notificationnumber,_ss_serviceconfiguration_value,ss_startdateandtime,ss_title&$filter=(statecode eq 0 and statuscode eq 1 and ss_livenotification eq true and _ss_serviceconfiguration_value eq ${serviceConfgiurationId})&$orderby=ss_title asc`;
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

  // const selectNotification = (notification: any) => {
  //   setSelectedNotification(notification);
  //   set({
  //     ...serviceDetails,
  //     ss_notificationid: notification.ss_notificationid,
  //     ss_notification_title: notification.ss_title,
  //     incidentid: "",
  //     title: "",
  //     knowledgeBase: false,
  //   });
  // };

  const selectNotification = (
    event: React.ChangeEvent<HTMLInputElement>,
    notification: any
  ) => {
    if (event.target.checked) {
      // setSelectedResidentHistory(history);
      set({
        ...serviceDetails,
        ss_notificationid: notification.ss_notificationid,
        ss_notification_title: notification.ss_title,
        incidentid: "",
        title: "",
        knowledgeBase: false,
        ss_applicationreferencenumber: "",
      });
    } else {
      // setSelectedResidentHistory(null);
      set({
        ...serviceDetails,
        ss_notificationid: "",
        ss_notification_title: "",
        incidentid: "",
        title: "",
        knowledgeBase: false,
        ss_applicationreferencenumber: "",
      });
    }
  };

  const onHandleKnowledge = (e: any) => {
    // serviceDetails.knowledgeBase = e.target.checked;
    if (e.target.checked === true) {
      set({
        ...serviceDetails,
        knowledgeBase: e.target.checked,
        ss_notificationid: "",
        ss_notification_title: "",
        incidentid: "",
        title: "",
        ss_applicationreferencenumber: "",
      });
    } else {
      set({
        ...serviceDetails,
        knowledgeBase: false,
        ss_notificationid: "",
        ss_notification_title: "",
        incidentid: "",
        title: "",
        ss_applicationreferencenumber: "",
      });
    }
  };

  return (
    <div className="">
      <div className="breadcrumb mb-4">
        <span className="breadcrumb-text">
          {serviceDetails.customerFullName}
        </span>
        {serviceDetails.reportedByUserId !== "" &&
          serviceDetails.ss_reportingonbehalfofsomeone === true && (
            <>
              <FaChevronRight className="breadcrumb-icon" />
              <span className="breadcrumb-text">
                {serviceDetails.reportedByUserFullName}
              </span>
            </>
          )}
        {serviceDetails.ss_service_name !== "" && (
          <>
            <FaChevronRight className="breadcrumb-icon" />
            <span className="breadcrumb-text">
              {serviceDetails.ss_service_name}
            </span>
          </>
        )}
        {serviceDetails.ss_notificationid !== "" && (
          <>
            <FaChevronRight className="breadcrumb-icon" />
            <span className="breadcrumb-text">
              {serviceDetails.ss_notification_title}
            </span>
          </>
        )}
        {serviceDetails.knowledgeBase === true && (
          <>
            <FaChevronRight className="breadcrumb-icon" />
            <span className="breadcrumb-text">Knowledge Base Enquiry</span>
          </>
        )}
      </div>

      <div className="mb-3">
        <h2 className="h5 p-0">Active Notifications</h2>
        {/* <h5 className="text-primary">Refresh History</h5> */}
        <Form className="d-flex mb-3 box-shadow rounded-2 p-3 bg-white">
          <Form.Check
            type="checkbox"
            id="1"
            label="Knowledge Base Enquiry"
            onChange={onHandleKnowledge}
            checked={serviceDetails.knowledgeBase}
          />
        </Form>
      </div>
      <div className="row justify-content-center">
        <div className="col-md-12 col-12">
          <div className="table-style box-shadow">
            <Table hover responsive="md" className="m-0">
              <thead>
                <tr>
                  <th style={{ width: "40px" }}></th>
                  <th style={{ width: "180px" }}>Notification Number</th>
                  <th style={{ width: "400px" }}>Title</th>
                  <th>Description</th>
                  <th style={{ width: "150px" }}>Start Date</th>
                  <th style={{ width: "150px" }}>End Date</th>
                  {/* <th>Live</th> */}
                </tr>
              </thead>
              <tbody>
                {notifications.map((curr, ind) => (
                  <tr key={ind}>
                    <td>
                      {/* <Button
                        variant="primary"
                        className="mt-md-0 mt-3"
                        onClick={() => selectNotification(curr)}
                      >
                        {curr["ss_notificationid"] ==
                        serviceDetails.ss_notificationid
                          ? "Selected"
                          : "Select"}
                      </Button> */}
                      <Form.Check
                        type="checkbox"
                        id={`checkbox-${ind}`}
                        onChange={(e) => selectNotification(e, curr)}
                        checked={
                          curr["ss_notificationid"] ===
                          serviceDetails.ss_notificationid
                        }
                      />
                    </td>
                    <td>{curr["ss_notificationnumber"]}</td>
                    <td>{curr["ss_title"]}</td>
                    <td>{curr["ss_description"]}</td>
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
                    {/* <td>
                      {
                        curr[
                          "ss_livenotification@OData.Community.Display.V1.FormattedValue"
                        ]
                      }
                    </td> */}
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
