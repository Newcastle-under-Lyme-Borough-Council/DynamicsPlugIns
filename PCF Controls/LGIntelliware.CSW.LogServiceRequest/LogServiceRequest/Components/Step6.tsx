import React, { useState, useEffect } from "react";
import Col from "react-bootstrap/Col";
import Form from "react-bootstrap/Form";
import Row from "react-bootstrap/Row";

import { FaHome, FaChevronRight } from "react-icons/fa";

import { useServiceStore } from "../store";

//Submit Service Job (Case)
const Step6 = (props: any) => {
  const { set, serviceDetails } = useServiceStore();
  const [service, setService] = useState(serviceDetails);

  useEffect(() => {
    if (serviceDetails.reportedByUserId === "") {
      set({
        ...serviceDetails,
        reportedByUserId: serviceDetails.customerId,
        reportedByUserFullName: serviceDetails.customerFullName,
      });
      setService({
        ...serviceDetails,
        reportedByUserId: serviceDetails.customerId,
        reportedByUserFullName: serviceDetails.customerFullName,
      });
    }
  }, []);

  const onHandleChange = (e: any) => {
    let details = e.target.value;

    set({
      ...serviceDetails,
      ss_description: details,
    });
  };
  const HandleApplicationReferenceNumber = (e: any) => {
    let ApplicationReferenceNumber = e.target.value;

    set({
      ...serviceDetails,
      ss_applicationreferencenumber: ApplicationReferenceNumber,
    });
  };

  return (
    <div>
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
      <h2 className="h5">
        {serviceDetails.ss_allowservicerequest
          ? "Submit Enquiry / Service Request"
          : "Submit Enquiry"}
      </h2>
      <div className="row mt-4">
        <div className="col-md-12">
          <div className="bg-white p-5 box-shadow rounded-2">
            <Form>
              <Form.Group
                as={Row}
                className="mb-3"
                controlId="formPlaintextEmail"
              >
                <Form.Label column sm="2">
                  Service Request
                </Form.Label>
                <Col sm="10">
                  <Form.Control
                    plaintext
                    readOnly
                    defaultValue={service.ss_service_name}
                  />
                </Col>
              </Form.Group>
              <Form.Group
                as={Row}
                className="mb-3"
                controlId="formPlaintextEmail"
              >
                <Form.Label column sm="2">
                  Resident
                </Form.Label>
                <Col sm="10">
                  <Form.Control
                    plaintext
                    readOnly
                    defaultValue={service.customerFullName}
                  />
                </Col>
              </Form.Group>
              {serviceDetails.ss_reportingonbehalfofsomeone && (
                <Form.Group
                  as={Row}
                  className="mb-3"
                  controlId="formPlaintextEmail"
                >
                  <Form.Label column sm="2">
                    Reporting On behalf of
                  </Form.Label>
                  <Col sm="10">
                    <Form.Control
                      plaintext
                      readOnly
                      defaultValue={service.reportedByUserFullName}
                    />
                  </Col>
                </Form.Group>
              )}
              {serviceDetails.ss_servicelogicalname ===
                "ss_planningpermission" &&
                serviceDetails.ss_notificationid === "" &&
                serviceDetails.incidentid === "" &&
                !serviceDetails.knowledgeBase && (
                  <Form.Group as={Row} className="mb-3">
                    <Form.Label column sm="2">
                      Application Reference Number
                    </Form.Label>
                    <Col sm="10" className="d-flex">
                      <span className="text-danger steric-style">*</span>
                      <Form.Control
                        className="reference-input"
                        onChange={HandleApplicationReferenceNumber}
                        defaultValue={
                          serviceDetails?.ss_applicationreferencenumber
                        }
                        type="text"
                        pattern="\d*" // This allows only digits
                        onKeyPress={(event) => {
                          if (!/^\d+$/.test(event.key)) {
                            event.preventDefault(); // Prevent non-numeric characters
                          }
                        }}
                      />
                    </Col>
                  </Form.Group>
                )}
              <Form.Group
                as={Row}
                className="mb-3"
                controlId="formPlaintextPassword"
              >
                <Form.Label column sm="2">
                  Description
                </Form.Label>
                <Col sm="10" className="d-flex">
                  <span className="text-danger steric-style">*</span>
                  <Form.Control
                    as="textarea"
                    rows={5}
                    defaultValue={
                      serviceDetails.ss_description
                        ? serviceDetails.ss_description
                        : ""
                    }
                    onChange={onHandleChange}
                  />
                </Col>
              </Form.Group>
            </Form>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Step6;
