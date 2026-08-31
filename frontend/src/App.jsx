import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import AppLayout from './components/layout/AppLayout';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import Rooms from './pages/Rooms';
import Customers from './pages/Customers';
import Bookings from './pages/Bookings';
import Payments from './pages/Payments';
import Employees from './pages/Employees';

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />

          {/* Protected routes */}
          <Route
            element={
              <ProtectedRoute>
                <AppLayout />
              </ProtectedRoute>
            }
          >
            <Route index element={<Dashboard />} />
            <Route path="rooms" element={<Rooms />} />
            <Route path="customers" element={<Customers />} />
            <Route path="bookings" element={<Bookings />} />
            <Route path="payments" element={<Payments />} />
            <Route path="employees" element={<Employees />} />
          </Route>

          {/* Fallback */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>

      <Toaster
        position="top-right"
        toastOptions={{
          duration: 3500,
          style: {
            background: '#1a1f2e',
            color: '#e8eaed',
            border: '1px solid rgba(255,255,255,0.06)',
            borderRadius: '10px',
            fontSize: '0.875rem',
          },
          success: {
            iconTheme: { primary: '#34d399', secondary: '#0b0e14' },
          },
          error: {
            iconTheme: { primary: '#ef4444', secondary: '#0b0e14' },
          },
        }}
      />
    </AuthProvider>
  );
}
