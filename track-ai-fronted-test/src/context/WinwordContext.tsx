import { createContext, useState, useContext } from "react";
import axiosInstance from "../api/axiosInstance";
import qs from "qs";
import { useLoading } from "./LoadingContext";

interface TrackedShipmentsResponse {
  data: {
    trackedShipments: {
      data: any[];
    };
  };
}

interface WinwordContextType {
  winwordData: TrackedShipmentsResponse | null;

  filterShipmentsByMultipleFields: (
    fields: string[],
    values: string[],
    customerCodes: string[]
  ) => Promise<TrackedShipmentsResponse | null>; // <- החזרנו את התוצאה

  resetWinwordData: () => void;
}

export const WinwordContext = createContext<WinwordContextType | null>(null);

export const WinwordProvider = ({ children }: { children: React.ReactNode }) => {
  const [winwordData, setWinwordData] = useState<TrackedShipmentsResponse | null>(null);
  const { showLoading, hideLoading } = useLoading();

  const filterShipmentsByMultipleFields = async (
    fields: string[],
    values: string[],
    customerCodes: string[]
  ): Promise<TrackedShipmentsResponse | null> => {
    try {
      showLoading();

      const response = await axiosInstance.get<TrackedShipmentsResponse>(
        "/winword/filter",
        {
          params: {
            fields,
            values,
            customerCodes,
          },
          paramsSerializer: (params) =>
            qs.stringify(params, { arrayFormat: "repeat" }),
        }
      );

      setWinwordData(response.data);  
      return response.data;                       
    } catch (error) {
      console.error("Failed to fetch filtered WIN data:", error);
      return null;
    } finally {
      hideLoading();
    }
  };

  const resetWinwordData = () => {
    setWinwordData(null);
  };

  return (
    <WinwordContext.Provider
      value={{
        winwordData,
        filterShipmentsByMultipleFields,
        resetWinwordData,
      }}
    >
      {children}
    </WinwordContext.Provider>
  );
};

export const useWinwordContext = () => {
  const context = useContext(WinwordContext);
  if (!context) {
    throw new Error("useWinwordContext must be used within a WinwordProvider");
  }
  return context;
};
