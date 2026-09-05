import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Calendar, Plus } from 'lucide-react';

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
          <h1 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--galaxy-black)', fontSize: '1.5rem', fontWeight: '700' }}>
            <Calendar color="var(--primary-color)" /> Attendance Sessions
          </h1>
          <p style={{ color: 'var(--text-secondary)', marginTop: '0.25rem' }}>Manage and monitor attendance sessions.</p>
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

      <div className="data-table-container">
        {loading ? (
          <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
            Loading sessions...
          </div>
        ) : filteredSessions.length === 0 ? (
          <div style={{ padding: '4rem 2rem', textAlign: 'center' }}>
            <Calendar size={48} color="var(--text-secondary)" style={{ marginBottom: '1rem', opacity: 0.5, margin: '0 auto' }} />
            <h3 style={{ color: 'var(--galaxy-black)', fontSize: '1.25rem', marginBottom: '0.5rem' }}>No sessions found</h3>
            <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>Start by creating a new attendance session or change your filter.</p>
            <Link to="/admin/sessions/create" className="btn-secondary" style={{ display: 'inline-block' }}>
              Create Session
            </Link>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Department</th>
                <th>Date</th>
                <th>Status</th>
                <th>Attendance</th>
                <th style={{ textAlign: 'right' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredSessions.map((session) => (
                <tr key={session.id}>
                  <td style={{ fontWeight: '500', color: 'var(--galaxy-black)' }}>{session.departmentName}</td>
                  <td style={{ color: 'var(--galaxy-black)' }}>{session.date}</td>
                  <td>
                    <span className={`badge ${
                      session.status === 'Active' ? 'badge-success' : 
                      session.status === 'Cancelled' ? 'badge-danger' : ''
                    }`} style={
                      (session.status !== 'Active' && session.status !== 'Cancelled') ? { background: 'rgba(148, 163, 184, 0.1)', color: 'var(--text-secondary)' } : {}
                    }>
                      {session.status}
                    </span>
                  </td>
                  <td>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--galaxy-black)' }}>
                      <span style={{ fontWeight: '500' }}>{session.presentCount} / {session.totalEmployees}</span>
                      <span style={{ color: 'var(--text-secondary)', fontSize: '0.875rem' }}>
                        ({session.totalEmployees > 0 ? Math.round((session.presentCount / session.totalEmployees) * 100) : 0}%)
                      </span>
                    </div>
                  </td>
                  <td style={{ textAlign: 'right' }}>
                    <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
                      <Link 
                        to={`/admin/sessions/${session.id}`} 
                        className="btn-secondary"
                        style={{ padding: '0.35rem 0.75rem', fontSize: '0.85rem' }}
                      >
                        View
                      </Link>
                      <button 
                        onClick={() => handleDelete(session.id)}
                        className="btn-danger"
                        style={{ padding: '0.35rem 0.75rem', fontSize: '0.85rem' }}
                      >
                        Delete
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
