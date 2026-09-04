import { AlertCircle } from 'lucide-react';

export default function AlertError({ message }) {
  if (!message) return null;

  return (
    <div style={{ 
      background: 'rgba(239, 68, 68, 0.1)', 
      borderLeft: '4px solid var(--danger-color)', 
      padding: '0.75rem', 
      marginBottom: '1.5rem', 
      borderRadius: '4px',
      display: 'flex',
      alignItems: 'center',
      gap: '0.5rem'
    }}>
      <AlertCircle size={18} color="var(--danger-color)" />
      <p style={{ color: 'var(--danger-color)', fontSize: '0.875rem', margin: 0 }}>{message}</p>
    </div>
  );
}
