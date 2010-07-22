using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Nito.KitchenSink.Mathematics
{
    /// <summary>
    /// Provides financial-related calculations with reasonable constraints and special cases.
    /// </summary>
    [CLSCompliant(false)]
    public static class Financial
    {
        /// <summary>
        /// Verifies that a given argument represents a non-negative money value with no more than two decimal places.
        /// </summary>
        /// <param name="value">The money value to check.</param>
        /// <param name="name">The name of the parameter.</param>
        private static void VerifyMoneyArgument(decimal value, string name)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(name, "Money cannot have a negative value (" + name + ").");
            }

            if (Math.Round(value, 2) != value)
            {
                throw new ArgumentException("Money cannot have fractional pennies (" + name + ").", name);
            }
        }

        /// <summary>
        /// Calculates the number of periods remaining on a loan.
        /// </summary>
        /// <param name="rate">The periodic interest rate (applied every period). This must be greater than or equal to zero.</param>
        /// <param name="payment">The payment made each period. This must be greater than or equal to zero and not contain fractional pennies.</param>
        /// <param name="currentBalance">The current balance of the loan. This must be greater than or equal to zero and not contain fractional pennies.</param>
        /// <param name="futureBalance">The future balance of the loan. This must be greater than or equal to zero and not contain fractional pennies.</param>
        /// <param name="payAtBeginningOfPeriod">Set to <c>true</c> to indicate payments are made at the beginning of the period; by default, payments are made at the end of the period.</param>
        /// <returns>The number of periods remaining on the loan.</returns>
        /// <exception cref="InvalidOperationException">The number of periods could not be calculated.</exception>
        public static uint NumberOfPeriods(decimal rate, decimal payment, decimal currentBalance, decimal futureBalance = decimal.Zero, bool payAtBeginningOfPeriod = false)
        {
            Contract.Requires<ArgumentOutOfRangeException>(rate >= 0, "Rate may not be less than zero.");
            VerifyMoneyArgument(payment, "payment");
            VerifyMoneyArgument(currentBalance, "currentBalance");
            VerifyMoneyArgument(futureBalance, "futureBalance");

            var result = Exact.NumberOfPeriods(rate, -payment, currentBalance, futureBalance, payAtBeginningOfPeriod);
            if (result < decimal.Zero)
            {
                var e = new InvalidOperationException("The number of periods could not be calculated.");
                e.Data["rate"] = rate;
                e.Data["payment"] = payment;
                e.Data["currentBalance"] = currentBalance;
                e.Data["futureBalance"] = futureBalance;
                e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                e.Data["result"] = result;
                throw e;
            }

            try
            {
                return checked((uint)Math.Ceiling(result));
            }
            catch (OverflowException ex)
            {
                var e = new InvalidOperationException("The number of periods could not be calculated.", ex);
                e.Data["rate"] = rate;
                e.Data["payment"] = payment;
                e.Data["currentBalance"] = currentBalance;
                e.Data["futureBalance"] = futureBalance;
                e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                e.Data["result"] = result;
                throw e;
            }
        }

        /// <summary>
        /// Calculates the minimum payment for a loan.
        /// </summary>
        /// <param name="rate">The periodic interest rate (applied every period). This must be greater than or equal to zero.</param>
        /// <param name="numberOfPeriods">The number of periods remaining on the loan.</param>
        /// <param name="currentBalance">The current balance of the loan. This must be greater than or equal to zero and not contain fractional pennies.</param>
        /// <param name="futureBalance">The future balance of the loan. This must be greater than or equal to zero and not contain fractional pennies.</param>
        /// <param name="payAtBeginningOfPeriod">Set to <c>true</c> to indicate payments are made at the beginning of the period; by default, payments are made at the end of the period.</param>
        /// <returns>The minimum payment made each period.</returns>
        /// <exception cref="InvalidOperationException">The payment could not be calculated.</exception>
        public static decimal Payment(decimal rate, uint numberOfPeriods, decimal currentBalance, decimal futureBalance = decimal.Zero, bool payAtBeginningOfPeriod = false)
        {
            Contract.Requires<ArgumentOutOfRangeException>(rate >= 0, "Rate may not be less than zero.");
            VerifyMoneyArgument(currentBalance, "currentBalance");
            VerifyMoneyArgument(futureBalance, "futureBalance");

            var result = -Exact.Payment(rate, numberOfPeriods, currentBalance, futureBalance, payAtBeginningOfPeriod);
            if (result < decimal.Zero)
            {
                var e = new InvalidOperationException("The payment could not be calculated.");
                e.Data["rate"] = rate;
                e.Data["numberOfPeriods"] = numberOfPeriods;
                e.Data["currentBalance"] = currentBalance;
                e.Data["futureBalance"] = futureBalance;
                e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                e.Data["result"] = result;
                throw e;
            }

            try
            {
                return checked(Math.Ceiling(result * 100) / 100);
            }
            catch (OverflowException ex)
            {
                var e = new InvalidOperationException("The payment could not be calculated.", ex);
                e.Data["rate"] = rate;
                e.Data["numberOfPeriods"] = numberOfPeriods;
                e.Data["currentBalance"] = currentBalance;
                e.Data["futureBalance"] = futureBalance;
                e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                e.Data["result"] = result;
                throw e;
            }
        }

        /// <summary>
        /// Calculates the future balance of a loan.
        /// </summary>
        /// <param name="rate">The periodic interest rate (applied every period). This must be greater than or equal to zero.</param>
        /// <param name="numberOfPeriods">The total number of periods.</param>
        /// <param name="payment">The payment made each period. This must be greater than or equal to zero and not contain fractional pennies.</param>
        /// <param name="currentBalance">The current balance of the loan. This must be greater than or equal to zero and not contain fractional pennies.</param>
        /// <param name="payAtBeginningOfPeriod">Set to <c>true</c> to indicate payments are made at the beginning of the period; by default, payments are made at the end of the period.</param>
        /// <returns>The future balance of the loan.</returns>
        /// <exception cref="InvalidOperationException">The future balance could not be calculated.</exception>
        public static decimal FutureBalance(decimal rate, uint numberOfPeriods, decimal payment, decimal currentBalance, bool payAtBeginningOfPeriod = false)
        {
            Contract.Requires<ArgumentOutOfRangeException>(rate >= 0, "Rate may not be less than zero.");
            VerifyMoneyArgument(payment, "payment");
            VerifyMoneyArgument(currentBalance, "currentBalance");

            var result = Exact.FutureValue(rate, numberOfPeriods, -payment, currentBalance, payAtBeginningOfPeriod);
            try
            {
                return checked(Math.Ceiling(result * 100) / 100);
            }
            catch (OverflowException ex)
            {
                var e = new InvalidOperationException("The future balance could not be calculated.", ex);
                e.Data["rate"] = rate;
                e.Data["numberOfPeriods"] = numberOfPeriods;
                e.Data["payment"] = payment;
                e.Data["currentBalance"] = currentBalance;
                e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                e.Data["result"] = result;
                throw e;
            }
        }

        /// <summary>
        /// Calculates the principal portion of a specific payment on a loan.
        /// </summary>
        /// <param name="rate">The periodic interest rate (applied every period). This must be greater than or equal to zero.</param>
        /// <param name="numberOfPeriods">The total number of periods.</param>
        /// <param name="currentBalance">The current balance of the loan. This must be greater than or equal to zero and not contain fractional pennies.</param>
        /// <param name="period">The period for which to calculate the principal portion. This must be in the range <c>[1,<paramref name="numberOfPeriods"/>]</c> and defaults to <c>1</c>.</param>
        /// <param name="futureBalance">The future balance of the loan. This must be greater than or equal to zero and not contain fractional pennies.</param>
        /// <param name="payAtBeginningOfPeriod">Set to <c>true</c> to indicate payments are made at the beginning of the period; by default, payments are made at the end of the period.</param>
        /// <returns>The principal portion of the specific payment.</returns>
        /// <exception cref="InvalidOperationException">The principal portion of the payment could not be calculated.</exception>
        public static decimal PrincipalPayment(decimal rate, uint numberOfPeriods, decimal currentBalance, int period = 1, decimal futureBalance = decimal.Zero, bool payAtBeginningOfPeriod = false)
        {
            Contract.Requires<ArgumentOutOfRangeException>(rate >= 0, "Rate may not be less than zero.");
            VerifyMoneyArgument(currentBalance, "currentBalance");
            VerifyMoneyArgument(futureBalance, "futureBalance");

            var result = -Exact.PrincipalPayment(rate, numberOfPeriods, currentBalance, period, futureBalance, payAtBeginningOfPeriod);
            if (result < decimal.Zero)
            {
                var e = new InvalidOperationException("The principal portion of the payment could not be calculated.");
                e.Data["rate"] = rate;
                e.Data["numberOfPeriods"] = numberOfPeriods;
                e.Data["currentBalance"] = currentBalance;
                e.Data["period"] = period;
                e.Data["futureBalance"] = futureBalance;
                e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                e.Data["result"] = result;
                throw e;
            }

            try
            {
                return checked(Math.Ceiling(result * 100) / 100);
            }
            catch (OverflowException ex)
            {
                var e = new InvalidOperationException("The principal portion of the payment could not be calculated.", ex);
                e.Data["rate"] = rate;
                e.Data["numberOfPeriods"] = numberOfPeriods;
                e.Data["currentBalance"] = currentBalance;
                e.Data["period"] = period;
                e.Data["futureBalance"] = futureBalance;
                e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                e.Data["result"] = result;
                throw e;
            }
        }

        /// <summary>
        /// Calculates the interest portion of a payment on a loan.
        /// </summary>
        /// <param name="rate">The periodic interest rate (applied every period). This must be greater than or equal to zero.</param>
        /// <param name="numberOfPeriods">The total number of periods.</param>
        /// <param name="currentBalance">The current balance of the loan. This must be greater than or equal to zero and not contain fractional pennies.</param>
        /// <param name="period">The period for which to calculate the principal portion. This must be in the range <c>[1,<paramref name="numberOfPeriods"/>]</c> and defaults to <c>1</c>.</param>
        /// <param name="futureBalance">The future balance of the loan. This must be greater than or equal to zero and not contain fractional pennies.</param>
        /// <param name="payAtBeginningOfPeriod">Set to <c>true</c> to indicate payments are made at the beginning of the period; by default, payments are made at the end of the period.</param>
        /// <returns>The interest portion of the specific payment.</returns>
        /// <exception cref="InvalidOperationException">The interest portion of the payment could not be calculated.</exception>
        public static decimal InterestPayment(decimal rate, uint numberOfPeriods, decimal currentBalance, int period = 1, decimal futureBalance = decimal.Zero, bool payAtBeginningOfPeriod = false)
        {
            Contract.Requires<ArgumentOutOfRangeException>(rate >= 0, "Rate may not be less than zero.");
            VerifyMoneyArgument(currentBalance, "currentBalance");
            VerifyMoneyArgument(futureBalance, "futureBalance");

            var result = -Exact.InterestPayment(rate, numberOfPeriods, currentBalance, period, futureBalance, payAtBeginningOfPeriod);
            if (result < decimal.Zero)
            {
                var e = new InvalidOperationException("The interest portion of the payment could not be calculated.");
                e.Data["rate"] = rate;
                e.Data["numberOfPeriods"] = numberOfPeriods;
                e.Data["currentBalance"] = currentBalance;
                e.Data["period"] = period;
                e.Data["futureBalance"] = futureBalance;
                e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                e.Data["result"] = result;
                throw e;
            }

            try
            {
                return checked(Math.Floor(result * 100) / 100);
            }
            catch (OverflowException ex)
            {
                var e = new InvalidOperationException("The interest portion of the payment could not be calculated.", ex);
                e.Data["rate"] = rate;
                e.Data["numberOfPeriods"] = numberOfPeriods;
                e.Data["currentBalance"] = currentBalance;
                e.Data["period"] = period;
                e.Data["futureBalance"] = futureBalance;
                e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                e.Data["result"] = result;
                throw e;
            }
        }

        /// <summary>
        /// Provides financial-related calculations with exact definitions (no special cases).
        /// </summary>
        public static class Exact
        {
            /// <summary>
            /// Calculates the total number of periods in an annuity.
            /// </summary>
            /// <param name="rate">The periodic interest rate (applied every period). This may be zero.</param>
            /// <param name="payment">The payment made each period. This should be a negative number if payments are being made on a loan.</param>
            /// <param name="currentValue">The current value of the annuity.</param>
            /// <param name="futureValue">The future value of the annuity.</param>
            /// <param name="payAtBeginningOfPeriod">Set to <c>true</c> to indicate payments are made at the beginning of the period; by default, payments are made at the end of the period.</param>
            /// <returns>The total number of periods in an annuity.</returns>
            /// <exception cref="InvalidOperationException">The number of periods could not be calculated.</exception>
            public static decimal NumberOfPeriods(decimal rate, decimal payment, decimal currentValue, decimal futureValue = decimal.Zero, bool payAtBeginningOfPeriod = false)
            {
                try
                {
                    checked
                    {
                        if (rate == decimal.Zero)
                        {
                            return -(currentValue + futureValue) / payment;
                        }

                        decimal type = payAtBeginningOfPeriod ? decimal.One : decimal.Zero;
                        decimal numerator = payment + payment * rate * type - futureValue * rate;
                        decimal denominator = currentValue * rate + payment + payment * rate * type;
                        return (decimal)Math.Log((double)(numerator / denominator), (double)(rate + decimal.One));
                    }
                }
                catch (OverflowException ex)
                {
                    var e = new InvalidOperationException("The number of periods could not be calculated.", ex);
                    e.Data["rate"] = rate;
                    e.Data["payment"] = payment;
                    e.Data["currentValue"] = currentValue;
                    e.Data["futureValue"] = futureValue;
                    e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                    throw e;
                }
                catch (DivideByZeroException ex)
                {
                    var e = new InvalidOperationException("The number of periods could not be calculated.", ex);
                    e.Data["rate"] = rate;
                    e.Data["payment"] = payment;
                    e.Data["currentValue"] = currentValue;
                    e.Data["futureValue"] = futureValue;
                    e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                    throw e;
                }
            }

            /// <summary>
            /// Calculates the regular payment in an annuity.
            /// </summary>
            /// <param name="rate">The periodic interest rate (applied every period). This may be zero.</param>
            /// <param name="numberOfPeriods">The total number of periods.</param>
            /// <param name="currentValue">The current value of the annuity.</param>
            /// <param name="futureValue">The future value of the annuity.</param>
            /// <param name="payAtBeginningOfPeriod">Set to <c>true</c> to indicate payments are made at the beginning of the period; by default, payments are made at the end of the period.</param>
            /// <returns>The payment made each period in an annuity.</returns>
            /// <exception cref="InvalidOperationException">The payment could not be calculated.</exception>
            public static decimal Payment(decimal rate, decimal numberOfPeriods, decimal currentValue, decimal futureValue = decimal.Zero, bool payAtBeginningOfPeriod = false)
            {
                try
                {
                    checked
                    {
                        if (rate == decimal.Zero)
                        {
                            return -(currentValue + futureValue) / numberOfPeriods;
                        }

                        decimal type = payAtBeginningOfPeriod ? decimal.One : decimal.Zero;
                        decimal rateFactor = (decimal)Math.Pow((double)(rate + decimal.One), (double)numberOfPeriods);
                        decimal numerator = rate * futureValue + rate * currentValue * rateFactor;
                        decimal denominator = (rate * type + decimal.One) * (rateFactor - decimal.One);
                        return - numerator / denominator;
                    }
                }
                catch (OverflowException ex)
                {
                    var e = new InvalidOperationException("The payment could not be calculated.", ex);
                    e.Data["rate"] = rate;
                    e.Data["numberOfPeriods"] = numberOfPeriods;
                    e.Data["currentValue"] = currentValue;
                    e.Data["futureValue"] = futureValue;
                    e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                    throw e;
                }
                catch (DivideByZeroException ex)
                {
                    var e = new InvalidOperationException("The payment could not be calculated.", ex);
                    e.Data["rate"] = rate;
                    e.Data["numberOfPeriods"] = numberOfPeriods;
                    e.Data["currentValue"] = currentValue;
                    e.Data["futureValue"] = futureValue;
                    e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                    throw e;
                }
            }

            /// <summary>
            /// Calculates the current value of an annuity.
            /// </summary>
            /// <param name="rate">The periodic interest rate (applied every period). This may be zero.</param>
            /// <param name="numberOfPeriods">The total number of periods.</param>
            /// <param name="payment">The payment made each period. This should be a negative number if payments are being made on a loan.</param>
            /// <param name="futureValue">The future value of the annuity.</param>
            /// <param name="payAtBeginningOfPeriod">Set to <c>true</c> to indicate payments are made at the beginning of the period; by default, payments are made at the end of the period.</param>
            /// <returns>The current value of an annuity.</returns>
            /// <exception cref="InvalidOperationException">The current value could not be calculated.</exception>
            public static decimal CurrentValue(decimal rate, decimal numberOfPeriods, decimal payment, decimal futureValue = decimal.Zero, bool payAtBeginningOfPeriod = false)
            {
                try
                {
                    checked
                    {
                        if (rate == decimal.Zero)
                        {
                            return -futureValue - payment * numberOfPeriods;
                        }

                        decimal type = payAtBeginningOfPeriod ? decimal.One : decimal.Zero;
                        decimal rateFactor = (decimal)Math.Pow((double)(rate + decimal.One), (double)numberOfPeriods);
                        decimal firstFraction = - futureValue / rateFactor;
                        decimal secondFraction = (-payment * (rate * type + decimal.One) * (rateFactor - decimal.One)) / (rate * rateFactor);
                        return firstFraction + secondFraction;
                    }
                }
                catch (OverflowException ex)
                {
                    var e = new InvalidOperationException("The current value could not be calculated.", ex);
                    e.Data["rate"] = rate;
                    e.Data["numberOfPeriods"] = numberOfPeriods;
                    e.Data["payment"] = payment;
                    e.Data["futureValue"] = futureValue;
                    e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                    throw e;
                }
                catch (DivideByZeroException ex)
                {
                    var e = new InvalidOperationException("The current value could not be calculated.", ex);
                    e.Data["rate"] = rate;
                    e.Data["numberOfPeriods"] = numberOfPeriods;
                    e.Data["payment"] = payment;
                    e.Data["futureValue"] = futureValue;
                    e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                    throw e;
                }
            }

            /// <summary>
            /// Calculates the future value of an annuity.
            /// </summary>
            /// <param name="rate">The periodic interest rate (applied every period). This may be zero.</param>
            /// <param name="numberOfPeriods">The total number of periods.</param>
            /// <param name="payment">The payment made each period. This should be a negative number if payments are being made on a loan.</param>
            /// <param name="currentValue">The current value of the annuity.</param>
            /// <param name="payAtBeginningOfPeriod">Set to <c>true</c> to indicate payments are made at the beginning of the period; by default, payments are made at the end of the period.</param>
            /// <returns>The future value of an annuity.</returns>
            /// <exception cref="InvalidOperationException">The future value could not be calculated.</exception>
            public static decimal FutureValue(decimal rate, decimal numberOfPeriods, decimal payment, decimal currentValue, bool payAtBeginningOfPeriod = false)
            {
                try
                {
                    checked
                    {
                        if (rate == decimal.Zero)
                        {
                            return -currentValue - payment * numberOfPeriods;
                        }

                        decimal type = payAtBeginningOfPeriod ? decimal.One : decimal.Zero;
                        decimal rateFactor = (decimal)Math.Pow((double)(rate + decimal.One), (double)numberOfPeriods);
                        decimal firstFraction = -currentValue * rateFactor;
                        decimal secondFraction = (-payment * (rate * type + decimal.One) * (rateFactor - decimal.One)) / rate;
                        return firstFraction + secondFraction;
                    }
                }
                catch (OverflowException ex)
                {
                    var e = new InvalidOperationException("The future value could not be calculated.", ex);
                    e.Data["rate"] = rate;
                    e.Data["numberOfPeriods"] = numberOfPeriods;
                    e.Data["payment"] = payment;
                    e.Data["currentValue"] = currentValue;
                    e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                    throw e;
                }
                catch (DivideByZeroException ex)
                {
                    var e = new InvalidOperationException("The future value could not be calculated.", ex);
                    e.Data["rate"] = rate;
                    e.Data["numberOfPeriods"] = numberOfPeriods;
                    e.Data["payment"] = payment;
                    e.Data["currentValue"] = currentValue;
                    e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                    throw e;
                }
            }

            /// <summary>
            /// Calculates the principal portion of a specific payment in an annuity.
            /// </summary>
            /// <param name="rate">The periodic interest rate (applied every period). This may be zero.</param>
            /// <param name="numberOfPeriods">The total number of periods.</param>
            /// <param name="currentValue">The current value of the annuity.</param>
            /// <param name="period">The period for which to calculate the principal portion. This must be in the range <c>[1,<paramref name="numberOfPeriods"/>]</c> and defaults to <c>1</c>.</param>
            /// <param name="futureValue">The future value of the annuity.</param>
            /// <param name="payAtBeginningOfPeriod">Set to <c>true</c> to indicate payments are made at the beginning of the period; by default, payments are made at the end of the period.</param>
            /// <returns>The principal portion of the specific payment.</returns>
            /// <exception cref="ArgumentOutOfRangeException">Period must be in the range [1, numberOfPeriods].</exception>
            /// <exception cref="InvalidOperationException">The principal portion of the payment could not be calculated.</exception>
            public static decimal PrincipalPayment(decimal rate, decimal numberOfPeriods, decimal currentValue, int period = 1, decimal futureValue = decimal.Zero, bool payAtBeginningOfPeriod = false)
            {
                Contract.Requires<ArgumentOutOfRangeException>(period >= 1 && period <= numberOfPeriods, "Period must be in the range [1, numberOfPeriods].");

                try
                {
                    checked
                    {
                        var payment = Payment(rate, numberOfPeriods, currentValue, futureValue, payAtBeginningOfPeriod);

                        // TODO: double-check sign
                        return currentValue - FutureValue(rate, period, payment, currentValue, payAtBeginningOfPeriod);
                    }
                }
                catch (OverflowException ex)
                {
                    var e = new InvalidOperationException("The principal portion of the payment could not be calculated.", ex);
                    e.Data["rate"] = rate;
                    e.Data["numberOfPeriods"] = numberOfPeriods;
                    e.Data["currentValue"] = currentValue;
                    e.Data["period"] = period;
                    e.Data["futureValue"] = futureValue;
                    e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                    throw e;
                }
            }

            /// <summary>
            /// Calculates the interest portion of a payment in an annuity.
            /// </summary>
            /// <param name="rate">The periodic interest rate (applied every period). This may be zero.</param>
            /// <param name="numberOfPeriods">The total number of periods.</param>
            /// <param name="currentValue">The current value of the annuity.</param>
            /// <param name="period">The period for which to calculate the interest portion. This must be in the range <c>[1,<paramref name="numberOfPeriods"/>]</c> and defaults to <c>1</c>.</param>
            /// <param name="futureValue">The future value of the annuity.</param>
            /// <param name="payAtBeginningOfPeriod">Set to <c>true</c> to indicate payments are made at the beginning of the period; by default, payments are made at the end of the period.</param>
            /// <returns>The interest portion of the specific payment.</returns>
            /// <exception cref="ArgumentOutOfRangeException">Period must be in the range [1, numberOfPeriods].</exception>
            /// <exception cref="InvalidOperationException">The interest portion of the payment could not be calculated.</exception>
            public static decimal InterestPayment(decimal rate, decimal numberOfPeriods, decimal currentValue, int period = 1, decimal futureValue = decimal.Zero, bool payAtBeginningOfPeriod = false)
            {
                Contract.Requires<ArgumentOutOfRangeException>(period >= 1 && period <= numberOfPeriods, "Period must be in the range [1, numberOfPeriods].");

                try
                {
                    checked
                    {
                        var payment = Payment(rate, numberOfPeriods, currentValue, futureValue, payAtBeginningOfPeriod);

                        // TODO: double-check sign
                        return payment - currentValue + FutureValue(rate, period, payment, currentValue, payAtBeginningOfPeriod);
                    }
                }
                catch (OverflowException ex)
                {
                    var e = new InvalidOperationException("The interest portion of the payment could not be calculated.", ex);
                    e.Data["rate"] = rate;
                    e.Data["numberOfPeriods"] = numberOfPeriods;
                    e.Data["currentValue"] = currentValue;
                    e.Data["period"] = period;
                    e.Data["futureValue"] = futureValue;
                    e.Data["payAtBeginningOfPeriod"] = payAtBeginningOfPeriod;
                    throw e;
                }
            }
        }
    }
}
