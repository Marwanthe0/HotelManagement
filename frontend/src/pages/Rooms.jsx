import { useEffect, useState, useCallback } from 'react';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import api from '../api/axios';
import DataTable from '../components/ui/DataTable';
import StatusBadge from '../components/ui/StatusBadge';
import Modal from '../components/ui/Modal';
import ConfirmDialog from '../components/ui/ConfirmDialog';
import toast from 'react-hot-toast';
import './Rooms.css';

const emptyForm = { roomNumber: '', roomType: 'Standard', pricePerNight: '', isAvailable: true };

export default function Rooms() {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin';

  const [rooms, setRooms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const fetchRooms = useCallback(async () => {
    try {
      const { data } = await api.get('/rooms');
      setRooms(data);
    } catch {
      toast.error('Failed to load rooms.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchRooms(); }, [fetchRooms]);

  function openCreate() {
    setEditing(null);
    setForm(emptyForm);
    setModalOpen(true);
  }

  function openEdit(room) {
    setEditing(room);
    setForm({
      roomNumber: room.roomNumber,
      roomType: room.roomType,
      pricePerNight: room.pricePerNight,
      isAvailable: room.isAvailable,
    });
    setModalOpen(true);
  }

  function update(field, value) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  async function handleSave(e) {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        roomNumber: form.roomNumber.trim(),
        roomType: form.roomType,
        pricePerNight: parseFloat(form.pricePerNight),
        isAvailable: form.isAvailable,
      };

      if (editing) {
        await api.put(`/rooms/${editing.id}`, payload);
        toast.success('Room updated.');
      } else {
        await api.post('/rooms', payload);
        toast.success('Room created.');
      }
      setModalOpen(false);
      fetchRooms();
    } catch (err) {
      toast.error(err.response?.data?.detail || 'Failed to save room.');
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete() {
    setDeleting(true);
    try {
      await api.delete(`/rooms/${deleteTarget.id}`);
      toast.success('Room deleted.');
      setDeleteTarget(null);
      fetchRooms();
    } catch (err) {
      toast.error(err.response?.data?.detail || 'Failed to delete room.');
    } finally {
      setDeleting(false);
    }
  }

  const columns = [
    { key: 'roomNumber', label: 'Room #', render: (r) => (
      <span className="font-medium">{r.roomNumber}</span>
    )},
    { key: 'roomType', label: 'Type', render: (r) => (
      <span className="rooms-type-tag">{r.roomType}</span>
    )},
    { key: 'pricePerNight', label: 'Price / Night', render: (r) => (
      <span className="price-cell">${r.pricePerNight.toFixed(2)}</span>
    )},
    { key: 'isAvailable', label: 'Status', render: (r) => (
      <StatusBadge status={r.isAvailable ? 'Available' : 'Maintenance'} />
    )},
    ...(isAdmin ? [{
      key: 'actions', label: '', style: { width: 100 }, render: (r) => (
        <div className="actions-cell">
          <button className="btn-icon" onClick={() => openEdit(r)} title="Edit">
            <Pencil size={15} />
          </button>
          <button className="btn-icon" onClick={() => setDeleteTarget(r)} title="Delete"
            style={{ color: 'var(--error)' }}>
            <Trash2 size={15} />
          </button>
        </div>
      ),
    }] : []),
  ];

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Rooms</h1>
          <p className="page-subtitle">{rooms.length} rooms total</p>
        </div>
        {isAdmin && (
          <div className="page-actions">
            <button className="btn btn-primary" onClick={openCreate}>
              <Plus size={16} /> Add Room
            </button>
          </div>
        )}
      </div>

      <DataTable columns={columns} data={rooms} loading={loading} emptyMessage="No rooms found. Add your first room." />

      {/* Create/Edit Modal */}
      {isAdmin && (
        <Modal open={modalOpen} onClose={() => setModalOpen(false)} title={editing ? 'Edit Room' : 'Add Room'}>
          <form onSubmit={handleSave}>
            <div className="modal-body">
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label" htmlFor="roomNumber">Room Number</label>
                  <input id="roomNumber" className="form-input" value={form.roomNumber}
                    onChange={(e) => update('roomNumber', e.target.value)} required placeholder="101" />
                </div>
                <div className="form-group">
                  <label className="form-label" htmlFor="roomType">Room Type</label>
                  <select id="roomType" className="form-select" value={form.roomType}
                    onChange={(e) => update('roomType', e.target.value)}>
                    <option>Standard</option>
                    <option>Deluxe</option>
                    <option>Suite</option>
                    <option>Penthouse</option>
                  </select>
                </div>
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label className="form-label" htmlFor="price">Price Per Night ($)</label>
                  <input id="price" className="form-input" type="number" min="1" step="0.01"
                    value={form.pricePerNight} onChange={(e) => update('pricePerNight', e.target.value)}
                    required placeholder="150.00" />
                </div>
                <div className="form-group">
                  <label className="form-label" htmlFor="available">Status</label>
                  <select id="available" className="form-select"
                    value={form.isAvailable ? 'true' : 'false'}
                    onChange={(e) => update('isAvailable', e.target.value === 'true')}>
                    <option value="true">Available</option>
                    <option value="false">Under Maintenance</option>
                  </select>
                </div>
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" onClick={() => setModalOpen(false)}>Cancel</button>
              <button type="submit" className="btn btn-primary" disabled={saving}>
                {saving ? 'Saving...' : editing ? 'Update' : 'Create'}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {/* Delete Confirmation */}
      {isAdmin && (
        <ConfirmDialog
          open={!!deleteTarget}
          onClose={() => setDeleteTarget(null)}
          onConfirm={handleDelete}
          title="Delete Room"
          message={`Are you sure you want to delete room ${deleteTarget?.roomNumber}? This action cannot be undone.`}
          confirmText="Delete"
          loading={deleting}
        />
      )}
    </div>
  );
}
