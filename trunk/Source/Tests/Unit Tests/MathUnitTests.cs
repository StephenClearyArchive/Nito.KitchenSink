using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Nito.KitchenSink.Mathematics;

namespace Tests.Unit_Tests
{
    [TestClass]
    public class MathUnitTests
    {
        [TestMethod]
        public void NumberOfPeriods_SampleMortgageData()
        {
            Assert.AreEqual(30u * 12, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 90333m));
            Assert.AreEqual(29u * 12 + 6, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 89645.68m));
            Assert.AreEqual(29u * 12 + 5, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 89531.82m));
            Assert.AreEqual(29u * 12 + 3, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 89292.49m));
            Assert.AreEqual(28u * 12 + 11, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 88777.19m));
            Assert.AreEqual(28u * 12 + 6, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 88259.80m));
            Assert.AreEqual(28u * 12 + 4, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 87940.31m));
            Assert.AreEqual(28u * 12 + 3, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 87819.52m));
            Assert.AreEqual(28u * 12 + 2, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 87698.24m));
            Assert.AreEqual(27u * 12 + 10, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 87176.46m));
            Assert.AreEqual(27u * 12 + 9, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 87052.56m));
            Assert.AreEqual(27u * 12 + 8, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 86928.16m));
            Assert.AreEqual(27u * 12 + 7, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 86803.26m));
            Assert.AreEqual(27u * 12 + 5, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 86577.85m));
            Assert.AreEqual(27u * 12 + 1, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 86140.76m));
            Assert.AreEqual(26u * 12 + 8, Financial.NumberOfPeriods(.04875m / 12, 478.05m, 85500.00m));
        }

#if NO
        [TestMethod]
        public void Test()
        {
            int periods = Financial.NumberOfPeriods(0.04875m / 12, -478.05m, 85500.00m);
            decimal low = 0;
            decimal high = 478.05m;
            var test = Numeric.Bisect(ref low, ref high, 0.01m, extra =>
            {
                int adjustedPeriods = Financial.NumberOfPeriods(0.04875m / 12, -478.05m, 85500.00m - extra);
                if (adjustedPeriods <= periods)
                    return 1;
                return -1;
            });

            Assert.IsFalse(test);
        }
#endif
    }
}
