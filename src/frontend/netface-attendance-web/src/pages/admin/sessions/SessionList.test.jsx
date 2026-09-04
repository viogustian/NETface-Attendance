import { render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { describe, it, expect } from 'vitest';
import SessionList from './SessionList';

describe('SessionList Component', () => {
  it('renders loading state initially and then shows mock sessions', async () => {
    render(
      <MemoryRouter>
        <SessionList />
      </MemoryRouter>
    );

    // Should show loading state initially
    expect(screen.getByText('Loading sessions...')).toBeInTheDocument();

    // After loading, it should display the mock data
    await waitFor(() => {
      expect(screen.getByText('Engineering')).toBeInTheDocument();
    }, { timeout: 1500 });

    expect(screen.getByText('Marketing')).toBeInTheDocument();
    expect(screen.getByText('Sales')).toBeInTheDocument();

    // Check status badges
    expect(screen.getByText('Active')).toBeInTheDocument();
    expect(screen.getAllByText('Finalized')).toHaveLength(2);

    // Check attendance formatting (8 / 12 for Engineering)
    expect(screen.getByText('8 / 12')).toBeInTheDocument();
  });
});
