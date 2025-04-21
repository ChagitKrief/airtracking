import React, { createContext, useContext, useState } from "react";
import axiosInstance from "../api/axiosInstance";
import { isAxiosError } from "axios";
import { Snackbar, Alert } from "@mui/material";
import { useLoading } from "./LoadingContext";
import { hasDuplicatePhones, normalizePhone } from "../utils/smsUtils";

export interface UserPhoneEntry {
  userId: string;
  phones: string[];
}

export interface SmsDto {
  id: string;
  container: string;
  userPhones: UserPhoneEntry[];
}

interface SmsContextType {
  userPhones: SmsDto[];
  fetchUserPhones: (userId: string) => Promise<void>;
  addOrUpdatePhones: (
    container: string,
    entry: UserPhoneEntry
  ) => Promise<void>;
  removePhones: (container: string, userId: string) => Promise<void>;
}

const SmsContext = createContext<SmsContextType | undefined>(undefined);

export const SmsProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [userPhones, setUserPhones] = useState<SmsDto[]>([]);
  const { showLoading, hideLoading } = useLoading();

  // Toast state
  const [toastOpen, setToastOpen] = useState(false);
  const [toastMessage, setToastMessage] = useState("");
  const [toastSeverity, setToastSeverity] = useState<"success" | "error">(
    "success"
  );

  const showToast = (message: string, severity: "success" | "error") => {
    setToastMessage(message);
    setToastSeverity(severity);
    setToastOpen(true);
  };

  const handleToastClose = () => {
    setToastOpen(false);
  };

  const fetchUserPhones = async (userId: string) => {
    showLoading();
    try {
      const response = await axiosInstance.get<SmsDto[]>(
        `/sms/by-user/${userId}`
      );
      setUserPhones(response.data);
    } catch (error) {
      console.error("Failed to fetch SMS by user:", error);
      showToast("Failed to load SMS messages", "error");
    } finally {
      hideLoading();
    }
  };

  const addOrUpdatePhones = async (
    container: string,
    entry: UserPhoneEntry
  ) => {
    showLoading();
    try {
      const normalizedPhones = entry.phones
        .filter((p) => p.trim() !== "")
        .map(normalizePhone);

      if (hasDuplicatePhones(entry.phones)) {
        showToast("Duplicate phone numbers are not allowed", "error");
        return;
      }

      await axiosInstance.post(`/sms/${container}`, {
        userId: entry.userId,
        phones: normalizedPhones,
      });

      await fetchUserPhones(entry.userId);
      showToast("Phone numbers updated successfully", "success");
    } catch (error) {
      if (isAxiosError(error) && error.response) {
        const status = error.response.status;
        const message = error.response.data?.message || "Unknown error";
        if (status === 409) {
          showToast(message, "error");
        } else {
          showToast("Failed to save phone numbers", "error");
        }
      } else {
        showToast("Unexpected error occurred", "error");
      }
      console.error("Failed to add or update phones:", error);
    } finally {
      hideLoading();
    }
  };

  const removePhones = async (container: string, userId: string) => {
    showLoading();
    try {
      await axiosInstance.delete(`/sms/${container}/${userId}`);
      await fetchUserPhones(userId);
      showToast("Phone numbers removed successfully", "success");
    } catch (error) {
      console.error("Failed to delete phones:", error);
      showToast("Failed to delete phone numbers", "error");
    } finally {
      hideLoading();
    }
  };

  return (
    <SmsContext.Provider
      value={{ userPhones, fetchUserPhones, addOrUpdatePhones, removePhones }}
    >
      {children}
      <Snackbar
        open={toastOpen}
        autoHideDuration={5000}
        onClose={handleToastClose}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert
          onClose={handleToastClose}
          severity={toastSeverity}
          sx={{ width: "100%" }}
        >
          {toastMessage}
        </Alert>
      </Snackbar>
    </SmsContext.Provider>
  );
};

export const useSmsContext = () => {
  const context = useContext(SmsContext);
  if (!context)
    throw new Error("useSmsContext must be used within a SmsProvider");
  return context;
};
