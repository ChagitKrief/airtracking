export interface CustomerDto {
    id: string;
    customerNumber: number;
    customerName: string;
}

export interface User {
    id: string;
    email: string;
    role: number;
    firstName: string;
    lastName: string;
    expiration: string;
    phone: string;
    reminders: string[];
    activeCustomers: CustomerDto[];
    winwordData?: any[];
}

export interface AuthResponse {
    token: string;
    refreshToken: string;
    email: string;
    role: number;
    expiration: string;
    id: string;
    firstName: string;
    lastName: string;
    phone: string;
    reminders: string[];
    activeCustomers: CustomerDto[];
    winwordData?: {} | null;
}
