export default function Dashboard() {
  return (
    <div>
      <h2 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '1.5rem', color: 'var(--galaxy-black)' }}>Dashboard</h2>
      <div className="metrics-grid">
        <div className="metric-card">
          <h3 className="metric-title">Total Employees</h3>
          <p className="metric-value">--</p>
        </div>
        <div className="metric-card">
          <h3 className="metric-title">Active Sessions</h3>
          <p className="metric-value">--</p>
        </div>
      </div>
    </div>
  );
}
