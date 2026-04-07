import React, { createContext, useState, useContext, type ReactNode } from 'react';
import type { User } from '../types';
import authService from '../services/auth.service';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isAdmin: boolean;
  loading: boolean;
  login: (tenantId: number, email: string, password: string) => Promise<void>;
  register: (tenantId: number, name: string, email: string, phone: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(() => authService.getCurrentUser());
  const [loading] = useState(false);

  const login = async (tenantId: number, email: string, password: string) => {
    const response = await authService.login({ tenantId, email, password });
    const userData: User = {
      userId: response.userId,
      tenantId: response.tenantId,
      name: response.name,
      email: response.email,
      phone: '',
      role: response.role,
      isActive: true
    };
    setUser(userData);
  };

  const register = async (tenantId: number, name: string, email: string, phone: string, password: string) => {
    const response = await authService.register({ tenantId, name, email, phone, password });
    const userData: User = {
      userId: response.userId,
      tenantId: response.tenantId,
      name: response.name,
      email: response.email,
      phone,
      role: response.role,
      isActive: true
    };
    setUser(userData);
  };

  const logout = () => {
    authService.logout();
    setUser(null);
  };

  const value: AuthContextType = {
    user,
    isAuthenticated: !!user,
    isAdmin: user?.role === 'Admin',
    loading,
    login,
    register,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
