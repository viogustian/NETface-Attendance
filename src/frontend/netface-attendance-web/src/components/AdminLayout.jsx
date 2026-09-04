import { Navigate, Outlet, Link } from 'react-router-dom';
import { Users, LayoutDashboard, Calendar } from 'lucide-react';

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
    <div className="flex h-screen bg-gray-100">
      {/* Sidebar */}
      <aside className="w-64 bg-slate-900 text-white flex flex-col">
        <div className="p-6">
          <h2 className="text-2xl font-bold text-white">Admin Panel</h2>
        </div>
        <nav className="flex-1 px-4 space-y-2">
          <Link to="/admin" className="flex items-center px-4 py-3 text-gray-300 rounded-lg hover:bg-slate-800 hover:text-white transition-colors">
            <LayoutDashboard className="w-5 h-5 mr-3" />
            Dashboard
          </Link>
          <Link to="/admin/employees" className="flex items-center px-4 py-3 text-gray-300 rounded-lg hover:bg-slate-800 hover:text-white transition-colors">
            <Users className="w-5 h-5 mr-3" />
            Employees
          </Link>
          <Link to="/admin/sessions" className="flex items-center px-4 py-3 text-gray-300 rounded-lg hover:bg-slate-800 hover:text-white transition-colors">
            <Calendar className="w-5 h-5 mr-3" />
            Sessions
          </Link>
        </nav>
        <div className="p-4">
          <button 
            onClick={handleLogout}
            className="w-full px-4 py-2 text-sm font-medium text-slate-900 bg-white rounded-lg hover:bg-gray-100"
          >
            Logout
          </button>
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 overflow-y-auto bg-gray-50">
        <header className="bg-white shadow-sm border-b px-8 py-4">
          <h1 className="text-xl font-semibold text-gray-800">NETFace Attendance Management</h1>
        </header>
        <div className="p-8">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
