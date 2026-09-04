import { useState, useEffect, useRef } from 'react';
import { Camera, CheckCircle, AlertCircle } from 'lucide-react';
import PinNumpad from '../../components/kiosk/PinNumpad';
import { calculateMotion } from '../../utils/motionDetection';

export default function KioskApp() {
  const videoRef = useRef(null);
  const canvasRef = useRef(null);
  const previousFrameRef = useRef(null);
  
  const [streamActive, setStreamActive] = useState(false);
  const [showPin, setShowPin] = useState(false);
  const [status, setStatus] = useState('standby'); // standby, capturing, success, error
  const [statusMessage, setStatusMessage] = useState('');
  const [employeeName, setEmployeeName] = useState('');

  // Start webcam
  useEffect(() => {
    let stream = null;
    const startCamera = async () => {
      try {
        stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: false });
        if (videoRef.current) {
          videoRef.current.srcObject = stream;
          setStreamActive(true);
        }
      } catch (err) {
        setStatus('error');
        setStatusMessage('Camera access denied or unavailable.');
      }
    };
    startCamera();

    return () => {
      if (stream) {
        stream.getTracks().forEach(track => track.stop());
      }
    };
  }, []);

  // Motion Detection Loop
  useEffect(() => {
    if (!streamActive || status !== 'standby') return;

    const interval = setInterval(() => {
      if (!videoRef.current || !canvasRef.current) return;
      
      const ctx = canvasRef.current.getContext('2d');
      // Draw scaled down video frame to canvas for faster processing
      ctx.drawImage(videoRef.current, 0, 0, 64, 48);
      const currentFrame = ctx.getImageData(0, 0, 64, 48).data;

      if (previousFrameRef.current) {
        const motion = calculateMotion(currentFrame, previousFrameRef.current, 45);
        // If more than 5% of pixels changed, trigger capture
        if (motion > 0.05) {
          handleCapture();
        }
      }
      
      previousFrameRef.current = currentFrame;
    }, 1000); // Check every second

    return () => clearInterval(interval);
  }, [streamActive, status]);

  const handleCapture = async () => {
    setStatus('capturing');
    setStatusMessage('Motion detected. Processing...');

    // Capture high-res frame for backend
    const captureCanvas = document.createElement('canvas');
    captureCanvas.width = videoRef.current.videoWidth;
    captureCanvas.height = videoRef.current.videoHeight;
    const ctx = captureCanvas.getContext('2d');
    ctx.drawImage(videoRef.current, 0, 0);
    const base64Image = captureCanvas.toDataURL('image/jpeg');

    try {
      const response = await fetch('/api/recognition/attempt', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ image: base64Image })
      });

      const data = await response.json().catch(() => ({}));

      if (!response.ok) {
        if (data.FallbackToPin) {
          setShowPin(true);
          setStatus('standby');
        } else {
          setStatus('error');
          setStatusMessage(data.message || 'Recognition failed. Please try again.');
          setTimeout(() => setStatus('standby'), 3000);
        }
        return;
      }

      // Success
      setStatus('success');
      setEmployeeName(data.employeeName || 'Unknown');
      setTimeout(() => setStatus('standby'), 3000);

    } catch (err) {
      setStatus('error');
      setStatusMessage('Network error. Retrying soon.');
      setTimeout(() => setStatus('standby'), 3000);
    }
  };

  return (
    <div style={{ height: '100vh', width: '100vw', background: 'var(--bg-color)', position: 'relative', overflow: 'hidden' }}>
      
      {/* Background Video */}
      <video 
        ref={videoRef} 
        autoPlay 
        playsInline 
        muted 
        style={{
          position: 'absolute',
          top: '50%',
          left: '50%',
          minWidth: '100%',
          minHeight: '100%',
          width: 'auto',
          height: 'auto',
          transform: 'translateX(-50%) translateY(-50%) scaleX(-1)', // Mirror effect
          objectFit: 'cover'
        }}
      />
      
      {/* Hidden canvas for motion processing */}
      <canvas ref={canvasRef} width="64" height="48" style={{ display: 'none' }} />

      {/* Overlay UI */}
      <div style={{
        position: 'absolute', top: 0, left: 0, right: 0, bottom: 0,
        background: 'linear-gradient(to bottom, rgba(0,0,0,0.6) 0%, transparent 20%, transparent 80%, rgba(0,0,0,0.8) 100%)',
        display: 'flex', flexDirection: 'column', justifyContent: 'space-between', padding: '2rem'
      }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ color: 'white', textShadow: '0 2px 4px rgba(0,0,0,0.5)' }}>NETFace Terminal</h2>
          <div style={{ 
            display: 'flex', alignItems: 'center', gap: '0.5rem', 
            background: 'rgba(0,0,0,0.5)', padding: '0.5rem 1rem', borderRadius: '20px', backdropFilter: 'blur(4px)' 
          }}>
            <div style={{ 
              width: '10px', height: '10px', borderRadius: '50%', 
              background: streamActive ? 'var(--success-color)' : 'var(--danger-color)',
              boxShadow: streamActive ? '0 0 8px var(--success-color)' : 'none'
            }} />
            <span style={{ fontSize: '0.875rem', fontWeight: '500' }}>{streamActive ? 'Live' : 'Camera Disconnected'}</span>
          </div>
        </div>

        {/* Status Indicators */}
        <div style={{ alignSelf: 'center', textAlign: 'center', marginBottom: '4rem' }}>
          {status === 'standby' && (
            <div className="animate-fade-in" style={{ color: 'white', textShadow: '0 2px 4px rgba(0,0,0,0.5)' }}>
              <Camera size={48} style={{ margin: '0 auto 1rem', opacity: 0.8 }} />
              <h3>Look at the camera</h3>
              <p style={{ opacity: 0.8 }}>Auto-capture active</p>
            </div>
          )}
          
          {status === 'capturing' && (
            <div className="animate-fade-in" style={{ background: 'rgba(59, 130, 246, 0.9)', padding: '1rem 2rem', borderRadius: '12px', backdropFilter: 'blur(4px)' }}>
              <h3 style={{ color: 'white', margin: 0 }}>{statusMessage}</h3>
            </div>
          )}

          {status === 'success' && (
            <div className="animate-fade-in" style={{ background: 'rgba(16, 185, 129, 0.9)', padding: '1.5rem 3rem', borderRadius: '16px', backdropFilter: 'blur(4px)', boxShadow: '0 10px 25px rgba(16, 185, 129, 0.3)' }}>
              <CheckCircle size={48} color="white" style={{ margin: '0 auto 1rem' }} />
              <h2 style={{ color: 'white', margin: 0 }}>Attendance Recorded</h2>
              <p style={{ color: 'rgba(255,255,255,0.9)', marginTop: '0.5rem', fontSize: '1.25rem' }}>{employeeName}</p>
            </div>
          )}

          {status === 'error' && (
            <div className="animate-fade-in" style={{ background: 'rgba(239, 68, 68, 0.9)', padding: '1rem 2rem', borderRadius: '12px', backdropFilter: 'blur(4px)' }}>
              <AlertCircle size={32} color="white" style={{ margin: '0 auto 0.5rem' }} />
              <h4 style={{ color: 'white', margin: 0 }}>{statusMessage}</h4>
            </div>
          )}
        </div>
      </div>

      {showPin && (
        <PinNumpad 
          onSubmit={() => {
            // Pin submitted, wait a bit then close
            setTimeout(() => setShowPin(false), 2000);
          }}
          onCancel={() => setShowPin(false)}
        />
      )}
    </div>
  );
}
