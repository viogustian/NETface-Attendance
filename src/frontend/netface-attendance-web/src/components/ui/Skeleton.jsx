export default function Skeleton({
  width = '100%',
  height = '1rem',
  borderRadius = '4px',
  circle = false,
  className = '',
  style = {},
  ...props
}) {
  const computedStyle = {
    width,
    height,
    borderRadius: circle ? '50%' : borderRadius,
    display: 'inline-block',
    ...style,
  };

  return (
    <div
      className={`skeleton ${className}`.trim()}
      style={computedStyle}
      data-testid="skeleton-loader"
      aria-hidden="true"
      {...props}
    />
  );
}
