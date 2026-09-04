import { useState, useEffect } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useNavigate, Link } from 'react-router-dom';
import { ArrowLeft, CalendarPlus, Users } from 'lucide-react';
import AlertError from '../../../components/ui/AlertError';

const sessionSchema = z.object({
  departmentName: z.string().min(1, 'Department Name is required'),
  employeeIds: z.array(z.string()).min(1, 'Select at least one employee for the roster'),
});

export default function CreateSession() {
  const navigate = useNavigate();
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const { register, handleSubmit, control, formState: { errors, isSubmitting } } = useForm({
    resolver: zodResolver(sessionSchema),
    defaultValues: {
      employeeIds: []
    }
  });

  useEffect(() => {
    const fetchActiveEmployees = async () => {
      try {
        const token = sessionStorage.getItem('adminToken');
        const res = await fetch('/api/employees', {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!res.ok) throw new Error('Failed to load employees');
        const data = await res.json();
        // Filter only active employees for roster
        setEmployees(data.filter(e => e.status === 'Active'));
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };
    fetchActiveEmployees();
  }, []);

  const onSubmit = async (data) => {
    setError(null);
    try {
      const token = sessionStorage.getItem('adminToken');
      
      // Map employee IDs to the correct DTO format expected by the backend API:
      // { employeeId, employeeCode, employeeName }
      const mappedEmployees = data.employeeIds.map(id => {
        const emp = employees.find(e => e.id === id);
        return {
          employeeId: emp.id,
          employeeCode: emp.employeeCode,
          employeeName: emp.fullName
        };
      });
      
      const payload = {
        departmentName: data.departmentName,
        employees: mappedEmployees
      };

      const response = await fetch('/api/attendance-sessions', {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(payload)
      });
      
      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to create session');
      }
      
      const createdSession = await response.json();
      navigate(`/admin/sessions/${createdSession.id}`);
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div className="page-container animate-fade-in">
      <Link to="/admin/sessions" style={{ display: 'inline-flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1.5rem', color: 'var(--text-secondary)' }}>
        <ArrowLeft size={16} /> Back to Sessions
      </Link>
      
      <div style={{ marginBottom: '2rem' }}>
        <h1 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <CalendarPlus color="var(--primary-color)" /> Create Attendance Session
        </h1>
        <p style={{ color: 'var(--text-secondary)' }}>Start a new session for a specific department. Date will automatically follow server time.</p>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 350px', gap: '2rem' }}>
        <div className="glass-panel" style={{ padding: '2rem' }}>
          <AlertError message={error} />

          <form id="create-session-form" onSubmit={handleSubmit(onSubmit)}>
            <div className="form-group">
              <label className="form-label" htmlFor="departmentName">Department Name</label>
              <input 
                id="departmentName"
                className="input-field" 
                placeholder="e.g. Engineering"
                {...register('departmentName')} 
              />
              {errors.departmentName && <span className="error-text">{errors.departmentName.message}</span>}
            </div>

            <div style={{ marginTop: '2.5rem' }}>
              <button type="submit" className="btn-primary" disabled={isSubmitting || loading}>
                {isSubmitting ? 'Creating...' : 'Create Session'}
              </button>
            </div>
          </form>
        </div>

        {/* Roster Selection Sidebar */}
        <div className="glass-panel" style={{ padding: '1.5rem', height: 'fit-content' }}>
          <h3 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1rem' }}>
            <Users size={20} /> Select Roster
          </h3>
          <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
            Select the active employees expected to attend this session.
          </p>

          {loading ? (
            <div style={{ color: 'var(--text-secondary)', fontSize: '0.875rem', textAlign: 'center' }}>Loading employees...</div>
          ) : employees.length === 0 ? (
            <div style={{ color: 'var(--text-secondary)', fontSize: '0.875rem', textAlign: 'center' }}>No active employees found.</div>
          ) : (
            <div style={{ maxHeight: '400px', overflowY: 'auto', paddingRight: '0.5rem' }}>
              <Controller
                name="employeeIds"
                control={control}
                render={({ field }) => (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                    {employees.map(emp => (
                      <label key={emp.id} style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', padding: '0.5rem', background: 'rgba(255,255,255,0.02)', borderRadius: '8px', cursor: 'pointer' }}>
                        <input
                          type="checkbox"
                          value={emp.id}
                          style={{ accentColor: 'var(--primary-color)', width: '1.2rem', height: '1.2rem' }}
                          onChange={(e) => {
                            const value = e.target.value;
                            const newIds = e.target.checked 
                              ? [...field.value, value]
                              : field.value.filter(id => id !== value);
                            field.onChange(newIds);
                          }}
                          checked={field.value.includes(emp.id)}
                        />
                        <div>
                          <div style={{ fontWeight: '500', fontSize: '0.9rem' }}>{emp.fullName}</div>
                          <div style={{ color: 'var(--text-secondary)', fontSize: '0.75rem' }}>{emp.employeeCode}</div>
                        </div>
                      </label>
                    ))}
                  </div>
                )}
              />
              {errors.employeeIds && <span className="error-text" style={{ marginTop: '1rem' }}>{errors.employeeIds.message}</span>}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
