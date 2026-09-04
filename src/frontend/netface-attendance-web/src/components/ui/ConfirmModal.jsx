import { AlertTriangle } from 'lucide-react';

export default function ConfirmModal({ isOpen, title, message, onConfirm, onCancel, confirmText = 'Confirm', isDanger = false, isLoading = false }) {
  if (!isOpen) return null;

  return (
    <div style={{
      position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
      background: 'rgba(0,0,0,0.6)', backdropFilter: 'blur(4px)',
      display: 'flex', justifyContent: 'center', alignItems: 'center',
      zIndex: 100
    }}>
      <div className="glass-panel animate-fade-in" style={{ padding: '2rem', width: '400px', maxWidth: '90%' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '1rem' }}>
          <AlertTriangle size={24} color={isDanger ? 'var(--danger-color)' : 'var(--warning-color)'} />
          <h3 style={{ margin: 0 }}>{title}</h3>
        </div>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem' }}>{message}</p>
        
        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '1rem' }}>
          <button 
            className="btn-secondary" 
            onClick={onCancel}
            disabled={isLoading}
          >
            Cancel
          </button>
          <button 
            className="btn-primary" 
            onClick={onConfirm}
            disabled={isLoading}
            style={{ 
              background: isDanger ? 'var(--danger-color)' : undefined, 
              boxShadow: isDanger ? '0 4px 12px rgba(239,68,68,0.3)' : undefined 
            }}
          >
            {isLoading ? 'Processing...' : confirmText}
          </button>
        </div>
      </div>
    </div>
  );
}
