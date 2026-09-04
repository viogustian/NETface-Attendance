export const DEVICE_TOKEN_HEADER = 'X-Device-Token';
export const DEFAULT_DEVICE_TOKEN =
  (typeof import.meta !== 'undefined' && import.meta.env?.VITE_DEVICE_API_KEY) ||
  'netface-terminal-default-api-key';

/**
 * Captures a video frame from an HTMLVideoElement and converts it to a Blob.
 * @param {HTMLVideoElement} videoElement
 * @param {string} mimeType
 * @param {number} quality
 * @returns {Promise<Blob>}
 */
export async function captureCanvasBlob(videoElement, mimeType = 'image/jpeg', quality = 0.9) {
  if (!videoElement) {
    throw new Error('Invalid video element');
  }

  const width = videoElement.videoWidth;
  const height = videoElement.videoHeight;

  if (!width || !height || width <= 0 || height <= 0) {
    throw new Error('Invalid video dimensions');
  }

  const canvas = document.createElement('canvas');
  canvas.width = width;
  canvas.height = height;

  const ctx = canvas.getContext('2d');
  ctx.drawImage(videoElement, 0, 0);

  return new Promise((resolve, reject) => {
    if (typeof canvas.toBlob === 'function') {
      canvas.toBlob(
        (blob) => {
          if (blob) {
            resolve(blob);
          } else {
            reject(new Error('Failed to create image blob from canvas'));
          }
        },
        mimeType,
        quality
      );
    } else {
      reject(new Error('Canvas toBlob is not supported'));
    }
  });
}

/**
 * Sends a facial recognition attempt using multipart/form-data.
 * @param {Blob} imageBlob
 * @param {string|null} sessionId
 * @param {string} deviceToken
 * @returns {Promise<{
 *   success: boolean,
 *   message: string,
 *   employeeId?: string,
 *   employeeCode?: string,
 *   employeeName?: string,
 *   confidence?: number,
 *   fallbackToPin?: boolean,
 *   recognitionLogId?: string,
 *   data: any
 * }>}
 */
export async function sendRecognitionAttempt(
  imageBlob,
  sessionId = null,
  deviceToken = DEFAULT_DEVICE_TOKEN
) {
  const formData = new FormData();
  formData.append('image', imageBlob, 'frame.jpg');

  if (sessionId) {
    formData.append('sessionId', sessionId);
  }

  const response = await fetch('/api/recognition/attempt', {
    method: 'POST',
    headers: {
      [DEVICE_TOKEN_HEADER]: deviceToken
    },
    body: formData
  });

  const data = await response.json().catch(() => ({}));

  return {
    success: Boolean(response.ok && data.success),
    message: data.message || (response.ok ? 'Recognition successful' : 'Recognition failed'),
    employeeId: data.employeeId,
    employeeCode: data.employeeCode,
    employeeName: data.employeeName,
    confidence: data.confidence,
    fallbackToPin: Boolean(data.fallbackToPin || data.FallbackToPin),
    recognitionLogId: data.recognitionLogId,
    data
  };
}
