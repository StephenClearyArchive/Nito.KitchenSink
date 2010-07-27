using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.Dynamic
{
    // http://blogs.msdn.com/lucabol/archive/2009/02/05/simulating-inumeric-with-dynamic-in-c-4-0.aspx
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DynamicNumeric<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicNumeric&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public DynamicNumeric(T value = default(T))
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value { get; set; }

        /// <summary>
        /// Gets the dynamic value.
        /// </summary>
        /// <value>The dynamic value.</value>
        public dynamic DynamicValue
        {
            get
            {
                return this.Value;
            }

            set
            {
                this.Value = value;
            }
        }

        public DynamicNumeric<U> Cast<U>()
        {
            return new DynamicNumeric<U>((U)this.DynamicValue);
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="a">A.</param>
        /// <returns>The result of the operator.</returns>
        public static DynamicNumeric<T> operator +(DynamicNumeric<T> a)
        {
            return a;
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="a">A.</param>
        /// <returns>The result of the operator.</returns>
        public static DynamicNumeric<T> operator -(DynamicNumeric<T> a)
        {
            return new DynamicNumeric<T>((T)(-a.DynamicValue));
        }

        /// <summary>
        /// Implements the operator ~.
        /// </summary>
        /// <param name="a">A.</param>
        /// <returns>The result of the operator.</returns>
        public static DynamicNumeric<T> operator ~(DynamicNumeric<T> a)
        {
            return new DynamicNumeric<T>((T)(~a.DynamicValue));
        }

        /// <summary>
        /// Implements the operator ++.
        /// </summary>
        /// <param name="a">A.</param>
        /// <returns>The result of the operator.</returns>
        public static DynamicNumeric<T> operator ++(DynamicNumeric<T> a)
        {
            return new DynamicNumeric<T>((T)(a.DynamicValue + 1));
        }

        public static DynamicNumeric<T> operator --(DynamicNumeric<T> a)
        {
            return new DynamicNumeric<T>((T)(a.DynamicValue - 1));
        }

        public static DynamicNumeric<T> operator +(DynamicNumeric<T> a, DynamicNumeric<T> b)
        {
            return new DynamicNumeric<T>((T)(a.DynamicValue + b.DynamicValue));
        }

        public static DynamicNumeric<T> operator -(DynamicNumeric<T> a, DynamicNumeric<T> b)
        {
            return new DynamicNumeric<T>((T)(a.DynamicValue - b.DynamicValue));
        }

        public static DynamicNumeric<T> operator *(DynamicNumeric<T> a, DynamicNumeric<T> b)
        {
            return new DynamicNumeric<T>((T)(a.DynamicValue * b.DynamicValue));
        }

        public static DynamicNumeric<T> operator /(DynamicNumeric<T> a, DynamicNumeric<T> b)
        {
            return new DynamicNumeric<T>((T)(a.DynamicValue / b.DynamicValue));
        }

        public static DynamicNumeric<T> operator %(DynamicNumeric<T> a, DynamicNumeric<T> b)
        {
            return new DynamicNumeric<T>((T)(a.DynamicValue % b.DynamicValue));
        }

        public static DynamicNumeric<T> operator &(DynamicNumeric<T> a, DynamicNumeric<T> b)
        {
            return new DynamicNumeric<T>((T)(a.DynamicValue & b.DynamicValue));
        }

        public static DynamicNumeric<T> operator |(DynamicNumeric<T> a, DynamicNumeric<T> b)
        {
            return new DynamicNumeric<T>((T)(a.DynamicValue | b.DynamicValue));
        }

        public static DynamicNumeric<T> operator ^(DynamicNumeric<T> a, DynamicNumeric<T> b)
        {
            return new DynamicNumeric<T>((T)(a.DynamicValue ^ b.DynamicValue));
        }

        public static DynamicNumeric<T> operator <<(DynamicNumeric<T> a, int b)
        {
            return new DynamicNumeric<T>((T)(a.DynamicValue << b));
        }

        public static DynamicNumeric<T> operator >>(DynamicNumeric<T> a, int b)
        {
            return new DynamicNumeric<T>((T)(a.DynamicValue >> b));
        }

        public static bool operator ==(DynamicNumeric<T> a, DynamicNumeric<T> b)
        {
            return a.DynamicValue == b.DynamicValue;
        }

        public static bool operator !=(DynamicNumeric<T> a, DynamicNumeric<T> b)
        {
            return a.DynamicValue != b.DynamicValue;
        }

        public static bool operator <(DynamicNumeric<T> a, DynamicNumeric<T> b)
        {
            return a.DynamicValue < b.DynamicValue;
        }

        public static bool operator >(DynamicNumeric<T> a, DynamicNumeric<T> b)
        {
            return a.DynamicValue > b.DynamicValue;
        }

        public static bool operator <=(DynamicNumeric<T> a, DynamicNumeric<T> b)
        {
            return a.DynamicValue <= b.DynamicValue;
        }

        public static bool operator >=(DynamicNumeric<T> a, DynamicNumeric<T> b)
        {
            return a.DynamicValue >= b.DynamicValue;
        }
    }

    public static class Math
    {
        public static T Abs<T>(T a)
        {
            // TODO: If T is unsigned, return a
            try
            {
                var dT = DynamicStaticTypeMembers.Create<T>();
                return dT.Abs(a);
            }
            catch
            {
                dynamic da = a;
                return Math.Abs(da);
            }
        }

        // Ceiling, Floor?

        public static T DivRem<T>(T dividend, T divisor, out T remainder)
        {
            try
            {
                var dT = DynamicStaticTypeMembers.Create<T>();
                return dT.DivRem(dividend, divisor, out remainder);
            }
            catch
            {
                var dT = DynamicStaticTypeMembers.Create(typeof(Math));
                return dT.DivRem(dividend, divisor, out remainder);
            }
        }

        public static T Max<T>(T a, T b)
        {

        }
    }
}
