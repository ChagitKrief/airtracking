import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";
import {
  Container,
  TextField,
  Button,
  Typography,
  Box,
  IconButton,
  InputAdornment,
} from "@mui/material";
import { Visibility, VisibilityOff } from "@mui/icons-material";
import "../css/Login.css";

const Login = () => {
  const navigate = useNavigate();
  const { login,logout } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  // const [loading, setLoading] = useState(false);

  useEffect(() => {
    logout();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!email.includes("@")) {
      setError("Invalid email format");
      return;
    }

    // setLoading(true);
    try {
      await login(email, password);
      navigate("/dashboard");
    } catch {
      setError("Login failed. Please check your credentials.");
    } finally {
      // setLoading(false);
    }
  };

  return (
    <div className="login-container">
     
        <Container maxWidth="xs" className="login-box">
          <Box sx={{ textAlign: "center" }}>
            <Typography variant="h4" className="login-title">
              Shipment Tracking
            </Typography>
            <form onSubmit={handleSubmit}>
              <TextField
                label="User Name"
                type="email"
                fullWidth
                margin="normal"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                className="login-input"
              />
              <TextField
                label="Password"
                type={showPassword ? "text" : "password"}
                fullWidth
                margin="normal"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                className="login-input"
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowPassword(!showPassword)}
                        edge="end"
                      >
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />
              {error && <Typography color="error">{error}</Typography>}
              <Button
                type="submit"
                variant="contained"
                color="primary"
                fullWidth
                sx={{ mt: 2 }}
                className="login-button"
              >
                Sign In
              </Button>
              <Typography
                variant="body2"
                className="forgot-password"
                sx={{
                  mt: 2,
                  cursor: "pointer",
                  textAlign: "center",
                  textDecoration: "underline",
                }}
                onClick={() => navigate("/change-password")}
              >
                Forgot Password?
              </Typography>
            </form>
          </Box>
        </Container>
      
    </div>
  );
};

export default Login;
