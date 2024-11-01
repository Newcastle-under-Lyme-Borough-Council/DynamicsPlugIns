import React, { useState } from "react";
import Button from "react-bootstrap/Button";
import Card from "react-bootstrap/Card";
// import CustomSearchDropdown from "./CustomSearchDropdown";
import InputGroup from "react-bootstrap/InputGroup";
import Form from "react-bootstrap/Form";
import { FaHome, FaChevronRight } from "react-icons/fa";

import { useServiceStore } from "../store";

//Find Customer
const Step3 = (props: any) => {
  const { serviceDetails, set, clear } = useServiceStore();
  const [customers, setCustomers]: any = useState([]);
  const [selectedCustomer, setSelectedCustomer]: any = useState({
    customerId: serviceDetails.contactid,
    customerFirstName: serviceDetails.customerFirstName,
    customerLastName: serviceDetails.customerLastName,
    customerAddress1_line1: serviceDetails.customerAddress1_line1,
    customerAddress1_postalcode: serviceDetails.customerAddress1_postalcode,
    customerEmailaddress1: serviceDetails.customerEmailaddress1,
    customerMobilephone: serviceDetails.customerMobilephone,
  });

  const searchCustomer = (e: any) => {
    let searchText = e.target.value;
    let query = `?$select=contactid,address1_line1,address1_postalcode,emailaddress1,firstname,fullname,lastname,mobilephone&$filter=((statecode eq 0 and statuscode eq 1) and (contains(fullname,'${searchText}') or contains(address1_line1,'${searchText}') or contains(address1_postalcode,'${searchText}') or contains(emailaddress1,'${searchText}') or contains(mobilephone,'${searchText}')))`;
    props.props.EContext.webAPI.retrieveMultipleRecords("contact", query).then(
      function success(results: any) {
        setCustomers(results.entities);
      },
      function (error: any) {
        console.log(error.message);
      }
    );
  };

  const selectCustomer = (customer: any) => {
    setSelectedCustomer({
      ...selectedCustomer,
      customerId: customer.contactid,
      customerFirstName: customer.firstname,
      customerLastName: customer.lastname,
      customerAddress1_line1: customer.address1_line1,
      customerAddress1_postalcode: customer.address1_postalcode,
      customerEmailaddress1: customer.emailaddress1,
      customerMobilephone: customer.mobilephone,
    });
    set({
      ...serviceDetails,
      customerId: customer.contactid,
      customerFirstName: customer.firstname,
      customerLastName: customer.lastname,
      customerFullName: customer.firstname + " " + customer.lastname,
      customerAddress1_line1: customer.address1_line1,
      customerAddress1_postalcode: customer.address1_postalcode,
      customerEmailaddress1: customer.emailaddress1,
      customerMobilephone: customer.mobilephone,
      ss_notificationid: "",
      ss_notification_title: "",
      incidentid: "",
      title: "",
    });
  };

  return (
    <div>
      {serviceDetails.customerId !== "" && (
        <div className="breadcrumb mb-4">
          <span className="breadcrumb-text">
            {serviceDetails.customerFullName}
          </span>
        </div>
      )}

      <h2 className="h5">Find a Resident</h2>
      <div className="row mt-4">
        <div className="col-md-6 box-style">
          <div className="box-shadow max-height-box-search p-3 rounded-2">
            <form className="d-flex mb-3">
              <InputGroup>
                <Form.Control
                  type="input"
                  placeholder="Search for a resident by fullname, address, email address, or mobile phone"
                  onChange={searchCustomer}
                  onKeyPress={(e) => {
                    if (e.key === "Enter") {
                      e.preventDefault();
                      e.stopPropagation();
                    }
                  }}
                  required
                />
                {/* <InputGroup.Text className="cursor-pointer" ><BsSearch /></InputGroup.Text> */}
              </InputGroup>
              {/* <button
                type="button"
                className="ms-3 mt-md-0 mt-3 btn btn-primary"
              >
                New Resident
              </button> */}
            </form>
            {customers.length !== 0 ? (
              customers.map((curr: any, ind: any) => (
                <div className="box-shadow p-3 rounded-2 mb-3 bg-white">
                  <div className="d-flex justify-content-between flex-wrap flex-md-nowrap">
                    <div>
                      <h5>{curr["firstname"] + " " + curr["lastname"]}</h5>
                      <p>
                        <strong className="fw-medium">Email:</strong>{" "}
                        {curr["emailaddress1"]}
                      </p>
                    </div>
                    <div className="d-md-flex align-items-md-start">
                      <Button
                        variant="primary"
                        className="mx-2 mt-md-0 mt-3"
                        onClick={() => selectCustomer(curr)}
                      >
                        {curr["contactid"] == selectedCustomer["customerId"]
                          ? "Selected"
                          : "Select"}
                      </Button>
                      {/* <Button
                        variant="primary"
                        className="mx-2 mt-md-0 mt-3"
                        onClick={() => openCustomer(curr)}
                      >
                        View
                      </Button> */}
                    </div>
                  </div>
                  <div className="row mt-md-0 mt-3">
                    <div className="col-md-6">
                      <p>
                        <strong className="fw-medium">Mobile Phone:</strong>{" "}
                        {curr["mobilephone"]}
                      </p>
                    </div>
                  </div>
                  <p>
                    Address:{" "}
                    {curr["address1_line1"] + " " + curr["address1_postalcode"]}
                  </p>
                </div>
              ))
            ) : (
              <span className="box-shadow max-height-empty p-3 d-flex align-items-center justify-content-center">
                We did not find anything to show here
              </span>
            )}
          </div>
        </div>
        <div className="col-md-6  ">
          <div className="bg-white p-3 box-shadow rounded-2">
            <Card className="border-0">
              <Card.Body>
                <div className="row align-items-center">
                  <div className="col-md-3 mb-3">
                    <label>First Name</label>
                  </div>
                  <div className="col-md-9 mb-3">
                    <Form.Control
                      placeholder=""
                      type="text"
                      value={selectedCustomer.customerFirstName}
                      readOnly
                    />
                  </div>
                  <div className="col-md-3 mb-3">
                    <label>Last Name</label>
                  </div>
                  <div className="col-md-9 mb-3">
                    <Form.Control
                      placeholder=""
                      type="text"
                      value={selectedCustomer.customerLastName}
                      readOnly
                    />
                  </div>
                  <div className="col-md-3 mb-3">
                    <label>Email</label>
                  </div>
                  <div className="col-md-9 mb-3">
                    <Form.Control
                      placeholder=""
                      type="text"
                      value={selectedCustomer.customerEmailaddress1}
                      readOnly
                    />
                  </div>
                  <div className="col-md-3 mb-3">
                    <label>Mobile Phone</label>
                  </div>
                  <div className="col-md-9 mb-3">
                    <Form.Control
                      placeholder=""
                      type="text"
                      value={selectedCustomer.customerMobilephone}
                      readOnly
                    />
                  </div>

                  <div className="col-md-3 mb-3">
                    <label>Address</label>
                  </div>
                  <div className="col-md-9 mb-3">
                    <Form.Control
                      placeholder=""
                      value={selectedCustomer.customerAddress1_line1}
                      type="text"
                      readOnly
                    />
                  </div>
                  <div className="col-md-3 mb-3">
                    <label>Post Code</label>
                  </div>
                  <div className="col-md-9 mb-3">
                    <Form.Control
                      placeholder=""
                      type="text"
                      value={selectedCustomer.customerAddress1_postalcode}
                      readOnly
                    />
                  </div>
                </div>
                {/* <div className="d-flex justify-content-end w-100">
                  <Button variant="primary" className="mx-2">
                    Clear All
                  </Button>
                  <Button variant="primary">Search</Button>
                </div> */}
              </Card.Body>
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Step3;
