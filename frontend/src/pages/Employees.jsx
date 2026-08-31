import { useEffect, useState, useCallback } from 'react';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import api from '../api/axios';
import DataTable from '../components/ui/DataTable';
import Modal from '../components/ui/Modal';
import ConfirmDialog from '../components/ui/ConfirmDialog';
import toast from 'react-hot-toast';
import './Employees.css';

const emptyForm = { firstName: '', lastName: '', email: '', phone: '', role: 'Receptionist', salary: '' };

export default function Employees() {
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const fetchEmployees = useCallback(async () => {
    try {
      const { data } = await api.get('/employees');
      setEmployees(data);
    } catch {
      toast.error('Failed to load employees.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchEmployees(); }, [fetchEmployees]);

  function openCreate() {
    setEditing(null);
    setForm(emptyForm);
    setModalOpen(true);
  }

  function openEdit(emp) {
    setEditing(emp);
    setForm({
      firstName: emp.firstName,
      lastName: emp.lastName,
      email: emp.email,
      phone: emp.phone,
      role: emp.role,
      salary: emp.salary,
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
      const payload = { ...form, salary: parseFloat(form.salary) };
      if (editing) {
        await api.put(`/employees/${editing.id}`, payload);
        toast.success('Employee updated.');
      } else {
        await api.post('/employees', payload);
        toast.success('Employee created.');
      }
      setModalOpen(false);
      fetchEmployees();
    } catch (err) {
      toast.error(err.response?.data?.detail || 'Failed to save employee.');
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete() {
    setDeleting(true);
    try {
      await api.delete(`/employees/${deleteTarget.id}`);
      toast.success('Employee deleted.');
      setDeleteTarget(null);
      fetchEmployees();
    } catch (err) {
      toast.error(err.response?.data?.detail || 'Failed to delete employee.');
    } finally {
      setDeleting(false);
    }
  }

  const columns = [
    { key: 'name', label: 'Name', render: (e) => (
      <span className="font-medium">{e.firstName} {e.lastName}</span>
    )},
    { key: 'email', label: 'Email', render: (e) => (
      <span className="text-secondary">{e.email}</span>
    )},
    { key: 'phone', label: 'Phone' },
    { key: 'role', label: 'Role', render: (e) => (
      <span className="employee-role-tag">{e.role}</span>
    )},
    { key: 'salary', label: 'Salary', render: (e) => (
      <span className="salary-cell">${e.salary.toLocaleString('en-US', { minimumFractionDigits: 2 })}</span>
    )},
    { key: 'actions', label: '', style: { width: 100 }, render: (e) => (
      <div className="actions-cell">
        <button className="btn-icon" onClick={() => openEdit(e)} title="Edit">
          <Pencil size={15} />
        </button>
        <button className="btn-icon" onClick={() => setDeleteTarget(e)} title="Delete"
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
          <h1 className="page-title">Employees</h1>
          <p className="page-subtitle">{employees.length} hotel staff members</p>
        </div>
        <div className="page-actions">
          <button className="btn btn-primary" onClick={openCreate}>
            <Plus size={16} /> Add Employee
          </button>
        </div>
      </div>

      <DataTable columns={columns} data={employees} loading={loading}
        emptyMessage="No employees found. Add your first employee." />

      <Modal open={modalOpen} onClose={() => setModalOpen(false)}
        title={editing ? 'Edit Employee' : 'Add Employee'}>
        <form onSubmit={handleSave}>
          <div className="modal-body">
            <div className="form-row">
              <div className="form-group">
                <label className="form-label" htmlFor="empFirst">First Name</label>
                <input id="empFirst" className="form-input" value={form.firstName}
                  onChange={(e) => update('firstName', e.target.value)} required placeholder="Jane" />
              </div>
              <div className="form-group">
                <label className="form-label" htmlFor="empLast">Last Name</label>
                <input id="empLast" className="form-input" value={form.lastName}
                  onChange={(e) => update('lastName', e.target.value)} required placeholder="Smith" />
              </div>
            </div>
            <div className="form-group">
              <label className="form-label" htmlFor="empEmail">Email</label>
              <input id="empEmail" className="form-input" type="email" value={form.email}
                onChange={(e) => update('email', e.target.value)} required placeholder="jane@hotel.com" />
            </div>
            <div className="form-row">
              <div className="form-group">
                <label className="form-label" htmlFor="empPhone">Phone</label>
                <input id="empPhone" className="form-input" value={form.phone}
                  onChange={(e) => update('phone', e.target.value)} required placeholder="+1 555-0199" />
              </div>
              <div className="form-group">
                <label className="form-label" htmlFor="empRole">Role</label>
                <select id="empRole" className="form-select" value={form.role}
                  onChange={(e) => update('role', e.target.value)}>
                  <option>Receptionist</option>
                  <option>Housekeeper</option>
                  <option>Manager</option>
                  <option>Maintenance</option>
                  <option>Security</option>
                  <option>Chef</option>
                  <option>Concierge</option>
                </select>
              </div>
            </div>
            <div className="form-group">
              <label className="form-label" htmlFor="empSalary">Monthly Salary ($)</label>
              <input id="empSalary" className="form-input" type="number" min="0" step="0.01"
                value={form.salary} onChange={(e) => update('salary', e.target.value)}
                required placeholder="3500.00" />
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
        title="Delete Employee"
        message={`Delete ${deleteTarget?.firstName} ${deleteTarget?.lastName}? This action cannot be undone.`}
        confirmText="Delete"
        loading={deleting}
      />
    </div>
  );
}
