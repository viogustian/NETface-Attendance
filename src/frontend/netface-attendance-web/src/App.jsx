import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import AdminLayout from './components/AdminLayout';
import ProtectedRoute from './components/auth/ProtectedRoute';
import Login from './pages/admin/Login';
import Dashboard from './pages/admin/Dashboard';
import EmployeeList from './pages/admin/EmployeeList';
import CreateEmployee from './pages/admin/CreateEmployee';
import SessionList from './pages/admin/sessions/SessionList';
import CreateSession from './pages/admin/sessions/CreateSession';
import SessionDetail from './pages/admin/sessions/SessionDetail';
import KioskLayout from './components/layouts/KioskLayout';
import KioskHome from './pages/kiosk/KioskHome';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/admin" replace />} />
        
        {/* Admin Routes */}
        <Route path="/admin/login" element={<Login />} />
        <Route element={<ProtectedRoute />}>
          <Route path="/admin" element={<AdminLayout />}>
            <Route index element={<Dashboard />} />
            <Route path="employees" element={<EmployeeList />} />
            <Route path="employees/create" element={<CreateEmployee />} />
            <Route path="sessions" element={<SessionList />} />
            <Route path="sessions/create" element={<CreateSession />} />
            <Route path="sessions/:id" element={<SessionDetail />} />
          </Route>
        </Route>

        {/* Kiosk Routes */}
        <Route path="/kiosk" element={<KioskLayout />}>
          <Route index element={<KioskHome />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
