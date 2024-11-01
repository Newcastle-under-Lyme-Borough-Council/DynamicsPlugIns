import create from "zustand";

const emptyService: any = {
  ss_service_name: "",
  ss_serviceconfigurationid: "",
  ss_allowservicerequest: true,
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
  knowledgeBase: false,
  // successCondition: false,
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
};

type ServiceStore = {
  serviceDetails: any;
  set: (service: any) => void;
  clear: () => void;
};

export const useServiceStore = create<ServiceStore>((set) => ({
  serviceDetails: emptyService,
  set: (serviceDetails: any) => set({ serviceDetails: serviceDetails }),
  clear: () => set({ serviceDetails: emptyService }),
}));
