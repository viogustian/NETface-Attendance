import { calculateMotion } from './motionDetection';
import { describe, it, expect } from 'vitest';

describe('motionDetection', () => {
  it('returns 0 if lengths mismatch or undefined', () => {
    expect(calculateMotion(null, null)).toBe(0);
    expect(calculateMotion(new Uint8ClampedArray([1,2,3,4]), new Uint8ClampedArray([1,2,3,4,5,6,7,8]))).toBe(0);
  });

  it('returns 0 if images are identical', () => {
    const data1 = new Uint8ClampedArray([100, 100, 100, 255,  50, 50, 50, 255]);
    const data2 = new Uint8ClampedArray([100, 100, 100, 255,  50, 50, 50, 255]);
    expect(calculateMotion(data1, data2)).toBe(0);
  });

  it('calculates motion correctly for different images', () => {
    // 2 pixels
    const data1 = new Uint8ClampedArray([
      200, 200, 200, 255, // Pixel 1 - changed
      50, 50, 50, 255     // Pixel 2 - same
    ]);
    const data2 = new Uint8ClampedArray([
      100, 100, 100, 255, // Pixel 1 - diff is 100 per channel
      50, 50, 50, 255     // Pixel 2
    ]);

    // threshold is 45, so diff of 100 is well above threshold. 1 out of 2 pixels changed = 0.5
    expect(calculateMotion(data1, data2, 45)).toBe(0.5);
  });
});
