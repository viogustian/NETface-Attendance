import { Navigate, Outlet, NavLink } from 'react-router-dom';
import { Users, LayoutDashboard, Calendar, Settings, LogOut } from 'lucide-react';

export default function AdminLayout() {
  const token = sessionStorage.getItem('adminToken');

  if (!token) {
    return <Navigate to="/admin/login" replace />;
  }

  const handleLogout = () => {
    sessionStorage.removeItem('adminToken');
    window.location.href = '/admin/login';
  };

  return (
    <div className="admin-layout">
      {/* Sidebar */}
      <aside className="sidebar">
        <div className="sidebar-header">
          <h2 className="sidebar-logo">NETFace</h2>
        </div>
        <nav className="sidebar-nav">
          <NavLink to="/admin" end className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <LayoutDashboard />
            Dashboard
          </NavLink>
          <NavLink to="/admin/employees" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <Users />
            Employees
          </NavLink>
          <NavLink to="/admin/sessions" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <Calendar />
            Sessions
          </NavLink>
          <NavLink to="/admin/settings" className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}>
            <Settings />
            Settings
          </NavLink>
        </nav>
        <div className="sidebar-footer">
          <button 
            onClick={handleLogout}
            className="btn-secondary"
            style={{ width: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '0.5rem' }}
          >
            <LogOut size={16} /> Logout
          </button>
        </div>
      </aside>

      {/* Main Content */}
      <main className="main-content">
        <header className="topbar">
          <h1>NETFace Attendance Management</h1>
        </header>
        <div className="content-area">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
