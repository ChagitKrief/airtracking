// File: src/context/AuthContext.tsx

import { createContext, useState, useEffect, ReactNode } from "react";
import axiosInstance from "../api/axiosInstance";
import { User } from "../types/AuthTypes";
import { useLoading } from "./LoadingContext";
import { Snackbar, AlertColor } from "@mui/material";
import { useNavigate } from "react-router-dom";

interface AuthContextType {
  user: User | null;
  isInitialized: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  fetchUser: () => Promise<void>;
  sendResetPassword: (userId: string) => Promise<boolean>;
  verifyTempPassword: (userId: string, tempPassword: string) => Promise<boolean>;
  resetPassword: (userId: string, tempPassword: string, newPassword: string) => Promise<boolean>;
}

interface ToastState {
  open: boolean;
  message: string;
  severity: AlertColor;
}

export const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isInitialized, setIsInitialized] = useState(false);
  const { showLoading, hideLoading } = useLoading();
  const [toast, setToast] = useState<ToastState>({
    open: false,
    message: "",
    severity: "info",
  });
  const navigate = useNavigate();
  const showToast = (message: string, severity: AlertColor = "info") => {
    setToast({ open: true, message, severity });
  };

  const closeToast = () => setToast({ ...toast, open: false });

  // useEffect(() => {
  //   const token = localStorage.getItem("token");

  //   if (token) {
  //     axiosInstance.defaults.headers.common["Authorization"] = `Bearer ${token}`;
  //     fetchUser();
  //   } else {
  //     setIsInitialized(true);
  //   }
  // }, []);

  

  const fetchUser = async () => {
    try {
      showLoading();
      const { data } = await axiosInstance.get<User>("/auth/me");

      setUser({
        id: data.id,
        email: data.email,
        role: data.role,
        firstName: data.firstName,
        lastName: data.lastName,
        phone: data.phone,
        expiration: data.expiration,
        reminders: data.reminders ?? [],
        activeCustomers: data.activeCustomers ?? [],
        winwordData: Array.isArray(data.winwordData)
          ? { data: { trackedShipments: { data: data.winwordData } } }
          : data.winwordData,
      });
    } catch (error: any) {
      console.error("Failed to fetch user", error);
      logout(); 
    } finally {
      hideLoading();
      setIsInitialized(true);
    }
  };

  const login = async (email: string, password: string) => {
    try {
      showLoading();
      const { data } = await axiosInstance.post("/auth/login", { email, password });
      localStorage.setItem("token", data.token);
      axiosInstance.defaults.headers.common["Authorization"] = `Bearer ${data.token}`;

      setUser({
        id: data.id,
        email: data.email,
        role: data.role,
        firstName: data.firstName,
        lastName: data.lastName,
        expiration: data.expiration,
        phone: data.phone,
        reminders: data.reminders ?? [],
        activeCustomers: data.activeCustomers ?? [],
        winwordData: Array.isArray(data.winwordData)
          ? { data: { trackedShipments: { data: data.winwordData } } }
          : data.winwordData,
      });
    } catch (error) {
      console.error("Login failed", error);
      throw new Error("Login failed");
    } finally {
      hideLoading();
    }
  };

  const logout = () => {
    localStorage.removeItem("token");
    setUser(null);
    delete axiosInstance.defaults.headers.common["Authorization"];
    navigate("/login", { replace: true }); 
  };
  

  const sendResetPassword = async (userId: string) => {
    try {
      showLoading();
      await axiosInstance.post("/auth/reset-password-request", userId);
      showToast("Temporary password sent to your email", "success");
      return true;
    } catch {
      showToast("Failed to send temporary password", "error");
      return false;
    } finally {
      hideLoading();
    }
  };

  const verifyTempPassword = async (userId: string, tempPassword: string) => {
    try {
      showLoading();
      await axiosInstance.post("/auth/verify-temp-password", {
        userId,
        tempPassword,
      });
      showToast("Identity verified", "success");
      return true;
    } catch {
      showToast("Invalid or expired temporary password", "error");
      return false;
    } finally {
      hideLoading();
    }
  };

  const resetPassword = async (userId: string, tempPassword: string, newPassword: string) => {
    try {
      showLoading();
      await axiosInstance.post("/auth/reset-password", {
        userId,
        tempPassword,
        newPassword,
      });
      showToast("Password reset successfully", "success");
      return true;
    } catch {
      showToast("Password reset failed", "error");
      return false;
    } finally {
      hideLoading();
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isInitialized,
        login,
        logout,
        fetchUser,
        sendResetPassword,
        verifyTempPassword,
        resetPassword,
      }}
    >
      {children}
      <Snackbar
        open={toast.open}
        autoHideDuration={3000}
        onClose={closeToast}
        anchorOrigin={{ vertical: "top", horizontal: "center" }}
      >
        <div style={{ padding: 0 }}>
          <div
            style={{
              backgroundColor: toast.severity === "success" ? "#4caf50" : "#f44336",
              color: "#fff",
              padding: "10px 20px",
              borderRadius: 4,
            }}
          >
            {toast.message}
          </div>
        </div>
      </Snackbar>
    </AuthContext.Provider>
  );
};
