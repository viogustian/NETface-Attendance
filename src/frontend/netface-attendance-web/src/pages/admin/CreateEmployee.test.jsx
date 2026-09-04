import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import CreateEmployee from './CreateEmployee';
import { describe, it, expect, vi, beforeEach } from 'vitest';

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

describe('CreateEmployee Component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    global.fetch = vi.fn();
    sessionStorage.clear();
  });

  it('renders form elements correctly', () => {
    render(
      <MemoryRouter>
        <CreateEmployee />
      </MemoryRouter>
    );

    expect(screen.getByRole('heading', { name: /add new employee/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/employee code/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/full name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/grant admin privileges/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /create employee/i })).toBeInTheDocument();
  });

  it('shows validation errors for empty fields', async () => {
    render(
      <MemoryRouter>
        <CreateEmployee />
      </MemoryRouter>
    );

    fireEvent.click(screen.getByRole('button', { name: /create employee/i }));

    expect(await screen.findByText('Employee Code is required')).toBeInTheDocument();
    expect(await screen.findByText('Full Name is required')).toBeInTheDocument();
  });

  it('submits form successfully', async () => {
    sessionStorage.setItem('adminToken', 'fake-token');
    global.fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ id: 1 })
    });

    render(
      <MemoryRouter>
        <CreateEmployee />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText(/employee code/i), { target: { value: 'EMP003' } });
    fireEvent.change(screen.getByLabelText(/full name/i), { target: { value: 'New User' } });
    fireEvent.click(screen.getByLabelText(/grant admin privileges/i));
    
    fireEvent.click(screen.getByRole('button', { name: /create employee/i }));

    await waitFor(() => {
      expect(global.fetch).toHaveBeenCalledWith('/api/employees', expect.objectContaining({
        method: 'POST',
        headers: expect.objectContaining({
          'Authorization': 'Bearer fake-token'
        })
      }));
      expect(mockNavigate).toHaveBeenCalledWith('/admin/employees');
    });
  });

  it('shows validation errors when fields contain only whitespace', async () => {
    render(
      <MemoryRouter>
        <CreateEmployee />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText(/employee code/i), { target: { value: '   ' } });
    fireEvent.change(screen.getByLabelText(/full name/i), { target: { value: '   ' } });
    fireEvent.click(screen.getByRole('button', { name: /create employee/i }));

    expect(await screen.findByText('Employee Code is required')).toBeInTheDocument();
    expect(await screen.findByText('Full Name is required')).toBeInTheDocument();
    expect(global.fetch).not.toHaveBeenCalled();
  });

  it('handles 23505 duplicate employee code error gracefully in the UI', async () => {
    sessionStorage.setItem('adminToken', 'fake-token');
    global.fetch.mockResolvedValueOnce({
      ok: false,
      status: 409,
      json: async () => ({ message: "Employee code 'EMP001' already exists." })
    });

    render(
      <MemoryRouter>
        <CreateEmployee />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText(/employee code/i), { target: { value: 'EMP001' } });
    fireEvent.change(screen.getByLabelText(/full name/i), { target: { value: 'Jane Doe' } });
    fireEvent.click(screen.getByRole('button', { name: /create employee/i }));

    expect(await screen.findByText("Employee code 'EMP001' already exists.")).toBeInTheDocument();
    expect(mockNavigate).not.toHaveBeenCalled();
    expect(screen.getByRole('button', { name: /create employee/i })).not.toBeDisabled();
  });

  it('handles server 500 error gracefully without crashing', async () => {
    sessionStorage.setItem('adminToken', 'fake-token');
    global.fetch.mockResolvedValueOnce({
      ok: false,
      status: 500,
      json: async () => ({})
    });

    render(
      <MemoryRouter>
        <CreateEmployee />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText(/employee code/i), { target: { value: 'EMP002' } });
    fireEvent.change(screen.getByLabelText(/full name/i), { target: { value: 'Jane Doe' } });
    fireEvent.click(screen.getByRole('button', { name: /create employee/i }));

    expect(await screen.findByText(/failed to create employee/i)).toBeInTheDocument();
    expect(mockNavigate).not.toHaveBeenCalled();
  });

  it('shows validation errors when fields exceed max length', async () => {
    render(
      <MemoryRouter>
        <CreateEmployee />
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText(/employee code/i), { 
      target: { value: 'A'.repeat(21) } 
    });
    fireEvent.change(screen.getByLabelText(/full name/i), { 
      target: { value: 'B'.repeat(101) } 
    });
    fireEvent.click(screen.getByRole('button', { name: /create employee/i }));

    expect(await screen.findByText('Employee Code is too long')).toBeInTheDocument();
    expect(await screen.findByText('Full Name is too long')).toBeInTheDocument();
    expect(global.fetch).not.toHaveBeenCalled();
  });
});

