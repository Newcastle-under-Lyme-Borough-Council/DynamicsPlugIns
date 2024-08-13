/*
*This is auto generated from the ControlManifest.Input.xml file
*/

// Define IInputs and IOutputs Type. They should match with ControlManifest.
export interface IInputs {
    sampleProperty: ComponentFramework.PropertyTypes.StringProperty;
    organization: ComponentFramework.PropertyTypes.StringProperty;
    line1: ComponentFramework.PropertyTypes.StringProperty;
    line2: ComponentFramework.PropertyTypes.StringProperty;
    line3: ComponentFramework.PropertyTypes.StringProperty;
    locality: ComponentFramework.PropertyTypes.StringProperty;
    towncity: ComponentFramework.PropertyTypes.StringProperty;
    county: ComponentFramework.PropertyTypes.StringProperty;
    country: ComponentFramework.PropertyTypes.StringProperty;
    postcode: ComponentFramework.PropertyTypes.StringProperty;
    uprn: ComponentFramework.PropertyTypes.StringProperty;
    easting: ComponentFramework.PropertyTypes.FloatingNumberProperty;
    northing: ComponentFramework.PropertyTypes.FloatingNumberProperty;
    longitude: ComponentFramework.PropertyTypes.FloatingNumberProperty;
    latitude: ComponentFramework.PropertyTypes.FloatingNumberProperty;
    address: ComponentFramework.PropertyTypes.StringProperty;
    apiKey: ComponentFramework.PropertyTypes.StringProperty;
    apiUrl: ComponentFramework.PropertyTypes.StringProperty;
}
export interface IOutputs {
    sampleProperty?: string;
    organization?: string;
    line1?: string;
    line2?: string;
    line3?: string;
    locality?: string;
    towncity?: string;
    county?: string;
    country?: string;
    postcode?: string;
    uprn?: string;
    easting?: number;
    northing?: number;
    longitude?: number;
    latitude?: number;
    address?: string;
}
