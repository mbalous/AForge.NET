using NUnit.Framework;

namespace AForge.Math.Geometry.Tests
{
    [TestFixture]
    public class FlatAnglesOptimizerTest
    {
        private readonly IShapeOptimizer optimizer = new FlatAnglesOptimizer(160);

        [TestCase(new[] {0, 0, 10, 0, 10, 10}, new[] {0, 0, 10, 0, 10, 10})]
        [TestCase(new[] {0, 0, 20, 0, 10, 1}, new[] {0, 0, 20, 0, 10, 1})]
        [TestCase(new[] {0, 0, 10, 1, 20, 0, 20, 20}, new[] {0, 0, 20, 0, 20, 20})]
        [TestCase(new[] {0, 0, 5, 1, 10, 0, 10, 10}, new[] {0, 0, 5, 1, 10, 0, 10, 10})]
        [TestCase(new[] {0, 0, 20, 0, 20, 20, 11, 9}, new[] {0, 0, 20, 0, 20, 20})]
        [TestCase(new[] {0, 0, 20, 0, 20, 20, 9, 11}, new[] {0, 0, 20, 0, 20, 20})]
        [TestCase(new[] {9, 11, 0, 0, 10, 1, 20, 0, 21, 10, 20, 20}, new[] {0, 0, 20, 0, 20, 20})]
        [TestCase(new[] {11, 9, 0, 0, 10, -1, 20, 0, 19, 10, 20, 20}, new[] {0, 0, 20, 0, 20, 20})]
        public void OptimizationTest(int[] coordinates, int[] expectedCoordinates)
        {
            ShapeOptimizerTestBase.TestOptimizer(coordinates, expectedCoordinates, this.optimizer);
        }
    }
}