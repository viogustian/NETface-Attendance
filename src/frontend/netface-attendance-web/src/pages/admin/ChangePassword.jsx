import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useNavigate } from 'react-router-dom';
import { KeyRound } from 'lucide-react';
import AlertError from '../../components/ui/AlertError';
import { setToken, getToken } from '../../utils/auth';

const changePasswordSchema = z.object({
  newPassword: z.string().min(6, 'Password must be at least 6 characters'),
  confirmPassword: z.string()
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: "Passwords don't match",
  path: ["confirmPassword"]
});

export default function ChangePassword() {
  const navigate = useNavigate();
  const [error, setError] = useState(null);
  
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm({
    resolver: zodResolver(changePasswordSchema)
  });

  const onSubmit = async (data) => {
    setError(null);
    try {
      const response = await fetch('/api/auth/change-password', {
        method: 'POST',
        headers: { 
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${getToken()}`
        },
        body: JSON.stringify({ newPassword: data.newPassword })
      });
      
      if (!response.ok) {
        let errorMessage = 'Failed to change password';
        try {
          const errData = await response.json();
          if (errData && errData.message) {
            errorMessage = errData.message;
          }
        } catch {
          // fallback
        }
        throw new Error(errorMessage);
      }
      
      const result = await response.json();
      setToken(result.token); // update with the new token
      navigate('/admin');
    } catch (err) {
      setError(err.message || 'An error occurred while changing password');
    }
  };

  return (
    <div className="page-container" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh', backgroundColor: '#fcfcfc' }}>
      <div className="card animate-fade-in" style={{ width: '100%', maxWidth: '400px', padding: '2.5rem' }}>
        <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
          <KeyRound size={48} color="var(--primary-color)" style={{ marginBottom: '1rem' }} />
          <h2 style={{ color: 'var(--galaxy-black)', fontSize: '1.5rem', fontWeight: '700' }}>Change Password</h2>
          <p style={{ color: 'var(--text-secondary)', marginTop: '0.5rem' }}>Please set a secure password for your account</p>
        </div>

        <AlertError message={error} />

        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="form-group">
            <label className="form-label" htmlFor="newPassword">New Password</label>
            <input 
              id="newPassword"
              type="password"
              className="input-field" 
              placeholder="••••••••"
              {...register('newPassword')} 
            />
            {errors.newPassword && <span className="error-text">{errors.newPassword.message}</span>}
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="confirmPassword">Confirm Password</label>
            <input 
              id="confirmPassword"
              type="password"
              className="input-field" 
              placeholder="••••••••"
              {...register('confirmPassword')} 
            />
            {errors.confirmPassword && <span className="error-text">{errors.confirmPassword.message}</span>}
          </div>

          <button type="submit" className="btn-primary" style={{ width: '100%', marginTop: '1rem' }} disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : 'Set Password'}
          </button>
        </form>
      </div>
    </div>
  );
}
