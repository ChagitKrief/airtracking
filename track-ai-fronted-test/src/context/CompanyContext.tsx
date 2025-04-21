import React, { createContext, useContext, useState } from "react";
import axiosInstance from "../api/axiosInstance";
import { isAxiosError } from "axios";
import { Snackbar, Alert } from "@mui/material";
import { useLoading } from "./LoadingContext";

interface Company {
  id: string;
  customerNumber: number;
  customerName: string;
  taxNumber: number;
  isActive: boolean;
}

interface CompanyContextType {
  companies: Company[];
  fetchCompanies: () => Promise<void>;
  addCompany: (company: Omit<Company, "id">) => Promise<void>;
  updateCompany: (id: string, company: Omit<Company, "id">) => Promise<void>;
  deleteCompany: (id: string) => Promise<void>;
}

const CompanyContext = createContext<CompanyContextType | undefined>(undefined);

export const CompanyProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [companies, setCompanies] = useState<Company[]>([]);
  const { showLoading, hideLoading } = useLoading();

  // Toast State
  const [toastOpen, setToastOpen] = useState(false);
  const [toastMessage, setToastMessage] = useState("");
  const [toastSeverity, setToastSeverity] = useState<"success" | "error">("success");

  const showToast = (message: string, severity: "success" | "error") => {
    setToastMessage(message);
    setToastSeverity(severity);
    setToastOpen(true);
  };

  const handleToastClose = () => {
    setToastOpen(false);
  };

  const fetchCompanies = async () => {
    showLoading();
    try {
      const response = await axiosInstance.get("/Company");
      setCompanies(response.data);
    } catch (error) {
      console.error("Error fetching companies:", error);
      showToast("Failed to load companies", "error");
    } finally {
      hideLoading();
    }
  };

  const addCompany = async (company: Omit<Company, "id">) => {
    showLoading();
    try {
      const response = await axiosInstance.post("/Company", company);
      setCompanies([...companies, response.data]);
      showToast("Company added successfully", "success");
    } catch (error: any) {
      if (isAxiosError(error) && error.response) {
        const status = error.response.status;
        const message = error.response.data?.message || "Unknown error occurred";
        if (status === 409) {
          showToast(message, "error");
        } else {
          showToast("Failed to add company", "error");
        }
      } else {
        showToast("Unexpected error occurred", "error");
      }
      console.error("Error adding company:", error);
    } finally {
      hideLoading();
    }
  };

  const updateCompany = async (id: string, company: Omit<Company, "id">) => {
    showLoading();
    try {
      await axiosInstance.put(`/Company/${id}`, company);
      setCompanies(companies.map((c) => (c.id === id ? { id, ...company } : c)));
      showToast("Company updated successfully", "success");
    } catch (error) {
      console.error("Error updating company:", error);
      showToast("Failed to update company", "error");
    } finally {
      hideLoading();
    }
  };

  const deleteCompany = async (id: string) => {
    showLoading();
    try {
      await axiosInstance.delete(`/Company/${id}`);
      setCompanies(companies.filter((company) => company.id !== id));
      showToast("Company deleted successfully", "success");
    } catch (error) {
      console.error("Error deleting company:", error);
      showToast("Failed to delete company", "error");
    } finally {
      hideLoading();
    }
  };

  return (
    <CompanyContext.Provider
      value={{ companies, fetchCompanies, addCompany, updateCompany, deleteCompany }}
    >
      {children}
      <Snackbar
        open={toastOpen}
        autoHideDuration={5000}
        onClose={handleToastClose}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert onClose={handleToastClose} severity={toastSeverity} sx={{ width: "100%" }}>
          {toastMessage}
        </Alert>
      </Snackbar>
    </CompanyContext.Provider>
  );
};

export const useCompanyContext = () => {
  const context = useContext(CompanyContext);
  if (!context) {
    throw new Error("useCompanyContext must be used within a CompanyProvider");
  }
  return context;
};
