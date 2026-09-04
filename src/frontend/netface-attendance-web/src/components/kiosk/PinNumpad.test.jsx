import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import PinNumpad from './PinNumpad';

describe('PinNumpad Component', () => {
  it('renders Enter PIN header', () => {
    render(<PinNumpad onSubmit={vi.fn()} onCancel={vi.fn()} />);
    expect(screen.getByText('Enter PIN')).toBeInTheDocument();
  });

  it('allows digit entry and enables OK button when PIN length >= 4', () => {
    render(<PinNumpad onSubmit={vi.fn()} onCancel={vi.fn()} />);
    
    const okBtn = screen.getByRole('button', { name: /OK/i });
    expect(okBtn).toBeDisabled();

    fireEvent.click(screen.getByText('1'));
    fireEvent.click(screen.getByText('2'));
    fireEvent.click(screen.getByText('3'));
    
    expect(okBtn).toBeDisabled();
    
    fireEvent.click(screen.getByText('4'));
    expect(okBtn).not.toBeDisabled();
  });

  it('shows Not Available message and calls onSubmit with the entered PIN', () => {
    const mockOnSubmit = vi.fn();
    render(<PinNumpad onSubmit={mockOnSubmit} onCancel={vi.fn()} />);
    
    fireEvent.click(screen.getByText('9'));
    fireEvent.click(screen.getByText('8'));
    fireEvent.click(screen.getByText('7'));
    fireEvent.click(screen.getByText('6'));
    
    fireEvent.click(screen.getByRole('button', { name: /OK/i }));
    
    expect(screen.getByText('PIN Fallback is Not Available yet.')).toBeInTheDocument();
    expect(mockOnSubmit).toHaveBeenCalledWith('9876');
  });

  it('calls onCancel when the cancel button is clicked', () => {
    const mockOnCancel = vi.fn();
    render(<PinNumpad onSubmit={vi.fn()} onCancel={mockOnCancel} />);
    
    // The cancel button is the first button in the component
    const buttons = screen.getAllByRole('button');
    const cancelBtn = buttons[0]; 
    
    fireEvent.click(cancelBtn);
    expect(mockOnCancel).toHaveBeenCalledTimes(1);
  });
});
