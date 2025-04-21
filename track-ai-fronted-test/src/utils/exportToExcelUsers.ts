// utils/exportToExcelUsers.ts

import * as XLSX from "xlsx";

interface UserRow {
  firstName: string;
  lastName: string;
  email: string;
  company: string;
  userType: string;
  status: string;
  phoneNumber: string;
}

export const exportUsersToExcel = (users: any[]) => {
  const data: UserRow[] = users.map((user) => ({
    firstName: user.firstName,
    lastName: user.lastName,
    email: user.email,
    company: user.company,
    userType: user.userType,
    status: user.status ? "Active" : "Inactive",
    phoneNumber: user.phoneNumber,
  }));

  const worksheet = XLSX.utils.json_to_sheet(data);
  const workbook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, "Users");

  XLSX.writeFile(workbook, "UserList.xlsx");
};
