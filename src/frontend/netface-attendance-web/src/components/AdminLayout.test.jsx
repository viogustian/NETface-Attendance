import { render, screen } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import AdminLayout from './AdminLayout';

describe('AdminLayout Component', () => {
  afterEach(() => {
    sessionStorage.clear();
  });

  it('redirects to login if not authenticated', () => {
    render(
      <MemoryRouter initialEntries={['/admin']}>
        <Routes>
          <Route path="/admin" element={<AdminLayout />}>
            <Route index element={<div>Dashboard</div>} />
          </Route>
          <Route path="/admin/login" element={<div>Login Page</div>} />
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText('Login Page')).toBeInTheDocument();
    expect(screen.queryByText('Dashboard')).not.toBeInTheDocument();
  });

  it('renders children if authenticated', () => {
    sessionStorage.setItem('adminToken', 'valid-token');

    render(
      <MemoryRouter initialEntries={['/admin']}>
        <Routes>
          <Route path="/admin" element={<AdminLayout />}>
            <Route index element={<div>Dashboard Content</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText('Dashboard Content')).toBeInTheDocument();
  });
});
