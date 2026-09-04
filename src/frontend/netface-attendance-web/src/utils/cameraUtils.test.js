import { describe, it, expect, vi, beforeEach } from 'vitest';
import { captureCanvasBlob, sendRecognitionAttempt, DEVICE_TOKEN_HEADER, DEFAULT_DEVICE_TOKEN } from './cameraUtils';

describe('cameraUtils', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  describe('captureCanvasBlob', () => {
    it('creates a canvas from video element and returns a blob', async () => {
      const mockVideo = {
        videoWidth: 640,
        videoHeight: 480
      };

      const mockBlob = new Blob(['dummy-image'], { type: 'image/jpeg' });
      const mockContext = {
        drawImage: vi.fn()
      };
      const mockCanvas = {
        width: 0,
        height: 0,
        getContext: vi.fn().mockReturnValue(mockContext),
        toBlob: vi.fn((cb) => cb(mockBlob))
      };

      vi.spyOn(document, 'createElement').mockReturnValue(mockCanvas);

      const blob = await captureCanvasBlob(mockVideo);

      expect(document.createElement).toHaveBeenCalledWith('canvas');
      expect(mockCanvas.width).toBe(640);
      expect(mockCanvas.height).toBe(480);
      expect(mockContext.drawImage).toHaveBeenCalledWith(mockVideo, 0, 0);
      expect(mockCanvas.toBlob).toHaveBeenCalled();
      expect(blob).toBe(mockBlob);
    });

    it('rejects if video element is missing or invalid dimensions', async () => {
      await expect(captureCanvasBlob(null)).rejects.toThrow('Invalid video element');
      await expect(captureCanvasBlob({ videoWidth: 0, videoHeight: 0 })).rejects.toThrow('Invalid video dimensions');
    });
  });

  describe('sendRecognitionAttempt', () => {
    it('sends multipart/form-data payload with image and device token header', async () => {
      const mockBlob = new Blob(['image-bytes'], { type: 'image/jpeg' });
      const mockApiResponse = {
        success: true,
        message: 'Attendance recorded successfully.',
        employeeName: 'John Doe',
        employeeCode: 'EMP001',
        confidence: 0.95
      };

      global.fetch = vi.fn().mockResolvedValue({
        ok: true,
        status: 200,
        json: async () => mockApiResponse
      });

      const result = await sendRecognitionAttempt(mockBlob);

      expect(global.fetch).toHaveBeenCalledTimes(1);
      const [url, options] = global.fetch.mock.calls[0];

      expect(url).toBe('/api/recognition/attempt');
      expect(options.method).toBe('POST');
      expect(options.headers[DEVICE_TOKEN_HEADER]).toBe(DEFAULT_DEVICE_TOKEN);
      expect(options.body.get('image')).toBeInstanceOf(Blob);
      expect(options.body.get('image').name).toBe('frame.jpg');

      expect(result.success).toBe(true);
      expect(result.employeeName).toBe('John Doe');
    });

    it('includes sessionId when provided', async () => {
      const mockBlob = new Blob(['image-bytes'], { type: 'image/jpeg' });
      const sessionId = '11111111-2222-3333-4444-555555555555';

      global.fetch = vi.fn().mockResolvedValue({
        ok: true,
        status: 200,
        json: async () => ({ success: true, employeeName: 'Jane' })
      });

      await sendRecognitionAttempt(mockBlob, sessionId);

      const [, options] = global.fetch.mock.calls[0];
      expect(options.body.get('sessionId')).toBe(sessionId);
    });

    it('handles backend error or fallback to PIN response', async () => {
      const mockBlob = new Blob(['image-bytes'], { type: 'image/jpeg' });
      const mockErrorResponse = {
        success: false,
        message: 'Consecutive failures. Please use PIN.',
        fallbackToPin: true
      };

      global.fetch = vi.fn().mockResolvedValue({
        ok: false,
        status: 401,
        json: async () => mockErrorResponse
      });

      const result = await sendRecognitionAttempt(mockBlob);

      expect(result.success).toBe(false);
      expect(result.fallbackToPin).toBe(true);
      expect(result.message).toBe('Consecutive failures. Please use PIN.');
    });
  });
});
