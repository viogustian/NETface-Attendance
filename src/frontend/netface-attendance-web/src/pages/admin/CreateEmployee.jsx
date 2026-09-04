import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useNavigate, Link } from 'react-router-dom';
import { ArrowLeft, UserPlus } from 'lucide-react';
import AlertError from '../../components/ui/AlertError';

const employeeSchema = z.object({
  employeeCode: z.string().min(1, 'Employee Code is required').max(20, 'Employee Code is too long'),
  fullName: z.string().min(1, 'Full Name is required').max(100, 'Full Name is too long'),
  isAdmin: z.boolean().default(false),
});

export default function CreateEmployee() {
  const navigate = useNavigate();
  const [error, setError] = useState(null);

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm({
    resolver: zodResolver(employeeSchema),
    defaultValues: {
      isAdmin: false
    }
  });

  const onSubmit = async (data) => {
    setError(null);
    try {
      const token = sessionStorage.getItem('adminToken');
      const response = await fetch('/api/employees', {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(data)
      });
      
      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || 'Failed to create employee');
      }
      
      navigate('/admin/employees');
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div className="page-container animate-fade-in">
      <Link to="/admin/employees" style={{ display: 'inline-flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1.5rem', color: 'var(--text-secondary)' }}>
        <ArrowLeft size={16} /> Back to Employees
      </Link>
      
      <div style={{ marginBottom: '2rem' }}>
        <h1 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <UserPlus color="var(--primary-color)" /> Add New Employee
        </h1>
        <p style={{ color: 'var(--text-secondary)' }}>Register a new employee for attendance tracking.</p>
      </div>

      <div className="glass-panel" style={{ maxWidth: '600px', padding: '2rem' }}>
        <AlertError message={error} />

        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="form-group">
            <label className="form-label" htmlFor="employeeCode">Employee Code</label>
            <input 
              id="employeeCode"
              className="input-field" 
              placeholder="e.g. EMP001"
              {...register('employeeCode')} 
            />
            {errors.employeeCode && <span className="error-text">{errors.employeeCode.message}</span>}
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="fullName">Full Name</label>
            <input 
              id="fullName"
              className="input-field" 
              placeholder="e.g. John Doe"
              {...register('fullName')} 
            />
            {errors.fullName && <span className="error-text">{errors.fullName.message}</span>}
          </div>

          <div className="form-group" style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginTop: '2rem' }}>
            <input 
              id="isAdmin"
              type="checkbox"
              style={{ width: '1.2rem', height: '1.2rem', accentColor: 'var(--primary-color)' }}
              {...register('isAdmin')} 
            />
            <label htmlFor="isAdmin" style={{ fontWeight: '500' }}>
              Grant Admin Privileges
              <span style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-secondary)', fontWeight: 'normal' }}>
                Allows the user to access the Admin Dashboard.
              </span>
            </label>
          </div>

          <div style={{ display: 'flex', gap: '1rem', marginTop: '2.5rem' }}>
            <button type="submit" className="btn-primary" disabled={isSubmitting}>
              {isSubmitting ? 'Creating...' : 'Create Employee'}
            </button>
            <Link to="/admin/employees" className="btn-secondary" style={{ display: 'inline-flex', alignItems: 'center' }}>
              Cancel
            </Link>
          </div>
        </form>
      </div>
    </div>
  );
}
