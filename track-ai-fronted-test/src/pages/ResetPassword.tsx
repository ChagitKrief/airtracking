// File: src/pages/ResetPasswordFlow.tsx

import React, { useContext, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";
import {
  Box,
  Button,
  TextField,
  Typography,
  Stack,
  InputBase,
  Card,
  CardContent,
} from "@mui/material";

const ResetPasswordFlow = () => {
  const [error, setError] = useState("");
  const { user, sendResetPassword, verifyTempPassword, resetPassword } =
    useContext(AuthContext)!;
  const navigate = useNavigate();

  const [step, setStep] = useState<"start" | "verify" | "newPassword">("start");
  const [tempPassword, setTempPassword] = useState<string[]>(Array(8).fill(""));
  const [newPassword, setNewPassword] = useState("");
  const [storedTempPassword, setStoredTempPassword] = useState("");

  const inputRefs = useRef<Array<HTMLInputElement | null>>([]);

  const handleInputChange = (index: number, value: string) => {
    if (/^[a-zA-Z0-9]*$/.test(value)) {
      const updated = [...tempPassword];
      if (value.length === 1) {
        updated[index] = value;
        setTempPassword(updated);
        if (index < inputRefs.current.length - 1) {
          inputRefs.current[index + 1]?.focus();
        }
      } else if (value.length === 8 && index === 0) {
        const chars = value.split("").slice(0, 8);
        setTempPassword(chars);
        chars.forEach((char, i) => {
          if (inputRefs.current[i]) {
            inputRefs.current[i]!.value = char;
          }
        });
        inputRefs.current[7]?.focus();
      }
    }
  };

  const handleKeyDown = (index: number, e: React.KeyboardEvent) => {
    if (e.key === "Backspace" && !tempPassword[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    } else if (e.key === "ArrowRight" && index < 7) {
      inputRefs.current[index + 1]?.focus();
    } else if (e.key === "ArrowLeft" && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  const handleSendResetPassword = async () => {
    const success = await sendResetPassword(user?.id!);
    if (success) setStep("verify");
  };

  const handleVerifyPassword = async () => {
    const password = tempPassword.join("");
    const success = await verifyTempPassword(user?.id!, password);
    if (success) {
      setStoredTempPassword(password);
      setStep("newPassword");
      setError("");
    } else {
      setTempPassword(Array(8).fill(""));
      inputRefs.current[0]?.focus();
      setError("Invalid or expired temporary password. Please try again.");
      setTimeout(() => setError(""), 2000);
    }
  };

  const handleResetPassword = async () => {
    const success = await resetPassword(
      user?.id!,
      storedTempPassword,
      newPassword
    );
    if (success) {
      navigate("/login");
    }
  };

  return (
    <Box
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="100vh"
      mt={-4}
    >
      <Card sx={{ p: 4, width: 400, boxShadow: 3 }}>
        <CardContent>
          <Box
            display="flex"
            flexDirection="column"
            gap={3}
            alignItems="center"
          >
            {step === "start" && (
              <>
                <Typography variant="h5">Reset your password</Typography>
                <Button
                  variant="contained"
                  onClick={handleSendResetPassword}
                  sx={{
                    bgcolor: "#1a237e",
                    color: "white",
                    fontWeight: "bold",
                    textTransform: "none",
                    borderRadius: "8px",
                    px: 4,
                  }}
                >
                  Send me a temporary password
                </Button>
                {user ? (
                  <Button
                    variant="outlined"
                    onClick={() => navigate("/dashboard")}
                    sx={{
                      mt: 1,
                      color: "#1a237e",
                      borderColor: "#1a237e",
                      fontWeight: "bold",
                      textTransform: "none",
                      borderRadius: "8px",
                      px: 4,
                      "&:hover": {
                        backgroundColor: "#e8eaf6",
                        borderColor: "#1a237e",
                      },
                    }}
                  >
                    Back to Dashboard
                  </Button>
                ) : (
                  <Button
                    variant="outlined"
                    onClick={() => navigate("/login")}
                    sx={{
                      mt: 1,
                      color: "#1a237e",
                      borderColor: "#1a237e",
                      fontWeight: "bold",
                      textTransform: "none",
                      borderRadius: "8px",
                      px: 4,
                      "&:hover": {
                        backgroundColor: "#e8eaf6",
                        borderColor: "#1a237e",
                      },
                    }}
                  >
                    Back to login
                  </Button>
                )}
              </>
            )}

            {step === "verify" && (
              <>
                <Typography variant="h6">Check your inbox Email... </Typography>
                <Stack
                  direction="row"
                  spacing={1}
                  className={error ? "shake" : ""}
                >
                  {tempPassword.map((val, idx) => (
                    <InputBase
                      key={idx}
                      defaultValue={val}
                      inputRef={(el) => (inputRefs.current[idx] = el)}
                      onChange={(e) => handleInputChange(idx, e.target.value)}
                      onKeyDown={(e) => handleKeyDown(idx, e)}
                      inputProps={{
                        maxLength: 8,
                        style: { textAlign: "center" },
                      }}
                      sx={{
                        width: 36,
                        height: 48,
                        border: "1px solid #ccc",
                        borderRadius: 1,
                        fontSize: 20,
                      }}
                    />
                  ))}
                </Stack>
                {error && (
                  <Typography color="error" fontSize={14}>
                    {error}
                  </Typography>
                )}
                <Button
                  variant="contained"
                  onClick={handleVerifyPassword}
                  sx={{
                    bgcolor: "#1a237e",
                    color: "white",
                    fontWeight: "bold",
                    textTransform: "none",
                    borderRadius: "8px",
                    px: 4,
                  }}
                >
                  Verify identity
                </Button>

                <Typography
                  component="button"
                  onClick={handleSendResetPassword}
                  sx={{
                    mt: 1,
                    color: "#ff6f00",
                    background: "none",
                    border: "none",
                    padding: 0,
                    cursor: "pointer",
                    textDecoration: "underline",
                    fontSize: "0.875rem",
                    fontWeight: 500,
                    "&:hover": { color: "#ff9100" },
                  }}
                >
                  Resend temporary password
                </Typography>
                <Button
                  variant="outlined"
                  onClick={() => navigate("/dashboard")}
                  sx={{
                    mt: 1,
                    color: "#1a237e",
                    borderColor: "#1a237e",
                    fontWeight: "bold",
                    textTransform: "none",
                    borderRadius: "8px",
                    px: 4,
                    "&:hover": {
                      backgroundColor: "#e8eaf6",
                      borderColor: "#1a237e",
                    },
                  }}
                >
                  Back to Dashboard
                </Button>
              </>
            )}

            {step === "newPassword" && (
              <>
                <Typography variant="h6">Enter a new password</Typography>
                <TextField
                  type="password"
                  fullWidth
                  label="New Password"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                />
                <Button
                  variant="contained"
                  onClick={handleResetPassword}
                  sx={{
                    bgcolor: "#1a237e",
                    color: "white",
                    fontWeight: "bold",
                    textTransform: "none",
                    borderRadius: "8px",
                    px: 4,
                  }}
                >
                  Reset Password
                </Button>
              </>
            )}
          </Box>
          <style>{`
  .shake {
    animation: shake 0.3s;
  }
  @keyframes shake {
    0% { transform: translateX(0); }
    25% { transform: translateX(-4px); }
    50% { transform: translateX(4px); }
    75% { transform: translateX(-4px); }
    100% { transform: translateX(0); }
  }
`}</style>
        </CardContent>
      </Card>
    </Box>
  );
};

export default ResetPasswordFlow;
