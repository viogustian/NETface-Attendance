import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Calendar, Plus, ExternalLink, Trash2 } from 'lucide-react';

export default function SessionList() {
  const [sessions, setSessions] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchSessions = async () => {
    try {
      const token = sessionStorage.getItem('adminToken');
      const res = await fetch('/api/attendance-sessions', {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      if (!res.ok) throw new Error('Failed to load sessions');
      const data = await res.json();
      
      const formattedSessions = data.map(session => {
        const entries = session.entries || [];
        const presentCount = entries.filter(e => e.status === 'Present').length;
        const totalEmployees = entries.length;
        return {
          ...session,
          presentCount,
          totalEmployees
        };
      });
      
      setSessions(formattedSessions);
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const [dateFilter, setDateFilter] = useState('');

  useEffect(() => {
    fetchSessions();
  }, []);

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this session?')) return;
    
    try {
      const token = sessionStorage.getItem('adminToken');
      const res = await fetch(`/api/attendance-sessions/${id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      
      if (res.ok) {
        setSessions(prev => prev.filter(s => s.id !== id));
      } else {
        alert('Failed to delete session');
      }
    } catch (error) {
      console.error(error);
      alert('Error deleting session');
    }
  };

  const filteredSessions = dateFilter ? sessions.filter(s => s.date === dateFilter) : sessions;

  return (
    <div className="page-container animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
        <div>
          <h1 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <Calendar color="var(--primary-color)" /> Attendance Sessions
          </h1>
          <p style={{ color: 'var(--text-secondary)' }}>Manage and monitor attendance sessions.</p>
        </div>
        <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
          <input 
            type="date" 
            className="input-field" 
            style={{ width: 'auto', padding: '0.5rem 1rem' }}
            value={dateFilter}
            onChange={(e) => setDateFilter(e.target.value)}
          />
          <Link to="/admin/sessions/create" className="btn-primary" style={{ display: 'inline-flex', alignItems: 'center', gap: '0.5rem' }}>
            <Plus size={18} /> Create Session
          </Link>
        </div>
      </div>

      <div className="glass-panel" style={{ overflow: 'hidden' }}>
        {loading ? (
          <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
            Loading sessions...
          </div>
        ) : filteredSessions.length === 0 ? (
          <div style={{ padding: '4rem 2rem', textAlign: 'center' }}>
            <Calendar size={48} color="var(--text-secondary)" style={{ marginBottom: '1rem', opacity: 0.5 }} />
            <h3>No sessions found</h3>
            <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>Start by creating a new attendance session or change your filter.</p>
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
              {filteredSessions.map((session) => (
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
                        ({session.totalEmployees > 0 ? Math.round((session.presentCount / session.totalEmployees) * 100) : 0}%)
                      </span>
                    </div>
                  </td>
                  <td style={{ padding: '1rem' }}>
                    <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
                      <Link 
                        to={`/admin/sessions/${session.id}`} 
                        style={{ display: 'inline-flex', alignItems: 'center', gap: '0.25rem', fontSize: '0.875rem', fontWeight: '500', color: 'var(--primary-color)' }}
                      >
                        View <ExternalLink size={14} />
                      </Link>
                      <button 
                        onClick={() => handleDelete(session.id)}
                        style={{ display: 'inline-flex', alignItems: 'center', gap: '0.25rem', fontSize: '0.875rem', fontWeight: '500', color: 'var(--danger-color)', background: 'none', border: 'none', cursor: 'pointer', padding: 0 }}
                      >
                        Delete <Trash2 size={14} />
                      </button>
                    </div>
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
