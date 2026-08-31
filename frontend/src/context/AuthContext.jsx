import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import api from '../api/axios';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const stored = localStorage.getItem('hotel_user');
    const token = localStorage.getItem('hotel_token');
    if (stored && token) {
      try {
        setUser(JSON.parse(stored));
      } catch {
        localStorage.removeItem('hotel_user');
        localStorage.removeItem('hotel_token');
      }
    }
    setLoading(false);
  }, []);

  const login = useCallback(async (email, password) => {
    const { data } = await api.post('/auth/login', { email, password });
    const userObj = {
      username: data.username,
      email: data.email,
      role: data.role,
      phoneNumber: data.phoneNumber || '',
      address: data.address || '',
      profilePictureUrl: data.profilePictureUrl || null,
      expiresAt: data.expiresAt,
    };
    localStorage.setItem('hotel_token', data.token);
    localStorage.setItem('hotel_user', JSON.stringify(userObj));
    setUser(userObj);
    return data;
  }, []);

  const register = useCallback(async (username, email, password, role, phoneNumber, address) => {
    const { data } = await api.post('/auth/register', {
      username,
      email,
      password,
      role: role || 'Staff',
      phoneNumber: phoneNumber || '',
      address: address || '',
    });
    const userObj = {
      username: data.username,
      email: data.email,
      role: data.role,
      phoneNumber: data.phoneNumber || '',
      address: data.address || '',
      profilePictureUrl: data.profilePictureUrl || null,
      expiresAt: data.expiresAt,
    };
    localStorage.setItem('hotel_token', data.token);
    localStorage.setItem('hotel_user', JSON.stringify(userObj));
    setUser(userObj);
    return data;
  }, []);

  const updateUser = useCallback((updatedFields) => {
    setUser((prev) => {
      const next = { ...prev, ...updatedFields };
      localStorage.setItem('hotel_user', JSON.stringify(next));
      return next;
    });
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('hotel_token');
    localStorage.removeItem('hotel_user');
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider value={{ user, loading, login, register, updateUser, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
