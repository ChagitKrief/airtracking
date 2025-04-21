// ManageUsers.tsx

import { useContext, useEffect, useRef, useState } from "react";
import {
  Box,
  Typography,
  Card,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  TableContainer,
  Paper,
  TextField,
  Button,
  Select,
  MenuItem,
  InputLabel,
  FormControl,
  Checkbox,
  Autocomplete,
  ListItemText,
  Backdrop,
} from "@mui/material";
import { AddCircleOutline } from "@mui/icons-material";
import DownloadIcon from "@mui/icons-material/Download";
import { useNavigate } from "react-router-dom";
import { useUserContext } from "../context/UserContext";
import { exportUsersToExcel } from "../utils/exportToExcelUsers";
import { validateEmail } from "../utils/validateEmail";
import PhoneInput from "react-phone-input-2";
import { AuthContext } from "../context/AuthContext";

export default function ManageUsers() {
  const navigate = useNavigate();
  const { users, fetchUsers, updateUser, companies } = useUserContext();
  const didFetch = useRef(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedUser, setSelectedUser] = useState<any | null>(null);
  const auth = useContext(AuthContext);
  const isSuperAdmin = auth?.user?.role === 1;
  const [isLoading, setIsLoading] = useState(true);

  const defaultReminders = [
    "Sunday",
    "Monday",
    "Tuesday",
    "Wednesday",
    "Thursday",
    "Friday",
    "Saturday",
  ];

  useEffect(() => {
    const loadData = async () => {
      if (!didFetch.current) {
        setIsLoading(true);
        await fetchUsers();
        setIsLoading(false);
        didFetch.current = true;
      } else {
        setIsLoading(false);
      }
    };
    loadData();
  }, [fetchUsers]);

  const [errors, setErrors] = useState<{
    firstName?: string;
    lastName?: string;
    email?: string;
    password?: string;
    companyIds?: string;
  }>({});
  const handleFieldChange = (e: any, field: string) => {
    const { value } = e.target;
    setSelectedUser((prev: any) => ({ ...prev, [field]: value }));
  };

  const handleRowClick = (user: any) => {
    if (selectedUser?.id === user.id) {
      setSelectedUser(null);
    } else {
      const userCompanyNames = user.company
        ? user.company.split(",").map((c: string) => c.trim())
        : [];

      const matchedCompanies = companies.filter((c) =>
        userCompanyNames.includes(c.customerName)
      );

      const companyIds = matchedCompanies.map((c) => c.id);

      setSelectedUser({
        ...user,
        companyIds,
        reminders: Array.isArray(user.reminders)
          ? [...user.reminders]
          : defaultReminders.filter(() => false),
      });
    }
  };

  const validateAndSave = async () => {
    if (!selectedUser) return;

    const newErrors: typeof errors = {};
    if (!selectedUser.firstName?.trim())
      newErrors.firstName = "First name is required.";
    if (!selectedUser.lastName?.trim())
      newErrors.lastName = "Last name is required.";
    if (!validateEmail(selectedUser.email))
      newErrors.email = "Invalid email format.";
    if (selectedUser.password && selectedUser.password.length < 8) {
      newErrors.password = "Password must be at least 8 characters.";
    }
    if (!selectedUser.companyIds || selectedUser.companyIds.length === 0)
      newErrors.companyIds = "At least one company must be selected.";

    setErrors(newErrors);
    if (Object.keys(newErrors).length > 0) return;
    const updatedUser = {
      id: selectedUser.id,
      firstName: selectedUser.firstName,
      lastName: selectedUser.lastName,
      email: selectedUser.email,
      Phone: selectedUser.phoneNumber,
      password: selectedUser.password || undefined,
      role:
        selectedUser.userType == "Super Admin"
          ? 1
          : selectedUser.userType == "Standard"
          ? 2
          : 3,
      IsActive: selectedUser.status,
      reminders: selectedUser.reminders || [],
      companyIds: selectedUser.companyIds || [],
    };

    try {
      await updateUser(selectedUser.id, updatedUser);
      setSelectedUser(null);
    } catch (error) {
      console.error("Failed to update user", error);
    }
  };

  const filteredUsers = users.filter((user) =>
    (
      user.firstName +
      user.lastName +
      user.email +
      user.company +
      user.phoneNumber +
      (user.status ? "active" : "inactive")
    )
      .toLowerCase()
      .includes(searchTerm.toLowerCase())
  );

  return (
    <Box
      sx={{
        p: { xs: 2, md: 3 },
        maxWidth: "1400px",
        mx: "auto",
      }}
    >
      <Typography
        variant="h4"
        textAlign="center"
        fontWeight="bold"
        color="#1a237e"
        mb={3}
      >
        User Details
      </Typography>
      {/* <Backdrop
        open={isLoading}
        sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}
      >
        <div className="loading-image"></div>
      </Backdrop> */}

      <Card sx={{ p: 2, mb: 3, borderRadius: 2, boxShadow: 2 }}>
        <Box
          display="flex"
          flexDirection={{ xs: "column", sm: "row" }}
          justifyContent="space-between"
          alignItems={{ xs: "stretch", sm: "center" }}
          gap={2}
          mb={2}
        >
          <TextField
            size="small"
            margin="dense"
            label="Search..."
            variant="outlined"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            sx={{ flex: 1, mr: 2 }}
            fullWidth
          />
          <Box>
            <Button
              fullWidth
              startIcon={<AddCircleOutline />}
              sx={{ textTransform: "none", fontWeight: "bold", mr: 2 }}
              onClick={() => navigate("/add-user")}
            >
              Add New User
            </Button>
            {/* <Button startIcon={<CloudUpload />} sx={{ mr: 2 }}>
              Upload User
            </Button> */}
            <Button
              fullWidth
              startIcon={<DownloadIcon />}
              onClick={() => exportUsersToExcel(filteredUsers)}
              sx={{
                textTransform: "none",
                fontWeight: "bold",
                color: "#1a237e",
              }}
            >
              Download Sample File
            </Button>
          </Box>
        </Box>

        <TableContainer
          component={Paper}
          sx={{
            maxHeight: selectedUser ? 400 : "calc(100vh - 200px)",
            overflowX: "auto",
            overflowY: "auto",
          }}
        >
          <Table stickyHeader>
            <TableHead>
              <TableRow>
                <TableCell>First Name</TableCell>
                <TableCell>Last Name</TableCell>
                <TableCell>Email</TableCell>
                <TableCell>Companies</TableCell>
                <TableCell>User Type</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Phone Number</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredUsers.map((user: any) => (
                <TableRow
                  key={user.id}
                  onClick={() => handleRowClick(user)}
                  sx={{
                    cursor: "pointer",
                    backgroundColor:
                      selectedUser?.id === user.id ? "#e3f2fd" : "inherit",
                    "&:hover": {
                      backgroundColor: "#e3f2fd80",
                    },
                  }}
                >
                  <TableCell>{user.firstName}</TableCell>
                  <TableCell>{user.lastName}</TableCell>
                  <TableCell>{user.email}</TableCell>
                  <TableCell>{user.company}</TableCell>
                  <TableCell>{user.userType}</TableCell>
                  <TableCell>{user.status ? "Active" : "Inactive"}</TableCell>
                  <TableCell>{user.phoneNumber}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Card>

      {selectedUser && (
        <Card
          sx={{
            px: { xs: 2, sm: 3, md: 4 },
            py: { xs: 2, sm: 3 },
            borderRadius: "12px",
            boxShadow: 2,
            border: "1px solid #1a237e",
            mt: 3,
            maxWidth: "100%",
            overflow: "visible",
          }}
        >
          <Typography
            variant="h5"
            sx={{
              fontWeight: "bold",
              color: "#1a237e",
              textAlign: "center",
              mb: 3,
            }}
          >
            Edit User
          </Typography>

          <Box
            display="grid"
            gridTemplateColumns={{
              xs: "1fr",
              sm: "1fr", 
              md: "repeat(2, 1fr)",
            }}
            gap={2}
            sx={{
              width: "100%",
              overflowX: "hidden",
            }}
          >
            <TextField
              size="small"
              margin="dense"
              fullWidth
              label="First Name"
              name="firstName"
              value={selectedUser.firstName}
              onChange={(e) => handleFieldChange(e, "firstName")}
              error={!!errors.firstName}
              helperText={errors.firstName}
            />
            <TextField
              fullWidth
              size="small"
              margin="dense"
              label="Last Name"
              name="lastName"
              value={selectedUser.lastName}
              onChange={(e) => handleFieldChange(e, "lastName")}
              error={!!errors.lastName}
              helperText={errors.lastName}
            />
            <TextField
              fullWidth
              size="small"
              margin="dense"
              label="Email Address"
              name="email"
              value={selectedUser.email}
              onChange={(e) => handleFieldChange(e, "email")}
              error={!!errors.email}
              helperText={errors.email}
            />
            <FormControl fullWidth>
              <InputLabel>Status</InputLabel>
              <Select
                fullWidth
                name="status"
                value={selectedUser.status}
                onChange={(e) =>
                  setSelectedUser((prev: any) => ({
                    ...prev,
                    status: e.target.value === "true",
                  }))
                }
              >
                <MenuItem value="true">Active</MenuItem>
                <MenuItem value="false">Inactive</MenuItem>
              </Select>
            </FormControl>

            <Autocomplete
              size="small"
              fullWidth
              multiple
              options={companies}
              disableCloseOnSelect
              getOptionLabel={(option) =>
                `${option.customerNumber} - ${option.customerName}`
              }
              isOptionEqualToValue={(option, value) => option.id === value.id}
              value={companies.filter((c) =>
                selectedUser.companyIds?.includes(c.id)
              )}
              onChange={(e, newValue) => {
                setSelectedUser((prev: any) => ({
                  ...prev,
                  companyIds: newValue.map((c) => c.id),
                }));
              }}
              renderOption={(props, option, { selected }) => (
                <li {...props}>
                  <Checkbox style={{ marginRight: 8 }} checked={selected} />
                  {option.customerNumber} - {option.customerName}
                </li>
              )}
              renderInput={(params) => (
                <TextField
                  margin="dense"
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
                value={selectedUser.phoneNumber}
                onChange={(value) =>
                  setSelectedUser((prev: any) => ({
                    ...prev,
                    phoneNumber: value,
                  }))
                }
                inputStyle={{
                  width: "100%",
                  fontSize: "16px",
                  paddingLeft: "48px",
                  height: "40px",
                }}
                buttonStyle={{
                  borderTopLeftRadius: 4,
                  borderBottomLeftRadius: 4,
                }}
                containerStyle={{
                  width: "100%",
                }}
              />
            </Box>

            <FormControl fullWidth disabled={!isSuperAdmin}>
              <InputLabel>User Type</InputLabel>
              <Select
                size="small"
                margin="dense"
                fullWidth
                name="userType"
                value={selectedUser.userType}
                onChange={(e) => handleFieldChange(e, "userType")}
              >
                <MenuItem value="Standard">Standard</MenuItem>
                <MenuItem value="Admin">Admin</MenuItem>
                <MenuItem value="Super Admin">Super Admin</MenuItem>
              </Select>
            </FormControl>

            <FormControl fullWidth>
              <InputLabel id="reminders-label">Reminders</InputLabel>
              <Select
                size="small"
                margin="dense"
                fullWidth
                key={selectedUser.id}
                labelId="reminders-label"
                multiple
                value={selectedUser.reminders || []}
                onChange={(e) =>
                  setSelectedUser((prev: any) => ({
                    ...prev,
                    reminders: e.target.value,
                  }))
                }
                renderValue={(selected) => (selected as string[]).join(", ")}
              >
                {defaultReminders.map((day) => (
                  <MenuItem key={day} value={day}>
                    <Checkbox
                      checked={
                        Array.isArray(selectedUser.reminders) &&
                        selectedUser.reminders.includes(day)
                      }
                    />
                    <ListItemText primary={day} />
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            <TextField
              fullWidth
              size="small"
              margin="dense"
              label="Password"
              name="password"
              value={selectedUser.password || ""}
              onChange={(e) => handleFieldChange(e, "password")}
              type="password"
              error={!!errors.password}
              helperText={errors.password}
            />
          </Box>

          <Box
            display="flex"
            flexDirection={{ xs: "column", sm: "row" }}
            justifyContent="center"
            alignItems="center"
            gap={2}
            mt={4}
          >
            <Button
              fullWidth={true}
              variant="contained"
              onClick={validateAndSave}
              sx={{
                bgcolor: "#1a237e",
                color: "white",
                fontWeight: "bold",
                textTransform: "none",
                borderRadius: "8px",
                px: 4,
                width: { xs: "100%", sm: "auto" },
                minWidth: "120px",
              }}
            >
              Update
            </Button>
            <Button
              fullWidth={true}
              variant="contained"
              onClick={() => setSelectedUser(null)}
              sx={{
                bgcolor: "#1a237e",
                color: "white",
                fontWeight: "bold",
                textTransform: "none",
                borderRadius: "8px",
                px: 4,
                width: { xs: "100%", sm: "auto" },
                minWidth: "120px",
              }}
            >
              Cancel
            </Button>
          </Box>
        </Card>
      )}
    </Box>
  );
}
