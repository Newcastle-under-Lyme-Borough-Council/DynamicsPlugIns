import React, { useState, useEffect } from "react";
import Button from "react-bootstrap/Button";
import Card from "react-bootstrap/Card";
import InputGroup from "react-bootstrap/InputGroup";
import Form from "react-bootstrap/Form";
import { FaHome, FaChevronRight } from "react-icons/fa";
import { v4 as uuidv3 } from "uuid";
import { useServiceStore } from "../store";
import { toast } from "react-toastify";

//Find Reporting Customer
const Step4 = (props: any) => {
  const { serviceDetails, set, clear } = useServiceStore();
  // const [toastId, setToastId]: any = useState(null);
  const customId = "1";
  const [customers, setCustomers]: any = useState([]);
  // const [searchValue, setSearchValue]: any = useState("");
  const [searchValue, setSearchValue] = useState<string>("");
  const [selectedCustomer, setSelectedCustomer]: any = useState({
    reportedByUserId: serviceDetails.contactid,
    reportedByUserFirstName: serviceDetails.reportedByUserFirstName,
    reportedByUserLastName: serviceDetails.reportedByUserLastName,
    reportedByUserAddress1_line1: serviceDetails.reportedByUserAddress1_line1,
    reportedByUserAddress1_postalcode:
      serviceDetails.reportedByUserAddress1_postalcode,
    reportedByUserEmailaddress1: serviceDetails.reportedByUserEmailaddress1,
    reportedByUserMobilephone: serviceDetails.reportedByUserMobilephone,
    ss_reportingonbehalfofsomeone: serviceDetails.ss_reportingonbehalfofsomeone,
  });

  // useEffect(() => {
  //   setToastId(null);
  // }, []);

  const searchCustomer = (e: any) => {
    setSearchValue(e.target.value);
    if (serviceDetails.ss_reportingonbehalfofsomeone === true) {
      let searchText = e.target.value;
      let query = `?$select=contactid,address1_line1,address1_postalcode,emailaddress1,firstname,fullname,lastname,mobilephone&$filter=((statecode eq 0 and statuscode eq 1) and (contains(fullname,'${searchText}') or contains(address1_line1,'${searchText}') or contains(address1_postalcode,'${searchText}') or contains(emailaddress1,'${searchText}') or contains(mobilephone,'${searchText}')))`;
      props.props.EContext.webAPI
        .retrieveMultipleRecords("contact", query)
        .then(
          function success(results: any) {
            setCustomers(results.entities);
          },
          function (error: any) {
            console.log(error.message);
          }
        );
    } else {
      toast.error(
        "Please select resident is reporting on behalf of someone option.",
        {
          toastId: customId,
          autoClose: 3000,
        }
      );
      // if (toastId !== null) {
      //   toast.update(toastId, {
      //     render:
      //       "Please select resident is reporting on behalf of someone option.",
      //     type: toast.TYPE.ERROR,
      //     autoClose: 3000,
      //     onClose: () => setToastId(null), // Reset the toastId when the toast closes
      //   });
      // } else {
      //   const id = uuidv3();
      //   toast.error(
      //     "Please select resident is reporting on behalf of someone option.",
      //     {
      //       position: "top-right",
      //       autoClose: 3000,
      //       hideProgressBar: false,
      //       closeOnClick: true,
      //       onClose: () => setToastId(null), // Reset the toastId when the toast closes
      //       progress: undefined,
      //       theme: "light",
      //     }
      //   );
      //   setToastId(id); // Cast the ID to string
      // }
    }
  };

  const onHandleChange = (e: any) => {
    if (e.target.checked === false) {
      setSearchValue("");
      setCustomers([]);
      setSelectedCustomer({
        ...selectedCustomer,
        ss_reportingonbehalfofsomeone: false,
        reportedByUserFirstName: "",
        reportedByUserLastName: "",
        reportedByUserFullName: "",
        reportedByUserId: "",
        reportedByUserAddress1_line1: "",
        reportedByUserAddress1_postalcode: "",
        reportedByUserEmailaddress1: "",
        reportedByUserMobilephone: "",
      });
      set({
        ...serviceDetails,
        ss_reportingonbehalfofsomeone: false,
        reportedByUserFirstName: "",
        reportedByUserLastName: "",
        reportedByUserFullName: "",
        reportedByUserId: "",
        reportedByUserAddress1_line1: "",
        reportedByUserAddress1_postalcode: "",
        reportedByUserEmailaddress1: "",
        reportedByUserMobilephone: "",
      });
    } else if (e.target.checked === true) {
      setSelectedCustomer({
        ...selectedCustomer,
        ss_reportingonbehalfofsomeone: e.target.checked,
        reportedByUserFullName: "",
        reportedByUserId: "",
      });
      set({
        ...serviceDetails,
        ss_reportingonbehalfofsomeone: e.target.checked,
        reportedByUserFullName: "",
        reportedByUserId: "",
      });
    }
  };

  const selectCustomer = (customer: any) => {
    setSelectedCustomer({
      ...selectedCustomer,
      reportedByUserId: customer.contactid,
      reportedByUserFirstName: customer.firstname,
      reportedByUserLastName: customer.lastname,
      reportedByUserAddress1_line1: customer.address1_line1,
      reportedByUserAddress1_postalcode: customer.address1_postalcode,
      reportedByUserEmailaddress1: customer.emailaddress1,
      reportedByUserMobilephone: customer.mobilephone,
      ss_reportingonbehalfofsomeone:
        serviceDetails.ss_reportingonbehalfofsomeone,
    });
    set({
      ...serviceDetails,
      reportedByUserId: customer.contactid,
      reportedByUserFirstName: customer.firstname,
      reportedByUserLastName: customer.lastname,
      reportedByUserFullName: customer.firstname + " " + customer.lastname,
      reportedByUserAddress1_line1: customer.address1_line1,
      reportedByUserAddress1_postalcode: customer.address1_postalcode,
      reportedByUserEmailaddress1: customer.emailaddress1,
      reportedByUserMobilephone: customer.mobilephone,
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
      </div>

      <h2 className="h5">Reporting On behalf of Someone</h2>
      <Form className="d-flex mb-3 box-shadow rounded-2 p-3 bg-white">
        <Form.Check // prettier-ignore
          type="checkbox"
          id="1"
          label="Resident is reporting On behalf of someone"
          onChange={onHandleChange}
          checked={serviceDetails.ss_reportingonbehalfofsomeone}
        />
      </Form>
      {/* <p className="text-gray">
        Type customer fullname or some other customers details to search the
        customer.
      </p> */}
      <div className="row mt-4">
        <div className="col-md-6 box-style">
          <div className="box-shadow max-height-box-search p-3 rounded-2">
            <form className="d-flex mb-3">
              <InputGroup>
                <Form.Control
                  type="input"
                  placeholder="Search for a resident by fullname, address, email address, or mobile phone"
                  onChange={searchCustomer}
                  value={searchValue}
                  onKeyPress={(e) => {
                    if (e.key === "Enter") {
                      e.preventDefault();
                      e.stopPropagation();
                    }
                  }}
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
                        {curr["contactid"] ==
                        selectedCustomer["reportedByUserId"]
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
                      value={selectedCustomer.reportedByUserFirstName}
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
                      value={selectedCustomer.reportedByUserLastName}
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
                      value={selectedCustomer.reportedByUserEmailaddress1}
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
                      value={selectedCustomer.reportedByUserMobilephone}
                      readOnly
                    />
                  </div>
                  <div className="col-md-3 mb-3">
                    <label>Address</label>
                  </div>
                  <div className="col-md-9 mb-3">
                    <Form.Control
                      placeholder=""
                      value={selectedCustomer.reportedByUserAddress1_line1}
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
                      value={selectedCustomer.reportedByUserAddress1_postalcode}
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

export default Step4;
