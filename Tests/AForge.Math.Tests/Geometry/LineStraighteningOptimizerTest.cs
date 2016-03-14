using NUnit.Framework;

namespace AForge.Math.Geometry.Tests
{
    [TestFixture]
    public class LineStraighteningOptimizerTest
    {
        private readonly IShapeOptimizer optimizer = new LineStraighteningOptimizer(3);

        [TestCase(new[] {0, 0, 10, 0, 10, 10}, new[] {0, 0, 10, 0, 10, 10})]
        [TestCase(new[] {0, 0, 10, 0, 5, 1}, new[] {0, 0, 10, 0, 5, 1})]
        [TestCase(new[] {0, 0, 10, 0, 10, 10, 5, 5}, new[] {0, 0, 10, 0, 10, 10})]
        [TestCase(new[] {0, 0, 10, 0, 10, 10, 4, 6}, new[] {0, 0, 10, 0, 10, 10})]
        [TestCase(new[] {0, 0, 10, 0, 10, 10, 6, 4}, new[] {0, 0, 10, 0, 10, 10})]
        [TestCase(new[] {0, 0, 10, 0, 10, 10, 7, 3}, new[] {0, 0, 10, 0, 10, 10})]
        [TestCase(new[] {0, 0, 10, 0, 10, 10, 8, 2}, new[] {0, 0, 10, 0, 10, 10, 8, 2})]
        [TestCase(new[] {4, 6, 0, 0, 5, 1, 10, 0, 10, 5, 10, 10}, new[] {0, 0, 10, 0, 10, 10})]
        [TestCase(new[] {6, 4, 0, 0, 6, -1, 10, 0, 9, 4, 10, 10}, new[] {0, 0, 10, 0, 10, 10})]
        public void OptimizationTest(int[] coordinates, int[] expectedCoordinates)
        {
            ShapeOptimizerTestBase.TestOptimizer(coordinates, expectedCoordinates, this.optimizer);
        }
    }
}