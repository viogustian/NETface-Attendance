import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { ArrowLeft, CheckCircle, XCircle, Clock } from 'lucide-react';
import ConfirmModal from '../../../components/ui/ConfirmModal';

export default function SessionDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [session, setSession] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [actionLoading, setActionLoading] = useState(false);
  const [confirmModal, setConfirmModal] = useState({ isOpen: false, action: null });

  useEffect(() => {
    const fetchSession = async () => {
      try {
        const token = sessionStorage.getItem('adminToken');
        const res = await fetch(`/api/attendance-sessions/${id}`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!res.ok) throw new Error('Failed to load session details');
        const data = await res.json();
        setSession(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };
    fetchSession();
  }, [id]);

  const requestAction = (action) => {
    setConfirmModal({ isOpen: true, action });
  };

  const handleAction = async () => {
    const action = confirmModal.action;
    setConfirmModal({ isOpen: false, action: null });
    
    setActionLoading(true);
    try {
      const token = sessionStorage.getItem('adminToken');
      const res = await fetch(`/api/attendance-sessions/${id}/${action}`, {
        method: 'PUT',
        headers: { 'Authorization': `Bearer ${token}` }
      });
      
      if (!res.ok) throw new Error(`Failed to ${action} session`);
      
      // Navigate back to list or reload
      navigate('/admin/sessions');
    } catch (err) {
      alert(err.message);
    } finally {
      setActionLoading(false);
    }
  };

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
          <h1 style={{ marginBottom: '0.5rem' }}>{session.departmentName} Session</h1>
          <div style={{ display: 'flex', gap: '1rem', color: 'var(--text-secondary)' }}>
            <span>Date: {session.date}</span>
            <span>Status: 
              <strong style={{ 
                color: session.status === 'Active' ? 'var(--primary-color)' : 
                       session.status === 'Cancelled' ? 'var(--danger-color)' : 'var(--success-color)',
                marginLeft: '0.25rem'
              }}>
                {session.status}
              </strong>
            </span>
          </div>
        </div>

        {session.status === 'Active' && (
          <div style={{ display: 'flex', gap: '1rem' }}>
            <button 
              className="btn-secondary" 
              style={{ color: 'var(--danger-color)', borderColor: 'var(--danger-color)' }}
              onClick={() => requestAction('cancel')}
              disabled={actionLoading}
            >
              Cancel Session
            </button>
            <button 
              className="btn-primary" 
              onClick={() => requestAction('finalize')}
              disabled={actionLoading}
            >
              Finalize Session
            </button>
          </div>
        )}
      </div>

      <ConfirmModal 
        isOpen={confirmModal.isOpen}
        title={confirmModal.action === 'cancel' ? 'Cancel Session' : 'Finalize Session'}
        message={`Are you sure you want to ${confirmModal.action} this session? This action cannot be undone.`}
        isDanger={confirmModal.action === 'cancel'}
        confirmText={confirmModal.action === 'cancel' ? 'Cancel Session' : 'Finalize'}
        onConfirm={handleAction}
        onCancel={() => setConfirmModal({ isOpen: false, action: null })}
      />

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '1.5rem', marginBottom: '2rem' }}>
        <div className="glass-panel" style={{ padding: '1.5rem', textAlign: 'center' }}>
          <div style={{ fontSize: '2rem', fontWeight: 'bold', color: 'var(--primary-color)' }}>{totalCount}</div>
          <div style={{ color: 'var(--text-secondary)' }}>Total Roster</div>
        </div>
        <div className="glass-panel" style={{ padding: '1.5rem', textAlign: 'center' }}>
          <div style={{ fontSize: '2rem', fontWeight: 'bold', color: 'var(--success-color)' }}>{presentCount}</div>
          <div style={{ color: 'var(--text-secondary)' }}>Present</div>
        </div>
        <div className="glass-panel" style={{ padding: '1.5rem', textAlign: 'center' }}>
          <div style={{ fontSize: '2rem', fontWeight: 'bold', color: 'var(--danger-color)' }}>{totalCount - presentCount}</div>
          <div style={{ color: 'var(--text-secondary)' }}>Absent</div>
        </div>
      </div>

      <div className="glass-panel" style={{ overflow: 'hidden' }}>
        <h3 style={{ padding: '1.5rem', borderBottom: '1px solid var(--surface-border)' }}>Attendance Entries</h3>
        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
          <thead>
            <tr style={{ borderBottom: '1px solid var(--surface-border)', background: 'rgba(255, 255, 255, 0.02)' }}>
              <th style={{ padding: '1rem' }}>Employee Code</th>
              <th style={{ padding: '1rem' }}>Name</th>
              <th style={{ padding: '1rem' }}>Status</th>
              <th style={{ padding: '1rem' }}>Marked At</th>
            </tr>
          </thead>
          <tbody>
            {session.entries?.map((entry) => (
              <tr key={entry.id || entry.employeeId} style={{ borderBottom: '1px solid var(--surface-border)' }}>
                <td style={{ padding: '1rem', fontWeight: '500' }}>{entry.employeeCode}</td>
                <td style={{ padding: '1rem' }}>{entry.employeeName}</td>
                <td style={{ padding: '1rem' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem',
                    color: entry.status === 'Present' ? 'var(--success-color)' : 'var(--text-secondary)'
                  }}>
                    {entry.status === 'Present' ? <CheckCircle size={16} /> : <Clock size={16} />}
                    {entry.status}
                  </div>
                </td>
                <td style={{ padding: '1rem', color: 'var(--text-secondary)', fontSize: '0.875rem' }}>
                  {entry.markedAt ? new Date(entry.markedAt).toLocaleString() : '-'}
                </td>
              </tr>
            ))}
            {(!session.entries || session.entries.length === 0) && (
              <tr>
                <td colSpan="4" style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
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
