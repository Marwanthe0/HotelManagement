import { useState } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { Menu } from 'lucide-react';
import Sidebar from './Sidebar';
import { useAuth } from '../../context/AuthContext';
import './AppLayout.css';

export default function AppLayout() {
  const location = useLocation();
  const { user } = useAuth();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const routeNames = {
    '/': 'Dashboard Overview',
    '/rooms': 'Room Management',
    '/customers': 'Guest Directory',
    '/bookings': 'Reservation Registry',
    '/payments': 'Financial Ledger',
    '/employees': 'Staff Roster',
  };

  const currentSection = routeNames[location.pathname] || 'Administration';

  return (
    <div className="app-layout">
      <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />
      <div className="app-main-wrapper">
        <header className="app-topbar">
          <div className="app-topbar-left">
            <button
              className="nav-toggle-btn"
              onClick={() => setSidebarOpen((prev) => !prev)}
              title="Toggle Navigation Menu"
              aria-label="Toggle navigation menu"
            >
              <Menu size={20} />
            </button>
            <span className="app-topbar-hotel">The Haunted Hotel</span>
            <span>/</span>
            <span>{currentSection}</span>
          </div>
          <div className="app-topbar-right">
            <span className="system-live-pill">System Online</span>
            {user && (
              <span>Signed in as <strong style={{ color: 'var(--text-primary)' }}>{user.username}</strong></span>
            )}
          </div>
        </header>
        <main className="app-main">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
