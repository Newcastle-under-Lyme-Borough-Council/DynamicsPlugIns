import React, { useState, useEffect } from "react";
import { Button, Modal } from "react-bootstrap";
import { IoWarning } from "react-icons/io5";

// Define the prop types for the component
interface GenericModalProps {
  heading: string;
  error: string;
  Icon: React.ReactNode;
  showModal: boolean;
  setShowModal: (type: boolean) => void;
}

export const GenericModal: React.FC<GenericModalProps> = ({
  heading,
  error,
  showModal,
  Icon,
  setShowModal,
}) => {
  // State to control modal visibility
  // const [show, setShow] = useState<boolean>(false);

  // Handle closing the modal
  const handleClose = () => {
    setShowModal(false);
  };

  return (
    <Modal
      centered
      show={showModal}
      onHide={handleClose}
      backdrop="static"
      keyboard={false}
      className="generic-modal"
    >
      <Modal.Header closeButton>
        <Modal.Title className="d-flex align-items-center">
          {Icon}
          <span className="ms-1"> {heading || "Default Title"}</span>
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>{error || "Default error message"}</Modal.Body>
      <Modal.Footer>
        <Button variant="primary" onClick={handleClose}>
          OK
        </Button>
      </Modal.Footer>
    </Modal>
  );
};
