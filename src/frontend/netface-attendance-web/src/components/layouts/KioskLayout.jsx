import { Outlet } from 'react-router-dom';

export default function KioskLayout() {
  return (
    <div
      style={{
        position: 'relative',
        width: '100vw',
        height: '100vh',
        overflow: 'hidden',
        background: 'var(--bg-color, #0f172a)',
        color: '#f8fafc',
        fontFamily: 'inherit'
      }}
    >
      {/* Top Header Overlay */}
      <header
        style={{
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          zIndex: 10,
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          padding: '1.5rem 2rem',
          background: 'linear-gradient(to bottom, rgba(15, 23, 42, 0.85) 0%, rgba(15, 23, 42, 0) 100%)',
          pointerEvents: 'none'
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <div
            style={{
              width: '12px',
              height: '12px',
              borderRadius: '50%',
              background: '#38bdf8',
              boxShadow: '0 0 12px #38bdf8'
            }}
          />
          <h1
            style={{
              fontSize: '1.25rem',
              fontWeight: '700',
              letterSpacing: '0.05em',
              textTransform: 'uppercase',
              margin: 0,
              color: '#ffffff',
              textShadow: '0 2px 8px rgba(0,0,0,0.6)'
            }}
          >
            NETFace Terminal
          </h1>
        </div>

        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '0.5rem',
            background: 'rgba(255, 255, 255, 0.08)',
            padding: '0.4rem 0.85rem',
            borderRadius: '9999px',
            border: '1px solid rgba(255, 255, 255, 0.12)',
            backdropFilter: 'blur(8px)',
            fontSize: '0.85rem',
            fontWeight: '500',
            color: '#e2e8f0'
          }}
        >
          <span>Walk-up Kiosk</span>
        </div>
      </header>

      {/* Main Outlet Container */}
      <main
        style={{
          position: 'relative',
          width: '100%',
          height: '100%'
        }}
      >
        <Outlet />
      </main>
    </div>
  );
}
