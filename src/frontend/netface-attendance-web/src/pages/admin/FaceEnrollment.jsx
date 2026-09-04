import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ArrowLeft, Camera, Upload, Trash2, AlertCircle } from 'lucide-react';
import { getToken } from '../../utils/auth';
import AlertError from '../../components/ui/AlertError';
import AlertSuccess from '../../components/ui/AlertSuccess';

export default function FaceEnrollment() {
  const { id } = useParams();
  const navigate = useNavigate();
  
  const [employee, setEmployee] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  
  const [stream, setStream] = useState(null);
  const videoRef = useRef(null);
  const canvasRef = useRef(null);
  
  const [capturedImages, setCapturedImages] = useState([]); // array of blobs/files
  
  // Fetch employee details
  useEffect(() => {
    const fetchEmployee = async () => {
      try {
        const res = await fetch(`/api/employees/${id}`, {
          headers: { 'Authorization': `Bearer ${getToken()}` }
        });
        if (!res.ok) throw new Error('Failed to fetch employee details');
        const data = await res.json();
        setEmployee(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };
    fetchEmployee();
  }, [id]);

  // Start webcam
  const startCamera = async () => {
    try {
      const mediaStream = await navigator.mediaDevices.getUserMedia({ video: true });
      setStream(mediaStream);
      if (videoRef.current) {
        videoRef.current.srcObject = mediaStream;
      }
    } catch (err) {
      setError("Failed to access camera. Please allow camera permissions.");
    }
  };

  const stopCamera = () => {
    if (stream) {
      stream.getTracks().forEach(track => track.stop());
      setStream(null);
    }
  };

  useEffect(() => {
    return () => stopCamera(); // Cleanup on unmount
  }, [stream]);

  const capturePhoto = () => {
    if (videoRef.current && canvasRef.current) {
      const video = videoRef.current;
      const canvas = canvasRef.current;
      canvas.width = video.videoWidth;
      canvas.height = video.videoHeight;
      const ctx = canvas.getContext('2d');
      ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
      
      canvas.toBlob((blob) => {
        if (blob) {
          const file = new File([blob], `capture-${Date.now()}.jpg`, { type: 'image/jpeg' });
          handleAddImage(file);
        }
      }, 'image/jpeg', 0.9);
    }
  };

  const handleFileUpload = (e) => {
    if (e.target.files && e.target.files.length > 0) {
      const files = Array.from(e.target.files);
      files.forEach(handleAddImage);
    }
  };

  const handleAddImage = (file) => {
    if (file.size > 5 * 1024 * 1024) {
      setError("Ukuran foto terlalu besar. Maksimum 5 MB.");
      return;
    }
    const ext = file.name.split('.').pop().toLowerCase();
    if (!['jpg', 'jpeg', 'png'].includes(ext)) {
      setError("File harus berupa JPG atau PNG.");
      return;
    }
    
    setCapturedImages(prev => {
      if (prev.length >= 3) {
        setError("Maksimal 3 foto dapat diunggah sekaligus.");
        return prev;
      }
      return [...prev, file];
    });
    setError(null);
  };

  const removeImage = (index) => {
    setCapturedImages(prev => prev.filter((_, i) => i !== index));
  };

  const submitEnrollment = async () => {
    if (capturedImages.length === 0) return;
    setSubmitting(true);
    setError(null);
    setSuccess(null);

    const formData = new FormData();
    capturedImages.forEach(file => {
      formData.append('files', file);
    });

    try {
      const res = await fetch(`/api/employees/${id}/faces`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${getToken()}` },
        body: formData
      });
      
      const data = await res.json();
      if (!res.ok) {
        throw new Error(data.message || 'Enrollment gagal karena server tidak dapat memproses permintaan. Silakan coba lagi.');
      }
      
      setSuccess(`Berhasil mendaftarkan ${data.enrolledCount} wajah.`);
      setCapturedImages([]);
      
      // Update employee state
      setEmployee(prev => ({
        ...prev,
        enrolledFacesCount: prev.enrolledFacesCount + data.enrolledCount
      }));
      
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleClearFaces = async () => {
    if (!window.confirm("Apakah Anda yakin ingin menghapus seluruh data wajah (embeddings) karyawan ini? Operasi ini tidak dapat dibatalkan.")) {
      return;
    }
    
    setSubmitting(true);
    setError(null);
    setSuccess(null);

    try {
      const res = await fetch(`/api/employees/${id}/faces`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${getToken()}` }
      });
      
      const data = await res.json();
      if (!res.ok) {
        throw new Error(data.message || 'Gagal mereset wajah.');
      }
      
      setSuccess(`Berhasil mereset seluruh wajah. Karyawan kini memiliki 0 terdaftar.`);
      
      // Update employee state
      setEmployee(prev => ({
        ...prev,
        enrolledFacesCount: 0
      }));
      
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <div className="page-container">Loading...</div>;
  if (!employee) return <div className="page-container">Employee not found.</div>;

  const currentCount = employee.enrolledFacesCount || 0;
  const isFull = currentCount >= 5;
  
  // Create progress bar visualization
  const blocks = [];
  for (let i = 0; i < 5; i++) {
    blocks.push(i < currentCount ? '█' : '░');
  }

  return (
    <div className="page-container animate-fade-in">
      <Link to="/admin/employees" style={{ display: 'inline-flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1.5rem', color: 'var(--text-secondary)' }}>
        <ArrowLeft size={16} /> Back to Employees
      </Link>
      
      <div style={{ marginBottom: '2rem' }}>
        <h1 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <Camera color="var(--primary-color)" /> Face Enrollment
        </h1>
        <p style={{ color: 'var(--text-secondary)' }}>Daftarkan data wajah untuk {employee.fullName} ({employee.employeeCode}).</p>
      </div>

      <AlertError message={error} />
      <AlertSuccess message={success} />

      <div className="glass-panel" style={{ padding: '2rem', marginBottom: '2rem' }}>
        <h3 style={{ marginBottom: '1rem', color: 'var(--text-primary)' }}>Status Pendaftaran Wajah</h3>
        <div style={{ background: 'var(--bg-primary)', padding: '1.5rem', borderRadius: '8px', border: '1px solid var(--border-color)' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.5rem' }}>
            <span style={{ fontWeight: '600' }}>Registered</span>
            <span style={{ fontFamily: 'monospace', fontSize: '1.1rem', letterSpacing: '2px', color: isFull ? 'var(--danger-color)' : 'var(--primary-color)' }}>
              [{blocks.join('')}] {currentCount} / 5
            </span>
          </div>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
            {currentCount} embeddings registered. {5 - currentCount} slots remaining.
          </p>
          
          <div style={{ marginTop: '1.5rem', display: 'flex', gap: '1rem' }}>
            <button 
              className="btn-danger" 
              onClick={handleClearFaces}
              disabled={currentCount === 0 || submitting}
              style={{ display: 'inline-flex', alignItems: 'center', gap: '0.5rem', padding: '0.5rem 1rem', background: 'var(--danger-color)', color: 'white', border: 'none', borderRadius: '4px', cursor: (currentCount === 0 || submitting) ? 'not-allowed' : 'pointer', opacity: (currentCount === 0 || submitting) ? 0.5 : 1 }}
            >
              <Trash2 size={16} /> Clear Faces
            </button>
          </div>
        </div>
      </div>

      <div className="glass-panel" style={{ padding: '2rem', opacity: isFull ? 0.5 : 1 }}>
        <h3 style={{ marginBottom: '1rem' }}>Tambah Wajah Baru</h3>
        
        {isFull ? (
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--danger-color)', padding: '1rem', background: 'rgba(239, 68, 68, 0.1)', borderRadius: '4px' }}>
            <AlertCircle size={20} />
            Employee sudah memiliki 5 face embedding. Hapus/reset embedding lama untuk mendaftarkan wajah baru.
          </div>
        ) : (
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem' }}>
            <div>
              <div style={{ marginBottom: '1rem', background: '#000', borderRadius: '8px', overflow: 'hidden', aspectRatio: '4/3', position: 'relative' }}>
                <video 
                  ref={videoRef} 
                  autoPlay 
                  playsInline 
                  muted 
                  style={{ width: '100%', height: '100%', objectFit: 'cover', display: stream ? 'block' : 'none' }} 
                />
                {!stream && (
                  <div style={{ position: 'absolute', inset: 0, display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#666' }}>
                    Camera Offline
                  </div>
                )}
                <canvas ref={canvasRef} style={{ display: 'none' }} />
              </div>
              
              <div style={{ display: 'flex', gap: '1rem' }}>
                {!stream ? (
                  <button className="btn-secondary" onClick={startCamera} style={{ flex: 1, display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '0.5rem' }}>
                    <Camera size={18} /> Start Camera
                  </button>
                ) : (
                  <button className="btn-secondary" onClick={stopCamera} style={{ flex: 1, display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '0.5rem', color: 'var(--danger-color)' }}>
                    Stop Camera
                  </button>
                )}
                <button 
                  className="btn-primary" 
                  onClick={capturePhoto} 
                  disabled={!stream || capturedImages.length >= Math.min(3, 5 - currentCount)}
                  style={{ flex: 1, display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '0.5rem' }}
                >
                  Capture
                </button>
              </div>

              <div style={{ marginTop: '1.5rem', textAlign: 'center' }}>
                <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginBottom: '0.5rem', display: 'block' }}>ATAU</span>
                <label className="btn-secondary" style={{ display: 'inline-flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer', opacity: capturedImages.length >= Math.min(3, 5 - currentCount) ? 0.5 : 1 }}>
                  <Upload size={18} /> Upload Image (JPG/PNG)
                  <input 
                    type="file" 
                    accept="image/jpeg, image/png" 
                    multiple 
                    onChange={handleFileUpload} 
                    style={{ display: 'none' }} 
                    disabled={capturedImages.length >= Math.min(3, 5 - currentCount)}
                  />
                </label>
              </div>
            </div>

            <div>
              <h4 style={{ marginBottom: '1rem', color: 'var(--text-secondary)' }}>Foto yang akan dikirim (Max 3/batch):</h4>
              {capturedImages.length === 0 ? (
                <div style={{ padding: '2rem', border: '2px dashed var(--border-color)', borderRadius: '8px', textAlign: 'center', color: 'var(--text-secondary)' }}>
                  Belum ada foto yang diambil.
                </div>
              ) : (
                <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                  {capturedImages.map((file, idx) => (
                    <div key={idx} style={{ display: 'flex', alignItems: 'center', gap: '1rem', padding: '0.5rem', background: 'var(--bg-primary)', borderRadius: '4px', border: '1px solid var(--border-color)' }}>
                      <img src={URL.createObjectURL(file)} alt="Preview" style={{ width: '60px', height: '60px', objectFit: 'cover', borderRadius: '4px' }} />
                      <div style={{ flex: 1, overflow: 'hidden' }}>
                        <div style={{ whiteSpace: 'nowrap', textOverflow: 'ellipsis', overflow: 'hidden', fontSize: '0.9rem' }}>{file.name}</div>
                        <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{(file.size / 1024).toFixed(1)} KB</div>
                      </div>
                      <button 
                        onClick={() => removeImage(idx)} 
                        style={{ background: 'transparent', border: 'none', color: 'var(--danger-color)', cursor: 'pointer', padding: '0.5rem' }}
                      >
                        <Trash2 size={18} />
                      </button>
                    </div>
                  ))}
                  <button 
                    className="btn-primary" 
                    onClick={submitEnrollment} 
                    disabled={submitting}
                    style={{ marginTop: '1rem', width: '100%', padding: '0.75rem' }}
                  >
                    {submitting ? 'Memproses...' : `Submit Enrollment (${capturedImages.length} foto)`}
                  </button>
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
