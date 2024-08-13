import create from "zustand";
import { Address } from "../types/Address";
import { AddressStore } from "../types/AddressStore";

const emptyAddress: Address = {
  organization: "",
  line1: "",
  line2: "",
  line3: "",
  locality: "",
  towncity: "",
  county: "",
  country: "",
  postcode: "",
  uprn: "",
  longitude: 0,
  latitude: 0,
  easting: 0,
  northing: 0,
  address: "",
  // xcord: 0,
  // ycord: 0,
};

export const useAddressStore = create<AddressStore>((set) => ({
  address: emptyAddress,
  set: (address: Address) => set({ address: address }),
  clear: () => set({ address: emptyAddress }),
}));
