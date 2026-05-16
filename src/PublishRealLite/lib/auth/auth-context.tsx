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
  profile: (ArtistProfileDto & { isAdminProfile: boolean }) | null;
}

interface AuthContextType extends AuthState {
  login: (dto: LoginDto) => Promise<void>;
  register: (dto: RegisterDto) => Promise<void>;
  logout: () => void;
  refreshProfile: () => Promise<void>;
  setProfile: (profile: (ArtistProfileDto & { isAdminProfile: boolean }) | null) => void;
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
    if (!apiClient.isAuthenticated()) {
      setState((prev) => ({ ...prev, profile: null }));
      return;
    }

    try {
      const profiles = await apiClient.getArtistProfiles();

      if (profiles.length > 0) {
        const profile = profiles[0];

        // Ensure isAdminProfile is always a boolean
        // Default to true if not provided (for backward compatibility)
        const isAdminProfile = typeof profile.isAdminProfile === 'boolean' 
          ? profile.isAdminProfile 
          : true;

        // Debug logging (remove in production)
        if (typeof window !== 'undefined' && process.env.NODE_ENV === 'development') {
          console.log('[AuthContext] Profile loaded:', {
            id: profile.id,
            artistName: profile.artistName,
            isAdminProfile,
            hasAdminFlag: 'isAdminProfile' in profile
          });
        }

        setState((prev) => ({
          ...prev,
          profile: {
            ...profile,
            isAdminProfile,
          },
        }));
      } else {
        // No profile found
        setState((prev) => ({ ...prev, profile: null }));

        if (typeof window !== 'undefined' && process.env.NODE_ENV === 'development') {
          console.log('[AuthContext] No profile found for current user');
        }
      }
    } catch (error) {
      // Profile may not exist yet, or API error
      setState((prev) => ({ ...prev, profile: null }));

      if (typeof window !== 'undefined' && process.env.NODE_ENV === 'development') {
        console.warn('[AuthContext] Error loading profile:', error);
      }
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
        await refreshProfile();
      } else {
        setState((prev) => ({ ...prev, isLoading: false }));
      }
    };

    checkAuth();
  }, [refreshProfile]);

  const login = async (dto: LoginDto) => {
    try {
      const response = await apiClient.login(dto);
      setState((prev) => ({
        ...prev,
        isAuthenticated: true,
        email: response.email,
      }));
      await refreshProfile();
    } catch (error) {
      setState((prev) => ({
        ...prev,
        isAuthenticated: false,
        profile: null,
      }));
      throw error;
    }
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

  const setProfile = (profile: (ArtistProfileDto & { isAdminProfile: boolean }) | null) => {
    setState((prev) => ({
      ...prev,
      profile: profile ? {
        ...profile,
        isAdminProfile: typeof profile.isAdminProfile === 'boolean' 
          ? profile.isAdminProfile 
          : true
      } : null,
    }));
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
