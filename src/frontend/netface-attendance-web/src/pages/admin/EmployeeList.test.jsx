import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { vi } from 'vitest';
import EmployeeList from './EmployeeList';
import * as authUtils from '../../utils/auth';

describe('EmployeeList Component', () => {
  beforeEach(() => {
    global.fetch = vi.fn();
    vi.spyOn(authUtils, 'getToken').mockReturnValue('mock-admin-token');
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('renders skeleton loaders during fetch without plain text loading message', () => {
    global.fetch.mockImplementation(() => new Promise(() => {})); // Never resolves
    render(
      <MemoryRouter>
        <EmployeeList />
      </MemoryRouter>
    );

    const skeletons = screen.getAllByTestId('skeleton-loader');
    expect(skeletons.length).toBeGreaterThan(0);
    expect(screen.queryByText(/loading employees\.\.\./i)).not.toBeInTheDocument();
  });

  it('fetches employees with auth token and renders data in a table', async () => {
    const mockEmployees = [
      { id: '1', employeeCode: 'EMP001', fullName: 'John Doe', status: 'Active', isAdmin: false },
      { id: '2', employeeCode: 'EMP002', fullName: 'Jane Admin', status: 'Inactive', isAdmin: true },
    ];

    global.fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockEmployees,
    });

    render(
      <MemoryRouter>
        <EmployeeList />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText('EMP001')).toBeInTheDocument();
      expect(screen.getByText('John Doe')).toBeInTheDocument();
      expect(screen.getByText('EMP002')).toBeInTheDocument();
      expect(screen.getByText('Jane Admin')).toBeInTheDocument();
    });

    expect(global.fetch).toHaveBeenCalledWith('/api/employees', {
      headers: {
        'Authorization': 'Bearer mock-admin-token',
      },
    });

    expect(screen.getByText('Active')).toBeInTheDocument();
    expect(screen.getByText('Inactive')).toBeInTheDocument();
    expect(screen.getByText('Employee')).toBeInTheDocument();
    expect(screen.getByText('Admin')).toBeInTheDocument();
  });

  it('renders empty state illustration and CTA when employee list is empty', async () => {
    global.fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => [],
    });

    render(
      <MemoryRouter>
        <EmployeeList />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/no employees found/i)).toBeInTheDocument();
      expect(screen.getByText(/get started by adding your first employee/i)).toBeInTheDocument();
    });

    const addButtons = screen.getAllByRole('link', { name: /add employee/i });
    expect(addButtons.length).toBeGreaterThanOrEqual(1);
    expect(addButtons.some((btn) => btn.getAttribute('href') === '/admin/employees/create')).toBe(true);
  });

  it('renders error message when fetch fails', async () => {
    global.fetch.mockResolvedValueOnce({
      ok: false,
      status: 500,
      json: async () => ({ message: 'Server error' }),
    });

    render(
      <MemoryRouter>
        <EmployeeList />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/failed to fetch employees/i)).toBeInTheDocument();
    });
  });
});
