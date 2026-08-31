import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  DoorOpen,
  CalendarCheck,
  LogIn as LogInIcon,
  DollarSign,
  Plus,
  Users,
  CreditCard,
} from 'lucide-react';
import api from '../api/axios';
import StatusBadge from '../components/ui/StatusBadge';
import './Dashboard.css';

export default function Dashboard() {
  const navigate = useNavigate();
  const [stats, setStats] = useState({
    totalRooms: 0,
    activeBookings: 0,
    checkedInToday: 0,
    totalRevenue: 0,
  });
  const [recentBookings, setRecentBookings] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function load() {
      try {
        const [roomsRes, bookingsRes, paymentsRes] = await Promise.all([
          api.get('/rooms'),
          api.get('/bookings'),
          api.get('/payments'),
        ]);

        const rooms = roomsRes.data;
        const bookings = bookingsRes.data;
        const payments = paymentsRes.data;

        const activeBookings = bookings.filter(
          (b) => b.status === 'Pending' || b.status === 'Confirmed' || b.status === 'CheckedIn'
        );

        const today = new Date().toISOString().split('T')[0];
        const checkedInToday = bookings.filter(
          (b) => b.status === 'CheckedIn' && b.checkInDate?.split('T')[0] === today
        );

        const totalRevenue = payments
          .filter((p) => p.paymentStatus === 'Paid')
          .reduce((sum, p) => sum + p.amount, 0);

        setStats({
          totalRooms: rooms.length,
          activeBookings: activeBookings.length,
          checkedInToday: checkedInToday.length,
          totalRevenue,
        });

        // Sort bookings by date descending, take the 5 most recent
        const sorted = [...bookings].sort(
          (a, b) => new Date(b.bookingDate) - new Date(a.bookingDate)
        );
        setRecentBookings(sorted.slice(0, 5));
      } catch {
        // Silently fail if non-critical
      } finally {
        setLoading(false);
      }
    }
    load();
  }, []);

  function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
    }).format(amount);
  }

  function formatDate(dateStr) {
    if (!dateStr) return 'N/A';
    return new Date(dateStr).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
    });
  }

  if (loading) {
    return (
      <div className="page">
        <div className="loader"><div className="spinner" /></div>
      </div>
    );
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Dashboard</h1>
          <p className="page-subtitle">Administrative control center for The Haunted Hotel</p>
        </div>
      </div>

      <div className="dashboard-stats">
        <div className="stat-card">
          <div className="stat-icon stat-icon--rooms">
            <DoorOpen size={22} />
          </div>
          <div className="stat-content">
            <div className="stat-value">{stats.totalRooms}</div>
            <div className="stat-label">Total Rooms</div>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon stat-icon--bookings">
            <CalendarCheck size={22} />
          </div>
          <div className="stat-content">
            <div className="stat-value">{stats.activeBookings}</div>
            <div className="stat-label">Active Bookings</div>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon stat-icon--checkins">
            <LogInIcon size={22} />
          </div>
          <div className="stat-content">
            <div className="stat-value">{stats.checkedInToday}</div>
            <div className="stat-label">Checked In Today</div>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon stat-icon--revenue">
            <DollarSign size={22} />
          </div>
          <div className="stat-content">
            <div className="stat-value">{formatCurrency(stats.totalRevenue)}</div>
            <div className="stat-label">Total Revenue</div>
          </div>
        </div>
      </div>

      <div className="dashboard-grid">
        <div className="card">
          <div className="card-header">
            <h2 className="card-title">Recent Bookings</h2>
          </div>
          <div className="recent-list">
            {recentBookings.length === 0 ? (
              <p className="text-muted" style={{ padding: '12px 0', fontSize: 'var(--text-sm)' }}>
                No bookings yet.
              </p>
            ) : (
              recentBookings.map((b) => (
                <div className="recent-item" key={b.id}>
                  <div className="recent-item-info">
                    <span className="recent-item-primary">
                      Booking #{b.id} | Room {b.roomId}
                    </span>
                    <span className="recent-item-secondary">
                      {formatDate(b.checkInDate)} to {formatDate(b.checkOutDate)}
                    </span>
                  </div>
                  <StatusBadge status={b.status} />
                </div>
              ))
            )}
          </div>
        </div>

        <div className="card">
          <div className="card-header">
            <h2 className="card-title">Quick Actions</h2>
          </div>
          <div className="quick-actions">
            <button className="quick-action-btn" onClick={() => navigate('/bookings')}>
              <Plus size={16} /> New Booking
            </button>
            <button className="quick-action-btn" onClick={() => navigate('/rooms')}>
              <DoorOpen size={16} /> Manage Rooms
            </button>
            <button className="quick-action-btn" onClick={() => navigate('/customers')}>
              <Users size={16} /> Customers
            </button>
            <button className="quick-action-btn" onClick={() => navigate('/payments')}>
              <CreditCard size={16} /> Payments
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
