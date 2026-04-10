"use client";

import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from "react";
import { apiClient } from "@/lib/api/client";
import type { LoginDto, RegisterDto, ArtistProfileDto } from "@/lib/api/types";

interface AuthState {
  isAuthenticated: boolean;
  isLoading: boolean;
  email: string | null;
  profile: ArtistProfileDto | null;
}

interface AuthContextType extends AuthState {
  login: (dto: LoginDto) => Promise<void>;
  register: (dto: RegisterDto) => Promise<void>;
  logout: () => void;
  refreshProfile: () => Promise<void>;
  setProfile: (profile: ArtistProfileDto | null) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({
    isAuthenticated: false,
    isLoading: true,
    email: null,
    profile: null,
  });

  const refreshProfile = useCallback(async () => {
    if (!apiClient.isAuthenticated()) return;

    try {
      const profiles = await apiClient.getArtistProfiles();
      // Get current user's profile (the API returns only the user's profile when authenticated)
      if (profiles.length > 0) {
        setState((prev) => ({ ...prev, profile: profiles[0] }));
      }
    } catch {
      // Profile may not exist yet
      setState((prev) => ({ ...prev, profile: null }));
    }
  }, []);

  useEffect(() => {
    // Check authentication state on mount
    const checkAuth = async () => {
      const token =
        typeof window !== "undefined"
          ? localStorage.getItem("publishreal_token")
          : null;
      const email =
        typeof window !== "undefined"
          ? localStorage.getItem("publishreal_email")
          : null;

      if (token && email) {
        setState((prev) => ({
          ...prev,
          isAuthenticated: true,
          email,
          isLoading: false,
        }));
        // Fetch profile in background
        refreshProfile();
      } else {
        setState((prev) => ({ ...prev, isLoading: false }));
      }
    };

    checkAuth();
  }, [refreshProfile]);

  const login = async (dto: LoginDto) => {
    const response = await apiClient.login(dto);
    setState((prev) => ({
      ...prev,
      isAuthenticated: true,
      email: response.email,
    }));
    await refreshProfile();
  };

  const register = async (dto: RegisterDto) => {
    await apiClient.register(dto);
    // Auto-login after registration
    await login(dto);
  };

  const logout = () => {
    apiClient.logout();
    setState({
      isAuthenticated: false,
      isLoading: false,
      email: null,
      profile: null,
    });
  };

  const setProfile = (profile: ArtistProfileDto | null) => {
    setState((prev) => ({ ...prev, profile }));
  };

  return (
    <AuthContext.Provider
      value={{
        ...state,
        login,
        register,
        logout,
        refreshProfile,
        setProfile,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
