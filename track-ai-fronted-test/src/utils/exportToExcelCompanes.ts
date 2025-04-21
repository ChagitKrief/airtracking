import * as XLSX from "xlsx";

/**
 * Exports a given array of company objects to an Excel file.
 * @param data Array of company objects to export
 */
export const exportCompaniesToExcel = (data: {
  customerNumber: number;
  customerName: string;
  taxNumber: number;
  isActive: boolean;
}[]) => {
  const worksheet = XLSX.utils.json_to_sheet(
    data.map((company) => ({
      "Customer Number": company.customerNumber,
      "Customer Name": company.customerName,
      "Tax Number": company.taxNumber,
      "Status": company.isActive ? "Active" : "Inactive",
    }))
  );

  const workbook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, "Companies");

  XLSX.writeFile(workbook, "Companies.xlsx");
};
