import { useState } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import {
  LayoutDashboard,
  DoorOpen,
  Users,
  CalendarCheck,
  CreditCard,
  UserCog,
  LogOut,
} from 'lucide-react';
import ProfileModal from '../profile/ProfileModal';
import './Sidebar.css';

const navItems = [
  { to: '/',          icon: LayoutDashboard, label: 'Dashboard',  section: 'Overview' },
  { to: '/rooms',     icon: DoorOpen,        label: 'Rooms',      section: 'Management' },
  { to: '/customers', icon: Users,           label: 'Customers',  section: 'Management' },
  { to: '/bookings',  icon: CalendarCheck,   label: 'Bookings',   section: 'Management' },
  { to: '/payments',  icon: CreditCard,      label: 'Payments',   section: 'Finance' },
  { to: '/employees', icon: UserCog,         label: 'Employees',  section: 'Finance' },
];

export default function Sidebar({ open, onClose }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [profileModalOpen, setProfileModalOpen] = useState(false);

  function handleLogout(e) {
    e.stopPropagation();
    logout();
    navigate('/login');
  }

  const initial = user?.username?.[0]?.toUpperCase() || '?';

  // Group nav items by section
  const sections = [];
  let lastSection = null;
  for (const item of navItems) {
    if (item.section !== lastSection) {
      sections.push({ label: item.section, items: [] });
      lastSection = item.section;
    }
    sections[sections.length - 1].items.push(item);
  }

  return (
    <>
      <div
        className={`sidebar-backdrop${open ? ' visible' : ''}`}
        onClick={onClose}
      />

      <aside className={`sidebar${open ? ' open' : ''}`}>
        <div className="sidebar-brand">
          <img src="/Logo.png" alt="The Haunted Hotel" className="sidebar-logo" />
          <span className="sidebar-brand-text">Management System</span>
        </div>

        <nav className="sidebar-nav">
          {sections.map((section) => (
            <div key={section.label}>
              <div className="sidebar-section-label">{section.label}</div>
              {section.items.map((item) => (
                <NavLink
                  key={item.to}
                  to={item.to}
                  end={item.to === '/'}
                  className={({ isActive }) =>
                    `sidebar-link${isActive ? ' active' : ''}`
                  }
                  onClick={onClose}
                >
                  <item.icon size={18} />
                  {item.label}
                </NavLink>
              ))}
            </div>
          ))}
        </nav>

        <div className="sidebar-footer">
          <div
            className="sidebar-user clickable"
            onClick={() => setProfileModalOpen(true)}
            title="View & Edit Profile"
          >
            {user?.profilePictureUrl ? (
              <img src={user.profilePictureUrl} alt="Avatar" className="sidebar-avatar-img" />
            ) : (
              <div className="sidebar-avatar">{initial}</div>
            )}
            <div className="sidebar-user-info">
              <div className="sidebar-user-name">{user?.username}</div>
              <div className="sidebar-user-role">{user?.role}</div>
            </div>
            <button
              className="sidebar-logout"
              onClick={handleLogout}
              title="Sign out"
            >
              <LogOut size={16} />
            </button>
          </div>
        </div>
      </aside>

      <ProfileModal
        open={profileModalOpen}
        onClose={() => setProfileModalOpen(false)}
      />
    </>
  );
}
