import { useEffect, useRef, useState } from "react";
import React = require("react");

const SearchableDropdown = (props: any) => {
  const [query, setQuery] = useState("");
    const [isOpen, setIsOpen] = useState(false);

  const inputRef = useRef(null);

  useEffect(() => {
    document.addEventListener("click", toggle);
    return () => document.removeEventListener("click", toggle);
  }, []);

  const selectOption = (option: any) => {
    setQuery(() => "");
    props.handleChange(option[props.label]);
    setIsOpen((isOpen) => !isOpen);
  };

  function toggle(e: any) {
    setIsOpen(e && e.target === inputRef.current);
  }

  const getDisplayValue = () => {
    if (query) return query;
    if (props.selectedVal) return props.selectedVal;

    return "";
  };

  const filter = (options: any) => {
    return options.filter(
      (option: any) => option[props.label].toLowerCase().indexOf(query.toLowerCase()) > -1
    );
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
            onChange={(e) => {
              setQuery(e.target.value);
              props.handleChange(null);
            }}
            onClick={toggle}
          />
        </div>
        <div className={`arrow ${isOpen ? "open" : ""}`}></div>
      </div>

      <div className={`options ${isOpen ? "open" : ""}`}>
        {filter(props.options).map((option: any, index: any) => {
          return (
            <div
              onClick={() => selectOption(option)}
              className={`option ${option[props.label] === props.selectedVal ? "selected" : ""
                }`}
              key={`${props.id}-${index}`}
            >
              {option[props.label]}
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default SearchableDropdown;
