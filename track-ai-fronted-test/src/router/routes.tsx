import { Routes, Route, Navigate } from "react-router-dom";
import Login from "../pages/Login";
import Layout from "../Layout";
import ManageCompany from "../pages/ManageCompany";
import ManageUsers from "../pages/ManageUsers";
import ChangePassword from "../pages/ResetPassword";
import DashboardWrapper from "../component/DashboardWrapper";
import AddCompany from "../pages/AddCompany";
import AddUser from "../pages/AddUser";
import WindWrapperPage from "../pages/WindWrapperPage";
import ProtectedRoute from "./protectedRoute";

const AppRoutes = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="/login" element={<Login />} />
      <Route path="/logout" element={<Login />} />

      <Route element={<Layout />}>
        <Route
          path="external-tracking/:containerId"
          element={<WindWrapperPage />}
        />
        <Route path="/change-password" element={<ChangePassword />} />
        
        <Route element={<ProtectedRoute />}>
          <Route path="dashboard" element={<DashboardWrapper />} />

          <Route element={<ProtectedRoute roles={[1, 3]} />}>
            <Route path="manage-company" element={<ManageCompany />} />
            <Route path="manage-users" element={<ManageUsers />} />
            <Route path="add-company" element={<AddCompany />} />
            <Route path="add-user" element={<AddUser />} />
          </Route>
        </Route>
      </Route>
    </Routes>
  );
};

export default AppRoutes;
