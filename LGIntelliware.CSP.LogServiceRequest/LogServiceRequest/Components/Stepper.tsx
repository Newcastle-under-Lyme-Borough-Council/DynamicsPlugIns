import React, { useState, useEffect } from "react";
import { Button } from "react-bootstrap";
import Step1 from "./Step1";
import Step2 from "./Step2";
import Step3 from "./Step3";
import Step4 from "./Step4";

import { useServiceStore } from "../store";
import { FaCircleCheck } from "react-icons/fa6";

import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

const steps = ["Find a Service", "Notifications", "Resident History", "Submit"];

function CustomStepper(props: any) {
  console.log(props, "props");

  const [activeStep, setActiveStep] = useState(0);
  const [showSuccessScreen, setShowSuccessScreen]: any = useState(false);
  const [loading, setLoading]: any = useState(false);
  const [recall, setRecall]: any = useState(false);
  // const [conditionSuccess, setConditionSuccess]: any = useState(false);
  const [newRecord, setNewRecord]: any = useState("");
  const [createServiceId, setCreateServiceId]: any = useState("");

  const [toastId, setToastId] = useState<string | null>(null);
  const [toastIdSubmit, setToastIdSubmit] = useState<string | null>(null);

  const { set, serviceDetails } = useServiceStore();

  useEffect(() => {
    if (createServiceId && showSuccessScreen && !newRecord) {
      setLoading(true);
      const query = `?$select=ss_referencenumber&$filter=${serviceDetails.ss_serviceprimarykeycolumnname} eq ${createServiceId}`;
      props.EContext.webAPI
        .retrieveMultipleRecords(serviceDetails.ss_servicelogicalname, query)
        .then(
          (results: any) => {
            const record = results?.entities[0];
            const recordNumber = record?.ss_referencenumber;

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

      const query = `?$select=ss_referencenumber&$filter=${serviceDetails.ss_serviceprimarykeycolumnname} eq ${createServiceId}`;
      props.EContext.webAPI
        .retrieveMultipleRecords(serviceDetails.ss_servicelogicalname, query)
        .then(
          (results: any) => {
            const record = results?.entities[0];
            const recordNumber = record?.ss_referencenumber;

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
    if (activeStep === 0 || activeStep === 3) {
      setToastId(null);
    }
  }, [activeStep]);

  const handleClose = () => {
    setShowSuccessScreen(false);
    setActiveStep(0);
    setNewRecord("");
  };

  const handleNext = () => {
    let errorText = "";

    if (activeStep === 0) {
      if (serviceDetails.ss_serviceconfigurationid === "") {
        errorText = "Please choose a service request.";
      }
      set({
        ...serviceDetails,
      });
    } else if (activeStep === 3 && serviceDetails.ss_description === "") {
      errorText = "Please provide description.";
    }

    if (errorText !== "") {
      if (toastId) {
        toast.update(toastId, {
          render: errorText,
          type: toast.TYPE.ERROR,
          autoClose: 3000,
          onClose: () => setToastId(null), // Reset the toastId when the toast closes
        });
      } else {
        const id = toast.error(errorText, {
          position: "top-right",
          autoClose: 3000,
          hideProgressBar: false,
          closeOnClick: true,
          progress: undefined,
          theme: "light",
          // onClose: () => setToastId(null), // Reset the toastId when the toast closes
        });
        setToastId(id as string); // Cast the ID to string
      }
    } else {
      setActiveStep(activeStep + 1);
      if (toastId) {
        toast.dismiss(toastId);
        setToastId(null);
      }
    }
  };

  const handleBack = () => {
    setActiveStep(activeStep - 1);
  };
  useEffect(() => {
    set({
      ...serviceDetails,
      customerId: props.userId || "",
      customerFullName: props.userName || "",
    });
  }, []);

  const submitService = () => {
    if (activeStep === 3 && serviceDetails.ss_description === "") {
      if (toastIdSubmit) {
        toast.update(toastIdSubmit, {
          render: "Please provide description.",
          type: toast.TYPE.ERROR,
          autoClose: 3000,
          onClose: () => setToastIdSubmit(null), // Reset the toastIdSubmit when the toast closes
        });
      } else {
        const id = toast.error("Please provide description.", {
          position: "top-right",
          autoClose: 3000,
          hideProgressBar: false,
          closeOnClick: true,
          progress: undefined,
          theme: "light",
          // onClose: () => setToastIdSubmit(null), // Reset the toastIdSubmit when the toast closes
        });
        setToastIdSubmit(id as string); // Cast the ID to string
      }

      return;
    }
    var record: any = {};
    record[
      "ss_Customer@odata.bind"
    ] = `/contacts(${serviceDetails.customerId})`; // Lookup
    record[
      "ss_Onbehalfofsomeone@odata.bind"
    ] = `/contacts(${serviceDetails.customerId})`; // Lookup
    record.ss_reportingonbehalfofsomeone =
      serviceDetails.ss_reportingonbehalfofsomeone;
    record.ss_description = serviceDetails.ss_description; // Text
    record[
      "ss_ServiceConfiguration@odata.bind"
    ] = `/ss_serviceconfigurations(${serviceDetails.ss_serviceconfigurationid})`; // Lookup
    record.ss_sourcetype = serviceDetails.ss_sourcetype;

    props.EContext.webAPI
      .createRecord(serviceDetails.ss_servicelogicalname, record)
      .then(
        function success(result: any) {
          setCreateServiceId(result.id);

          serviceDetails.ss_serviceconfigurationid = "";
          setShowSuccessScreen(true);
          set({
            ss_service_name: "",
            ss_serviceconfigurationid: "",
            ss_servicelogicalname: "",
            ss_serviceprimarykeycolumnname: "",
            ss_description: "",
            ss_sourcetype: 1,
            customerFullName: "",
            customerId: "",
          });
          setToastId(null);
        },
        function (error: any) {
          console.log(error.message);
        }
      );
  };

  return (
    <div className="px-3 border-0">
      <ToastContainer />
      <div className="main-stepper-box">
        <div className="custom-stepper pt-5 position-relative z-0">
          <div className="row w-100 mx-auto circle-stepper box-shadow pt-3 pb-5 justify-content-center rounded-2 bg-white">
            {steps.map((step, index) => (
              <div
                key={step}
                className={`col-3 step ${
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
              {activeStep === 0 && <Step1 props={props} />}
              {activeStep === 1 && <Step2 props={props} />}
              {activeStep === 2 && <Step3 props={props} />}
              {activeStep === 3 && <Step4 props={props} />}
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
            {activeStep === 3 && (
              <Button
                variant="primary"
                className="px-4"
                onClick={submitService}
              >
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
                    A service request has been successfully created
                  </h2>
                </div>
                <p>
                  A service request with reference number{" "}
                  <strong>{newRecord ? newRecord : ""}</strong> has been
                  successfully created.
                </p>

                {/* <p>
                  A case with reference number
                  <strong> {newRecord ? newRecord : ""}</strong> has been
                  successfully created.
                </p> */}
                <a
                  href="/"
                  className="px-4 mt-4 btn btn-primary"
                  onClick={handleClose}
                >
                  OK
                </a>
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
  );
}

export default CustomStepper;
