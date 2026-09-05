import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ArrowLeft, CheckCircle, Clock } from 'lucide-react';

export default function SessionDetail() {
  const { id } = useParams();
  const [session, setSession] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    let intervalId;
    
    const fetchSession = async () => {
      try {
        const token = sessionStorage.getItem('adminToken');
        const res = await fetch(`/api/attendance-sessions/${id}`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!res.ok) throw new Error('Failed to load session details');
        const data = await res.json();
        setSession(data);
        
        // If session is active, setup polling every 3 seconds
        if (data.status === 'Active' && !intervalId) {
            intervalId = setInterval(fetchSession, 3000);
        } else if (data.status !== 'Active' && intervalId) {
            clearInterval(intervalId);
        }
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };
    
    fetchSession();

    return () => {
      if (intervalId) clearInterval(intervalId);
    };
  }, [id]);


  if (loading) {
    return <div className="page-container">Loading session details...</div>;
  }

  if (error || !session) {
    return (
      <div className="page-container">
        <div style={{ color: 'var(--danger-color)' }}>{error || 'Session not found'}</div>
        <Link to="/admin/sessions" style={{ color: 'var(--primary-color)', marginTop: '1rem', display: 'block' }}>Return to Sessions</Link>
      </div>
    );
  }

  const presentCount = session.entries?.filter(e => e.status === 'Present').length || 0;
  const totalCount = session.entries?.length || 0;

  return (
    <div className="page-container animate-fade-in">
      <Link to="/admin/sessions" style={{ display: 'inline-flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1.5rem', color: 'var(--text-secondary)' }}>
        <ArrowLeft size={16} /> Back to Sessions
      </Link>

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '2rem' }}>
        <div>
          <h1 style={{ marginBottom: '0.5rem', color: 'var(--galaxy-black)', fontSize: '1.5rem', fontWeight: '700' }}>{session.departmentName} Session</h1>
          <div style={{ display: 'flex', gap: '1rem', color: 'var(--text-secondary)' }}>
            <span>Date: {session.date}</span>
            <span style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>Status: 
              <span className={`badge ${
                session.status === 'Active' ? 'badge-success' : 
                session.status === 'Cancelled' ? 'badge-danger' : ''
              }`} style={
                (session.status !== 'Active' && session.status !== 'Cancelled') ? { background: 'rgba(148, 163, 184, 0.1)', color: 'var(--text-secondary)' } : {}
              }>
                {session.status}
              </span>
            </span>
          </div>
        </div>
        <div>
          <button 
            className="btn-primary" 
            onClick={async () => {
              try {
                const token = sessionStorage.getItem('adminToken');
                const res = await fetch(`/api/attendance-sessions/${id}/export`, {
                  headers: { 'Authorization': `Bearer ${token}` }
                });
                if (!res.ok) throw new Error('Failed to export CSV');
                const blob = await res.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `${session.departmentName.replace(/\s+/g, '-')}-session-${session.date}.csv`;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                a.remove();
              } catch (err) {
                alert(err.message);
              }
            }}
          >
            Download CSV
          </button>
        </div>
      </div>

      <div className="metrics-grid" style={{ marginBottom: '2rem' }}>
        <div className="metric-card" style={{ textAlign: 'center' }}>
          <div className="metric-value">{totalCount}</div>
          <div className="metric-title">Total Roster</div>
        </div>
        <div className="metric-card" style={{ textAlign: 'center' }}>
          <div className="metric-value" style={{ color: 'var(--success-color)' }}>{presentCount}</div>
          <div className="metric-title">Present</div>
        </div>
        <div className="metric-card" style={{ textAlign: 'center' }}>
          <div className="metric-value" style={{ color: 'var(--danger-color)' }}>{totalCount - presentCount}</div>
          <div className="metric-title">Absent</div>
        </div>
      </div>

      <div className="data-table-container">
        <h3 style={{ padding: '1.5rem', borderBottom: '1px solid var(--surface-border)', color: 'var(--galaxy-black)' }}>Attendance Entries</h3>
        <table className="data-table">
          <thead>
            <tr>
              <th>Employee Code</th>
              <th>Name</th>
              <th>Status</th>
              <th>Clock In</th>
              <th>Clock Out</th>
              <th>Total Hours</th>
            </tr>
          </thead>
          <tbody>
            {session.entries?.map((entry) => (
              <tr key={entry.id || entry.employeeId}>
                <td style={{ fontWeight: '500', color: 'var(--galaxy-black)' }}>{entry.employeeCode}</td>
                <td style={{ color: 'var(--galaxy-black)' }}>{entry.employeeName}</td>
                <td>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem',
                    color: entry.status === 'Present' ? 'var(--success-color)' : 'var(--text-secondary)',
                    fontWeight: '500'
                  }}>
                    {entry.status === 'Present' ? <CheckCircle size={16} /> : <Clock size={16} />}
                    {entry.status}
                  </div>
                </td>
                <td style={{ color: 'var(--galaxy-black)' }}>
                  {entry.clockInTime ? new Date(entry.clockInTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '-'}
                </td>
                <td style={{ color: 'var(--galaxy-black)' }}>
                  {entry.clockOutTime ? new Date(entry.clockOutTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '-'}
                </td>
                <td style={{ fontWeight: 'bold', color: 'var(--galaxy-black)' }}>
                  {entry.totalWorkHours ? entry.totalWorkHours.toFixed(2) : '-'}
                </td>
              </tr>
            ))}
            {(!session.entries || session.entries.length === 0) && (
              <tr>
                <td colSpan="6" style={{ padding: '4rem 2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
                  No entries found in this session.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
