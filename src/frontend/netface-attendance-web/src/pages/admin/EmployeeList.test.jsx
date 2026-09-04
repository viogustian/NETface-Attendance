import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { vi } from 'vitest';
import EmployeeList from './EmployeeList';

describe('EmployeeList Component', () => {
  beforeEach(() => {
    global.fetch = vi.fn();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('renders loading state initially', () => {
    global.fetch.mockImplementation(() => new Promise(() => {})); // Never resolves
    render(
      <MemoryRouter>
        <EmployeeList />
      </MemoryRouter>
    );
    expect(screen.getByText(/loading employees/i)).toBeInTheDocument();
  });

  it('renders employee data in a table', async () => {
    const mockEmployees = [
      { id: '1', employeeCode: 'EMP001', fullName: 'John Doe', status: 'Active', isAdmin: false },
      { id: '2', employeeCode: 'EMP002', fullName: 'Jane Admin', status: 'Active', isAdmin: true },
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
  });

  it('renders empty state when no employees', async () => {
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
    });
  });
});
