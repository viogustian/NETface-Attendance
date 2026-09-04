import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import SessionDetail from './SessionDetail';

describe('SessionDetail Component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    global.fetch = vi.fn();
    sessionStorage.clear();
  });

  const mockSessionData = {
    id: 'test-session-id',
    departmentName: 'Engineering',
    date: '2026-09-04',
    status: 'Active',
    entries: [
      { id: '1', employeeId: 'emp1', employeeCode: 'E01', employeeName: 'Alice', status: 'Present' },
      { id: '2', employeeId: 'emp2', employeeCode: 'E02', employeeName: 'Bob', status: 'Absent' }
    ]
  };

  const renderComponent = () => {
    return render(
      <MemoryRouter initialEntries={['/admin/sessions/test-session-id']}>
        <Routes>
          <Route path="/admin/sessions/:id" element={<SessionDetail />} />
        </Routes>
      </MemoryRouter>
    );
  };

  it('fetches and displays real entry data from backend', async () => {
    sessionStorage.setItem('adminToken', 'fake-token');
    global.fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockSessionData
    });

    renderComponent();

    // Loading state initially
    expect(screen.getByText('Loading session details...')).toBeInTheDocument();

    // Should display department name and stats
    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Engineering Session' })).toBeInTheDocument();
    });

    expect(screen.getByText('Date: 2026-09-04')).toBeInTheDocument();
    expect(screen.getByText('Active')).toBeInTheDocument();

    // Stats
    expect(screen.getByText('Total Roster').previousSibling).toHaveTextContent('2');
    expect(screen.getAllByText('Present')[0].previousSibling).toHaveTextContent('1');
    expect(screen.getAllByText('Absent')[0].previousSibling).toHaveTextContent('1');

    // Table Data
    expect(screen.getByText('E01')).toBeInTheDocument();
    expect(screen.getByText('Alice')).toBeInTheDocument();
    expect(screen.getByText('E02')).toBeInTheDocument();
    expect(screen.getByText('Bob')).toBeInTheDocument();
  });
});
