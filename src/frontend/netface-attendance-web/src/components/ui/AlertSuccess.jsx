import { CheckCircle2 } from 'lucide-react';

export default function AlertSuccess({ message }) {
  if (!message) return null;

  return (
    <div style={{ 
      background: 'rgba(16, 185, 129, 0.1)', 
      borderLeft: '4px solid var(--success-color)', 
      padding: '0.75rem', 
      marginBottom: '1.5rem', 
      borderRadius: '4px',
      display: 'flex',
      alignItems: 'center',
      gap: '0.5rem'
    }}>
      <CheckCircle2 size={18} color="var(--success-color)" />
      <p style={{ color: 'var(--success-color)', fontSize: '0.875rem', margin: 0 }}>{message}</p>
    </div>
  );
}
