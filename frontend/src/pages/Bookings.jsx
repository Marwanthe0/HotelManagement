import { useEffect, useState, useCallback } from 'react';
import {
  Plus, Trash2, X as XIcon,
  LogIn as LogInIcon, LogOut as LogOutIcon,
} from 'lucide-react';
import api from '../api/axios';
import DataTable from '../components/ui/DataTable';
import StatusBadge from '../components/ui/StatusBadge';
import Modal from '../components/ui/Modal';
import ConfirmDialog from '../components/ui/ConfirmDialog';
import toast from 'react-hot-toast';
import './Bookings.css';

const STATUS_TABS = ['All', 'Pending', 'Confirmed', 'CheckedIn', 'CheckedOut', 'Cancelled'];

export default function Bookings() {
  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('All');
  const [customers, setCustomers] = useState([]);
  const [rooms, setRooms] = useState([]);

  // Create modal
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState({ customerId: '', roomId: '', checkInDate: '', checkOutDate: '' });
  const [saving, setSaving] = useState(false);

  // Delete
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const fetchBookings = useCallback(async (status) => {
    try {
      const params = status && status !== 'All' ? { status } : {};
      const { data } = await api.get('/bookings', { params });
      setBookings(data);
    } catch {
      toast.error('Failed to load bookings.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchBookings(activeTab);
  }, [fetchBookings, activeTab]);

  // Load customers & rooms for the create form
  useEffect(() => {
    async function loadDeps() {
      try {
        const [c, r] = await Promise.all([
          api.get('/customers'),
          api.get('/rooms'),
        ]);
        setCustomers(c.data);
        setRooms(r.data);
      } catch { /* non-critical */ }
    }
    loadDeps();
  }, []);

  function update(field, value) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  function openCreate() {
    setForm({ customerId: '', roomId: '', checkInDate: '', checkOutDate: '' });
    setModalOpen(true);
  }

  async function handleCreate(e) {
    e.preventDefault();
    setSaving(true);
    try {
      await api.post('/bookings', {
        customerId: parseInt(form.customerId),
        roomId: parseInt(form.roomId),
        checkInDate: form.checkInDate,
        checkOutDate: form.checkOutDate,
      });
      toast.success('Booking created.');
      setModalOpen(false);
      fetchBookings(activeTab);
    } catch (err) {
      toast.error(err.response?.data?.detail || 'Failed to create booking.');
    } finally {
      setSaving(false);
    }
  }

  async function handleAction(id, action, label) {
    try {
      await api.patch(`/bookings/${id}/${action}`);
      toast.success(`Booking ${label}.`);
      fetchBookings(activeTab);
    } catch (err) {
      toast.error(err.response?.data?.detail || `Failed to ${label.toLowerCase()} booking.`);
    }
  }

  async function handleDelete() {
    setDeleting(true);
    try {
      await api.delete(`/bookings/${deleteTarget.id}`);
      toast.success('Booking deleted.');
      setDeleteTarget(null);
      fetchBookings(activeTab);
    } catch (err) {
      toast.error(err.response?.data?.detail || 'Failed to delete booking.');
    } finally {
      setDeleting(false);
    }
  }

  function formatDate(d) {
    if (!d) return 'N/A';
    return new Date(d).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }

  // Build lookup maps so the table can display names instead of raw IDs
  const customerMap = {};
  for (const c of customers) {
    customerMap[c.id] = `${c.firstName} ${c.lastName}`;
  }
  const roomMap = {};
  for (const r of rooms) {
    roomMap[r.id] = r.roomNumber;
  }

  const columns = [
    { key: 'id', label: 'ID', render: (b) => (
      <span className="font-medium">#{b.id}</span>
    )},
    { key: 'ids', label: 'Customer / Room', render: (b) => (
      <div className="booking-ids">
        <span>Customer: {b.customerId} {customerMap[b.customerId] ? `(${customerMap[b.customerId]})` : ''}</span>
        <span>Room: {b.roomId} {roomMap[b.roomId] ? `(No. ${roomMap[b.roomId]})` : ''}</span>
      </div>
    )},
    { key: 'dates', label: 'Dates', render: (b) => (
      <div className="booking-dates">
        <span>In: {formatDate(b.checkInDate)}</span>
        <span>Out: {formatDate(b.checkOutDate)}</span>
      </div>
    )},
    { key: 'totalAmount', label: 'Total', render: (b) => (
      <span className="booking-amount">${b.totalAmount.toFixed(2)}</span>
    )},
    { key: 'status', label: 'Status', render: (b) => (
      <StatusBadge status={b.status} />
    )},
    { key: 'actions', label: '', style: { width: 160 }, render: (b) => (
      <div className="booking-actions-cell">
        {b.status === 'Confirmed' && (
          <button className="btn btn-success btn-sm"
            onClick={() => handleAction(b.id, 'check-in', 'checked in')} title="Check In">
            <LogInIcon size={13} /> Check In
          </button>
        )}
        {b.status === 'CheckedIn' && (
          <button className="btn btn-secondary btn-sm"
            onClick={() => handleAction(b.id, 'check-out', 'checked out')} title="Check Out">
            <LogOutIcon size={13} /> Check Out
          </button>
        )}
        {(b.status === 'Pending' || b.status === 'Confirmed') && (
          <button className="btn btn-danger btn-sm"
            onClick={() => handleAction(b.id, 'cancel', 'cancelled')} title="Cancel">
            <XIcon size={13} /> Cancel
          </button>
        )}
        {b.status === 'Pending' && (
          <button className="btn-icon" onClick={() => setDeleteTarget(b)}
            title="Delete" style={{ color: 'var(--error)' }}>
            <Trash2 size={14} />
          </button>
        )}
      </div>
    )},
  ];

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Bookings</h1>
          <p className="page-subtitle">{bookings.length} reservations</p>
        </div>
        <div className="page-actions">
          <button className="btn btn-primary" onClick={openCreate}>
            <Plus size={16} /> New Booking
          </button>
        </div>
      </div>

      <div className="tabs">
        {STATUS_TABS.map((tab) => (
          <button
            key={tab}
            className={`tab${activeTab === tab ? ' active' : ''}`}
            onClick={() => { setActiveTab(tab); setLoading(true); }}
          >
            {tab === 'CheckedIn' ? 'Checked In' : tab === 'CheckedOut' ? 'Checked Out' : tab}
          </button>
        ))}
      </div>

      <DataTable columns={columns} data={bookings} loading={loading}
        emptyMessage="No bookings found." />

      {/* Create Booking Modal */}
      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title="New Booking">
        <form onSubmit={handleCreate}>
          <div className="modal-body">
            <div className="form-row">
              <div className="form-group">
                <label className="form-label" htmlFor="bCust">Customer</label>
                <select id="bCust" className="form-select" value={form.customerId}
                  onChange={(e) => update('customerId', e.target.value)} required>
                  <option value="">Select customer...</option>
                  {customers.map((c) => (
                    <option key={c.id} value={c.id}>{c.firstName} {c.lastName}</option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label className="form-label" htmlFor="bRoom">Room</label>
                <select id="bRoom" className="form-select" value={form.roomId}
                  onChange={(e) => update('roomId', e.target.value)} required>
                  <option value="">Select room...</option>
                  {rooms.filter((r) => r.isAvailable).map((r) => (
                    <option key={r.id} value={r.id}>
                      Room {r.roomNumber} ({r.roomType}) : ${r.pricePerNight}/night
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <div className="form-row">
              <div className="form-group">
                <label className="form-label" htmlFor="bCheckIn">Check-In Date</label>
                <input id="bCheckIn" className="form-input" type="date" value={form.checkInDate}
                  onChange={(e) => update('checkInDate', e.target.value)} required />
              </div>
              <div className="form-group">
                <label className="form-label" htmlFor="bCheckOut">Check-Out Date</label>
                <input id="bCheckOut" className="form-input" type="date" value={form.checkOutDate}
                  onChange={(e) => update('checkOutDate', e.target.value)} required />
              </div>
            </div>
          </div>
          <div className="modal-footer">
            <button type="button" className="btn btn-secondary" onClick={() => setModalOpen(false)}>Cancel</button>
            <button type="submit" className="btn btn-primary" disabled={saving}>
              {saving ? 'Creating...' : 'Create Booking'}
            </button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Booking"
        message={`Delete booking #${deleteTarget?.id}? This cannot be undone.`}
        confirmText="Delete"
        loading={deleting}
      />
    </div>
  );
}
