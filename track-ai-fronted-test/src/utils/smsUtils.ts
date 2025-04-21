// utils/phoneUtils.ts
export function normalizePhone(phone: string): string {
    const digitsOnly = phone.replace(/\D/g, "");
    if (digitsOnly.length > 10) {
        return digitsOnly.slice(digitsOnly.length - 9);
    }

    return digitsOnly;
}


export function hasDuplicatePhones(phones: string[]): boolean {
    const normalized = phones.map(normalizePhone).filter(p => p.trim() !== "");
    return new Set(normalized).size !== normalized.length;
}