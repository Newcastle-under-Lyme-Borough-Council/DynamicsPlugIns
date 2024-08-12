import React from "react";
import Col from "react-bootstrap/Col";
import Form from "react-bootstrap/Form";
import Row from "react-bootstrap/Row";

import { useServiceStore } from "../store";

//Submit Service Job (Case)
const Step4 = (props: any) => {
  const { set, serviceDetails } = useServiceStore();

  const onHandleChange = (e: any) => {
    let details = e.target.value;

    set({
      ...serviceDetails,
      ss_description: details,
    });
  };

  return (
    <div>
      {serviceDetails.ss_service_name !== "" && (
        <div className="breadcrumb mb-4">
          <span className="breadcrumb-text">
            {serviceDetails.ss_service_name}
          </span>
        </div>
      )}
      <h2 className="h5">Submit Service Request</h2>
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
                    defaultValue={serviceDetails.ss_service_name}
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
                    defaultValue={serviceDetails.customerFullName}
                  />
                </Col>
              </Form.Group>
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

export default Step4;
