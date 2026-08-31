import { useEffect, useState, useCallback } from 'react';
import { CreditCard } from 'lucide-react';
import api from '../api/axios';
import DataTable from '../components/ui/DataTable';
import StatusBadge from '../components/ui/StatusBadge';
import toast from 'react-hot-toast';
import './Payments.css';

export default function Payments() {
  const [payments, setPayments] = useState([]);
  const [loading, setLoading] = useState(true);

  // Create payment form
  const [bookingId, setBookingId] = useState('');
  const [amount, setAmount] = useState('');
  const [method, setMethod] = useState('Cash');
  const [saving, setSaving] = useState(false);

  // Summary
  const [summary, setSummary] = useState(null);
  const [summaryLoading, setSummaryLoading] = useState(false);

  const fetchPayments = useCallback(async () => {
    try {
      const { data } = await api.get('/payments');
      setPayments(data);
    } catch {
      toast.error('Failed to load payments.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchPayments(); }, [fetchPayments]);

  // Fetch summary when booking ID changes
  useEffect(() => {
    if (!bookingId || isNaN(parseInt(bookingId))) {
      setSummary(null);
      return;
    }
    const timer = setTimeout(async () => {
      setSummaryLoading(true);
      try {
        const { data } = await api.get(`/payments/booking/${bookingId}/summary`);
        setSummary(data);
      } catch {
        setSummary(null);
      } finally {
        setSummaryLoading(false);
      }
    }, 500);
    return () => clearTimeout(timer);
  }, [bookingId]);

  async function handleCreate(e) {
    e.preventDefault();
    setSaving(true);
    try {
      await api.post('/payments', {
        bookingId: parseInt(bookingId),
        amount: parseFloat(amount),
        paymentMethod: method,
      });
      toast.success('Payment recorded.');
      setAmount('');
      fetchPayments();
      // Refresh summary
      const { data } = await api.get(`/payments/booking/${bookingId}/summary`);
      setSummary(data);
    } catch (err) {
      toast.error(err.response?.data?.detail || 'Failed to record payment.');
    } finally {
      setSaving(false);
    }
  }

  function formatDate(d) {
    if (!d) return 'N/A';
    return new Date(d).toLocaleDateString('en-US', {
      month: 'short', day: 'numeric', year: 'numeric',
      hour: '2-digit', minute: '2-digit',
    });
  }

  const columns = [
    { key: 'id', label: 'ID', render: (p) => (
      <span className="font-medium">#{p.id}</span>
    )},
    { key: 'bookingId', label: 'Booking', render: (p) => (
      <span>#{p.bookingId}</span>
    )},
    { key: 'amount', label: 'Amount', render: (p) => (
      <span className="booking-amount">${p.amount.toFixed(2)}</span>
    )},
    { key: 'paymentMethod', label: 'Method', render: (p) => (
      <span className="payment-method-tag">{p.paymentMethod}</span>
    )},
    { key: 'paymentStatus', label: 'Status', render: (p) => (
      <StatusBadge status={p.paymentStatus} />
    )},
    { key: 'paymentDate', label: 'Date', render: (p) => (
      <span className="text-secondary">{formatDate(p.paymentDate)}</span>
    )},
  ];

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-title">Payments</h1>
          <p className="page-subtitle">{payments.length} financial transactions</p>
        </div>
      </div>

      <div className="payments-layout">
        <DataTable columns={columns} data={payments} loading={loading}
          emptyMessage="No payments recorded yet." />

        <div className="card payment-form-card">
          <div className="card-header">
            <h2 className="card-title">Record Payment</h2>
          </div>

          <form onSubmit={handleCreate}>
            <div className="form-group">
              <label className="form-label" htmlFor="pBooking">Booking ID</label>
              <input id="pBooking" className="form-input" type="number" min="1"
                value={bookingId} onChange={(e) => setBookingId(e.target.value)}
                required placeholder="Enter booking ID" />
            </div>

            {summaryLoading && (
              <div style={{ display: 'flex', justifyContent: 'center', padding: '8px' }}>
                <div className="spinner" style={{ width: 20, height: 20 }} />
              </div>
            )}

            {summary && !summaryLoading && (
              <div className="payment-summary">
                <div className="payment-summary-row">
                  <span>Total Amount</span>
                  <span>${summary.totalAmount.toFixed(2)}</span>
                </div>
                <div className="payment-summary-row">
                  <span>Paid</span>
                  <span>${summary.paidAmount.toFixed(2)}</span>
                </div>
                <hr className="payment-summary-divider" />
                <div className="payment-summary-row highlight">
                  <span>Remaining</span>
                  <span>${summary.remainingAmount.toFixed(2)}</span>
                </div>
                <div style={{ marginTop: 4 }}>
                  <StatusBadge status={summary.paymentStatus} />
                </div>
              </div>
            )}

            <div className="form-group">
              <label className="form-label" htmlFor="pAmount">Amount ($)</label>
              <input id="pAmount" className="form-input" type="number" min="0.01" step="0.01"
                value={amount} onChange={(e) => setAmount(e.target.value)}
                required placeholder="0.00" />
            </div>

            <div className="form-group">
              <label className="form-label" htmlFor="pMethod">Payment Method</label>
              <select id="pMethod" className="form-select" value={method}
                onChange={(e) => setMethod(e.target.value)}>
                <option>Cash</option>
                <option>Credit Card</option>
                <option>Debit Card</option>
                <option>Bank Transfer</option>
                <option>Online</option>
              </select>
            </div>

            <button type="submit" className="btn btn-primary" disabled={saving} style={{ width: '100%' }}>
              <CreditCard size={16} />
              {saving ? 'Processing...' : 'Record Payment'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
