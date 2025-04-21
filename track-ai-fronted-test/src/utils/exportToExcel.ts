import * as XLSX from "xlsx";

export function exportToExcel(columns: any[], rows: any[], fileName: string) {
  if (!rows || rows.length === 0) {
    console.warn("NO DATA");
    return;
  }

  const dataForExport = rows.map((row) => {
    const rowData: { [key: string]: any } = {};

    columns.forEach((col) => {
      const columnName = col.headerName || col.field;
      let cellValue = row[col.field];

      if (col.field === "pol" && cellValue && typeof cellValue === "object") {
        cellValue = row.polLocode && row.polName
          ? `${row.polLocode} - ${row.polName}`
          : "N/A";
      }

      else if (col.field === "pod" && cellValue && typeof cellValue === "object") {
        cellValue = row.podLocode && row.podName
          ? `${row.podLocode} - ${row.podName}`
          : "N/A";
      }

      else if (col.field === "path" && cellValue && typeof cellValue === "object") {
        cellValue = `${row.polLocode || "N/A"} → ${row.podLocode || "N/A"} (${Math.round(row.path.progress || 0)}%)`;
      }

      else if (col.field === "latestCarrierETAOrATA" && cellValue) {
        cellValue = new Date(cellValue).toLocaleString();
      }

      else if (col.field === "events" && Array.isArray(cellValue)) {
        cellValue = cellValue.map((event: any) => event.description || "Unknown").join(", ");
      }

      else if (typeof cellValue === "object" && cellValue !== null) {
        cellValue = Object.values(cellValue)[0] || "N/A";
      }

      rowData[columnName] = cellValue || "N/A";
    });

    return rowData;
  });

  const worksheet = XLSX.utils.json_to_sheet(dataForExport);
  const workbook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, "Shipments Data");
  XLSX.writeFile(workbook, fileName);
}
