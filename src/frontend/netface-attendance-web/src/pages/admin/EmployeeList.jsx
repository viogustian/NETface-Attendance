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
    <div className="animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
        <div>
          <h1 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--galaxy-black)', fontSize: '1.5rem', fontWeight: '700' }}>
            <Users color="var(--primary-color)" /> Employees
          </h1>
          <p style={{ color: 'var(--text-secondary)', marginTop: '0.25rem' }}>Manage your organization's employees.</p>
        </div>
        <Link to="/admin/employees/create" className="btn-primary" style={{ display: 'inline-flex', alignItems: 'center', gap: '0.5rem' }}>
          <Plus size={18} /> Add Employee
        </Link>
      </div>

      <AlertError message={error} />

      <div className="data-table-container">
        {loading ? (
          <table className="data-table">
            <thead>
              <tr>
                <th>Code</th>
                <th>Full Name</th>
                <th>Status</th>
                <th>Role</th>
                <th style={{ textAlign: 'right' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {[...Array(5)].map((_, idx) => (
                <tr key={idx}>
                  <td><Skeleton width="80px" height="1.2rem" /></td>
                  <td><Skeleton width="180px" height="1.2rem" /></td>
                  <td><Skeleton width="60px" height="1.5rem" borderRadius="12px" /></td>
                  <td><Skeleton width="70px" height="1.2rem" /></td>
                  <td style={{ textAlign: 'right' }}>
                    <Skeleton width="100px" height="1.8rem" borderRadius="4px" style={{ marginLeft: 'auto' }} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : employees.length === 0 ? (
          <div style={{ padding: '4rem 2rem', textAlign: 'center' }} data-testid="empty-state">
            <Users size={48} color="var(--text-secondary)" style={{ marginBottom: '1rem', opacity: 0.5, margin: '0 auto' }} />
            <h3 style={{ color: 'var(--galaxy-black)', fontSize: '1.25rem', marginBottom: '0.5rem' }}>No employees found</h3>
            <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>Get started by adding your first employee.</p>
            <Link to="/admin/employees/create" className="btn-secondary" style={{ display: 'inline-block' }}>
              Add Employee
            </Link>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Code</th>
                <th>Full Name</th>
                <th>Status</th>
                <th>Role</th>
                <th style={{ textAlign: 'right' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {employees.map((emp) => (
                <tr key={emp.id}>
                  <td style={{ fontWeight: '500', color: 'var(--galaxy-black)' }}>{emp.employeeCode}</td>
                  <td style={{ color: 'var(--galaxy-black)' }}>{emp.fullName}</td>
                  <td>
                    <span className={`badge ${emp.status === 'Active' ? 'badge-success' : 'badge-danger'}`}>
                      {emp.status}
                    </span>
                  </td>
                  <td style={{ color: 'var(--text-secondary)' }}>
                    {emp.isAdmin ? 'Admin' : 'Employee'}
                  </td>
                  <td style={{ textAlign: 'right' }}>
                    <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
                      <Link to={`/admin/employees/${emp.id}/faces`} className="btn-secondary" style={{ padding: '0.35rem 0.75rem', fontSize: '0.85rem' }}>
                        Faces ({emp.enrolledFacesCount}/5)
                      </Link>
                      <button 
                        onClick={() => handleDelete(emp.id)} 
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
