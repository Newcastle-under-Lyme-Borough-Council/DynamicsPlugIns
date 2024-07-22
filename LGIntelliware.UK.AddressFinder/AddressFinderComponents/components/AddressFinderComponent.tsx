import * as React from "react";
import axios from "axios";
import proj4 from "proj4";
import "proj4/dist/proj4";
import { useAddressStore } from "../store/address";

interface IMyReactComponentProps {}
type Address = {
  // Define the properties of the address object
  // Adjust them according to your API response structure
  property1: string;
  property2: number;
  // ...
};

const AddressFinderComponent: React.FC<IMyReactComponentProps> = (
  props: any
) => {
  const [query, setQuery] = React.useState("");
  const [selectedVal, setSelectedVal] = React.useState("");
  const [currentPage, setCurrentPage] = React.useState(0);
  const [totalRecord, setTotalRecord] = React.useState(0);
  const [loading, setLoading] = React.useState(false);

  // const [addressList, setAddressList] = React.useState([]);
  const [addressList, setAddressList] = React.useState<Address[]>([]);
  const ApiKey = props?.componentContext?.parameters?.apiKey?.raw || "";
  const ApiUrl = props?.componentContext?.parameters?.apiUrl?.raw || "";
  console.log(addressList, "addressList");
  proj4.defs(
    "EPSG:27700",
    "+proj=tmerc +lat_0=49 +lon_0=-2 +k=0.9996012717 +x_0=400000 +y_0=-100000 +datum=OSGB36 +units=m +no_defs"
  );

  // Define the destination projection (WGS 84 - EPSG:4326)
  const destProjection = "EPSG:4326";

  const { address, set, clear } = useAddressStore();
  const [isOpen, setIsOpen] = React.useState(false);

  const inputRef = React.useRef(null);

  React.useEffect(() => {
    document.addEventListener("click", toggle);
    return () => document.removeEventListener("click", toggle);
  }, []);
  const setProperties = (option: any) => {
    let pao_start_number =
      option.LPI.PAO_START_NUMBER == undefined
        ? ""
        : option.LPI.PAO_START_NUMBER;
    let pao_start_suffix =
      option.LPI.PAO_START_SUFFIX == undefined
        ? ""
        : option.LPI.PAO_START_SUFFIX;
    let pao_end_number =
      option.LPI.PAO_END_NUMBER == undefined ? "" : option.LPI.PAO_END_NUMBER;
    let pao_end_suffix =
      option.LPI.PAO_END_SUFFIX == undefined ? "" : option.LPI.PAO_END_SUFFIX;
    let sao_start_number =
      option.LPI.SAO_START_NUMBER == undefined
        ? ""
        : option.LPI.SAO_START_NUMBER;
    let sao_start_suffix =
      option.LPI.SAO_START_SUFFIX == undefined
        ? ""
        : option.LPI.SAO_START_SUFFIX;
    let sao_end_number =
      option.LPI.SAO_END_NUMBER == undefined ? "" : option.LPI.SAO_END_NUMBER;
    let sao_end_suffix =
      option.LPI.SAO_END_SUFFIX == undefined ? "" : option.LPI.SAO_END_SUFFIX;
    let pao_text = option.LPI.PAO_TEXT == undefined ? "" : option.LPI.PAO_TEXT;
    let sao_text = option.LPI.SAO_TEXT == undefined ? "" : option.LPI.SAO_TEXT;
    let street_description =
      option.LPI.STREET_DESCRIPTION == undefined
        ? ""
        : option.LPI.STREET_DESCRIPTION;

    let paonum =
      pao_end_number + pao_end_suffix == ""
        ? pao_start_number + pao_start_suffix
        : pao_start_number +
          pao_start_suffix +
          " - " +
          pao_end_number +
          pao_end_suffix;
    let saonum =
      sao_end_number + sao_end_suffix == ""
        ? sao_start_number + sao_start_suffix
        : sao_start_number +
          sao_start_suffix +
          " - " +
          sao_end_number +
          sao_end_suffix;
    let pao = pao_text;
    let sao = saonum == "" ? sao_text : saonum + " " + sao_text;
    let sl =
      paonum == "" ? street_description : paonum + " " + street_description;
    let result = { l1: "", l2: "", l3: "" };
    result.l1 = sao != "" ? sao : pao == "" ? sl : pao;
    result.l2 = sao != "" && pao != "" ? pao : sao != "" || pao != "" ? sl : "";
    result.l3 = sao != "" && pao != "" ? sl : "";
    return result;
  };
  const selectOption = (option: any) => {
    setQuery(() => "");
    // handleChange(option[label]);
    setIsOpen((isOpen) => !isOpen);

    if (option.LPI.ADDRESS) {
      const result = setProperties(option);
      const convertedCoordinates = proj4("EPSG:27700", destProjection, [
        option.LPI.X_COORDINATE,
        option.LPI.Y_COORDINATE,
      ]);
      console.log(convertedCoordinates, "convertedCoordinates");

      set({
        organization:
          option.LPI.ORGANISATION == undefined ? "" : option.LPI.ORGANISATION,
        line1: result.l1,
        line2: result.l2,
        line3: result.l3,
        locality:
          option.LPI.LOCALITY_NAME == undefined ? "" : option.LPI.LOCALITY_NAME,
        towncity: option.LPI.TOWN_NAME == undefined ? "" : option.LPI.TOWN_NAME,
        county:
          option.LPI.ADMINISTRATIVE_AREA == undefined
            ? ""
            : option.LPI.ADMINISTRATIVE_AREA,
        country: "UNITED KINGDOM",
        postcode: option.LPI.POSTCODE_LOCATOR,
        uprn: option.LPI.UPRN,
        // xcord: Number(option.LPI.X_COORDINATE),
        // ycord: Number(option.LPI.Y_COORDINATE),
        xcord: convertedCoordinates[0],
        ycord: convertedCoordinates[1],
        address: option.LPI.ADDRESS,
      });
      setSelectedVal(option.LPI.ADDRESS);
    } else {
      clear();
      setSelectedVal("");
    }
    props.notify();

    return option;
  };

  function toggle(e: any) {
    setIsOpen(e && e.target === inputRef.current);
  }

  const getDisplayValue = () => {
    if (query) return query;
    if (selectedVal) return selectedVal;

    return "";
  };

  const filter = (options: any) => {
    return options.filter(
      (option: any) =>
        option[props.label].toLowerCase().indexOf(query.toLowerCase()) > -1
    );
  };

  const handleChange = async (value: string, page: number) => {
    setLoading(true);
    try {
      if (value == "") {
        setSelectedVal("");
        setAddressList([]);
        return;
      }

      const result = await axios.get(
        // `https://api.os.uk/search/places/v1/postcode?postcode=${value}&key=4ZqAIx6MIR0VWVu7eDXv9Gz2OxsA084v&dataset=LPI&offset=${page}`
        `${ApiUrl}?postcode=${value}&key=${ApiKey}&dataset=LPI&offset=${page}`
      );

      // setAddressList(addressList.concat(result?.data?.results || []));
      setAddressList((prevState) => [
        ...prevState,
        ...(result?.data?.results || []),
      ]);
      console.log(result?.data.header?.totalresults, "result?.data?");
      setTotalRecord(result?.data.header?.totalresults);
      setLoading(false);
    } catch (error) {
      /* empty */
      setLoading(false);
    }
  };

  const handleScroll = (e: any) => {
    const bottom =
      e.target.scrollHeight - e.target.scrollTop - e.target.clientHeight < 2;
    if (bottom && currentPage < totalRecord) {
      handleChange(query, currentPage + 100);
      setCurrentPage(currentPage + 100);
    }
  };

  return (
    <div className="dropdown">
      <div className="control">
        <div className="selected-value">
          <input
            ref={inputRef}
            type="text"
            value={getDisplayValue()}
            name="searchTerm"
            placeholder="Enter your postcode here."
            onChange={(e) => {
              setAddressList([]);
              setCurrentPage(0);
              setTotalRecord(0);
              setQuery(e.target.value);
              if (e.target.value !== "") {
                handleChange(e.target.value, 0);
              }
            }}
            onClick={toggle}
          />
        </div>
        <div className={`arrow ${isOpen ? "open" : ""}`}></div>
      </div>

      <div
        onScroll={handleScroll}
        className={`options ${isOpen ? "open" : ""}`}
      >
        {addressList.length > 0 && (
          <div className="dropdown-content">
            {addressList.map((option: any, ind: any) => (
              <a
                style={{ display: "block" }}
                key={ind}
                onClick={() => selectOption(option)}
              >
                {option.LPI.ADDRESS}
              </a>
            ))}
            {loading && <>Loading....</>}
          </div>
        )}
      </div>
    </div>
  );
};
export default AddressFinderComponent;
