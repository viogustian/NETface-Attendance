import { render, screen } from '@testing-library/react';
import Skeleton from './Skeleton';

describe('Skeleton Component', () => {
  it('renders skeleton loader with default styles', () => {
    render(<Skeleton />);
    const skeleton = screen.getByTestId('skeleton-loader');
    expect(skeleton).toBeInTheDocument();
    expect(skeleton).toHaveClass('skeleton');
    expect(skeleton).toHaveStyle({ width: '100%', height: '1rem' });
  });

  it('accepts custom width, height, and borderRadius', () => {
    render(<Skeleton width="200px" height="2rem" borderRadius="8px" />);
    const skeleton = screen.getByTestId('skeleton-loader');
    expect(skeleton).toHaveStyle({
      width: '200px',
      height: '2rem',
      borderRadius: '8px',
    });
  });

  it('supports circle variant', () => {
    render(<Skeleton circle width="40px" height="40px" />);
    const skeleton = screen.getByTestId('skeleton-loader');
    expect(skeleton).toHaveStyle({
      borderRadius: '50%',
      width: '40px',
      height: '40px',
    });
  });

  it('merges custom className and style', () => {
    render(
      <Skeleton
        className="custom-class"
        style={{ marginTop: '10px' }}
      />
    );
    const skeleton = screen.getByTestId('skeleton-loader');
    expect(skeleton).toHaveClass('skeleton');
    expect(skeleton).toHaveClass('custom-class');
    expect(skeleton).toHaveStyle({ marginTop: '10px' });
  });
});
