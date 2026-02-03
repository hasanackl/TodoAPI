import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';
import type { User } from '../types';

const TOKEN_KEY = '@fitness_token';
const USER_KEY = '@fitness_user';

interface AuthContextType {
  user: User | null;
  token: string | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (token: string, user: User) => Promise<void>;
  logout: () => Promise<void>;
  setUser: (user: User) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUserState] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const loadStoredAuth = useCallback(async () => {
    try {
      const [storedToken, storedUser] = await Promise.all([
        AsyncStorage.getItem(TOKEN_KEY),
        AsyncStorage.getItem(USER_KEY),
      ]);
      if (storedToken && storedUser) {
        setToken(storedToken);
        setUserState(JSON.parse(storedUser));
      }
    } catch (e) {
      console.warn('Auth load error', e);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadStoredAuth();
  }, [loadStoredAuth]);

  const login = useCallback(async (newToken: string, newUser: User) => {
    await AsyncStorage.multiSet([
      [TOKEN_KEY, newToken],
      [USER_KEY, JSON.stringify(newUser)],
    ]);
    setToken(newToken);
    setUserState(newUser);
  }, []);

  const logout = useCallback(async () => {
    await AsyncStorage.multiRemove([TOKEN_KEY, USER_KEY]);
    setToken(null);
    setUserState(null);
  }, []);

  const setUser = useCallback((updatedUser: User) => {
    setUserState(updatedUser);
    AsyncStorage.setItem(USER_KEY, JSON.stringify(updatedUser));
  }, []);

  const value: AuthContextType = {
    user,
    token,
    isLoading,
    isAuthenticated: !!token,
    login,
    logout,
    setUser,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
