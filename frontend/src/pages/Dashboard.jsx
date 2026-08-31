import { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  DoorOpen,
  CalendarCheck,
  Key,
  Users,
  DollarSign,
  CheckCircle,
  Plus,
  CreditCard,
  TrendingUp,
  PieChart,
  BarChart3,
  AlertCircle,
  RefreshCw,
  ArrowRight,
  ShieldCheck,
  Clock,
  LogOut,
  XCircle,
} from 'lucide-react';
import api from '../api/axios';
import StatusBadge from '../components/ui/StatusBadge';
import './Dashboard.css';

export default function Dashboard() {
  const navigate = useNavigate();
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [activeTooltip, setActiveTooltip] = useState(null);

  const fetchDashboard = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.get('/dashboard');
      setData(res.data);
    } catch (err) {
      console.error('Failed to load dashboard data:', err);
      setError('Unable to load dashboard analytics. Please verify your connection.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchDashboard();
  }, [fetchDashboard]);

  function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount || 0);
  }

  function formatDate(dateStr) {
    if (!dateStr) return 'N/A';
    return new Date(dateStr).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }

  if (loading) {
    return (
      <div className="page">
        <div className="page-header">
          <div>
            <h1 className="page-title">Dashboard</h1>
            <p className="page-subtitle">Administrative control center for The Haunted Hotel</p>
          </div>
        </div>
        <div className="dashboard-loading-container">
          <div className="spinner" />
          <p className="dashboard-loading-text">Loading hotel analytics...</p>
        </div>
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="page">
        <div className="page-header">
          <div>
            <h1 className="page-title">Dashboard</h1>
            <p className="page-subtitle">Administrative control center for The Haunted Hotel</p>
          </div>
        </div>
        <div className="dashboard-error-card">
          <AlertCircle size={36} className="dashboard-error-icon" />
          <h3>Analytics Unavailable</h3>
          <p>{error || 'An unexpected error occurred while loading analytics.'}</p>
          <button className="btn btn-primary" onClick={fetchDashboard}>
            <RefreshCw size={16} /> Retry Analytics
          </button>
        </div>
      </div>
    );
  }

  // Safe property extraction supporting both camelCase and PascalCase
  const totalRooms = data.totalRooms ?? data.TotalRooms ?? 0;
  const availableRooms = data.availableRooms ?? data.AvailableRooms ?? 0;
  const occupiedRooms = data.occupiedRooms ?? data.OccupiedRooms ?? 0;
  const reservedRooms = data.reservedRooms ?? data.ReservedRooms ?? 0;
  const activeBookings = data.activeBookings ?? data.ActiveBookings ?? 0;
  const totalCustomers = data.totalCustomers ?? data.TotalCustomers ?? 0;
  const totalRevenue = data.totalRevenue ?? data.TotalRevenue ?? 0;
  const outstandingAmount = data.outstandingAmount ?? data.OutstandingAmount ?? 0;

  const rawBookingStatus = data.bookingStatus ?? data.BookingStatus ?? {};
  const pendingCount = rawBookingStatus.pending ?? rawBookingStatus.Pending ?? 0;
  const confirmedCount = rawBookingStatus.confirmed ?? rawBookingStatus.Confirmed ?? 0;
  const checkedInCount = rawBookingStatus.checkedIn ?? rawBookingStatus.CheckedIn ?? 0;
  const checkedOutCount = rawBookingStatus.checkedOut ?? rawBookingStatus.CheckedOut ?? 0;
  const cancelledCount = rawBookingStatus.cancelled ?? rawBookingStatus.Cancelled ?? 0;

  const monthlyRevenue = data.monthlyRevenue ?? data.MonthlyRevenue ?? [];
  const monthlyOccupancy = data.monthlyOccupancy ?? data.MonthlyOccupancy ?? [];
  const roomCategoryOccupancy = data.roomCategoryOccupancy ?? data.RoomCategoryOccupancy ?? [];
  const recentBookings = data.recentBookings ?? data.RecentBookings ?? [];
  const recentPayments = data.recentPayments ?? data.RecentPayments ?? [];

  const rawPaymentOverview = data.paymentOverview ?? data.PaymentOverview ?? {};
  const totalPaid = rawPaymentOverview.totalPaid ?? rawPaymentOverview.TotalPaid ?? totalRevenue;
  const totalOutstanding = rawPaymentOverview.totalOutstanding ?? rawPaymentOverview.TotalOutstanding ?? outstandingAmount;
  const fullyPaidBookings = rawPaymentOverview.fullyPaidBookings ?? rawPaymentOverview.FullyPaidBookings ?? 0;
  const partiallyPaidBookings = rawPaymentOverview.partiallyPaidBookings ?? rawPaymentOverview.PartiallyPaidBookings ?? 0;
  const unpaidBookings = rawPaymentOverview.unpaidBookings ?? rawPaymentOverview.UnpaidBookings ?? 0;

  const currentOccupancyRate = totalRooms > 0
    ? Math.round((occupiedRooms / totalRooms) * 100)
    : 0;

  // Compute max values for charts scaling
  const maxRevenue = Math.max(
    ...(monthlyRevenue.map((m) => (m.amount ?? m.Amount ?? 0)) || [0]),
    1000
  );
  const totalStatusCount =
    pendingCount + confirmedCount + checkedInCount + checkedOutCount + cancelledCount;

  return (
    <div className="page dashboard-page">
      {/* Page Header */}
      <div className="page-header">
        <div>
          <h1 className="page-title">Dashboard</h1>
          <p className="page-subtitle">Administrative control center for The Haunted Hotel</p>
        </div>
        <div className="page-actions">
          <button className="btn btn-secondary btn-sm" onClick={fetchDashboard} title="Refresh Data">
            <RefreshCw size={14} /> Refresh
          </button>
          <button className="btn btn-primary btn-sm" onClick={() => navigate('/bookings')}>
            <Plus size={14} /> New Booking
          </button>
        </div>
      </div>

      {/* SECTION 1: Summary Cards Grid (6 metrics) */}
      <div className="dashboard-stats-grid">
        <div className="stat-card">
          <div className="stat-icon stat-icon--rooms">
            <DoorOpen size={20} />
          </div>
          <div className="stat-content">
            <div className="stat-value">{totalRooms}</div>
            <div className="stat-label">Total Rooms</div>
            <div className="stat-subtext">{roomCategoryOccupancy.length} Room Categories</div>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon stat-icon--available">
            <CheckCircle size={20} />
          </div>
          <div className="stat-content">
            <div className="stat-value">{availableRooms}</div>
            <div className="stat-label">Available Rooms</div>
            <div className="stat-subtext">Ready for immediate check-in</div>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon stat-icon--occupied">
            <Key size={20} />
          </div>
          <div className="stat-content">
            <div className="stat-value">{occupiedRooms}</div>
            <div className="stat-label">Occupied Rooms</div>
            <div className="stat-subtext">{currentOccupancyRate}% current occupancy</div>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon stat-icon--bookings">
            <CalendarCheck size={20} />
          </div>
          <div className="stat-content">
            <div className="stat-value">{activeBookings}</div>
            <div className="stat-label">Active Bookings</div>
            <div className="stat-subtext">Pending, Confirmed & Checked In</div>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon stat-icon--customers">
            <Users size={20} />
          </div>
          <div className="stat-content">
            <div className="stat-value">{totalCustomers}</div>
            <div className="stat-label">Total Guests</div>
            <div className="stat-subtext">Registered guest profiles</div>
          </div>
        </div>

        <div className="stat-card stat-card--highlight">
          <div className="stat-icon stat-icon--revenue">
            <DollarSign size={20} />
          </div>
          <div className="stat-content">
            <div className="stat-value">{formatCurrency(totalRevenue)}</div>
            <div className="stat-label">Total Revenue</div>
            <div className="stat-subtext">Recognized paid transactions</div>
          </div>
        </div>
      </div>

      {/* SECTION 2 & 3: Occupancy Overview & Booking Status */}
      <div className="dashboard-grid-2">
        {/* Occupancy Overview Card */}
        <div className="card analytics-card">
          <div className="card-header">
            <div className="card-title-group">
              <TrendingUp size={18} className="card-header-icon" />
              <h2 className="card-title">Occupancy Overview</h2>
            </div>
            <span className="analytics-badge">{currentOccupancyRate}% Occupied</span>
          </div>

          <div className="occupancy-overview-body">
            {/* SVG Donut Chart */}
            <div className="donut-chart-container">
              <svg viewBox="0 0 160 160" className="donut-svg">
                {/* Background Ring */}
                <circle
                  cx="80"
                  cy="80"
                  r="60"
                  className="donut-bg-ring"
                />

                {totalRooms > 0 ? (
                  <>
                    {/* Available Segment */}
                    <circle
                      cx="80"
                      cy="80"
                      r="60"
                      className="donut-segment segment-available"
                      strokeDasharray={`${(availableRooms / totalRooms) * 377} 377`}
                      strokeDashoffset="0"
                    />
                    {/* Occupied Segment */}
                    <circle
                      cx="80"
                      cy="80"
                      r="60"
                      className="donut-segment segment-occupied"
                      strokeDasharray={`${(occupiedRooms / totalRooms) * 377} 377`}
                      strokeDashoffset={`${-((availableRooms / totalRooms) * 377)}`}
                    />
                    {/* Reserved Segment */}
                    <circle
                      cx="80"
                      cy="80"
                      r="60"
                      className="donut-segment segment-reserved"
                      strokeDasharray={`${(reservedRooms / totalRooms) * 377} 377`}
                      strokeDashoffset={`${-(((availableRooms + occupiedRooms) / totalRooms) * 377)}`}
                    />
                  </>
                ) : null}

                {/* Center Content */}
                <text x="80" y="74" textAnchor="middle" className="donut-center-value">
                  {totalRooms}
                </text>
                <text x="80" y="94" textAnchor="middle" className="donut-center-label">
                  Total Rooms
                </text>
              </svg>
            </div>

            {/* Legend & Breakdown */}
            <div className="occupancy-breakdown">
              <div className="breakdown-item">
                <div className="breakdown-item-header">
                  <span className="legend-indicator bg-available" />
                  <span className="breakdown-label">Available</span>
                  <span className="breakdown-value">{availableRooms} rooms</span>
                </div>
                <div className="progress-bar-bg">
                  <div
                    className="progress-bar-fill fill-available"
                    style={{ width: `${totalRooms > 0 ? (availableRooms / totalRooms) * 100 : 0}%` }}
                  />
                </div>
              </div>

              <div className="breakdown-item">
                <div className="breakdown-item-header">
                  <span className="legend-indicator bg-occupied" />
                  <span className="breakdown-label">Occupied (Checked In)</span>
                  <span className="breakdown-value">{occupiedRooms} rooms</span>
                </div>
                <div className="progress-bar-bg">
                  <div
                    className="progress-bar-fill fill-occupied"
                    style={{ width: `${totalRooms > 0 ? (occupiedRooms / totalRooms) * 100 : 0}%` }}
                  />
                </div>
              </div>

              <div className="breakdown-item">
                <div className="breakdown-item-header">
                  <span className="legend-indicator bg-reserved" />
                  <span className="breakdown-label">Reserved / Pending</span>
                  <span className="breakdown-value">{reservedRooms} rooms</span>
                </div>
                <div className="progress-bar-bg">
                  <div
                    className="progress-bar-fill fill-reserved"
                    style={{ width: `${totalRooms > 0 ? (reservedRooms / totalRooms) * 100 : 0}%` }}
                  />
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Booking Status Distribution Card */}
        <div className="card analytics-card">
          <div className="card-header">
            <div className="card-title-group">
              <PieChart size={18} className="card-header-icon" />
              <h2 className="card-title">Booking Status Distribution</h2>
            </div>
            <span className="analytics-badge">{totalStatusCount} Reservations</span>
          </div>

          <div className="booking-status-body">
            {totalStatusCount === 0 ? (
              <div className="empty-analytics-placeholder">
                <AlertCircle size={28} />
                <p>No reservation records found in the database yet.</p>
              </div>
            ) : (
              <div className="status-grid-list">
                <div className="status-metric-row">
                  <div className="status-metric-info">
                    <span className="status-bullet status-bullet--confirmed" />
                    <span className="status-name">Confirmed</span>
                  </div>
                  <div className="status-metric-counts">
                    <span className="status-count">{confirmedCount}</span>
                    <span className="status-pct">
                      ({totalStatusCount > 0 ? Math.round((confirmedCount / totalStatusCount) * 100) : 0}%)
                    </span>
                  </div>
                </div>

                <div className="status-metric-row">
                  <div className="status-metric-info">
                    <span className="status-bullet status-bullet--checkedin" />
                    <span className="status-name">Checked In</span>
                  </div>
                  <div className="status-metric-counts">
                    <span className="status-count">{checkedInCount}</span>
                    <span className="status-pct">
                      ({totalStatusCount > 0 ? Math.round((checkedInCount / totalStatusCount) * 100) : 0}%)
                    </span>
                  </div>
                </div>

                <div className="status-metric-row">
                  <div className="status-metric-info">
                    <span className="status-bullet status-bullet--pending" />
                    <span className="status-name">Pending Payment</span>
                  </div>
                  <div className="status-metric-counts">
                    <span className="status-count">{pendingCount}</span>
                    <span className="status-pct">
                      ({totalStatusCount > 0 ? Math.round((pendingCount / totalStatusCount) * 100) : 0}%)
                    </span>
                  </div>
                </div>

                <div className="status-metric-row">
                  <div className="status-metric-info">
                    <span className="status-bullet status-bullet--checkedout" />
                    <span className="status-name">Checked Out</span>
                  </div>
                  <div className="status-metric-counts">
                    <span className="status-count">{checkedOutCount}</span>
                    <span className="status-pct">
                      ({totalStatusCount > 0 ? Math.round((checkedOutCount / totalStatusCount) * 100) : 0}%)
                    </span>
                  </div>
                </div>

                <div className="status-metric-row">
                  <div className="status-metric-info">
                    <span className="status-bullet status-bullet--cancelled" />
                    <span className="status-name">Cancelled</span>
                  </div>
                  <div className="status-metric-counts">
                    <span className="status-count">{cancelledCount}</span>
                    <span className="status-pct">
                      ({totalStatusCount > 0 ? Math.round((cancelledCount / totalStatusCount) * 100) : 0}%)
                    </span>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* SECTION 4 & 5: Monthly Revenue & Monthly Occupancy Charts */}
      <div className="dashboard-grid-2">
        {/* Monthly Revenue Bar Chart */}
        <div className="card analytics-card">
          <div className="card-header">
            <div className="card-title-group">
              <BarChart3 size={18} className="card-header-icon" />
              <h2 className="card-title">Monthly Revenue (Last 6 Months)</h2>
            </div>
            <span className="analytics-badge">Paid Transactions</span>
          </div>

          <div className="chart-wrapper">
            <div className="bar-chart-container">
              {monthlyRevenue.map((item, index) => {
                const month = item.month ?? item.Month ?? '';
                const amount = item.amount ?? item.Amount ?? 0;
                const heightPct = maxRevenue > 0 ? Math.round((amount / maxRevenue) * 100) : 0;
                return (
                  <div
                    key={index}
                    className="bar-column"
                    onMouseEnter={() => setActiveTooltip(`rev-${index}`)}
                    onMouseLeave={() => setActiveTooltip(null)}
                  >
                    {activeTooltip === `rev-${index}` && (
                      <div className="chart-tooltip">
                        <strong>{month}</strong>
                        <span>{formatCurrency(amount)}</span>
                      </div>
                    )}
                    <div className="bar-track">
                      <div
                        className="bar-fill"
                        style={{ height: `${Math.max(heightPct, amount > 0 ? 8 : 2)}%` }}
                      />
                    </div>
                    <span className="bar-value-label">{formatCurrency(amount)}</span>
                    <span className="bar-x-label">{month}</span>
                  </div>
                );
              })}
            </div>
          </div>
        </div>

        {/* Monthly Occupancy Line/Area Chart */}
        <div className="card analytics-card">
          <div className="card-header">
            <div className="card-title-group">
              <TrendingUp size={18} className="card-header-icon" />
              <h2 className="card-title">Monthly Occupancy Trend</h2>
            </div>
            <span className="analytics-badge">Room Days Basis</span>
          </div>

          <div className="chart-wrapper">
            <div className="occupancy-bars-container">
              {monthlyOccupancy.map((item, index) => {
                const month = item.month ?? item.Month ?? '';
                const percentage = item.percentage ?? item.Percentage ?? 0;
                return (
                  <div
                    key={index}
                    className="occupancy-bar-column"
                    onMouseEnter={() => setActiveTooltip(`occ-${index}`)}
                    onMouseLeave={() => setActiveTooltip(null)}
                  >
                    {activeTooltip === `occ-${index}` && (
                      <div className="chart-tooltip">
                        <strong>{month}</strong>
                        <span>{percentage}% Average Occupancy</span>
                      </div>
                    )}
                    <div className="occupancy-track">
                      <div
                        className="occupancy-fill"
                        style={{ height: `${Math.max(percentage, percentage > 0 ? 6 : 2)}%` }}
                      />
                    </div>
                    <span className="occupancy-value-label">{percentage}%</span>
                    <span className="bar-x-label">{month}</span>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      </div>

      {/* SECTION 6 & 9: Room Category Occupancy & Financial Ledger */}
      <div className="dashboard-grid-2">
        {/* Room Category Occupancy */}
        <div className="card analytics-card">
          <div className="card-header">
            <div className="card-title-group">
              <DoorOpen size={18} className="card-header-icon" />
              <h2 className="card-title">Occupancy by Room Category</h2>
            </div>
            <span className="analytics-badge">Inventory Analysis</span>
          </div>

          <div className="category-occupancy-list">
            {roomCategoryOccupancy.length === 0 ? (
              <p className="text-muted" style={{ padding: '16px 0' }}>No room categories registered.</p>
            ) : (
              roomCategoryOccupancy.map((cat, idx) => {
                const category = cat.category ?? cat.Category ?? '';
                const catTotal = cat.totalRooms ?? cat.TotalRooms ?? 0;
                const catOccupied = cat.occupiedRooms ?? cat.OccupiedRooms ?? 0;
                const catPct = cat.percentage ?? cat.Percentage ?? 0;
                return (
                  <div key={idx} className="category-row">
                    <div className="category-row-meta">
                      <span className="category-name">{category}</span>
                      <span className="category-counts">
                        {catOccupied} of {catTotal} occupied
                      </span>
                    </div>
                    <div className="category-progress-container">
                      <div className="progress-bar-bg">
                        <div
                          className="progress-bar-fill fill-occupied"
                          style={{ width: `${catPct}%` }}
                        />
                      </div>
                      <span className="category-pct-badge">{catPct}%</span>
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>

        {/* Financial Ledger & Payment Overview */}
        <div className="card analytics-card">
          <div className="card-header">
            <div className="card-title-group">
              <CreditCard size={18} className="card-header-icon" />
              <h2 className="card-title">Financial Ledger & Collections</h2>
            </div>
            <span className="analytics-badge">Payment Health</span>
          </div>

          <div className="financial-ledger-body">
            <div className="financial-summary-cards">
              <div className="finance-mini-card">
                <span className="finance-mini-label">Total Collected</span>
                <span className="finance-mini-val val--paid">{formatCurrency(totalPaid)}</span>
              </div>
              <div className="finance-mini-card">
                <span className="finance-mini-label">Outstanding Balance</span>
                <span className="finance-mini-val val--outstanding">{formatCurrency(totalOutstanding)}</span>
              </div>
            </div>

            <div className="ledger-breakdown-box">
              <h4 className="ledger-box-title">Reservation Payment Statuses</h4>
              <div className="ledger-status-pills">
                <div className="ledger-pill">
                  <ShieldCheck size={14} className="pill-icon text-success" />
                  <span className="pill-text">Fully Paid:</span>
                  <strong className="pill-val">{fullyPaidBookings}</strong>
                </div>
                <div className="ledger-pill">
                  <Clock size={14} className="pill-icon text-warning" />
                  <span className="pill-text">Partially Paid:</span>
                  <strong className="pill-val">{partiallyPaidBookings}</strong>
                </div>
                <div className="ledger-pill">
                  <XCircle size={14} className="pill-icon text-danger" />
                  <span className="pill-text">Unpaid:</span>
                  <strong className="pill-val">{unpaidBookings}</strong>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* SECTION 7 & 8: Recent Bookings & Recent Payments */}
      <div className="dashboard-grid-2">
        {/* Recent Bookings Card */}
        <div className="card">
          <div className="card-header">
            <div className="card-title-group">
              <CalendarCheck size={18} className="card-header-icon" />
              <h2 className="card-title">Recent Reservations</h2>
            </div>
            <button className="btn-link" onClick={() => navigate('/bookings')}>
              View All <ArrowRight size={13} />
            </button>
          </div>

          <div className="recent-list">
            {recentBookings.length === 0 ? (
              <p className="text-muted" style={{ padding: '16px 0', fontSize: 'var(--text-sm)' }}>
                No reservations found in database.
              </p>
            ) : (
              recentBookings.map((b) => {
                const id = b.id ?? b.Id;
                const roomNumber = b.roomNumber ?? b.RoomNumber ?? b.roomId ?? b.RoomId;
                const customerName = b.customerName ?? b.CustomerName ?? '';
                const checkInDate = b.checkInDate ?? b.CheckInDate;
                const checkOutDate = b.checkOutDate ?? b.CheckOutDate;
                const totalAmt = b.totalAmount ?? b.TotalAmount ?? 0;
                const status = b.status ?? b.Status ?? '';
                return (
                  <div className="recent-item" key={id}>
                    <div className="recent-item-info">
                      <div className="recent-item-primary">
                        <span>Booking #{id}</span>
                        <span className="meta-separator">|</span>
                        <span className="text-gold">Room {roomNumber}</span>
                        {customerName && (
                          <>
                            <span className="meta-separator">|</span>
                            <span className="text-secondary">{customerName}</span>
                          </>
                        )}
                      </div>
                      <div className="recent-item-secondary">
                        {formatDate(checkInDate)} to {formatDate(checkOutDate)}
                        <span className="meta-separator">|</span>
                        <span className="booking-amount-inline">${totalAmt.toFixed(2)}</span>
                      </div>
                    </div>
                    <StatusBadge status={status} />
                  </div>
                );
              })
            )}
          </div>
        </div>

        {/* Recent Payments Card */}
        <div className="card">
          <div className="card-header">
            <div className="card-title-group">
              <CreditCard size={18} className="card-header-icon" />
              <h2 className="card-title">Recent Transactions</h2>
            </div>
            <button className="btn-link" onClick={() => navigate('/payments')}>
              View All <ArrowRight size={13} />
            </button>
          </div>

          <div className="recent-list">
            {recentPayments.length === 0 ? (
              <p className="text-muted" style={{ padding: '16px 0', fontSize: 'var(--text-sm)' }}>
                No payment transactions recorded yet.
              </p>
            ) : (
              recentPayments.map((p) => {
                const id = p.id ?? p.Id;
                const bookingId = p.bookingId ?? p.BookingId;
                const amount = p.amount ?? p.Amount ?? 0;
                const paymentMethod = p.paymentMethod ?? p.PaymentMethod ?? 'Cash';
                const paymentDate = p.paymentDate ?? p.PaymentDate;
                const paymentStatus = p.paymentStatus ?? p.PaymentStatus ?? 'Paid';
                return (
                  <div className="recent-item" key={id}>
                    <div className="recent-item-info">
                      <div className="recent-item-primary">
                        <span>Payment #{id}</span>
                        <span className="meta-separator">|</span>
                        <span className="text-secondary">Booking #{bookingId}</span>
                        <span className="meta-separator">|</span>
                        <span className="payment-method-tag">{paymentMethod}</span>
                      </div>
                      <div className="recent-item-secondary">
                        {formatDate(paymentDate)}
                      </div>
                    </div>
                    <div className="payment-amount-cell">
                      <span className="text-gold font-medium">${amount.toFixed(2)}</span>
                      <StatusBadge status={paymentStatus} />
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>
      </div>

      {/* SECTION 10: Quick Actions */}
      <div className="card quick-actions-card">
        <div className="card-header">
          <h2 className="card-title">Hotel Administrative Quick Actions</h2>
        </div>
        <div className="quick-actions-grid">
          <button className="quick-action-btn" onClick={() => navigate('/bookings')}>
            <Plus size={16} className="btn-action-icon" />
            <span>New Reservation</span>
          </button>
          <button className="quick-action-btn" onClick={() => navigate('/rooms')}>
            <DoorOpen size={16} className="btn-action-icon" />
            <span>Manage Inventory</span>
          </button>
          <button className="quick-action-btn" onClick={() => navigate('/customers')}>
            <Users size={16} className="btn-action-icon" />
            <span>Guest Directory</span>
          </button>
          <button className="quick-action-btn" onClick={() => navigate('/payments')}>
            <CreditCard size={16} className="btn-action-icon" />
            <span>Record Payment</span>
          </button>
          <button className="quick-action-btn" onClick={() => navigate('/bookings')}>
            <Key size={16} className="btn-action-icon" />
            <span>Guest Check-in</span>
          </button>
          <button className="quick-action-btn" onClick={() => navigate('/bookings')}>
            <LogOut size={16} className="btn-action-icon" />
            <span>Guest Check-out</span>
          </button>
        </div>
      </div>
    </div>
  );
}
