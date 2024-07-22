import React, { useState, useEffect } from "react";
import Table from "react-bootstrap/Table";
import { FaHome, FaChevronRight } from "react-icons/fa";

import { useServiceStore } from "../store";
import moment from "moment";
import { Button, Form } from "react-bootstrap";

//Customer History
const Step5 = (props: any) => {
  const { serviceDetails, set, clear } = useServiceStore();

  const [customerHistory, setCustomerHistory] = useState([]);
  // const [selectedResidentHistory, setSelectedResidentHistory]: any = useState({
  //   incidentid: serviceDetails.incidentid,
  // });

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

  const selectResidentHistory = (
    event: React.ChangeEvent<HTMLInputElement>,
    history: any
  ) => {
    if (event.target.checked) {
      // setSelectedResidentHistory(history);
      set({
        ...serviceDetails,
        incidentid: history.incidentid,
        title: history.ticketnumber,
        ss_notificationid: "",
        ss_notification_title: "",
        knowledgeBase: false,
      });
    } else {
      // setSelectedResidentHistory(null);
      set({
        ...serviceDetails,
        incidentid: "",
        title: "",
        ss_notificationid: "",
        ss_notification_title: "",
        knowledgeBase: false,
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
      });
    } else {
      set({
        ...serviceDetails,
        knowledgeBase: false,
        ss_notificationid: "",
        ss_notification_title: "",
        incidentid: "",
        title: "",
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
        {serviceDetails.incidentid !== "" && (
          <>
            <FaChevronRight className="breadcrumb-icon" />
            <span className="breadcrumb-text">{serviceDetails.title}</span>
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
        <h2 className="h5 p-0">Resident History</h2>
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
                  <th>Title</th>
                  <th>Case Number</th>
                  {/* <th>Priority</th>
                  <th>Origin</th> */}
                  <th>Status Reason</th>
                  <th>Created On</th>
                </tr>
              </thead>
              <tbody>
                {customerHistory &&
                  customerHistory.map((curr, ind) => (
                    <tr key={ind}>
                      <td>
                        {/* <Button
                          variant="primary"
                          className="mt-md-0 mt-3"
                          onClick={() => selectResidentHistory(curr)}
                        >
                          {curr["incidentid"] == serviceDetails.incidentid
                            ? "Selected"
                            : "Select"}
                        </Button> */}
                        <Form.Check
                          type="checkbox"
                          id={`checkbox-${ind}`}
                          onChange={(e) => selectResidentHistory(e, curr)}
                          checked={
                            curr["incidentid"] === serviceDetails.incidentid
                          }
                        />
                      </td>
                      <td>{curr["title"]}</td>
                      <td>{curr["ticketnumber"]}</td>

                      {/* <td>
                        {
                          curr[
                            "prioritycode@OData.Community.Display.V1.FormattedValue"
                          ]
                        }
                      </td>
                      <td>
                        {
                          curr[
                            "caseorigincode@OData.Community.Display.V1.FormattedValue"
                          ]
                        }
                      </td> */}

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

export default Step5;
