import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import CreateSession from './CreateSession';
import { describe, it, expect, vi, beforeEach } from 'vitest';

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

describe('CreateSession Component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    global.fetch = vi.fn();
    sessionStorage.clear();
  });

  const mockEmployees = [
    { id: '1', employeeCode: 'EMP001', fullName: 'Alice Active', status: 'Active' },
    { id: '2', employeeCode: 'EMP002', fullName: 'Bob Inactive', status: 'Inactive' },
    { id: '3', employeeCode: 'EMP003', fullName: 'Charlie Active', status: 'Active' },
  ];

  it('renders form and fetches active employees only', async () => {
    sessionStorage.setItem('adminToken', 'fake-token');
    global.fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockEmployees
    });

    render(
      <MemoryRouter>
        <CreateSession />
      </MemoryRouter>
    );

    expect(screen.getByRole('heading', { name: /create attendance session/i })).toBeInTheDocument();
    
    // Wait for the fetch to resolve and employees to be rendered
    expect(await screen.findByText('Alice Active')).toBeInTheDocument();
    expect(screen.getByText('Charlie Active')).toBeInTheDocument();
    
    // Bob should not be in the document because he is Inactive
    expect(screen.queryByText('Bob Inactive')).not.toBeInTheDocument();
  });

  it('shows validation errors for empty fields', async () => {
    global.fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockEmployees
    });

    render(
      <MemoryRouter>
        <CreateSession />
      </MemoryRouter>
    );

    // Wait for data load
    await screen.findByText('Alice Active');

    // Clear the date input which has a default value
    const dateInput = screen.getByLabelText(/date/i);
    fireEvent.change(dateInput, { target: { value: '' } });

    fireEvent.click(screen.getByRole('button', { name: /create session/i }));

    expect(await screen.findByText('Department Name is required')).toBeInTheDocument();
    expect(await screen.findByText('Date is required')).toBeInTheDocument();
    expect(await screen.findByText('Select at least one employee for the roster')).toBeInTheDocument();
  });

  it('submits form successfully with correct API contract payload', async () => {
    sessionStorage.setItem('adminToken', 'fake-token');
    // First fetch is for employees
    global.fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockEmployees
    });

    // Second fetch is the POST submission
    global.fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ id: 'new-session-id' })
    });

    render(
      <MemoryRouter>
        <CreateSession />
      </MemoryRouter>
    );

    await screen.findByText('Alice Active');

    fireEvent.change(screen.getByLabelText(/department name/i), { target: { value: 'Engineering' } });
    fireEvent.change(screen.getByLabelText(/date/i), { target: { value: '2026-09-04' } });
    
    // Select Alice
    const checkbox = screen.getAllByRole('checkbox')[0];
    fireEvent.click(checkbox);
    
    fireEvent.click(screen.getByRole('button', { name: /create session/i }));

    await waitFor(() => {
      // Expect second call to be POST
      expect(global.fetch).toHaveBeenCalledTimes(2);
      expect(global.fetch).toHaveBeenNthCalledWith(2, '/api/attendance-sessions', expect.objectContaining({
        method: 'POST',
        headers: expect.objectContaining({
          'Content-Type': 'application/json',
          'Authorization': 'Bearer fake-token'
        }),
        body: JSON.stringify({
          departmentName: 'Engineering',
          date: '2026-09-04',
          employees: [
            {
              employeeId: '1',
              employeeCode: 'EMP001',
              employeeName: 'Alice Active'
            }
          ]
        })
      }));
      expect(mockNavigate).toHaveBeenCalledWith('/admin/sessions/new-session-id');
    });
  });

  it('handles server errors during fetch gracefully', async () => {
    global.fetch.mockResolvedValueOnce({
      ok: false,
      status: 500,
      json: async () => ({})
    });

    render(
      <MemoryRouter>
        <CreateSession />
      </MemoryRouter>
    );

    expect(await screen.findByText('Failed to load employees')).toBeInTheDocument();
  });

  it('handles server errors during submission gracefully', async () => {
    global.fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockEmployees
    });

    global.fetch.mockResolvedValueOnce({
      ok: false,
      status: 500,
      json: async () => ({ message: 'Something went wrong' })
    });

    render(
      <MemoryRouter>
        <CreateSession />
      </MemoryRouter>
    );

    await screen.findByText('Alice Active');

    fireEvent.change(screen.getByLabelText(/department name/i), { target: { value: 'Engineering' } });
    // Date already has default value
    const checkbox = screen.getAllByRole('checkbox')[0];
    fireEvent.click(checkbox);
    
    fireEvent.click(screen.getByRole('button', { name: /create session/i }));

    expect(await screen.findByText('Something went wrong')).toBeInTheDocument();
    expect(mockNavigate).not.toHaveBeenCalled();
  });
});
