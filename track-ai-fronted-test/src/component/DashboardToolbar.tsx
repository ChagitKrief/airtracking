// 📁 File: src/component/DashboardToolbar.tsx

import React from "react";
import {
  Box,
  Button,
  Select,
  MenuItem,
  Checkbox,
  TextField,
  Popper,
  Paper,
  ClickAwayListener,
  Tooltip,
} from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import FilterListIcon from "@mui/icons-material/FilterList";
import CloudDownloadIcon from "@mui/icons-material/CloudDownload";
import FiltersPanel from "./FiltersPanel";
import { useTheme } from "@mui/material/styles";
import useMediaQuery from "@mui/material/useMediaQuery";

interface Company {
  id: string;
  customerNumber: string;
  customerName: string;
}

interface Props {
  searchText: string;
  requestSearch: (text: string) => void;
  filterOpen: boolean;
  setFilterOpen: (open: boolean) => void;
  selectedCompanies: string[];
  setSelectedCompanies: (companies: string[]) => void;
  pendingInsights: string[];
  pendingStatuses: string[];
  handlePendingFilterChange: (
    value: string,
    type: "insight" | "status"
  ) => void;
  handleDownloadExcel: () => void;
  companies: Company[];
  onApplyFilters: () => void;
}

const DashboardToolbar: React.FC<Props> = ({
  searchText,
  requestSearch,
  filterOpen,
  setFilterOpen,
  selectedCompanies,
  setSelectedCompanies,
  pendingInsights,
  pendingStatuses,
  handlePendingFilterChange,
  handleDownloadExcel,
  companies,
  onApplyFilters,
}) => {
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("sm"));
  const isTablet = useMediaQuery(theme.breakpoints.between("sm", "md"));
  const isNarrow = isMobile || isTablet;

  const handleFilterClick = (event: React.MouseEvent<HTMLElement>) => {
    if (!filterOpen) {
      setAnchorEl(event.currentTarget);
      setFilterOpen(true);
    } else {
      setFilterOpen(false);
    }
  };

  const handleClickAway = () => {
    setFilterOpen(false);
    // setAnchorEl(null);
  };

  return (
    <Box
      display="flex"
      flexDirection={isNarrow ? "column" : "row"}
      alignItems={isNarrow ? "stretch" : "center"}
      justifyContent="space-between"
      gap={isNarrow ? 1.5 : 2}
      sx={{ my: 2, px: isNarrow ? 1 : 0 }}
    >
      <Tooltip title="Global Search">
        <TextField
          variant="outlined"
          placeholder="Search in list"
          value={searchText}
          onChange={(e) => requestSearch(e.target.value)}
          InputProps={{
            startAdornment: <SearchIcon sx={{ color: "gray", mr: 1 }} />,
          }}
          sx={{
            width: isMobile ? "100%" : 250,
            bgcolor: "white",
            borderRadius: "4px",
          }}
        />
      </Tooltip>

      <Box>
        <Tooltip title="Open advanced filter options">
          <Button
            variant="outlined"
            startIcon={<FilterListIcon />}
            onClick={handleFilterClick}
            sx={{
              mx: isMobile ? 0 : 1,
              width: isMobile ? "100%" : "auto",
              bgcolor: "white",
              borderRadius: "8px",
              fontWeight: 600,
              textTransform: "none",
            }}
          >
            Filter
          </Button>
        </Tooltip>
        <Popper
          open={filterOpen}
          anchorEl={anchorEl}
          placement="bottom-start"
          modifiers={[{ name: "offset", options: { offset: [0, 8] } }]}
          style={{ zIndex: 1300 }}
        >
          <ClickAwayListener onClickAway={handleClickAway}>
            <Paper
              elevation={3}
              sx={{
                width: 260,
                bgcolor: "white",
                borderRadius: 2,
                boxShadow: 3,
                p: 2,
                maxHeight: 400,
                overflowY: "auto",
              }}
            >
              <FiltersPanel
                open={filterOpen}
                onClose={onApplyFilters}
                selectedInsights={pendingInsights}
                selectedStatuses={pendingStatuses}
                handleFilterChange={handlePendingFilterChange}
                onApplyFilters={onApplyFilters}
              />
            </Paper>
          </ClickAwayListener>
        </Popper>
      </Box>

      <Tooltip title="Filter data by one or more companies">
        <Select
          multiple
          displayEmpty
          value={selectedCompanies}
          onChange={(e) => {
            const value = e.target.value;
            const selected =
              typeof value === "string" ? value.split(",") : value;
            setSelectedCompanies(selected);
            onApplyFilters();
          }}
          renderValue={(selected) => {
            if (selected.length === 0) {
              return <em style={{ color: "#999" }}>Select Company</em>;
            }

            const selectedNames = companies
              .filter((c) => selected.includes(c.customerNumber.toString()))
              .map((c) => c.customerName);

            return selectedNames.join(", ");
          }}
          sx={{
            minWidth: isMobile ? "100%" : 300,
            bgcolor: "white",
            borderRadius: "4px",
            fontWeight: 500,
          }}
        >
          {companies.map((company) => {
            const customerNum = company.customerNumber.toString();
            return (
              <MenuItem key={company.id} value={customerNum}>
                <Checkbox checked={selectedCompanies.includes(customerNum)} />
                {company.customerName}
              </MenuItem>
            );
          })}
        </Select>
      </Tooltip>
      <Tooltip title="Download current table as Excel file">
        <Button
          sx={{
            ml: isNarrow ? 0 : 1,
            width: isNarrow ? "100%" : "auto",
            bgcolor: "orange",
            color: "white",
            fontWeight: "bold",
            textTransform: "none",
          }}
          onClick={handleDownloadExcel}
        >
          <CloudDownloadIcon />
        </Button>
      </Tooltip>
    </Box>
  );
};

export default DashboardToolbar;
