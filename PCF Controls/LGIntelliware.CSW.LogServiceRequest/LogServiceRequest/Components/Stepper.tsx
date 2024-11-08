import React, { useState, useEffect, useRef } from "react";
import { Button, Modal } from "react-bootstrap";
import Step1 from "./Step1";
import Step2 from "./Step2";
import Step3 from "./Step3";
import Step4 from "./Step4";
import Step5 from "./Step5";
import Step6 from "./Step6";
import { MdCancel } from "react-icons/md";
import { useServiceStore } from "../store";
import { FaCircleCheck } from "react-icons/fa6";

import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { GenericModal } from "./GenericModal";

const steps = [
  "Find a Resident",
  "Reporting On behalf of",

  "Find a Service",
  "Notifications",
  "Resident History",

  "Submit",
];

function CustomStepper(props: any) {
  const [activeStep, setActiveStep] = useState(0);
  const [showSuccessScreen, setShowSuccessScreen]: any = useState(false);
  const [loading, setLoading]: any = useState(false);
  const [btnLoading, setBtnLoading]: any = useState(false);
  const [recall, setRecall]: any = useState(false);
  const [showModal, setShowModal] = useState<boolean>(false);
  // const [conditionSuccess, setConditionSuccess]: any = useState(false);
  const [newRecord, setNewRecord]: any = useState("");
  const [createServiceId, setCreateServiceId]: any = useState("");
  const [navigateUrl, setNavigateUrl]: any = useState("");
  const [UpdatedNavigateUrl, setUpdatedNavigateUrl]: any = useState("");
  const [showMessage, setShowMessage]: any = useState("");
  const [submitErrorText, setSubmitErrorText]: any = useState("");
  const [show, setShow]: any = useState(false);
  const customId = "1";

  const { set, serviceDetails } = useServiceStore();
  useEffect(() => {
    if (createServiceId && navigateUrl) {
      const UpdatedUrl = navigateUrl
        ?.replace(
          "<service_logical_name>",
          serviceDetails.ss_notificationid ||
            serviceDetails.incidentid ||
            serviceDetails.knowledgeBase
            ? "ss_enquiry"
            : serviceDetails.ss_servicelogicalname
        )
        ?.replace("<create_new_service_id>", createServiceId);

      setUpdatedNavigateUrl(UpdatedUrl);
    }
  }, [createServiceId, navigateUrl]);

  const ClearState = () => {
    set({
      ss_service_name: "",
      ss_serviceconfigurationid: "",
      ss_servicelogicalname: "",
      ss_serviceprimarykeycolumnname: "",
      ss_description: "",
      ss_sourcetype: 1,
      customerFirstName: "",
      customerLastName: "",
      customerFullName: "",
      customerId: "",
      customerAddress1_line1: "",
      customerAddress1_postalcode: "",
      customerEmailaddress1: "",
      customerMobilephone: "",
      ss_reportingonbehalfofsomeone: false,
      ss_allowservicerequest: true,
      knowledgeBase: false,
      reportedByUserFirstName: "",
      reportedByUserLastName: "",
      reportedByUserFullName: "",
      reportedByUserId: "",
      reportedByUserAddress1_line1: "",
      reportedByUserAddress1_postalcode: "",
      reportedByUserEmailaddress1: "",
      reportedByUserMobilephone: "",
      ss_notificationid: "",
      ss_notification_title: "",
      incidentid: "",
      title: "",
      alreadyRequestReported: "",
      alreadyRequestReportedData: "",
      ss_applicationreferencenumber: "",
    });
  };

  useEffect(() => {
    if (newRecord !== "") {
      ClearState();
    }
  }, [newRecord]);

  useEffect(() => {
    if (!showSuccessScreen) {
      setNewRecord("");
    }
  }, [showSuccessScreen]);

  useEffect(() => {
    if (createServiceId && showSuccessScreen && !newRecord) {
      setLoading(true);

      const isEnquiry =
        serviceDetails.ss_notificationid ||
        serviceDetails.incidentid ||
        serviceDetails.knowledgeBase;
      // const selectField = isEnquiry ? "ss_referencenumber" : "ticketnumber";
      const filterField = isEnquiry
        ? "ss_enquiryid"
        : serviceDetails.ss_serviceprimarykeycolumnname;

      // const query = `?$select=${selectField}&$filter=${filterField} eq ${createServiceId}`;
      const query = `?$select=ss_referencenumber&$filter=${filterField} eq ${createServiceId}`;

      props.EContext.webAPI
        .retrieveMultipleRecords(
          isEnquiry ? "ss_enquiry" : serviceDetails.ss_servicelogicalname,
          query
        )
        .then(
          (results: any) => {
            const record = results?.entities[0];
            const recordNumber = isEnquiry
              ? record?.ss_referencenumber
              : record?.ss_referencenumber;

            setNewRecord(recordNumber);

            if (!recordNumber) {
              setRecall(!recall);
            } else {
              setLoading(false);
            }
          },
          (error: any) => {
            console.log(error.message);
            setLoading(false);
          }
        );
    }
  }, [createServiceId, showSuccessScreen, recall]);

  useEffect(() => {
    if (createServiceId && showSuccessScreen && !newRecord) {
      setLoading(true);

      const isEnquiry =
        serviceDetails.ss_notificationid ||
        serviceDetails.incidentid ||
        serviceDetails.knowledgeBase;
      // const selectField = isEnquiry ? "ss_referencenumber" : "ticketnumber";
      const filterField = isEnquiry
        ? "ss_enquiryid"
        : serviceDetails.ss_serviceprimarykeycolumnname;

      // const query = `?$select=${selectField}&$filter=${filterField} eq ${createServiceId}`;
      const query = `?$select=ss_referencenumber&$filter=${filterField} eq ${createServiceId}`;

      props.EContext.webAPI
        .retrieveMultipleRecords(
          isEnquiry ? "ss_enquiry" : serviceDetails.ss_servicelogicalname,
          query
        )
        .then(
          (results: any) => {
            const record = results?.entities[0];
            const recordNumber = isEnquiry
              ? record?.ss_referencenumber
              : record?.ss_referencenumber;

            setNewRecord(recordNumber);

            if (!recordNumber) {
              setRecall(!recall);
            } else {
              setLoading(false);
            }
          },
          function (error: any) {
            console.log(error.message);
            setLoading(false);
          }
        );
    }
  }, [recall]);

  useEffect(() => {
    let query = `?$select=ss_value&$filter=(statecode eq 0 and statuscode eq 1 and ss_name eq 'LGINTELLIWARE_CUSTOMER_SERVICE_WORKSPACE_OPEN_EXISTING_RECORD_URL')`;
    props.EContext.webAPI
      .retrieveMultipleRecords("ss_applicationconfiguration", query)
      .then(
        function success(results: any) {
          setNavigateUrl(results?.entities[0]?.ss_value);
        },
        function (error: any) {
          console.log(error.message);
        }
      );
  }, []);

  const handleClose = () => {
    setShowSuccessScreen(false);
    setActiveStep(0);
    setNewRecord("");
  };

  const handleNext = () => {
    let errorText = "";
    console.log(errorText, "errorText");

    if (activeStep === 2) {
      if (serviceDetails.ss_serviceconfigurationid === "") {
        errorText = "Please choose a service request.";
      }
      set({
        ...serviceDetails,
        // ss_notificationid: "",
        // ss_notification_title: "",
      });
    } else if (activeStep === 3) {
      if (serviceDetails.ss_notificationid || serviceDetails.knowledgeBase) {
        setShowMessage("An enquiry");
      } else {
        setShowMessage("A service request");
      }
    } else if (activeStep === 4) {
      if (
        serviceDetails.incidentid ||
        serviceDetails.knowledgeBase ||
        serviceDetails.ss_notificationid
      ) {
        setShowMessage("An enquiry");
      } else if (
        !serviceDetails.incidentid &&
        !serviceDetails.knowledgeBase &&
        !serviceDetails.ss_notificationid &&
        !serviceDetails.ss_allowservicerequest
      ) {
        setShowMessage("An enquiry");
        set({
          ...serviceDetails,
          knowledgeBase: true,
        });
        // errorText = `Service request creation is not allowed for ${serviceDetails.ss_service_name}, only enquiries can be created.`;
      } else {
        setShowMessage("A service request");
      }
    } else if (activeStep === 0 && serviceDetails.customerId === "") {
      errorText = "Please find a resident.";
      set({
        ...serviceDetails,
      });
    } else if (
      activeStep === 1 &&
      serviceDetails.ss_reportingonbehalfofsomeone === true &&
      serviceDetails.reportedByUserId === ""
    ) {
      errorText = "Please select a resident reporting on behalf of someone.";
    } else if (
      activeStep === 1 &&
      serviceDetails.ss_reportingonbehalfofsomeone === false
    ) {
      set({
        ...serviceDetails,
        reportedByUserId: "",
        reportedByUserFullName: "",
      });
    } else if (activeStep === 5 && serviceDetails.ss_description === "") {
      errorText = "Please provide description.";
    }

    if (errorText !== "") {
      setSubmitErrorText(errorText);
      setShowModal(true);
    } else {
      setActiveStep(activeStep + 1);
    }
  };

  const handleBack = () => {
    setActiveStep(activeStep - 1);
  };

  const submitService = () => {
    let submitError;
    setBtnLoading(true);

    let selectFields = ["ss_referencenumber"];
    let filters = [
      `_ss_customer_value eq '${serviceDetails?.customerId}'`,
      `_ss_serviceconfiguration_value eq '${serviceDetails?.ss_serviceconfigurationid}'`,
      "statecode eq 0",
    ];

    if (
      serviceDetails?.ss_service_name === "Planning Permission" &&
      serviceDetails?.ss_applicationreferencenumber
    ) {
      selectFields.push("ss_applicationreferencenumber");
      filters.push(
        `ss_applicationreferencenumber eq '${serviceDetails.ss_applicationreferencenumber}'`
      );
    }

    let query = `?$select=${selectFields.join(",")}&$filter=(${filters.join(
      " and "
    )})`;

    // let selectFields = "ss_referencenumber";
    // // let query = `?$select=ss_referencenumber&$filter=(_ss_customer_value eq '${serviceDetails?.customerId}' and _ss_serviceconfiguration_value eq '${serviceDetails?.ss_serviceconfigurationid}' and statecode eq 0)`;
    // let query = `?$select=${selectFields}&$filter=(_ss_customer_value eq '${serviceDetails?.customerId}' and _ss_serviceconfiguration_value eq '${serviceDetails?.ss_serviceconfigurationid}' and statecode eq 0`;

    // if (
    //   serviceDetails.ss_service_name === "Planning Permission" &&
    //   serviceDetails?.ss_applicationreferencenumber
    // ) {
    //   query += ` and ss_applicationreferencenumber eq '${serviceDetails.ss_applicationreferencenumber}'`;
    //   selectFields += ",ss_applicationreferencenumber";
    // }
    // query += ")";
    props.EContext.webAPI
      .retrieveMultipleRecords(serviceDetails?.ss_servicelogicalname, query)
      .then(
        function success(results: any) {
          // setAllServiceRequest(results?.entities);

          if (
            activeStep === 5 &&
            serviceDetails.ss_service_name === "Planning Permission" &&
            serviceDetails.ss_applicationreferencenumber === "" &&
            serviceDetails.ss_notificationid === "" &&
            serviceDetails.incidentid === "" &&
            !serviceDetails.knowledgeBase
          ) {
            submitError = "Please provide application reference number.";
            submitErrorFunc(submitError);
          } else if (activeStep === 5 && serviceDetails.ss_description === "") {
            submitError = "Please provide description.";
            submitErrorFunc(submitError);
          }

          // if (activeStep === 5) {
          //   if (
          //     serviceDetails.ss_servicelogicalname === "ss_planningpermission"
          //   ) {
          //     if (serviceDetails.ss_applicationreferencenumber === "") {
          //       submitError = "Please provide application reference number";
          //       showSubmitError(submitError);
          //     }
          //   }
          //   if (serviceDetails.ss_description === "") {
          //     submitError = "Please provide description.";
          //     showSubmitError(submitError);
          //   }
          // }
          else if (
            activeStep === 5 &&
            results?.entities?.length > 0 &&
            serviceDetails.ss_notificationid === "" &&
            serviceDetails.incidentid === "" &&
            !serviceDetails.knowledgeBase &&
            serviceDetails.ss_description !== ""
          ) {
            submitError = `A service request for ${
              serviceDetails.ss_applicationreferencenumber
                ? `Application Reference Number: ${results?.entities[0]?.ss_applicationreferencenumber}`
                : `${serviceDetails.ss_service_name}`
            } has already been reported. Reference Number is ${
              results?.entities[0]?.ss_referencenumber
            }`;
            submitErrorFunc(submitError);
          } else {
            var record: any = {};
            record[
              "ss_Customer@odata.bind"
            ] = `/contacts(${serviceDetails.customerId})`; // Lookup
            record[
              "ss_Onbehalfofsomeone@odata.bind"
            ] = `/contacts(${serviceDetails.reportedByUserId})`; // Lookup
            record.ss_reportingonbehalfofsomeone =
              serviceDetails.ss_reportingonbehalfofsomeone;
            record.ss_description = serviceDetails.ss_description; // Text
            record[
              "ss_ServiceConfiguration@odata.bind"
            ] = `/ss_serviceconfigurations(${serviceDetails.ss_serviceconfigurationid})`; // Lookup

            if (
              serviceDetails?.ss_servicelogicalname ===
                "ss_planningpermission" &&
              serviceDetails.ss_notificationid === "" &&
              serviceDetails.incidentid === "" &&
              !serviceDetails.knowledgeBase
            ) {
              record.ss_applicationreferencenumber =
                serviceDetails.ss_applicationreferencenumber;
            }

            if (
              serviceDetails.ss_notificationid &&
              serviceDetails.incidentid === "" &&
              !serviceDetails.knowledgeBase
            ) {
              record[
                "ss_NotificationId@odata.bind"
              ] = `/ss_notifications(${serviceDetails.ss_notificationid})`; // Lookup
              record.ss_baserecordtitle = serviceDetails.ss_notification_title;
            } else if (
              serviceDetails.incidentid &&
              serviceDetails.ss_notificationid === "" &&
              !serviceDetails.knowledgeBase
            ) {
              record[
                "ss_IncidentId@odata.bind"
              ] = `/incidents(${serviceDetails.incidentid})`; // Lookup
            } else if (
              !serviceDetails.knowledgeBase &&
              serviceDetails.ss_notificationid === "" &&
              serviceDetails.incidentid === ""
            ) {
              record.ss_sourcetype = serviceDetails.ss_sourcetype;
            }

            let entityName =
              serviceDetails.ss_notificationid !== "" ||
              serviceDetails.incidentid !== "" ||
              serviceDetails.knowledgeBase
                ? "ss_enquiry"
                : serviceDetails.ss_servicelogicalname;
            props.EContext.webAPI.createRecord(entityName, record).then(
              function success(result: any) {
                setCreateServiceId(result.id);

                serviceDetails.ss_serviceconfigurationid = "";
                setShowSuccessScreen(true);
                setBtnLoading(false);
              },
              function (error: any) {
                console.log(error.message);
                setBtnLoading(false);
              }
            );
          }
        },
        function (error: any) {
          console.log(error.message);
          setBtnLoading(false);
        }
      );
  };

  const submitErrorFunc = (submitError: string) => {
    setBtnLoading(false);
    if (submitError !== "") {
      setSubmitErrorText(submitError);
      setShowModal(true);
    }
  };

  return (
    <>
      <div className="px-3 border-0 h-100">
        {/* <ToastContainer /> */}
        <GenericModal
          Icon={<MdCancel fill="#e21b1c" size={25} />}
          heading="Error"
          error={submitErrorText}
          showModal={showModal}
          setShowModal={setShowModal}
        />
        <div className="main-stepper-box">
          <div className="custom-stepper pt-5 position-relative z-0">
            <div className="row w-100 mx-auto circle-stepper box-shadow pt-3 pb-5 justify-content-center rounded-2 bg-white">
              {steps.map((step, index) => (
                <div
                  key={step}
                  className={`col-2 step ${
                    index === activeStep
                      ? "active"
                      : index < activeStep
                      ? "completed"
                      : ""
                  }`}
                >
                  <div>
                    <span> </span>
                  </div>
                  <p className="step-heading"> {step}</p>{" "}
                </div>
              ))}
            </div>
            {!showSuccessScreen && (
              <div className="mt-4">
                {activeStep === 2 && <Step1 props={props} />}
                {activeStep === 3 && <Step2 props={props} />}
                {activeStep === 0 && <Step3 props={props} />}
                {activeStep === 1 && <Step4 props={props} />}
                {activeStep === 4 && <Step5 props={props} />}
                {activeStep === 5 && <Step6 props={props} />}
              </div>
            )}
          </div>
          {!showSuccessScreen && (
            <div className="d-flex justify-content-end mt-4 next-prev-btn">
              {activeStep > 0 && (
                <Button
                  variant="primary"
                  className="px-4 me-3"
                  onClick={handleBack}
                >
                  Previous
                </Button>
              )}
              {activeStep < steps.length - 1 && (
                <Button variant="primary" className="px-4" onClick={handleNext}>
                  Next
                </Button>
              )}
              {activeStep === 5 && (
                <Button
                  variant="primary"
                  className="px-4 d-flex align-items-center justify-content-center"
                  onClick={submitService}
                  disabled={btnLoading}
                >
                  {btnLoading && (
                    <div
                      className="spinner-border btn-loader"
                      role="status"
                    ></div>
                  )}
                  Submit
                </Button>
              )}
            </div>
          )}

          {showSuccessScreen && (
            <div className="mt-4 h-100">
              <>
                <div className="text-center box-shadow p-4 rounded-2 bg-white record-created">
                  <div className="d-flex align-items-center mb-15">
                    <FaCircleCheck fill="#32872d" size={40} />

                    <h2 className="h5 mb-0 ms-2">
                      {showMessage} has been successfully created
                    </h2>
                  </div>
                  <p>
                    {showMessage} with reference number{" "}
                    <a
                      onClick={() => setShow(true)}
                      className="h5 fw-600 cursor-pointer"
                      // href={UpdatedNavigateUrl}
                      // target="_blank"
                    >
                      {newRecord ? newRecord : ""}
                    </a>{" "}
                    has been successfully created.
                  </p>

                  {/* <p>
                  A case with reference number
                  <strong> {newRecord ? newRecord : ""}</strong> has been
                  successfully created.
                </p> */}
                  <Button
                    variant="primary"
                    className="px-4 mt-4"
                    onClick={handleClose}
                  >
                    OK
                  </Button>
                </div>
                <div
                  className={`${
                    loading ? "d-flex" : "d-none"
                  } position-fixed w-100 h-100 loading-box`}
                >
                  <div className="text-center">
                    <div className="spinner-border text-white" role="status">
                      <span className="visually-hidden">Loading...</span>
                    </div>
                    <h2 className="text-white">Processing...</h2>
                  </div>
                </div>
              </>
            </div>
          )}

          {/* <div className="chatbot">
          <img
            src="https://static-00.iconduck.com/assets.00/power-virtual-agents-colored-icon-2048x1957-d1kff0zn.png"
            width={40}
            height={40}
            className="cursor-pointer"
            onClick={() => setChatBoat(!chatBoat)}
          />
          {chatBoat && (
            <div className="chatbot-box shadow ">
              <iframe
                //src="https://web.powerva.microsoft.com/environments/0cba36b9-669c-e403-b9b6-ca6b669b0da6/bots/cr74b_lgIntelliwareMda/webchat?__version__=2"
                src="https://web.powerva.microsoft.com/environments/12b2bb30-2588-ebf3-92bd-89da18055605/bots/cr74b_lgIntelliwareMda/webchat?__version__=2"
                frameBorder="0"
                style={{ width: "100%", height: "100%" }}
              ></iframe>
            </div>
          )}
        </div> */}
        </div>
      </div>
      {/* <Button variant="primary" onClick={() => setShow(true)}>
        Custom Width Modal
      </Button> */}

      <Modal
        show={show}
        onHide={() => setShow(false)}
        dialogClassName="modal-90w custom-modal"
        aria-labelledby="example-custom-modal-styling-title"
        centered
      >
        <Modal.Header closeButton></Modal.Header>
        <Modal.Body className="custom-modal-body">
          <iframe
            src={UpdatedNavigateUrl}
            frameBorder="0"
            width="100%"
            height="100%"
          ></iframe>
        </Modal.Body>
      </Modal>
    </>
  );
}

export default CustomStepper;
