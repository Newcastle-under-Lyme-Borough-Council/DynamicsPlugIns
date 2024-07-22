import { IInputs, IOutputs } from "./generated/ManifestTypes";
import * as React from "react";
import * as ReactDOM from "react-dom";

import AddressFinderComponent from "../AddressFinderComponents/components/AddressFinderComponent";

import { useAddressStore } from "../AddressFinderComponents/store/address";
import { Address } from "../AddressFinderComponents/types/Address";

import "./style.css";

export class AddressFinder
  implements ComponentFramework.StandardControl<IInputs, IOutputs>
{
  private _container: HTMLDivElement;
  private _context: ComponentFramework.Context<IInputs>;
  state: ComponentFramework.Dictionary;
  _notifyOutputChanged: () => void;
  private _props: any;

  constructor() {}
  /**
   * Used to initialize the control instance. Controls can kick off remote server calls and other initialization actions here.
   * Data-set values are not initialized here, use updateView.
   * @param context The entire property bag available to control via Context Object; It contains values as set up by the customizer mapped to property names defined in the manifest, as well as utility functions.
   * @param notifyOutputChanged A callback method to alert the framework that the control has new outputs ready to be retrieved asynchronously.
   * @param state A piece of data that persists in one session for a single user. Can be set at any point in a controls life cycle by calling 'setControlState' in the Mode interface.
   * @param container If a control is marked control-type='standard', it will receive an empty div element within which it can render its content.
   */
  public init(
    context: ComponentFramework.Context<IInputs>,
    notifyOutputChanged: () => void,
    state: ComponentFramework.Dictionary,
    container: HTMLDivElement
  ): void {
    // Add control initialization code
    this._context = context;
    this._notifyOutputChanged = notifyOutputChanged;
    this.state = state;
    this._container = container;
    this._props = {
      componentContext: context,
      notify: this._notifyOutputChanged,
    };
    if (context.parameters.line1.raw) {
      useAddressStore.setState({
        address: {
          organization: context.parameters.organization.raw ?? "",
          line1: context.parameters.line1.raw ?? "",
          line2: context.parameters.line2.raw ?? "",
          line3: context.parameters.line3.raw ?? "",
          locality: context.parameters.locality.raw ?? "",
          towncity: context.parameters.towncity.raw ?? "",
          county: context.parameters.county.raw ?? "",
          country: context.parameters.country.raw ?? "",
          postcode: context.parameters.postcode.raw ?? "",
          uprn: context.parameters.uprn.raw ?? "",
          xcord: Number(context.parameters.xcord.raw) ?? 0,
          ycord: Number(context.parameters.ycord.raw) ?? 0,
          address: context.parameters.address.raw ?? "",
        },
      });
    }
    ReactDOM.render(
      React.createElement(AddressFinderComponent, this._props),
      this._container
    );
  }

  /**
   * Called when any value in the property bag has changed. This includes field values, data-sets, global values such as container height and width, offline status, control metadata values such as label, visible, etc.
   * @param context The entire property bag available to control via Context Object; It contains values as set up by the customizer mapped to names defined in the manifest, as well as utility functions
   */
  public updateView(context: ComponentFramework.Context<IInputs>): void {
    this._context = context;
  }

  /**
   * It is called by the framework prior to a control receiving new data.
   * @returns an object based on nomenclature defined in manifest, expecting object[s] for property marked as “bound” or “output”
   */
  public getOutputs(): IOutputs {
    let address: Address = useAddressStore.getState().address;
    return {
      organization: address.organization,
      line1: address.line1,
      line2: address.line2,
      line3: address.line3,
      locality: address.locality,
      towncity: address.towncity,
      county: address.county,
      country: address.country,
      postcode: address.postcode,
      uprn: address.uprn,
      xcord: address.xcord,
      ycord: address.ycord,
      address: address.address,
    };
  }

  /**
   * Called when the control is to be removed from the DOM tree. Controls should use this call for cleanup.
   * i.e. cancelling any pending remote calls, removing listeners, etc.
   */
  public destroy(): void {
    // Add code to cleanup control if necessary
    ReactDOM.unmountComponentAtNode(this._container);
  }
}
