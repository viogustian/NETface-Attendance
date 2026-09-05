import { useState, useEffect, useRef } from 'react';
import { Camera, CheckCircle, AlertCircle } from 'lucide-react';
import PinNumpad from '../../components/kiosk/PinNumpad';
import { calculateMotion } from '../../utils/motionDetection';
import { captureCanvasBlob, sendRecognitionAttempt } from '../../utils/cameraUtils';

export default function KioskHome({ checkIntervalMs = 1000 }) {
  const videoRef = useRef(null);
  const canvasRef = useRef(null);
  const previousFrameRef = useRef(null);
  const statusTimeoutRef = useRef(null);
  const pinTimeoutRef = useRef(null);

  const [streamActive, setStreamActive] = useState(false);
  const [showPin, setShowPin] = useState(false);
  const [status, setStatus] = useState('standby'); // 'standby' | 'capturing' | 'success' | 'error'
  const [statusMessage, setStatusMessage] = useState('');
  const [employeeName, setEmployeeName] = useState('');

  const clearStatusTimeout = () => {
    if (statusTimeoutRef.current) {
      clearTimeout(statusTimeoutRef.current);
      statusTimeoutRef.current = null;
    }
  };

  const clearPinTimeout = () => {
    if (pinTimeoutRef.current) {
      clearTimeout(pinTimeoutRef.current);
      pinTimeoutRef.current = null;
    }
  };

  // 1. Initialize camera stream on mount
  useEffect(() => {
    let stream = null;
    let isMounted = true;

    const startCamera = async () => {
      try {
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
          throw new Error('Camera not supported by browser');
        }

        stream = await navigator.mediaDevices.getUserMedia({
          video: true,
          audio: false
        });

        if (isMounted && videoRef.current) {
          videoRef.current.srcObject = stream;
          try {
            await videoRef.current.play();
          } catch {
            // Autoplay policy fallback
          }
          setStreamActive(true);
        }
      } catch (err) {
        if (isMounted) {
          setStatus('error');
          setStatusMessage(err?.message || 'Camera access denied or unavailable.');
        }
      }
    };

    startCamera();

    return () => {
      isMounted = false;
      clearStatusTimeout();
      clearPinTimeout();
      if (stream) {
        stream.getTracks().forEach((track) => track.stop());
      }
    };
  }, []);

  // 2. Handle Frame Capture & API attempt
  const handleCapture = async () => {
    if (!videoRef.current) return;

    setStatus('capturing');
    setStatusMessage('Motion detected. Processing...');

    try {
      // Extract high-resolution frame as Blob
      const imageBlob = await captureCanvasBlob(videoRef.current);

      // Submit multipart/form-data payload to backend
      const result = await sendRecognitionAttempt(imageBlob);

      if (!result.success) {
        if (result.fallbackToPin) {
          setShowPin(true);
          setStatus('standby');
        } else {
          setStatus('error');
          setStatusMessage(result.message || 'Recognition failed. Please try again.');
          clearStatusTimeout();
          statusTimeoutRef.current = setTimeout(() => {
            setStatus('standby');
          }, 3000);
        }
        return;
      }

      // Recognition succeeded: display Employee Name and Green Tick
      setStatus('success');
      setEmployeeName(result.employeeName || 'Employee');
      setStatusMessage(result.message || 'Attendance Recorded');

      clearStatusTimeout();
      statusTimeoutRef.current = setTimeout(() => {
        setStatus('standby');
        setEmployeeName('');
      }, 3000);
    } catch (err) {
      setStatus('error');
      setStatusMessage(err.message || 'Network error. Retrying soon.');
      clearStatusTimeout();
      statusTimeoutRef.current = setTimeout(() => {
        setStatus('standby');
      }, 3000);
    }
  };

  // 3. Motion Detection Loop
  useEffect(() => {
    if (!streamActive || status !== 'standby') return;

    const interval = setInterval(() => {
      if (!videoRef.current || !canvasRef.current) return;

      const canvas = canvasRef.current;
      const ctx = canvas.getContext('2d');
      if (!ctx) return;

      // Draw scaled down video frame to canvas for lightweight motion processing
      ctx.drawImage(videoRef.current, 0, 0, 64, 48);
      const currentFrame = ctx.getImageData(0, 0, 64, 48).data;

      if (previousFrameRef.current) {
        const motion = calculateMotion(currentFrame, previousFrameRef.current, 45);
        // If more than 5% of pixels changed, trigger auto-capture
        if (motion > 0.05) {
          handleCapture();
        }
      }

      previousFrameRef.current = currentFrame;
    }, checkIntervalMs);

    return () => clearInterval(interval);
  }, [streamActive, status, checkIntervalMs]);

  return (
    <div
      style={{
        position: 'relative',
        width: '100vw',
        height: '100vh',
        overflow: 'hidden',
        background: '#ffffff',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center'
      }}
    >
      <div style={{ position: 'absolute', top: '2rem', left: '2rem' }}>
        <h1 style={{ fontSize: '1.5rem', fontWeight: 700, color: '#2A282A', letterSpacing: '-0.5px', margin: 0 }}>
          NETFace Kiosk
        </h1>
      </div>

      <div style={{
        position: 'relative',
        width: '640px',
        maxWidth: '90vw',
        aspectRatio: '4/3',
        borderRadius: '24px',
        boxShadow: '0 25px 50px -12px rgba(42, 40, 42, 0.25)',
        background: '#2A282A',
        overflow: 'hidden'
      }}>
        {/* Webcam Background Video */}
        <video
          ref={videoRef}
          autoPlay
          playsInline
          muted
          style={{
            position: 'absolute',
            inset: 0,
            width: '100%',
            height: '100%',
            objectFit: 'cover',
            transform: 'scaleX(-1)' // Mirror effect
          }}
        />

        {/* Hidden offscreen canvas for lightweight motion pre-detection */}
        <canvas ref={canvasRef} width="64" height="48" style={{ display: 'none' }} />

        {/* UI Overlay */}
        <div
          style={{
            position: 'absolute',
            inset: 0,
            display: 'flex',
            flexDirection: 'column',
            justifyContent: 'space-between',
            padding: '1.5rem',
            background: 'linear-gradient(to bottom, rgba(42, 40, 42, 0.4) 0%, transparent 20%, transparent 80%, rgba(42, 40, 42, 0.6) 100%)',
            pointerEvents: 'none'
          }}
        >
          {/* Top Status Badge */}
          <div
            style={{
              display: 'flex',
              justifyContent: 'flex-end',
              alignItems: 'center'
            }}
          >
            <div
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '0.6rem',
                background: 'rgba(42, 40, 42, 0.75)',
                padding: '0.5rem 1rem',
                borderRadius: '9999px',
                border: '1px solid rgba(255, 255, 255, 0.2)',
                backdropFilter: 'blur(8px)'
              }}
            >
              <div
                style={{
                  width: '10px',
                  height: '10px',
                  borderRadius: '50%',
                  background: streamActive ? '#10b981' : '#CE3081',
                  boxShadow: streamActive ? '0 0 10px #10b981' : 'none'
                }}
              />
              <span style={{ fontSize: '0.875rem', fontWeight: 600, color: '#ffffff' }}>
                {streamActive ? 'Live' : 'Camera Disconnected'}
              </span>
            </div>
          </div>

          {/* Dynamic Center Feedback States */}
          <div
            style={{
              alignSelf: 'center',
              textAlign: 'center',
              marginBottom: '2rem',
              maxWidth: '480px',
              width: '100%'
            }}
          >
            {status === 'standby' && (
              <div
                style={{
                  color: '#ffffff',
                  textShadow: '0 2px 10px rgba(0,0,0,0.8)'
                }}
              >
                <Camera
                  size={54}
                  style={{
                    margin: '0 auto 1rem',
                    opacity: 0.9,
                    filter: 'drop-shadow(0 2px 8px rgba(0,0,0,0.6))'
                  }}
                />
                <h2 style={{ fontSize: '1.75rem', fontWeight: 700, margin: '0 0 0.5rem 0' }}>
                  Look at the camera
                </h2>
                <p style={{ margin: 0, opacity: 0.85, fontSize: '1rem' }}>
                  Walk up to terminal for automatic attendance recognition
                </p>
              </div>
            )}

            {status === 'capturing' && (
              <div
                style={{
                  background: 'rgba(239, 148, 46, 0.85)', // Creamsicle
                  border: '1px solid rgba(239, 148, 46, 0.5)',
                  padding: '1.25rem 2.5rem',
                  borderRadius: '16px',
                  backdropFilter: 'blur(12px)',
                  boxShadow: '0 8px 32px rgba(239, 148, 46, 0.4)'
                }}
              >
                <h3 style={{ color: '#ffffff', margin: 0, fontSize: '1.25rem' }}>
                  {statusMessage}
                </h3>
              </div>
            )}

            {status === 'success' && (
              <div
                data-testid="success-state-container"
                style={{
                  background: 'rgba(16, 185, 129, 0.85)', // Success
                  border: '1px solid rgba(16, 185, 129, 0.5)',
                  padding: '2rem 3rem',
                  borderRadius: '20px',
                  backdropFilter: 'blur(12px)',
                  boxShadow: '0 12px 40px rgba(16, 185, 129, 0.35)',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center'
                }}
              >
                <CheckCircle
                  data-testid="recognition-success-tick"
                  size={64}
                  color="#ffffff"
                  style={{
                    margin: '0 auto 1rem',
                    filter: 'drop-shadow(0 0 12px rgba(255,255,255,0.5))'
                  }}
                />
                <h2 style={{ color: '#ffffff', margin: 0, fontSize: '1.5rem', fontWeight: 700 }}>
                  Attendance Recorded
                </h2>
                <p
                  style={{
                    color: '#ffffff',
                    marginTop: '0.75rem',
                    marginBottom: 0,
                    fontSize: '1.5rem',
                    fontWeight: 600
                  }}
                >
                  {employeeName}
                </p>
              </div>
            )}

            {status === 'error' && (
              <div
                style={{
                  background: 'rgba(206, 48, 129, 0.85)', // Floral Magenta
                  border: '1px solid rgba(206, 48, 129, 0.5)',
                  padding: '1.25rem 2.5rem',
                  borderRadius: '16px',
                  backdropFilter: 'blur(12px)',
                  boxShadow: '0 8px 32px rgba(206, 48, 129, 0.4)',
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center'
                }}
              >
                <AlertCircle size={40} color="#ffffff" style={{ margin: '0 auto 0.75rem' }} />
                <h4 style={{ color: '#ffffff', margin: 0, fontSize: '1.15rem', fontWeight: 600 }}>
                  {statusMessage}
                </h4>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Fallback PIN Modal */}
      {showPin && (
        <div style={{ pointerEvents: 'auto' }}>
          <PinNumpad
            onSubmit={() => {
              clearPinTimeout();
              pinTimeoutRef.current = setTimeout(() => setShowPin(false), 2000);
            }}
            onCancel={() => {
              clearPinTimeout();
              setShowPin(false);
            }}
          />
        </div>
      )}
    </div>
  );
}
