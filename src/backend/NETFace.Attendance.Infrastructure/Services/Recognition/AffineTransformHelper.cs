using MathNet.Numerics.LinearAlgebra;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NETFace.Attendance.Infrastructure.Services.Recognition;

public static class AffineTransformHelper
{
    // Standard 112x112 template points for ArcFace/MobileFaceNet
    private static readonly PointF[] TemplatePoints = {
        new PointF(38.2946f, 51.6963f),
        new PointF(73.5318f, 51.5014f),
        new PointF(56.0252f, 71.7366f),
        new PointF(41.5493f, 92.3655f),
        new PointF(70.7299f, 92.2041f)
    };

    /// <summary>
    /// Calculates the 2x3 affine similarity transform matrix (Umeyama algorithm).
    /// Maps srcPoints to dstPoints.
    /// </summary>
    public static float[,] CalculateSimilarityTransform(PointF[] srcPoints, PointF[] dstPoints)
    {
        int n = srcPoints.Length;
        if (n != dstPoints.Length)
            throw new ArgumentException("Source and destination points must have the same length.");

        var M = Matrix<double>.Build;

        // 1. Calculate centroids
        double srcMeanX = 0, srcMeanY = 0;
        double dstMeanX = 0, dstMeanY = 0;

        for (int i = 0; i < n; i++)
        {
            srcMeanX += srcPoints[i].X;
            srcMeanY += srcPoints[i].Y;
            dstMeanX += dstPoints[i].X;
            dstMeanY += dstPoints[i].Y;
        }

        srcMeanX /= n; srcMeanY /= n;
        dstMeanX /= n; dstMeanY /= n;

        // 2. Subtract centroids
        var srcDemean = M.Dense(n, 2);
        var dstDemean = M.Dense(n, 2);
        
        double srcVar = 0;

        for (int i = 0; i < n; i++)
        {
            double sx = srcPoints[i].X - srcMeanX;
            double sy = srcPoints[i].Y - srcMeanY;
            srcDemean[i, 0] = sx;
            srcDemean[i, 1] = sy;
            
            srcVar += sx * sx + sy * sy;
            
            dstDemean[i, 0] = dstPoints[i].X - dstMeanX;
            dstDemean[i, 1] = dstPoints[i].Y - dstMeanY;
        }
        
        srcVar /= n;

        // 3. Covariance matrix H = (dstDemean^T * srcDemean) / n
        var H = dstDemean.TransposeThisAndMultiply(srcDemean) / n;

        // 4. SVD of H
        var svd = H.Svd();
        var U = svd.U;
        var VT = svd.VT; // Note: MathNet returns VT, not V

        // 5. Calculate R
        var det = U.Determinant() * VT.Determinant();
        var S = M.DenseDiagonal(2, 2, 1.0);
        if (det < 0)
        {
            S[1, 1] = -1.0;
        }

        var R = U * S * VT;

        // 6. Calculate scale
        var varMatrix = svd.W * S;
        double scale = 1.0 / srcVar * varMatrix.Trace();

        // 7. Calculate translation
        var srcMeanMatrix = M.Dense(2, 1);
        srcMeanMatrix[0, 0] = srcMeanX;
        srcMeanMatrix[1, 0] = srcMeanY;

        var dstMeanMatrix = M.Dense(2, 1);
        dstMeanMatrix[0, 0] = dstMeanX;
        dstMeanMatrix[1, 0] = dstMeanY;

        var translation = dstMeanMatrix - scale * (R * srcMeanMatrix);

        // 8. Construct 2x3 affine matrix
        float[,] T = new float[2, 3];
        T[0, 0] = (float)(scale * R[0, 0]);
        T[0, 1] = (float)(scale * R[0, 1]);
        T[0, 2] = (float)translation[0, 0];
        T[1, 0] = (float)(scale * R[1, 0]);
        T[1, 1] = (float)(scale * R[1, 1]);
        T[1, 2] = (float)translation[1, 0];

        return T;
    }

    /// <summary>
    /// Align face using ImageSharp purely based on 5 landmarks to 112x112 output.
    /// </summary>
    public static Image<Rgb24> AlignFace(Image<Rgb24> sourceImage, PointF[] landmarks)
    {
        // 1. Calculate the similarity transform matrix
        var transformMatrix = CalculateSimilarityTransform(landmarks, TemplatePoints);
        
        // ImageSharp uses a 3x2 matrix for AffineTransformBuilder, defined as:
        // m11 m12
        // m21 m22
        // m31 m32 (translation)
        // Note: The math convention maps (x, y) * Matrix
        
        var m3x2 = new System.Numerics.Matrix3x2(
            transformMatrix[0, 0], transformMatrix[1, 0],
            transformMatrix[0, 1], transformMatrix[1, 1],
            transformMatrix[0, 2], transformMatrix[1, 2]
        );

        // We want to transform the source image so that the face lands exactly at the template points on a 112x112 canvas.
        // Wait, the transformMatrix we calculated maps SourcePoints to TemplatePoints (Dst).
        // ImageSharp's Transform applies mapping from Source space to Dst space.
        // ImageSharp's AffineTransformBuilder applies matrix multiplication: v' = v * M.
        // Let's create an affine transform builder.
        var builder = new AffineTransformBuilder().PrependMatrix(m3x2);

        // We clone and mutate the image. 
        var alignedImage = sourceImage.Clone(ctx => 
        {
            // We set the output size to 112x112
            // And use Bicubic or Bilinear sampler
            ctx.Transform(new Rectangle(0, 0, 112, 112), builder, KnownResamplers.Bicubic);
        });

        return alignedImage;
    }
}
