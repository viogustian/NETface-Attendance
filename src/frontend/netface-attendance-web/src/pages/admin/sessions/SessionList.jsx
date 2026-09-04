import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Calendar, Plus, ExternalLink } from 'lucide-react';

export default function SessionList() {
  const [sessions, setSessions] = useState([]);
  const [loading, setLoading] = useState(true);

  // Using Mock Data as specified in ADR 0004 since GET /api/attendance-sessions is pending
  useEffect(() => {
    const fetchMockSessions = async () => {
      // Simulate network delay
      await new Promise(resolve => setTimeout(resolve, 800));
      
      setSessions([
        { id: 1, departmentName: 'Engineering', date: '2026-09-04', status: 'Active', totalEmployees: 12, presentCount: 8 },
        { id: 2, departmentName: 'Marketing', date: '2026-09-03', status: 'Finalized', totalEmployees: 5, presentCount: 5 },
        { id: 3, departmentName: 'Sales', date: '2026-09-02', status: 'Finalized', totalEmployees: 8, presentCount: 7 },
      ]);
      setLoading(false);
    };

    fetchMockSessions();
  }, []);

  return (
    <div className="page-container animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
        <div>
          <h1 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <Calendar color="var(--primary-color)" /> Attendance Sessions
          </h1>
          <p style={{ color: 'var(--text-secondary)' }}>Manage and monitor attendance sessions.</p>
        </div>
        <Link to="/admin/sessions/create" className="btn-primary" style={{ display: 'inline-flex', alignItems: 'center', gap: '0.5rem' }}>
          <Plus size={18} /> Create Session
        </Link>
      </div>

      <div className="glass-panel" style={{ overflow: 'hidden' }}>
        {loading ? (
          <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
            Loading sessions...
          </div>
        ) : sessions.length === 0 ? (
          <div style={{ padding: '4rem 2rem', textAlign: 'center' }}>
            <Calendar size={48} color="var(--text-secondary)" style={{ marginBottom: '1rem', opacity: 0.5 }} />
            <h3>No sessions found</h3>
            <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>Start by creating a new attendance session.</p>
            <Link to="/admin/sessions/create" className="btn-secondary" style={{ display: 'inline-block' }}>
              Create Session
            </Link>
          </div>
        ) : (
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
            <thead>
              <tr style={{ borderBottom: '1px solid var(--surface-border)', background: 'rgba(255, 255, 255, 0.02)' }}>
                <th style={{ padding: '1rem' }}>Department</th>
                <th style={{ padding: '1rem' }}>Date</th>
                <th style={{ padding: '1rem' }}>Status</th>
                <th style={{ padding: '1rem' }}>Attendance</th>
                <th style={{ padding: '1rem' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {sessions.map((session) => (
                <tr key={session.id} style={{ borderBottom: '1px solid var(--surface-border)' }}>
                  <td style={{ padding: '1rem', fontWeight: '500' }}>{session.departmentName}</td>
                  <td style={{ padding: '1rem' }}>{session.date}</td>
                  <td style={{ padding: '1rem' }}>
                    <span style={{ 
                      padding: '0.25rem 0.5rem', 
                      borderRadius: '12px', 
                      fontSize: '0.75rem', 
                      fontWeight: '500',
                      background: session.status === 'Active' ? 'rgba(59, 130, 246, 0.1)' : 'rgba(148, 163, 184, 0.1)',
                      color: session.status === 'Active' ? 'var(--primary-color)' : 'var(--text-secondary)'
                    }}>
                      {session.status}
                    </span>
                  </td>
                  <td style={{ padding: '1rem' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                      <span style={{ fontWeight: '500' }}>{session.presentCount} / {session.totalEmployees}</span>
                      <span style={{ color: 'var(--text-secondary)', fontSize: '0.875rem' }}>
                        ({Math.round((session.presentCount / session.totalEmployees) * 100)}%)
                      </span>
                    </div>
                  </td>
                  <td style={{ padding: '1rem' }}>
                    <Link 
                      to={`/admin/sessions/${session.id}`} 
                      style={{ display: 'inline-flex', alignItems: 'center', gap: '0.25rem', fontSize: '0.875rem', fontWeight: '500' }}
                    >
                      View <ExternalLink size={14} />
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
