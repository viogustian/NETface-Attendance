import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Users, Plus } from 'lucide-react';
import AlertError from '../../components/ui/AlertError';
import Skeleton from '../../components/ui/Skeleton';
import { getToken } from '../../utils/auth';

export default function EmployeeList() {
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchEmployees = async () => {
      try {
        const token = getToken();
        const res = await fetch('/api/employees', {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });
        if (!res.ok) throw new Error('Failed to fetch employees');
        const data = await res.json();
        setEmployees(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchEmployees();
  }, []);

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this employee? This will permanently remove their data and face embeddings.')) return;
    
    try {
      const token = getToken();
      const res = await fetch(`/api/employees/${id}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
      
      if (!res.ok) throw new Error('Failed to delete employee');
      setEmployees(prev => prev.filter(e => e.id !== id));
    } catch (err) {
      alert(err.message);
    }
  };

  return (
    <div className="page-container animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
        <div>
          <h1 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <Users color="var(--primary-color)" /> Employees
          </h1>
          <p style={{ color: 'var(--text-secondary)' }}>Manage your organization's employees.</p>
        </div>
        <Link to="/admin/employees/create" className="btn-primary" style={{ display: 'inline-flex', alignItems: 'center', gap: '0.5rem' }}>
          <Plus size={18} /> Add Employee
        </Link>
      </div>

      <AlertError message={error} />

      <div className="glass-panel" style={{ overflow: 'hidden' }}>
        {loading ? (
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
            <thead>
              <tr style={{ borderBottom: '1px solid var(--surface-border)', background: 'rgba(255, 255, 255, 0.02)' }}>
                <th style={{ padding: '1rem' }}>Code</th>
                <th style={{ padding: '1rem' }}>Full Name</th>
                <th style={{ padding: '1rem' }}>Status</th>
                <th style={{ padding: '1rem' }}>Role</th>
                <th style={{ padding: '1rem', textAlign: 'right' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {[...Array(5)].map((_, idx) => (
                <tr key={idx} style={{ borderBottom: '1px solid var(--surface-border)' }}>
                  <td style={{ padding: '1rem' }}>
                    <Skeleton width="80px" height="1.2rem" />
                  </td>
                  <td style={{ padding: '1rem' }}>
                    <Skeleton width="180px" height="1.2rem" />
                  </td>
                  <td style={{ padding: '1rem' }}>
                    <Skeleton width="60px" height="1.2rem" borderRadius="12px" />
                  </td>
                  <td style={{ padding: '1rem' }}>
                    <Skeleton width="70px" height="1.2rem" />
                  </td>
                  <td style={{ padding: '1rem', textAlign: 'right' }}>
                    <Skeleton width="100px" height="1.8rem" borderRadius="4px" style={{ marginLeft: 'auto' }} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : employees.length === 0 ? (
          <div style={{ padding: '4rem 2rem', textAlign: 'center' }} data-testid="empty-state">
            <Users size={48} color="var(--text-secondary)" style={{ marginBottom: '1rem', opacity: 0.5 }} />
            <h3>No employees found</h3>
            <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>Get started by adding your first employee.</p>
            <Link to="/admin/employees/create" className="btn-secondary" style={{ display: 'inline-block' }}>
              Add Employee
            </Link>
          </div>
        ) : (
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
            <thead>
              <tr style={{ borderBottom: '1px solid var(--surface-border)', background: 'rgba(255, 255, 255, 0.02)' }}>
                <th style={{ padding: '1rem' }}>Code</th>
                <th style={{ padding: '1rem' }}>Full Name</th>
                <th style={{ padding: '1rem' }}>Status</th>
                <th style={{ padding: '1rem' }}>Role</th>
                <th style={{ padding: '1rem', textAlign: 'right' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {employees.map((emp) => (
                <tr key={emp.id} style={{ borderBottom: '1px solid var(--surface-border)' }}>
                  <td style={{ padding: '1rem', fontWeight: '500' }}>{emp.employeeCode}</td>
                  <td style={{ padding: '1rem' }}>{emp.fullName}</td>
                  <td style={{ padding: '1rem' }}>
                    <span style={{ 
                      padding: '0.25rem 0.5rem', 
                      borderRadius: '12px', 
                      fontSize: '0.75rem', 
                      fontWeight: '500',
                      background: emp.status === 'Active' ? 'rgba(16, 185, 129, 0.1)' : 'rgba(239, 68, 68, 0.1)',
                      color: emp.status === 'Active' ? 'var(--success-color)' : 'var(--danger-color)'
                    }}>
                      {emp.status}
                    </span>
                  </td>
                  <td style={{ padding: '1rem', color: 'var(--text-secondary)' }}>
                    {emp.isAdmin ? 'Admin' : 'Employee'}
                  </td>
                  <td style={{ padding: '1rem', textAlign: 'right' }}>
                    <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
                      <Link to={`/admin/employees/${emp.id}/faces`} className="btn-secondary" style={{ padding: '0.25rem 0.75rem', fontSize: '0.85rem' }}>
                        Face Enrollment ({emp.enrolledFacesCount}/5)
                      </Link>
                      <button 
                        onClick={() => handleDelete(emp.id)} 
                        className="btn-danger" 
                        style={{ padding: '0.25rem 0.75rem', fontSize: '0.85rem', background: 'var(--danger-color)', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
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
