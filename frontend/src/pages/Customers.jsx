import { useEffect, useState, useCallback } from 'react';
import { Plus, Pencil, Trash2, Search } from 'lucide-react';
import api from '../api/axios';
import DataTable from '../components/ui/DataTable';
import Modal from '../components/ui/Modal';
import ConfirmDialog from '../components/ui/ConfirmDialog';
import toast from 'react-hot-toast';
import './Customers.css';

const emptyForm = { firstName: '', lastName: '', email: '', phone: '', address: '' };

export default function Customers() {
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const fetchCustomers = useCallback(async (query = '') => {
    try {
      const params = query ? { search: query } : {};
      const { data } = await api.get('/customers', { params });
      setCustomers(data);
    } catch {
      toast.error('Failed to load customers.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchCustomers(); }, [fetchCustomers]);

  // Debounced search
  useEffect(() => {
    const timer = setTimeout(() => {
      fetchCustomers(search);
    }, 400);
    return () => clearTimeout(timer);
  }, [search, fetchCustomers]);

  function openCreate() {
    setEditing(null);
    setForm(emptyForm);
    setModalOpen(true);
  }

  function openEdit(c) {
    setEditing(c);
    setForm({
      firstName: c.firstName,
      lastName: c.lastName,
      email: c.email,
      phone: c.phone,
      address: c.address,
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
      if (editing) {
        await api.put(`/customers/${editing.id}`, form);
        toast.success('Customer updated.');
      } else {
        await api.post('/customers', form);
        toast.success('Customer created.');
      }
      setModalOpen(false);
      fetchCustomers(search);
    } catch (err) {
      toast.error(err.response?.data?.detail || 'Failed to save customer.');
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete() {
    setDeleting(true);
    try {
      await api.delete(`/customers/${deleteTarget.id}`);
      toast.success('Customer deleted.');
      setDeleteTarget(null);
      fetchCustomers(search);
    } catch (err) {
      toast.error(err.response?.data?.detail || 'Failed to delete customer.');
    } finally {
      setDeleting(false);
    }
  }

  const columns = [
    { key: 'name', label: 'Name', render: (c) => (
      <span className="font-medium">{c.firstName} {c.lastName}</span>
    )},
    { key: 'email', label: 'Email', render: (c) => (
      <span className="text-secondary">{c.email}</span>
    )},
    { key: 'phone', label: 'Phone' },
    { key: 'address', label: 'Address', render: (c) => (
      <span className="text-secondary">{c.address || 'N/A'}</span>
    )},
    { key: 'actions', label: '', style: { width: 100 }, render: (c) => (
      <div className="actions-cell">
        <button className="btn-icon" onClick={() => openEdit(c)} title="Edit">
          <Pencil size={15} />
        </button>
        <button className="btn-icon" onClick={() => setDeleteTarget(c)} title="Delete"
          style={{ color: 'var(--error)' }}>
          <Trash2 size={15} />
        </button>
      </div>
    )},
  ];

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Customers</h1>
          <p className="page-subtitle">{customers.length} registered guests</p>
        </div>
        <div className="page-actions">
          <button className="btn btn-primary" onClick={openCreate}>
            <Plus size={16} /> Add Customer
          </button>
        </div>
      </div>

      <div className="customers-search">
        <div style={{ position: 'relative' }}>
          <Search size={16} style={{
            position: 'absolute', left: 12, top: '50%', transform: 'translateY(-50%)',
            color: 'var(--text-muted)', pointerEvents: 'none'
          }} />
          <input
            className="form-input"
            placeholder="Search by name or email..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            style={{ paddingLeft: 36 }}
          />
        </div>
      </div>

      <DataTable columns={columns} data={customers} loading={loading}
        emptyMessage="No customers found." />

      <Modal open={modalOpen} onClose={() => setModalOpen(false)}
        title={editing ? 'Edit Customer' : 'Add Customer'}>
        <form onSubmit={handleSave}>
          <div className="modal-body">
            <div className="form-row">
              <div className="form-group">
                <label className="form-label" htmlFor="firstName">First Name</label>
                <input id="firstName" className="form-input" value={form.firstName}
                  onChange={(e) => update('firstName', e.target.value)} required placeholder="John" />
              </div>
              <div className="form-group">
                <label className="form-label" htmlFor="lastName">Last Name</label>
                <input id="lastName" className="form-input" value={form.lastName}
                  onChange={(e) => update('lastName', e.target.value)} required placeholder="Doe" />
              </div>
            </div>
            <div className="form-group">
              <label className="form-label" htmlFor="custEmail">Email</label>
              <input id="custEmail" className="form-input" type="email" value={form.email}
                onChange={(e) => update('email', e.target.value)} required placeholder="john@example.com" />
            </div>
            <div className="form-row">
              <div className="form-group">
                <label className="form-label" htmlFor="phone">Phone</label>
                <input id="phone" className="form-input" value={form.phone}
                  onChange={(e) => update('phone', e.target.value)} required placeholder="+1 555-0123" />
              </div>
              <div className="form-group">
                <label className="form-label" htmlFor="address">Address</label>
                <input id="address" className="form-input" value={form.address}
                  onChange={(e) => update('address', e.target.value)} placeholder="123 Main St" />
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

      <ConfirmDialog
        open={!!deleteTarget}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleDelete}
        title="Delete Customer"
        message={`Delete ${deleteTarget?.firstName} ${deleteTarget?.lastName}? This action cannot be undone.`}
        confirmText="Delete"
        loading={deleting}
      />
    </div>
  );
}
