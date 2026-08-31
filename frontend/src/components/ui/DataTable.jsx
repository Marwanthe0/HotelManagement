import { Inbox } from 'lucide-react';
import './DataTable.css';

export default function DataTable({ columns, data, loading, emptyMessage }) {
  if (loading) {
    return (
      <div className="data-table-wrapper">
        <div className="loader"><div className="spinner" /></div>
      </div>
    );
  }

  if (!data || data.length === 0) {
    return (
      <div className="data-table-wrapper">
        <div className="empty-state">
          <Inbox size={40} />
          <p>{emptyMessage || 'No records found.'}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="data-table-wrapper">
      <table className="data-table">
        <thead>
          <tr>
            {columns.map((col) => (
              <th key={col.key} style={col.style}>
                {col.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((row, i) => (
            <tr key={row.id ?? i}>
              {columns.map((col) => (
                <td key={col.key} style={col.style}>
                  {col.render ? col.render(row) : row[col.key]}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
