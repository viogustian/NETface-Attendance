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
});
