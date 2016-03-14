using NUnit.Framework;

namespace AForge.Math.Tests
{
    [TestFixture]
    public class StatisticsTest
    {
        [Test]
        public void ModeTest()
        {
            int[] values = {1, 2, 2, 3, 3, 3};
            int mode = Statistics.Mode(values);
            Assert.AreEqual(3, mode);

            values = new[] {1, 1, 1, 2, 2, 2};
            mode = Statistics.Mode(values);
            Assert.AreEqual(3, mode);

            values = new[] {2, 2, 2, 1, 1, 1};
            mode = Statistics.Mode(values);
            Assert.AreEqual(0, mode);

            values = new[] {0, 0, 0, 0, 0, 0};
            mode = Statistics.Mode(values);
            Assert.AreEqual(0, mode);

            values = new[] {1, 1, 2, 3, 6, 8, 11, 12, 7, 3};
            mode = Statistics.Mode(values);
            Assert.AreEqual(7, mode);
        }
    }
}