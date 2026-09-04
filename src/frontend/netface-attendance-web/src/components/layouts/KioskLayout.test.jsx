import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import KioskLayout from './KioskLayout';

describe('KioskLayout', () => {
  it('renders the kiosk header title and child outlet content', () => {
    render(
      <MemoryRouter initialEntries={['/kiosk']}>
        <Routes>
          <Route path="/kiosk" element={<KioskLayout />}>
            <Route index element={<div data-testid="kiosk-child">Child Screen Content</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByText(/NETFace Terminal/i)).toBeInTheDocument();
    expect(screen.getByTestId('kiosk-child')).toHaveTextContent('Child Screen Content');
  });
});
