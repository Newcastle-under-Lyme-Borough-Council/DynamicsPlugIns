import React, { useState, useEffect } from "react";
import Button from "react-bootstrap/Button";
import { Form, InputGroup } from "react-bootstrap";
import { useServiceStore } from "../store";

//Find Service
const Step1 = (props: any) => {
  const { serviceDetails, set, clear } = useServiceStore();

  const [services, setServices]: any = useState([]);
  const [topServices, setTopServices]: any = useState([]);
  const [selectedService, setSelectedService]: any = useState({
    ss_service_name: serviceDetails.ss_name,
    ss_servicelogicalname: serviceDetails.ss_servicelogicalname,
    ss_serviceprimarykeycolumnname:
      serviceDetails.ss_serviceprimarykeycolumnname,
    ss_serviceconfigurationid: serviceDetails.ss_serviceconfigurationid,
  });

  useEffect(() => {
    let query = `?$select=ss_serviceconfigurationid,ss_name,ss_servicelogicalname,ss_serviceprimarykeycolumnname,ss_totalassociatedcases&$filter=(statecode eq 0 and statuscode eq 1)&$orderby=ss_name asc`;
    props.props.EContext.webAPI
      .retrieveMultipleRecords("ss_serviceconfiguration", query)
      .then(
        function success(results: any) {
          setServices(results.entities);
          setTopServices(results.entities);
        },
        function (error: any) {
          console.log(error.message);
        }
      );
  }, []);

  const getService = (e: any) => {
    let serviceName = e.target.value;
    let query = `?$select=ss_serviceconfigurationid,ss_name,ss_totalassociatedcases,ss_servicelogicalname,ss_serviceprimarykeycolumnname&$filter=(contains(ss_name,'${serviceName}') and statecode eq 0 and statuscode eq 1)&$orderby=ss_name asc`;
    props.props.EContext.webAPI
      .retrieveMultipleRecords("ss_serviceconfiguration", query)
      .then(
        function success(results: any) {
          setServices(results.entities);
        },
        function (error: any) {
          console.log(error.message);
        }
      );
  };
  const selectService = (serviceConfiguration: any) => {
    setSelectedService(serviceConfiguration);
    set({
      ...serviceDetails,
      ss_service_name: serviceConfiguration.ss_name,
      ss_servicelogicalname: serviceConfiguration.ss_servicelogicalname,
      ss_serviceprimarykeycolumnname:
        serviceConfiguration.ss_serviceprimarykeycolumnname,
      ss_serviceconfigurationid: serviceConfiguration.ss_serviceconfigurationid,
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

      <div className="row">
        <div className="col-md-6">
          <h2 className="h5">Choose a Service Request</h2>
          <div className="mt-3 bg-white rounded-2">
            <form className="d-flex">
              <InputGroup>
                <Form.Control
                  type="input"
                  placeholder="Search for a service request"
                  onChange={getService}
                  onKeyPress={(e) => {
                    if (e.key === "Enter") {
                      e.preventDefault();
                      e.stopPropagation();
                    }
                  }}
                  required
                />
              </InputGroup>
            </form>
          </div>
          <div className="box-shadow p-4 mt-4 rounded-2 bg-white max-height-box2">
            {services &&
              services.map((curr: any, ind: any) => (
                <div className="service-list bg-gray mb-2">
                  <div className="d-flex justify-content-between p-3 align-items-center flex-wrap">
                    <p className="m-0">{curr["ss_name"]}</p>
                    <Button
                      variant="primary"
                      className="mt-md-0 mt-3"
                      onClick={() => selectService(curr)}
                    >
                      {curr["ss_serviceconfigurationid"] ==
                      selectedService["ss_serviceconfigurationid"]
                        ? "Selected"
                        : "Select"}
                    </Button>
                  </div>
                </div>
              ))}
          </div>
        </div>

        <div className="col-md-6 mt-md-0 mt-4">
          <h2 className="h5">Top Services</h2>
          <div className="box-shadow p-4 mt-3 rounded-2 bg-white max-height-box">
            {topServices &&
              topServices.map((curr: any, ind: any) => (
                <div className="service-list bg-gray mb-2">
                  <div className="d-flex justify-content-between p-3 align-items-center flex-wrap">
                    <div className="d-flex align-items-center">
                      <span className="rounde-number me-2">{ind + 1}</span>
                      <div className="ms-2">
                        <p className="m-0">{curr["ss_name"]}</p>
                        <h6 className="mt-1 mb-0 badge-custom">
                          Total Cases: {curr["ss_totalassociatedcases"]}
                        </h6>
                      </div>
                    </div>

                    <Button
                      variant="primary"
                      className="mt-lg-0 mt-3"
                      onClick={() => selectService(curr)}
                    >
                      {curr["ss_serviceconfigurationid"] ==
                      selectedService["ss_serviceconfigurationid"]
                        ? "Selected"
                        : "Select"}
                    </Button>
                  </div>
                </div>
              ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Step1;
