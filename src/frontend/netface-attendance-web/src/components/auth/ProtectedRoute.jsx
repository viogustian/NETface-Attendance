import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { isAuthenticated, requiresPasswordChange } from '../../utils/auth';

const ProtectedRoute = ({ children }) => {
  const location = useLocation();

  if (!isAuthenticated()) {
    return <Navigate to="/admin/login" replace />;
  }

  if (requiresPasswordChange() && location.pathname !== '/admin/change-password') {
    return <Navigate to="/admin/change-password" replace />;
  }

  return children ? children : <Outlet />;
};

export default ProtectedRoute;
