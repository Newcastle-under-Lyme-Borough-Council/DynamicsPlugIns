import create from "zustand";

const emptyService: any = {
  ss_service_name: "",
  ss_serviceconfigurationid: "",
  ss_servicelogicalname: "",
  ss_serviceprimarykeycolumnname: "",
  ss_description: "",
  ss_sourcetype: 1,
  customerFullName: "",
  customerId: "",
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
