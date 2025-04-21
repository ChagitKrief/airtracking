import React, { useState, useEffect, useRef } from "react";
import { useCompanyContext } from "../context/CompanyContext";
import {
  Box,
  Typography,
  Card,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Button,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Backdrop,
} from "@mui/material";
import { AddCircleOutline } from "@mui/icons-material";
import DownloadIcon from "@mui/icons-material/Download";
import { useNavigate } from "react-router-dom";
import { exportCompaniesToExcel } from "../utils/exportToExcelCompanes";

export default function ManageCompany() {
  const navigate = useNavigate();
  const { companies, fetchCompanies, updateCompany } = useCompanyContext();
  const didFetch = useRef(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const loadData = async () => {
      if (!didFetch.current && companies.length === 0) {
        setIsLoading(true);
        await fetchCompanies();
        setIsLoading(false);
        didFetch.current = true;
      } else {
        setIsLoading(false);
      }
    };
    loadData();
  }, [companies, fetchCompanies]);

  const [searchTerm, setSearchTerm] = useState("");
  const [selectedCompany, setSelectedCompany] = useState<{
    id: string;
    customerNumber: string;
    customerName: string;
    taxNumber: string;
    isActive: boolean;
  } | null>(null);

  const isRowSelected = Boolean(selectedCompany);

  const handleRowClick = (company: any) => {
    if (selectedCompany?.id === company.id) {
      setSelectedCompany(null);
    } else {
      setSelectedCompany({
        id: company.id,
        customerNumber: company.customerNumber.toString(),
        customerName: company.customerName,
        taxNumber: company.taxNumber.toString(),
        isActive: company.isActive,
      });
    }
  };

  const handleInputChange = (
    e: React.ChangeEvent<
      HTMLInputElement | HTMLTextAreaElement | { name?: string; value: unknown }
    >
  ) => {
    if (!selectedCompany) return;
    const { name, value } = e.target;

    setSelectedCompany((prev) => ({
      ...prev!,
      [name as string]: value,
    }));
  };

  const handleUpdate = async () => {
    if (!selectedCompany) return;

    try {
      await updateCompany(selectedCompany.id, {
        customerNumber: parseInt(selectedCompany.customerNumber),
        customerName: selectedCompany.customerName,
        taxNumber: parseInt(selectedCompany.taxNumber),
        isActive: selectedCompany.isActive,
      });
      setSelectedCompany(null);
    } catch (error) {
      console.error("Update failed:", error);
    }
  };

  const filteredCompanies = companies.filter((company) => {
    const combinedFields =
      company.customerNumber.toString() +
      company.customerName.toLowerCase() +
      company.taxNumber.toString() +
      (company.isActive ? "active" : "inactive");
    return combinedFields.includes(searchTerm.toLowerCase());
  });

  return (
    <Box
      sx={{
        p: { xs: 2, md: 3 },
        maxWidth: "1400px",
        mx: "auto",
      }}
    >
      <Typography
        variant="h4"
        textAlign="center"
        fontWeight="bold"
        color="#1a237e"
        mb={3}
      >
        Company Details
      </Typography>
      {/* <Backdrop
        open={isLoading}
        sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}
      >
        <div className="loading-image"></div>
      </Backdrop> */}

      <Card sx={{ p: 2, mb: 3, borderRadius: "12px", boxShadow: 2 }}>
      <Box
          display="flex"
          flexDirection={{ xs: "column", sm: "row" }}
          justifyContent="space-between"
          alignItems={{ xs: "stretch", sm: "center" }}
          gap={2}
          mb={2}
        >
          <TextField
             size="small"
             margin="dense"
             label="Search..."
             variant="outlined"
             value={searchTerm}
             onChange={(e) => setSearchTerm(e.target.value)}
             sx={{ flex: 1, mr: 2 }}
             fullWidth
          />
          <Box>
            <Button
            fullWidth
              startIcon={<AddCircleOutline />}
              sx={{ textTransform: "none", fontWeight: "bold", mr: 2 }}
              onClick={() => navigate("/add-company")}
            >
              Add New Company
            </Button>
            {/* <Button startIcon={<CloudUpload />} sx={{ textTransform: "none", fontWeight: "bold", mr: 2 }}>
              Upload Company
            </Button> */}
            <Button
            fullWidth
              startIcon={<DownloadIcon />}
              sx={{
                textTransform: "none",
                fontWeight: "bold",
                color: "#1a237e",
              }}
              onClick={() =>
                exportCompaniesToExcel(
                  filteredCompanies.map((c) => ({
                    customerNumber: c.customerNumber,
                    customerName: c.customerName,
                    taxNumber: c.taxNumber,
                    isActive: c.isActive,
                  }))
                )
              }
            >
              Download Sample File
            </Button>
          </Box>
        </Box>

        <TableContainer
          component={Paper}
          sx={{
            maxHeight: selectedCompany ? 400 : "calc(100vh - 200px)",
            overflowX: "auto",
            overflowY: "auto",
          }}
        >
          <Table stickyHeader>
            <TableHead>
              <TableRow sx={{ backgroundColor: "#f0f0f0" }}>
                <TableCell>Customer Number</TableCell>
                <TableCell>Customer Name</TableCell>
                <TableCell>Tax Number</TableCell>
                <TableCell>Status</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredCompanies.map((company) => (
                <TableRow
                  key={company.id}
                  onClick={() => handleRowClick(company)}
                  sx={{
                    cursor: "pointer",
                    backgroundColor:
                      selectedCompany?.id === company.id
                        ? "#e3f2fd"
                        : "inherit",
                    "&:hover": {
                      backgroundColor: "#e3f2fd80",
                    },
                  }}
                >
                  <TableCell>{company.customerNumber}</TableCell>
                  <TableCell>{company.customerName}</TableCell>
                  <TableCell>{company.taxNumber}</TableCell>
                  <TableCell>
                    {company.isActive ? "Active" : "Inactive"}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Card>

      {selectedCompany && (
        <Card sx={{ p: 3, borderRadius: "12px", boxShadow: 2 }}>
          <Typography variant="h6" sx={{ mb: 2, color: "#1a237e" }}>
            Update Company
          </Typography>
          <Box display="grid" gridTemplateColumns="repeat(2, 1fr)" gap={2}>
            <TextField
              label="Customer Number"
              name="customerNumber"
              value={selectedCompany.customerNumber}
              onChange={handleInputChange}
              fullWidth
            />
            <TextField
              label="Customer Name"
              name="customerName"
              value={selectedCompany.customerName}
              onChange={handleInputChange}
              fullWidth
            />
            <TextField
              label="Tax Number"
              name="taxNumber"
              value={selectedCompany.taxNumber}
              onChange={handleInputChange}
              fullWidth
            />
            <FormControl fullWidth>
              <InputLabel>Status</InputLabel>
              <Select
                name="isActive"
                value={selectedCompany.isActive ? "Active" : "Inactive"}
                onChange={(e) =>
                  setSelectedCompany((prev) => ({
                    ...prev!,
                    isActive: e.target.value === "Active",
                  }))
                }
              >
                <MenuItem value="Active">Active</MenuItem>
                <MenuItem value="Inactive">Inactive</MenuItem>
              </Select>
            </FormControl>
          </Box>
          <Box display="flex" justifyContent="center" gap={2} mt={3}>
            <Button
              variant="contained"
              onClick={handleUpdate}
              sx={{
                bgcolor: "#1a237e",
                color: "white",
                fontWeight: "bold",
                textTransform: "none",
                borderRadius: "8px",
                px: 4,
              }}
            >
              Update
            </Button>
            <Button
              variant="outlined"
              onClick={() => setSelectedCompany(null)}
              sx={{
                bgcolor: "#1a237e",
                color: "white",
                fontWeight: "bold",
                textTransform: "none",
                borderRadius: "8px",
                px: 4,
              }}
            >
              Cancel
            </Button>
          </Box>
        </Card>
      )}
    </Box>
  );
}
