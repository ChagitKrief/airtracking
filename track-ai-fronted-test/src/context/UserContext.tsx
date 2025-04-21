import React, { createContext, useContext, useState } from "react";
import axios from "../api/axiosInstance";
import { useLoading } from "./LoadingContext";
import { Snackbar, Alert } from "@mui/material";
import { isAxiosError } from "axios";
interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  status: boolean;
  userType: string;
  company?: string;
  password?: string;
  reminders?:string[]
}

export interface Company {
  id: string;
  customerNumber: number;
  customerName: string;
  taxNumber: number;
  isActive: boolean;
}

interface UserContextType {
  users: User[];
  companies: Company[];
  fetchUsers: () => Promise<void>;
  updateUser: (id: string, user: Partial<User>) => Promise<void>;
  addUser: (user: Partial<User>) => Promise<void>;
}

const UserContext = createContext<UserContextType | undefined>(undefined);

export const UserProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [users, setUsers] = useState<User[]>([]);
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

  const fetchUsers = async () => {
    showLoading();
    try {
      const res = await axios.get("/Company/with-users");
      const companies = res.data;
      setCompanies(companies);

      const usersMap: Record<string, User> = {};

      companies.forEach((companyItem: any) => {
        const companyName = companyItem.customerName;
        const companyUsers = companyItem.users || [];

        companyUsers.forEach((u: any) => {
          if (!usersMap[u.id]) {
            usersMap[u.id] = {
              id: u.id,
              firstName: u.firstName,
              lastName: u.lastName,
              email: u.email,
              phoneNumber: u.phone,
              status: u.isActive,
              userType: u.role === 1 ? "Super Admin" : u.role === 2 ? "Standard" : "Admin",
              company: companyName,
              reminders: Array.isArray(u.reminders) ? u.reminders : [], 
            };
            
          } else {
            const existingUser = usersMap[u.id];
            existingUser.company = existingUser.company + ", " + companyName;
          }
        });
      });

      const aggregatedUsers = Object.values(usersMap);
      setUsers(aggregatedUsers);
    } catch (error) {
      console.error("Failed to fetch users:", error);
      showToast("Failed to load users", "error");
    } finally {
      hideLoading();
    }
  };

  const updateUser = async (id: string, updatedUser: Partial<User>) => {
    showLoading();
    try {
      await axios.put(`/User/${id}`, updatedUser);
      await fetchUsers();
      showToast("User updated successfully", "success");
    } catch (error) {
      console.error("Failed to update user:", error);
      showToast("Failed to update user", "error");
    } finally {
      hideLoading();
    }
  };

  const addUser = async (user: Partial<User>) => {
    showLoading();
    try {
      await axios.post("/User", user);
      await fetchUsers();
      showToast("User added successfully", "success");
    } catch (error: any) {
      if (isAxiosError(error) && error.response) {
        const message = error.response.data?.message || "Unknown error";
        const status = error.response.status;

        if (status === 409) {
          showToast(message, "error");
        } else {
          showToast("Failed to add user", "error");
        }
      } else {
        showToast("Unexpected error occurred", "error");
      }
      console.error("Error while adding user:", error);
    } finally {
      hideLoading();
    }
  };

  return (
    <UserContext.Provider value={{ users, fetchUsers, updateUser, addUser, companies }}>
      {children}
      <Snackbar open={toastOpen} autoHideDuration={5000} onClose={handleToastClose} anchorOrigin={{ vertical: "bottom", horizontal: "center" }}>
        <Alert onClose={handleToastClose} severity={toastSeverity} sx={{ width: "100%" }}>
          {toastMessage}
        </Alert>
      </Snackbar>
    </UserContext.Provider>
  );
};

export const useUserContext = () => {
  const context = useContext(UserContext);
  if (!context) throw new Error("useUserContext must be used within a UserProvider");
  return context;
};
