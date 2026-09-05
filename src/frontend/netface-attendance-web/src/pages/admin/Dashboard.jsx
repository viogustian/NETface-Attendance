import { useState, useEffect } from 'react';
import { getToken } from '../../utils/auth';

export default function Dashboard() {
  const [totalEmployees, setTotalEmployees] = useState('--');
  const [activeSessions, setActiveSessions] = useState('--');

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        const token = getToken();
        const headers = { 'Authorization': `Bearer ${token}` };

        // Fetch employees
        const empRes = await fetch('/api/employees', { headers });
        if (empRes.ok) {
          const empData = await empRes.json();
          setTotalEmployees(empData.length || 0);
        }

        // Fetch sessions
        const sessRes = await fetch('/api/attendance-sessions', { headers });
        if (sessRes.ok) {
          const sessData = await sessRes.json();
          const active = sessData.filter(s => s.status === 'Active').length;
          setActiveSessions(active || 0);
        }
      } catch (error) {
        console.error('Failed to fetch dashboard data:', error);
      }
    };

    fetchDashboardData();
  }, []);

  return (
    <div className="animate-fade-in">
      <h2 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '1.5rem', color: 'var(--galaxy-black)' }}>Dashboard</h2>
      <div className="metrics-grid">
        <div className="metric-card">
          <h3 className="metric-title">Total Employees</h3>
          <p className="metric-value" style={{ color: 'var(--primary-color)' }}>{totalEmployees}</p>
        </div>
        <div className="metric-card">
          <h3 className="metric-title">Active Sessions</h3>
          <p className="metric-value" style={{ color: 'var(--success-color)' }}>{activeSessions}</p>
        </div>
      </div>
    </div>
  );
}
