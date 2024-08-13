import React, { useState, useEffect, useRef } from "react";

interface CustomSearchDropdownProps {
  // Define any props you may need
}

const CustomSearchDropdown: React.FC<CustomSearchDropdownProps> = () => {
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [searchResults, setSearchResults] = useState<string[]>([]);
  const [isDropdownVisible, setIsDropdownVisible] = useState<boolean>(false);
  const [selectedItem, setSelectedItem] = useState<string>("");
  const dropdownRef = useRef<HTMLDivElement>(null);

  const allResults: string[] = ["Hamza 1", "Ali 2", "Musa 3"];

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const query = e.target.value;
    setSearchTerm(query);
    performSearch(query);
    // if (query.length < 1) {
    //   setIsDropdownVisible(false);
    // } else {
    //   setIsDropdownVisible(true);
    // }
  };

  const handleInputFocus = () => {
    // Show all results when the input is focused
    setSearchResults(allResults);
    setIsDropdownVisible(true);
  };

  const performSearch = (query: string) => {
    // Filter the results based on the query
    const filteredResults = allResults.filter((result) =>
      result.toLowerCase().includes(query.toLowerCase())
    );
    setSearchResults(filteredResults);
  };
  const handleDocumentClick = (e: MouseEvent) => {
    // Close the dropdown if the click is outside the dropdown or input field
    if (
      dropdownRef.current &&
      !dropdownRef.current.contains(e.target as Node)
    ) {
      setIsDropdownVisible(false);
    }
  };
  const handleItemClick = (item: string) => {
    // Set the selected item in the input field
    setSelectedItem(item);
    setIsDropdownVisible(false); // Hide the dropdown
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Backspace") {
      // Handle backspace key press to remove one character at a time
      setSelectedItem(selectedItem.slice(0, -1));
    }
  };

  useEffect(() => {
    // Attach the click event listener to the document
    document.addEventListener("click", handleDocumentClick);
    // Clean up the event listener on component unmount
    return () => {
      document.removeEventListener("click", handleDocumentClick);
    };
  }, []);

  return (

    <div className="custom-dropdown" ref={dropdownRef}>

      <input
        type="text"
        id="search-input"
        placeholder="Search..."
        value={selectedItem || searchTerm}
        onChange={handleSearchChange}
        onFocus={handleInputFocus}
        onKeyDown={handleKeyDown}
      />
      {isDropdownVisible && (
        <ul id="search-results">
          {searchResults.map((result, index) => (
            <li key={index} onClick={() => handleItemClick(result)}>
              {result}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};

export default CustomSearchDropdown;
