import { useState } from 'react';
import { Delete, X } from 'lucide-react';

export default function PinNumpad({ onSubmit, onCancel }) {
  const [pin, setPin] = useState('');
  const [message, setMessage] = useState('');

  const handleDigit = (digit) => {
    if (pin.length < 6) {
      setPin(prev => prev + digit);
    }
  };

  const handleDelete = () => {
    setPin(prev => prev.slice(0, -1));
  };

  const handleSubmit = () => {
    if (pin.length >= 4) {
      // Mock static message as specified in ADR 0004
      setMessage('PIN Fallback is Not Available yet.');
      onSubmit(pin);
    }
  };

  const numpadDigits = [1, 2, 3, 4, 5, 6, 7, 8, 9];

  return (
    <div style={{
      position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
      background: 'rgba(0,0,0,0.8)', backdropFilter: 'blur(8px)',
      display: 'flex', justifyContent: 'center', alignItems: 'center',
      zIndex: 50
    }}>
      <div className="glass-panel animate-fade-in" style={{ padding: '2rem', width: '350px', position: 'relative' }}>
        <button 
          onClick={onCancel}
          style={{ position: 'absolute', top: '1rem', right: '1rem', background: 'transparent', color: 'var(--text-secondary)' }}
        >
          <X size={24} />
        </button>

        <h3 style={{ textAlign: 'center', marginBottom: '1rem' }}>Enter PIN</h3>
        
        <div style={{ 
          background: 'rgba(0,0,0,0.3)', 
          padding: '1rem', 
          borderRadius: '8px', 
          textAlign: 'center',
          fontSize: '1.5rem',
          letterSpacing: '0.5rem',
          minHeight: '60px',
          marginBottom: '1.5rem',
          color: 'white'
        }}>
          {pin.padEnd(6, '•').substring(0, pin.length ? pin.length : 6).split('').map((char, i) => (
             <span key={i} style={{ opacity: i < pin.length ? 1 : 0.2 }}>{char}</span>
          ))}
        </div>

        {message && (
          <div style={{ color: 'var(--warning-color)', textAlign: 'center', marginBottom: '1rem', fontSize: '0.875rem' }}>
            {message}
          </div>
        )}

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '0.75rem', marginBottom: '1.5rem' }}>
          {numpadDigits.map(d => (
            <button 
              key={d} 
              onClick={() => handleDigit(d.toString())}
              className="btn-secondary"
              style={{ padding: '1rem', fontSize: '1.25rem', borderRadius: '50%', aspectRatio: '1/1' }}
            >
              {d}
            </button>
          ))}
          <button 
            onClick={handleDelete}
            className="btn-secondary"
            style={{ padding: '1rem', fontSize: '1.25rem', borderRadius: '50%', aspectRatio: '1/1', background: 'rgba(239, 68, 68, 0.2)', color: 'var(--danger-color)' }}
          >
            <Delete size={24} style={{ margin: 'auto' }} />
          </button>
          <button 
            onClick={() => handleDigit('0')}
            className="btn-secondary"
            style={{ padding: '1rem', fontSize: '1.25rem', borderRadius: '50%', aspectRatio: '1/1' }}
          >
            0
          </button>
          <button 
            onClick={handleSubmit}
            className="btn-primary"
            disabled={pin.length < 4}
            style={{ padding: '1rem', fontSize: '1rem', borderRadius: '50%', aspectRatio: '1/1' }}
          >
            OK
          </button>
        </div>
      </div>
    </div>
  );
}
