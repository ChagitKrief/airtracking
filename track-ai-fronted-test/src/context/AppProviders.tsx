import { AuthProvider } from "./AuthContext";
import { CompanyProvider } from "./CompanyContext";
import { LoadingProvider } from "./LoadingContext";
import { SmsProvider } from "./SmsContext";
import { UserProvider } from "./UserContext";
import { WinwordProvider } from "./WinwordContext";

// AppProviders.tsx
export default function AppProviders({ children }: { children: React.ReactNode }) {
    return (
      <LoadingProvider>
        <AuthProvider>
          <CompanyProvider>
            <UserProvider>
              <SmsProvider>
                <WinwordProvider>
                  {children}
                </WinwordProvider>
              </SmsProvider>
            </UserProvider>
          </CompanyProvider>
        </AuthProvider>
      </LoadingProvider>
    );
  }
  