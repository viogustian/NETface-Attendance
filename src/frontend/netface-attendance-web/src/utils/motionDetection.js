/**
 * Calculates the percentage of pixels that changed significantly between two frames.
 * @param {Uint8ClampedArray} data1 - Current frame pixel data (RGBA)
 * @param {Uint8ClampedArray} data2 - Previous frame pixel data (RGBA)
 * @param {number} threshold - The RGB difference threshold for a pixel to be considered 'changed' (0-255)
 * @returns {number} Percentage of changed pixels (0.0 to 1.0)
 */
export function calculateMotion(data1, data2, threshold = 45) {
  if (!data1 || !data2 || data1.length !== data2.length) {
    return 0;
  }

  let changedPixels = 0;
  const totalPixels = data1.length / 4; // 4 values per pixel (R, G, B, A)

  // Step by 4 to compare pixels
  for (let i = 0; i < data1.length; i += 4) {
    const diffR = Math.abs(data1[i] - data2[i]);
    const diffG = Math.abs(data1[i + 1] - data2[i + 1]);
    const diffB = Math.abs(data1[i + 2] - data2[i + 2]);

    // Simple sum of differences
    const diff = diffR + diffG + diffB;

    if (diff > threshold * 3) {
      changedPixels++;
    }
  }

  return changedPixels / totalPixels;
}
