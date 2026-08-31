import './StatusBadge.css';

export default function StatusBadge({ status }) {
  const normalized = (status || '').toLowerCase().replace(/[\s-]/g, '');

  const labels = {
    pending: 'Pending',
    confirmed: 'Confirmed',
    checkedin: 'Checked In',
    checkedout: 'Checked Out',
    cancelled: 'Cancelled',
    paid: 'Paid',
    unpaid: 'Unpaid',
    partiallypaid: 'Partial',
    available: 'Available',
    maintenance: 'Under Maintenance',
  };

  return (
    <span className={`status-badge status-badge--${normalized}`}>
      {labels[normalized] || status}
    </span>
  );
}
