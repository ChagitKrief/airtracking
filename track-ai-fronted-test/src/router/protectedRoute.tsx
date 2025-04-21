import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";

// interface ProtectedRouteProps {
//   roles?: number[];
// }

// const ProtectedRoute = ({ roles }: ProtectedRouteProps) => {
//   const { user, isInitialized } = useAuth();

//   if (!isInitialized) return null;

//   const token = localStorage.getItem("token");

//   if (!token || !user) {
//     return <Navigate to="/login" replace />;
//   }

//   if (roles && !roles.includes(user.role)) {
//     return <Navigate to="/dashboard" replace />;
//   }

//   return <Outlet />;
// };

// export default ProtectedRoute;


const ProtectedRoute = () => {
  return <Outlet />;
};

export default ProtectedRoute;
