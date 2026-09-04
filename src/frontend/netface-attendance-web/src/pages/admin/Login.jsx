import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useNavigate } from 'react-router-dom';
import { KeyRound } from 'lucide-react';
import AlertError from '../../components/ui/AlertError';
import { setToken } from '../../utils/auth';

const loginSchema = z.object({
  employeeCode: z.string().min(1, 'Employee Code is required'),
  password: z.string().min(1, 'Password is required'),
});

export default function Login() {
  const navigate = useNavigate();
  const [error, setError] = useState(null);
  
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm({
    resolver: zodResolver(loginSchema)
  });

  const onSubmit = async (data) => {
    setError(null);
    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
      });
      
      if (!response.ok) {
        let errorMessage = 'Invalid credentials';
        try {
          const errData = await response.json();
          if (errData && errData.message) {
            errorMessage = errData.message;
          }
        } catch {
          // fallback if response is not json
        }
        throw new Error(errorMessage);
      }
      
      const result = await response.json();
      setToken(result.token);
      navigate('/admin');
    } catch (err) {
      setError(err.message || 'An error occurred during login');
    }
  };

  return (
    <div className="page-container" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh' }}>
      <div className="glass-panel animate-fade-in" style={{ width: '100%', maxWidth: '400px', padding: '2rem' }}>
        <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
          <KeyRound size={48} color="var(--primary-color)" style={{ marginBottom: '1rem' }} />
          <h2>Admin Login</h2>
          <p style={{ color: 'var(--text-secondary)' }}>Sign in to NETFace Admin</p>
        </div>

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
            <label className="form-label" htmlFor="password">Password</label>
            <input 
              id="password"
              type="password"
              className="input-field" 
              placeholder="••••••••"
              {...register('password')} 
            />
            {errors.password && <span className="error-text">{errors.password.message}</span>}
          </div>

          <button type="submit" className="btn-primary" style={{ width: '100%', marginTop: '1rem' }} disabled={isSubmitting}>
            {isSubmitting ? 'Signing in...' : 'Sign In'}
          </button>
        </form>
      </div>
    </div>
  );
}
