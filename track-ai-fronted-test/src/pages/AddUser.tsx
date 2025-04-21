// components/AddUserForm.tsx
import { useContext, useState } from "react";
import {
  Box,
  Button,
  Card,
  MenuItem,
  Select,
  TextField,
  Typography,
  InputLabel,
  FormControl,
  Checkbox,
  Autocomplete,
  ListItemText,
} from "@mui/material";
import PhoneInput from "react-phone-input-2";
import "react-phone-input-2/lib/material.css";
import { useNavigate } from "react-router-dom";
import { useUserContext } from "../context/UserContext";
import { validateEmail } from "../utils/validateEmail";
import { AuthContext } from "../context/AuthContext";

interface NewUserForm {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  password: string;
  role: string;
  companyIds: string[];
  reminders: string[];
}

const defaultReminders = [
  "Sunday",
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
];

export default function AddUser() {
  const navigate = useNavigate();
  const { addUser, fetchUsers, companies } = useUserContext();
  const auth = useContext(AuthContext);
  const isSuperAdmin = auth?.user?.role === 1;
  const [companySearch, setCompanySearch] = useState("");
  const [form, setForm] = useState<NewUserForm>({
    firstName: "",
    lastName: "",
    email: "",
    phone: "",
    password: "",
    role: "Standard",
    companyIds: [],
    reminders: [],
  });

  const [errors, setErrors] = useState<{
    firstName?: string;
    lastName?: string;
    email?: string;
    password?: string;
    companyIds?: string;
  }>({});

  const handleSubmit = async () => {
    const newErrors: typeof errors = {};

    if (!form.firstName.trim()) newErrors.firstName = "First name is required.";
    if (!form.lastName.trim()) newErrors.lastName = "Last name is required.";
    if (!validateEmail(form.email)) newErrors.email = "Invalid email format.";
    if (form.password.length < 8)
      newErrors.password = "Password must be at least 8 characters.";
    if (form.companyIds.length === 0)
      newErrors.companyIds = "At least one company must be selected.";

    setErrors(newErrors);
    if (Object.keys(newErrors).length > 0) return;

    try {
      await addUser({
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        phone: form.phone || "",
        password: form.password,
        role: form.role === "Super Admin" ? 1 : form.role === "Admin" ? 3 : 2,
        isActive: true,
        reminders: form.reminders || [],
        verificationTmpPass: "",
        maxSubscriptionDate: null,
        createdAt: new Date().toISOString(),
        companyIds: form.companyIds,
      });

      await fetchUsers();
      navigate("/manage-users");
    } catch (error) {
      console.error("Error adding user:", error);
    }
  };

  const filteredCompanies = companies.filter(
    (company) =>
      company.customerName
        .toLowerCase()
        .includes(companySearch.toLowerCase()) ||
      company.customerNumber.toString().includes(companySearch)
  );

  return (
    <Box sx={{ p: 3, width: "100%", maxWidth: "900px", margin: "0 auto" }}>
      <Typography
        variant="h4"
        sx={{
          fontWeight: "bold",
          color: "#1a237e",
          textAlign: "center",
          mb: 3,
        }}
      >
        Add New User
      </Typography>

      <Card
        sx={{
          p: 4,
          borderRadius: "12px",
          boxShadow: 2,
          border: "1px solid #1a237e",
        }}
      >
        <Box display="grid" gridTemplateColumns="repeat(2, 1fr)" gap={3}>
          <TextField
            label="First Name"
            value={form.firstName}
            onChange={(e) => setForm({ ...form, firstName: e.target.value })}
            variant="outlined"
            fullWidth
            error={!!errors.firstName}
            helperText={errors.firstName}
          />
          <TextField
            label="Last Name"
            value={form.lastName}
            onChange={(e) => setForm({ ...form, lastName: e.target.value })}
            variant="outlined"
            fullWidth
            error={!!errors.lastName}
            helperText={errors.lastName}
          />
          <TextField
            label="Email Address"
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
            variant="outlined"
            fullWidth
            error={!!errors.email}
            helperText={errors.email}
          />

          <Autocomplete
            multiple
            options={filteredCompanies}
            disableCloseOnSelect
            getOptionLabel={(option) =>
              `${option.customerNumber} - ${option.customerName}`
            }
            isOptionEqualToValue={(option, value) => option.id === value.id}
            filterSelectedOptions
            onChange={(e, newValue) => {
              setForm({
                ...form,
                companyIds: newValue.map((company) => company.id),
              });
            }}
            filterOptions={(options, { inputValue }) =>
              options.filter((option) =>
                `${option.customerNumber} - ${option.customerName}`
                  .toLowerCase()
                  .includes(inputValue.toLowerCase())
              )
            }
            value={companies.filter((c) => form.companyIds.includes(c.id))}
            renderOption={(props, option, { selected }) => (
              <li {...props}>
                <Checkbox style={{ marginRight: 8 }} checked={selected} />
                {option.customerNumber} - {option.customerName}
              </li>
            )}
            renderInput={(params) => (
              <TextField
                {...params}
                label="Select Companies"
                placeholder="Search company by name or number"
                error={!!errors.companyIds}
                helperText={errors.companyIds}
              />
            )}
          />

          <Box gridColumn="span 2">
            <PhoneInput
              country={"il"}
              value={form.phone}
              onChange={(value) => setForm({ ...form, phone: value })}
              inputStyle={{ width: "100%" }}
            />
          </Box>

          <FormControl fullWidth disabled={!isSuperAdmin}>
            <InputLabel>User Type</InputLabel>
            <Select
              value={form.role}
              onChange={(e) => setForm({ ...form, role: e.target.value })}
            >
              <MenuItem value="Standard">Standard</MenuItem>
              <MenuItem value="Admin">Admin</MenuItem>
              <MenuItem value="Super Admin">Super Admin</MenuItem>
            </Select>
          </FormControl>

          <FormControl fullWidth>
            <InputLabel>Reminders</InputLabel>
            <Select
              multiple
              value={form.reminders}
              onChange={(e) =>
                setForm({ ...form, reminders: e.target.value as string[] })
              }
              renderValue={(selected) => selected.join(", ")}
            >
              {defaultReminders.map((day) => (
                <MenuItem key={day} value={day}>
                  <Checkbox checked={form.reminders.includes(day)} />
                  <ListItemText primary={day} />
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <TextField
            label="Password"
            value={form.password}
            onChange={(e) => setForm({ ...form, password: e.target.value })}
            variant="outlined"
            fullWidth
            type="password"
            error={!!errors.password}
            helperText={errors.password}
          />
        </Box>

        <Box display="flex" justifyContent="center" gap={2} mt={4}>
          <Button
            variant="contained"
            onClick={handleSubmit}
            sx={{
              bgcolor: "#1a237e",
              color: "white",
              fontWeight: "bold",
              textTransform: "none",
              borderRadius: "8px",
              px: 4,
            }}
          >
            Save
          </Button>
          <Button
            variant="contained"
            onClick={() => navigate("/manage-users")}
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
    </Box>
  );
}
