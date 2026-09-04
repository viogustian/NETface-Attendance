import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import KioskHome from './KioskHome';
import * as cameraUtils from '../../utils/cameraUtils';
import * as motionUtils from '../../utils/motionDetection';

describe('KioskHome', () => {
  let mockTrack;
  let mockStream;

  beforeEach(() => {
    vi.restoreAllMocks();

    mockTrack = { stop: vi.fn() };
    mockStream = {
      getTracks: vi.fn().mockReturnValue([mockTrack])
    };

    // Mock navigator.mediaDevices
    Object.defineProperty(global.navigator, 'mediaDevices', {
      value: {
        getUserMedia: vi.fn().mockResolvedValue(mockStream)
      },
      writable: true,
      configurable: true
    });

    // Mock HTMLMediaElement.prototype.play
    window.HTMLMediaElement.prototype.play = vi.fn().mockResolvedValue();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('activates camera on page load and displays standby status', async () => {
    render(<KioskHome />);

    expect(navigator.mediaDevices.getUserMedia).toHaveBeenCalledWith({
      video: true,
      audio: false
    });

    await waitFor(() => {
      expect(screen.getByText(/Look at the camera/i)).toBeInTheDocument();
      expect(screen.getByText(/Live/i)).toBeInTheDocument();
    });
  });

  it('captures frame and displays Employee Name and Green Tick on recognition success', async () => {
    const dummyBlob = new Blob(['face-image'], { type: 'image/jpeg' });
    vi.spyOn(cameraUtils, 'captureCanvasBlob').mockResolvedValue(dummyBlob);
    vi.spyOn(cameraUtils, 'sendRecognitionAttempt').mockResolvedValue({
      success: true,
      employeeName: 'Jane Smith',
      message: 'Attendance recorded'
    });

    // Mock calculateMotion to report motion > threshold
    vi.spyOn(motionUtils, 'calculateMotion').mockReturnValue(0.12);

    // Mock canvas context for motion check
    const mockContext = {
      drawImage: vi.fn(),
      getImageData: vi.fn().mockReturnValue({
        data: new Uint8ClampedArray(64 * 48 * 4)
      })
    };
    HTMLCanvasElement.prototype.getContext = vi.fn().mockReturnValue(mockContext);

    render(<KioskHome checkIntervalMs={50} />);

    await waitFor(() => {
      expect(screen.getByText(/Jane Smith/i)).toBeInTheDocument();
      expect(screen.getByTestId('recognition-success-tick')).toBeInTheDocument();
    });

    expect(cameraUtils.captureCanvasBlob).toHaveBeenCalled();
    expect(cameraUtils.sendRecognitionAttempt).toHaveBeenCalledWith(dummyBlob);
  });

  it('displays error message on recognition failure', async () => {
    const dummyBlob = new Blob(['face-image'], { type: 'image/jpeg' });
    vi.spyOn(cameraUtils, 'captureCanvasBlob').mockResolvedValue(dummyBlob);
    vi.spyOn(cameraUtils, 'sendRecognitionAttempt').mockResolvedValue({
      success: false,
      message: 'No registered face matched.'
    });

    vi.spyOn(motionUtils, 'calculateMotion').mockReturnValue(0.15);

    const mockContext = {
      drawImage: vi.fn(),
      getImageData: vi.fn().mockReturnValue({
        data: new Uint8ClampedArray(64 * 48 * 4)
      })
    };
    HTMLCanvasElement.prototype.getContext = vi.fn().mockReturnValue(mockContext);

    render(<KioskHome checkIntervalMs={50} />);

    await waitFor(() => {
      expect(screen.getByText(/No registered face matched/i)).toBeInTheDocument();
    });
  });

  it('shows Fallback PIN UI when backend response indicates fallbackToPin', async () => {
    const dummyBlob = new Blob(['face-image'], { type: 'image/jpeg' });
    vi.spyOn(cameraUtils, 'captureCanvasBlob').mockResolvedValue(dummyBlob);
    vi.spyOn(cameraUtils, 'sendRecognitionAttempt').mockResolvedValue({
      success: false,
      message: 'Consecutive failures. Please use PIN.',
      fallbackToPin: true
    });

    vi.spyOn(motionUtils, 'calculateMotion').mockReturnValue(0.15);

    const mockContext = {
      drawImage: vi.fn(),
      getImageData: vi.fn().mockReturnValue({
        data: new Uint8ClampedArray(64 * 48 * 4)
      })
    };
    HTMLCanvasElement.prototype.getContext = vi.fn().mockReturnValue(mockContext);

    render(<KioskHome checkIntervalMs={50} />);

    await waitFor(() => {
      expect(screen.getByText(/Enter PIN/i)).toBeInTheDocument();
    });
  });

  it('stops media stream tracks when unmounted', async () => {
    const { unmount } = render(<KioskHome />);

    await waitFor(() => {
      expect(navigator.mediaDevices.getUserMedia).toHaveBeenCalled();
    });

    unmount();

    expect(mockTrack.stop).toHaveBeenCalled();
  });
});
