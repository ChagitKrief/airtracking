import React, { useState, useContext } from "react";
import {
  AppBar,
  Toolbar,
  Typography,
  Avatar,
  Menu,
  MenuItem,
  ListItemIcon,
  Divider,
  Box,
  Tooltip,
} from "@mui/material";
import { Dashboard, Business, People, Lock, Logout } from "@mui/icons-material";
import { useNavigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";

export default function Navbar() {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const navigate = useNavigate();
  const authContext = useContext(AuthContext);
  const user = authContext?.user;

  const fullName = `${user?.firstName ?? ""} ${user?.lastName ?? ""}`;
  const initials = `${user?.firstName?.charAt(0) ?? ""}${
    user?.lastName?.charAt(0) ?? ""
  }`.toUpperCase();
  const companies = user?.activeCustomers ?? [];
  const companyNames = companies.map((company) => company.customerName);
  const companyDisplay =
    companyNames.length > 2
      ? `${companyNames.slice(0, 2).join(", ")}...`
      : companyNames.join(", ");
  const isAdmin = user?.role === 3;
  const isSuperAdmin = user?.role === 1;

  const handleAvatarClick = (event: React.MouseEvent<HTMLDivElement>) => {
    setAnchorEl(event.currentTarget as HTMLElement);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleNavigate = (path: string) => {
    navigate(path);
    handleClose();
  };

  return (
    <AppBar
      position="fixed"
      sx={{ backgroundColor: "#1a237e", height: "90px", zIndex: 100 }}
    >
      <Toolbar
        sx={{ display: "flex", alignItems: "center", minHeight: "90px" }}
      >
        <Typography
          variant="h6"
          sx={{ flexGrow: 1, fontWeight: "bold", mt: 1 }}
        >
          <img
            src="/Krief-white-logo.png"
            alt="Logo"
            style={{ height: 40, marginRight: 10 }}
          />
        </Typography>

        <Typography sx={{ mr: 2, mt: 1 }}>
          {fullName} <br />
          <Typography variant="caption">{companyDisplay}</Typography>
        </Typography>
        {user && (
          <Tooltip title="Open advanced filter options">
            <Avatar
              sx={{ bgcolor: "orange", ml: 2, cursor: "pointer", mt: 1 }}
              onClick={handleAvatarClick}
            >
              {initials}
            </Avatar>
          </Tooltip>
        )}
        <Menu
          anchorEl={anchorEl}
          open={Boolean(anchorEl)}
          onClose={handleClose}
          PaperProps={{
            elevation: 3,
            sx: { width: 250, borderRadius: 2, mt: 2, overflow: "visible" },
          }}
          transformOrigin={{ horizontal: "center", vertical: "top" }}
          anchorOrigin={{ horizontal: "center", vertical: "bottom" }}
        >
          <Box
            sx={{
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              p: 2,
            }}
          >
            <Avatar sx={{ bgcolor: "orange", width: 56, height: 56 }}>
              {initials}
            </Avatar>
            <Typography sx={{ fontWeight: "bold", mt: 1 }}>
              {fullName}
            </Typography>
          </Box>

          <Divider />

          <MenuItem onClick={() => handleNavigate("/dashboard")}>
            <ListItemIcon>
              <Dashboard fontSize="small" />
            </ListItemIcon>
            Dashboard
          </MenuItem>

          {(isAdmin || isSuperAdmin) && (
            <MenuItem onClick={() => handleNavigate("/manage-company")}>
              <ListItemIcon>
                <Business fontSize="small" />
              </ListItemIcon>
              Manage Company
            </MenuItem>
          )}

          {(isAdmin || isSuperAdmin) && (
            <MenuItem onClick={() => handleNavigate("/manage-users")}>
              <ListItemIcon>
                <People fontSize="small" />
              </ListItemIcon>
              Manage Users
            </MenuItem>
          )}

          <MenuItem onClick={() => handleNavigate("/change-password")}>
            <ListItemIcon>
              <Lock fontSize="small" />
            </ListItemIcon>
            Change Password
          </MenuItem>

          <Divider />

          <MenuItem onClick={() => handleNavigate("/logout")}>
            <ListItemIcon>
              <Logout fontSize="small" />
            </ListItemIcon>
            Log out
          </MenuItem>
        </Menu>
      </Toolbar>
    </AppBar>
  );
}
