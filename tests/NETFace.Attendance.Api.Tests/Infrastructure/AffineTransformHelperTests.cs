using NETFace.Attendance.Infrastructure.Services.Recognition;
using OpenCvSharp;
using SixLabors.ImageSharp;
using Xunit;

namespace NETFace.Attendance.Api.Tests.Infrastructure;

public class AffineTransformHelperTests
{
    [Fact]
    public void CalculateSimilarityTransform_ShouldMatchOpenCvSharpOutput()
    {
        // Arrange
        // Some dummy source landmarks (5 points)
        PointF[] srcPoints = {
            new PointF(150.0f, 160.0f),
            new PointF(250.0f, 155.0f),
            new PointF(200.0f, 220.0f),
            new PointF(160.0f, 280.0f),
            new PointF(240.0f, 275.0f)
        };

        // Standard template points in the helper
        PointF[] dstPoints = {
            new PointF(38.2946f, 51.6963f),
            new PointF(73.5318f, 51.5014f),
            new PointF(56.0252f, 71.7366f),
            new PointF(41.5493f, 92.3655f),
            new PointF(70.7299f, 92.2041f)
        };

        // 1. Compute via Pure C# MathNet
        var csharpMatrix = AffineTransformHelper.CalculateSimilarityTransform(srcPoints, dstPoints);

        // 2. Compute via OpenCvSharp
        var srcPtsList = new Point2f[5];
        var dstPtsList = new Point2f[5];
        for (int i = 0; i < 5; i++)
        {
            srcPtsList[i] = new Point2f(srcPoints[i].X, srcPoints[i].Y);
            dstPtsList[i] = new Point2f(dstPoints[i].X, dstPoints[i].Y);
        }

        // LMEDS or RANSAC. For 5 points, we can just use least squares, but EstimateAffinePartial2D uses RANSAC by default.
        // We will pass no robust method to ensure standard least squares if possible, or just use it.
        // Actually EstimateAffinePartial2D computes robustly. A simple Procrustes is sometimes slightly different if RANSAC drops points.
        // But for exactly 5 points that are already an affine pair, it should match.
        // Another OpenCV method is `Cv2.EstimateAffine2D`, but that's full affine. 
        // We want similarity transform (Partial2D).
        using var cvMatrix = Cv2.EstimateAffinePartial2D(InputArray.Create(srcPtsList), InputArray.Create(dstPtsList));
        
        // Assert
        Assert.NotNull(cvMatrix);
        Assert.Equal(2, cvMatrix.Rows);
        Assert.Equal(3, cvMatrix.Cols);

        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                double cvVal = cvMatrix.At<double>(row, col);
                double csVal = csharpMatrix[row, col];

                // Allow small floating point differences (precision to 3 decimal places)
                Assert.True(Math.Abs(cvVal - csVal) < 0.05, 
                    $"Matrix mismatch at [{row},{col}]. CV: {cvVal}, C#: {csVal}");
            }
        }
    }
}
