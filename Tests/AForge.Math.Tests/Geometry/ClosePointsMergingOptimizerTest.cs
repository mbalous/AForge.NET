using NUnit.Framework;

namespace AForge.Math.Geometry.Tests
{
    [TestFixture]
    public class ClosePointsMergingOptimizerTest
    {
        private readonly IShapeOptimizer optimizer = new ClosePointsMergingOptimizer(3);

        [TestCase(new[] {0, 0, 10, 0, 10, 10}, new[] {0, 0, 10, 0, 10, 10})]
        [TestCase(new[] {0, 0, 10, 0, 1, 1}, new[] {0, 0, 10, 0, 1, 1})]
        [TestCase(new[] {0, 0, 10, 0, 10, 10, 2, 2}, new[] {1, 1, 10, 0, 10, 10})]
        [TestCase(new[] {0, 0, 10, 0, 10, 10, 3, 3}, new[] {0, 0, 10, 0, 10, 10, 3, 3})]
        [TestCase(new[] {0, 0, 8, 0, 10, 2, 10, 10}, new[] {0, 0, 9, 1, 10, 10})]
        [TestCase(new[] {2, 0, 8, 0, 10, 2, 10, 8, 8, 10, 0, 2}, new[] {1, 1, 9, 1, 9, 9})]
        public void OptimizationTest(int[] coordinates, int[] expectedCoordinates)
        {
            ShapeOptimizerTestBase.TestOptimizer(coordinates, expectedCoordinates, this.optimizer);
        }
    }
}