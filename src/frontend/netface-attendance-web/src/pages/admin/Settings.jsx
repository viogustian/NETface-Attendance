import { useState, useEffect } from 'react';
import { Settings as SettingsIcon, Save } from 'lucide-react';
import AlertError from '../../components/ui/AlertError';

export default function Settings() {
  const [settings, setSettings] = useState({ ClockOutStartTime: '12:00:00' });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [successMsg, setSuccessMsg] = useState(null);

  useEffect(() => {
    const fetchSettings = async () => {
      try {
        const token = sessionStorage.getItem('adminToken');
        const res = await fetch('/api/settings', {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!res.ok) throw new Error('Failed to load settings');
        const data = await res.json();
        setSettings(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };
    fetchSettings();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setError(null);
    setSuccessMsg(null);

    try {
      const token = sessionStorage.getItem('adminToken');
      const res = await fetch('/api/settings', {
        method: 'PUT',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(settings)
      });
      if (!res.ok) throw new Error('Failed to save settings');
      setSuccessMsg('Settings saved successfully.');
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  };

  const handleChange = (key, value) => {
    // If value is missing seconds, append it (time input might return HH:mm)
    let formattedValue = value;
    if (value && value.split(':').length === 2) {
        formattedValue = value + ':00';
    }
    setSettings(prev => ({ ...prev, [key]: formattedValue }));
  };

  if (loading) {
    return <div className="page-container">Loading settings...</div>;
  }

  return (
    <div className="page-container animate-fade-in">
      <div style={{ marginBottom: '2rem' }}>
        <h1 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <SettingsIcon color="var(--primary-color)" /> System Settings
        </h1>
        <p style={{ color: 'var(--text-secondary)' }}>Configure global application settings.</p>
      </div>

      <div className="glass-panel" style={{ padding: '2rem', maxWidth: '600px' }}>
        <AlertError message={error} />
        {successMsg && (
          <div style={{ background: 'rgba(34, 197, 94, 0.1)', color: 'var(--success-color)', padding: '1rem', borderRadius: '8px', marginBottom: '1.5rem', border: '1px solid rgba(34, 197, 94, 0.2)' }}>
            {successMsg}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label" htmlFor="ClockOutStartTime">
              Shift Boundary Time (Clock Out Start Time)
            </label>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.875rem', marginBottom: '0.5rem' }}>
              Time of day where any attendance scan will be considered a Clock Out instead of a Clock In. Format is HH:mm.
            </p>
            <input 
              id="ClockOutStartTime"
              type="time"
              className="input-field" 
              value={settings.ClockOutStartTime || ''}
              onChange={(e) => handleChange('ClockOutStartTime', e.target.value)}
              required
            />
          </div>

          <div style={{ marginTop: '2rem' }}>
            <button type="submit" className="btn-primary" disabled={saving} style={{ display: 'inline-flex', alignItems: 'center', gap: '0.5rem' }}>
              <Save size={18} />
              {saving ? 'Saving...' : 'Save Settings'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
