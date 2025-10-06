using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace PaintTogether.Common.Utilities
{
    // Ngl these are just copied from either atsalg or terrarias utils
    public static class MathUtils
    {
        #region Easings

        /// <summary>
        /// see https://www.desmos.com/calculator/2cbfpz3e7y for info
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float EaseInSine(float x)
        {
            // limit value
            x = MathHelper.Clamp(x, 0, 1);

            return 1 - (float)Math.Cos(Math.PI * x / 2);
        }
        
        /// <summary>
        /// see https://www.desmos.com/calculator/oaarwzk9u2 for info
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float EaseOutSine(float x)
        {
            x = MathHelper.Clamp(x, 0, 1);
            return (float)Math.Sin(Math.PI * x / 2);
        }

        /// <summary>
        /// see https://www.desmos.com/calculator/nmct2crors for info
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float EaseinInOutSine(float x)
        {
            x = MathHelper.Clamp(x, 0, 1);
            return (1 - (float)Math.Cos(Math.PI * x)) / 2;
        }

        /// <summary>
        /// see https://www.desmos.com/calculator/ljzwdei7qe for info
        /// </summary>
        /// <param name="x"></param>
        /// <param name="exponent">the easing strength, higher values mean more sudden movement</param>
        /// <returns></returns>
        public static float EaseInPolynomial(float x, float exponent)
        {
            x = MathHelper.Clamp(x, 0, 1);
            return (float)Math.Pow(x, exponent);
        }

        /// <summary>
        /// see https://www.desmos.com/calculator/ssfyfqlbku for info
        /// </summary>
        /// <param name="x"></param>
        /// <param name="exponent">the easing strength, higher values mean more sudden movement</param>
        /// <returns></returns>
        public static float EaseOutPolynomial(float x, float exponent)
        {
            x = MathHelper.Clamp(x, 0, 1f);
            return (float)Math.Pow(x, 1f / exponent);
        }

        /// <summary>
        /// see https://www.desmos.com/calculator/jsl9ry2ujv for info
        /// </summary>
        /// <param name="x"></param>
        /// <param name="exponent"></param>
        /// <returns></returns>
        public static float EaseInOutPolynomial(float x, float exponent)
        {
            x = MathHelper.Clamp(x, 0, 1);
            Func<float, float> fx = x => 1 - (float)Math.Pow(-2 * x + 2, exponent) / 2;
            return x < 0.5f ? 1 - fx(1 - x) : fx(x);
        }

        /// <summary>
        /// see https://easings.net/#easeInOutBack for info
        /// i couldnt get it to work in desmos lol
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float BackInOut(float x)
        {
            x = MathHelper.Clamp(x, 0, 1);

            float c1 = 1.70158f;
            float c2 = c1 * 1.525f;

            return x < 0.5
              ? MathF.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2) / 2
              : (MathF.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
        }

        /// <summary>
        /// see https://www.desmos.com/calculator/9ry8texblg for info
        /// </summary>
        /// <param name="x"></param>
        /// <param name="exponent">not actually a legit exponent, but functions like the exponent parameter of <see cref="EaseInOutPolynomial(float, float)"/></param>
        /// <returns></returns>
        public static float BackIn(float x, float exponent = 1.70158f)
        {
            exponent = MathHelper.Clamp(exponent, 1, float.PositiveInfinity);
            float c1 = exponent;
            float c3 = c1 + 1; // Idk where c2 went ask easings.net
            return (c3 * x * x * x) - (c1 * x * x);
        }

        #endregion
        
        #region Arithmetic
        
        /// <summary>
        /// Remaps a value in a range into a different value in a range
        /// for example : reMap(25,0,100,0,200) returns 50.
        /// because 25 into 100 from zero is the same as 50 into 200 from zero
        /// </summary>
        /// <param name="value">the value to be remapped</param>
        /// <param name="start1">the lower bound of the CURRENT range of values</param>
        /// <param name="end1">the upper bound of the CURRENT range of values</param>
        /// <param name="start2">the lower bound of the TARGET range of values</param>
        /// <param name="end2">the upper bound of the TARGET range of values</param>
        /// <returns>float</returns>
        public static float ReMap(float value, float start1, float end1, float start2, float end2)
        {
            float outValue = start2 + (end2 - start2) * ((value - start1) / (end1 - start1));
            if (float.IsNaN(outValue))
            {
                return -1;
            }
            if (float.IsInfinity(outValue))
            {
                return -1;
            }
            return MathHelper.Clamp(outValue, start2, end2);
        }

        /// <summary>
        /// Returns true if a value is within a given lower and upper bound
        /// </summary>
        /// <param name="value">the value to check</param>
        /// <param name="lowerBound">the largest value the number can be</param>
        /// <param name="upperBound">the smallest value the number can be</param>
        /// <returns>bool</returns>
        public static bool InRange(float value, float lowerBound, float upperBound, bool exclusive = false)
        {
            if (exclusive)
            {
                if (value > lowerBound && value < upperBound)
                {
                    return true;
                }
            }
            else
            {
                if (value >= lowerBound && value <= upperBound)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// A sin function with tailorable settings
        /// </summary>
        /// <param name="x">the x value to compute this sin of</param>
        /// <param name="interval">the interval between peaks, a value of 1 would look like this : https://www.desmos.com/calculator/qlwwaov3ww</param>
        /// <param name="amplitude">the multiplier of the peaks and troughs for the wave</param>
        /// <param name="XOffset">shifts the graph this far in the x direction</param>
        /// <param name="YOffset">shifts the graph this far in the y direction</param>
        /// <returns></returns>
        public static float CustomSine(float x, float interval = MathHelper.PiOver2, float amplitude = 1, float XOffset = 0, float YOffset = 0)
        {
            if (interval == 0)
            {
                return 0;
            }
            return (float)((Math.Sin(Math.PI * x * 2 / interval - XOffset) + YOffset) * amplitude);
        }

        /// <summary>
        /// https://www.desmos.com/calculator/x9zub9z75f <br/>
        /// A square wave with smoothing between positive and negative state.
        /// </summary>
        /// <param name="x">value to compute</param>
        /// <param name="smoothness">Controls how sharply the wave goes between its max and minimum. Reccomended ~0.1</param>
        /// <param name="interval">the interval between peaks of the wave</param>
        /// <param name="amplitude">the maximum value the wave reaches</param>
        /// <param name="XOffset">shifts the whole graph this much in the x direction</param>
        /// <param name="YOffset">shifts the whole graph this much in the y direction</param>
        /// <returns></returns>
        public static float SmoothSquareWave(float x, float smoothness, float interval = 1f, float amplitude = 1f, float XOffset = 0f, float YOffset = 0f)
        {
            float f = (2f / interval) * MathF.PI * x + XOffset;
            float main = MathF.Sin(f) / ((MathF.Sqrt(f) * MathF.Sin(f)) + (smoothness * smoothness));
            float error = MathF.Sqrt(1f + (smoothness * smoothness));

            return main * error * amplitude * YOffset;
        }

        /// <summary>
        /// finds all prime factors for a given int n
        /// </summary>
        /// <param name="n">integer</param>
        /// <returns>dictionary with keys as the prime factors as the values as how many times that factor is present (effecivley just key ^ value)</returns>
        public static Dictionary<int, int> FindPrimeFactors(int n)
        {
            Dictionary<int, int> factorCounts = new();

            // Print the number of 2s that divide n
            while (n % 2 == 0)
            {
                if (factorCounts.Keys.Contains(2))
                {
                    factorCounts[2]++;
                }
                else
                {
                    factorCounts.Add(2, 1);
                }

                n /= 2;
            }

            // n must be odd at this point. So we can
            // skip one element (Note i = i +2)
            for (int i = 3; i <= Math.Sqrt(n); i += 2)
            {
                // While i divides n, print i and divide n
                while (n % i == 0)
                {
                    if (factorCounts.Keys.Contains(i))
                    {
                        factorCounts[i]++;
                    }
                    else
                    {
                        factorCounts.Add(i, 1);
                    }
                    n /= i;
                }
            }

            // This condition is to handle the case when
            // n is a prime number greater than 2
            if (n > 2)
                if (factorCounts.Keys.Contains(n))
                {
                    factorCounts[n]++;
                }
                else
                {
                    factorCounts.Add(n, 1);
                }
            return factorCounts;
        }

        /// <summary>
        /// Returns factorial of N
        /// </summary>
        /// <param name="n">integer</param>
        /// <returns>integer</returns>
        public static int Factorial(int n)
        {
            if (n <= 1) return 1;
            return n * Factorial(n - 1);
        }

        /// <summary>
        /// <see cref="MathHelper.Lerp(float, float, float)"/> but handles the jump from -pi to pi
        /// </summary>
        /// <param name="value1">value to lerp from</param>
        /// <param name="value2">value to lerp to</param>
        /// <param name="amount">lerp amount</param>
        public static float AngleLerp(float value1, float value2, float amount)
        {
            float delta = MathHelper.WrapAngle(value2 - value1);
            return value1 + delta * amount;
        }

        /// <summary>
        /// <see cref="AngleLerp(float, float, float)"/> but uses 0 to represent vertically up (+Y)
        /// </summary>
        /// <param name="value1">value to lerp from</param>
        /// <param name="value2">value to lerp to</param>
        /// <param name="amount">lerp amount</param>
        public static float AngleLerpVertical(float value1, float value2, float amount)
        {
            float lerped = AngleLerp(value1 + MathHelper.PiOver2, value2, amount);
            return lerped - MathHelper.PiOver2;
        }

        #endregion
        
        #region Vectors

        public static float ToRotation(this Vector2 v) => (float)Math.Atan2(v.Y, v.X);
        public static Vector2 ToRotationVector2(this float f) => new Vector2((float)Math.Cos(f), (float)Math.Sin(f));

        public static Vector2 RotatedBy(this Vector2 spinningpoint, double radians, Vector2 center = default(Vector2))
        {
            float num = (float)Math.Cos(radians);
            float num2 = (float)Math.Sin(radians);
            Vector2 vector = spinningpoint - center;
            Vector2 result = center;
            result.X += vector.X * num - vector.Y * num2;
            result.Y += vector.X * num2 + vector.Y * num;
            return result;
        }
        
        public static Vector2 RotatedByRandom(this Vector2 spinninpoint, double maxRadians) => spinninpoint.RotatedBy(Main.rand.NextDouble() * maxRadians - Main.rand.NextDouble() * maxRadians);


        
        public static bool HasNaNs(this Vector2 vec)
        {
            if (!float.IsNaN(vec.X))
                return float.IsNaN(vec.Y);

            return true;
        }
        
        public static Vector2 SafeNormalize(this Vector2 v, Vector2 defaultValue)
        {
            if (v == Vector2.Zero || v.HasNaNs())
                return defaultValue;

            return Vector2.Normalize(v);
        }

        public static float AngleTowards(this float curAngle, float targetAngle, float maxChange)
        {
            curAngle = MathHelper.WrapAngle(curAngle);
            targetAngle = MathHelper.WrapAngle(targetAngle);
            if (curAngle < targetAngle) {
                if (targetAngle - curAngle > (float)Math.PI)
                    curAngle += (float)Math.PI * 2f;
            }
            else if (curAngle - targetAngle > (float)Math.PI) {
                curAngle -= (float)Math.PI * 2f;
            }

            curAngle += MathHelper.Clamp(targetAngle - curAngle, 0f - maxChange, maxChange);
            return MathHelper.WrapAngle(curAngle);
        }
        
        /// <summary>
        /// Creates a vector2 using magnitude and rotation
        /// </summary>
        /// <param name="rotation">In radians</param>
        /// <param name="magnitude"></param>
        /// <returns></returns>
        public static Vector2 PolarVector(float rotation, float magnitude)
        {
            return new Vector2(0, -1).RotatedBy(rotation) * magnitude;
        }

        /// <summary>
        /// Rotates a vector's direction towards an ideal angle at a specific incremental rate. Can be returned as a unit vector.
        /// </summary>
        /// <param name="originalVector">The original vector to turn.</param>
        /// <param name="idealAngle">The ideal direction to approach.</param>
        /// <param name="angleIncrement">The maximum angular increment to make to approach the destination.</param>
        /// <param name="returnUnitVector">Whether the vector should be returned as unit vector or not.</param>
        public static Vector2 RotateTowards(this Vector2 originalVector, float idealAngle, float angleIncrement, bool returnUnitVector = false)
        {
            Vector2 newDirection = originalVector.ToRotation().AngleTowards(idealAngle, angleIncrement).ToRotationVector2();
            if (!returnUnitVector)
                return newDirection * originalVector.Length();
            return newDirection;
        }

        /// <summary>
        /// Calculates the direction to a given position with safely performed underlying normalization.
        /// </summary>
        /// <param name="position">The position to perform the calculations relative to.</param>
        /// <param name="destination">The position to get the direction towards.</param>
        public static Vector2 SafeDirectionTo(this Vector2 position, Vector2 destination)
        {
            return (destination - position).SafeNormalize(Vector2.Zero);
        }

        /// <summary>
        /// Determines the angular distance between two vectors based on dot product comparisons. This method ensures underlying normalization is performed safely.
        /// </summary>
        /// <param name="v1">The first vector.</param>
        /// <param name="v2">The second vector.</param>
        public static float AngleBetween(this Vector2 v1, Vector2 v2)
        {
            return (float)Math.Acos(Vector2.Dot(v1.SafeNormalize(Vector2.Zero), v2.SafeNormalize(Vector2.Zero)));
        }

        /// <summary>
        /// Returns a vector with random direction and optionally randomised magnitude in a given range
        /// </summary>
        /// <param name="min">Magnitude floor</param>
        /// <param name="max">Magnitude ceiling</param>
        /// <returns></returns>
        public static Vector2 RandomVector(float min = 1f, float max = 1f)
        {
            return new Vector2(0, -1).RotatedByRandom(MathF.Tau) * Main.rand.NextFloat(min, max);
        }

        /// <summary>
        /// Returns true when a given position and velocity is moving towards a target position
        /// </summary>
        /// <param name="MovingPos">The position of the thing moving</param>
        /// <param name="MovingVelocity">The velocity of the thing moving</param>
        /// <param name="targetPos">The position of the target</param>
        public static bool MovingTowards(this Vector2 MovingPos, Vector2 MovingVelocity, Vector2 targetPos)
        {
            if (Vector2.Dot(MovingPos.SafeDirectionTo(targetPos),MovingVelocity) < 0f)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the cross product of 2 vectors
        /// </summary>
        public static float Cross(this Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        #endregion

        #region Polynomial Root solving

        /// <summary>
        /// Solves quadratic equations using the quadratic formula
        /// </summary>
        /// <param name="a">coefficient of x^2</param>
        /// <param name="b">coefficient of x</param>
        /// <param name="c">constant</param>
        /// <returns>array of floats sorted by smallest value</returns>
        public static float[] SolveQuadratic(float a, float b, float c)
        {
            float[] results = new float[2];
            results[0] = (float)((-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a));
            results[1] = (float)((-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a));
            Array.Sort(results);
            return results;
        }

        /// <summary>
        /// Solves quadratic equations using the quadratic formula
        /// </summary>
        /// <param name="a">coefficient of x^2</param>
        /// <param name="b">coefficient of x</param>
        /// <param name="c">constant</param>
        /// <returns>array of floats sorted by smallest value</returns>
        public static double[] SolveQuadratic(double a, double b, double c)
        {
            double[] results = new double[2];
            results[0] = (float)((-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a));
            results[1] = (float)((-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a));
            Array.Sort(results);
            return results;
        }

        /// <summary>
        /// Finds a root for a function given an inital guess
        /// </summary>
        /// <param name="fx">the function</param>
        /// <param name="guess">a starting value to base off</param>
        /// <param name="accuracy">how many iterations this should run for if the functions does not already converge within that limit</param>
        /// <returns></returns>
        public static double IterativeRootFind(this Func<double, double> fx, double guess, int accuracy)
        {
            double x = guess;
            double tolerance = 0.0001;
            double before;

            for (int i = 0; i < accuracy; i++)
            {
                before = x;
                x = x - fx(x) / fx.ApproximateDerivative(x);
                if (Math.Abs(x - before) < tolerance)
                {
                    return x;
                }
            }
            return x;
        }

        /// <summary>
        /// Used by <see cref="IterativeRootFind(Func{double, double}, double, int)"/>
        /// </summary>
        /// <param name="fx">the function</param>
        /// <param name="x">the value to find the derivative around</param>
        /// <returns></returns>
        public static double ApproximateDerivative(this Func<double, double> fx, double x)
        {
            double h = 0.00001; // a small but not too small value
            return (fx(x + h) - fx(x - h)) / (2 * h);
        }

        // Entry point: returns all System.Numerics.Complex roots. Real ones have |Imag| ~ 0.
        // Coeffs are highest degree first: a_n, a_{n-1}, ..., a_0
        public static System.Numerics.Complex[] DurandKernerAll(double[] coeffs, int maxIter = 200, double tol = 1e-12)
        {
            if (coeffs == null || coeffs.Length < 2)
                throw new ArgumentException("At least two coefficients required.");
            int n = coeffs.Length - 1;
            if (Math.Abs(coeffs[0]) == 0)
                throw new ArgumentException("Leading coefficient must be nonzero.");

            // 1) Variable scaling: x = R * y, with R = Cauchy root bound
            double R = CauchyRootBound(coeffs);

            // Build Q(y) = P(R y) and make it monic
            // Q_k = a_k * R^{n-k}, then divide all by leading term to make monic
            double[] q = new double[coeffs.Length];
            for (int k = 0; k <= n; k++)
            {
                // exponent for R is (n - k) because coeffs[0] multiplies y^n
                q[k] = coeffs[k] * Math.Pow(R, n - k);
            }
            // Normalize to monic
            double lead = q[0];
            for (int k = 0; k <= n; k++) q[k] /= lead;

            // 2) Initial guesses on unit circle (since scaled)
            System.Numerics.Complex[] y = new System.Numerics.Complex[n];
            double step = 2 * Math.PI / n;
            for (int i = 0; i < n; i++)
                y[i] = System.Numerics.Complex.FromPolarCoordinates(1.0, i * step);

            // 3) Durand–Kerner iterations on Q(y) (monic)
            for (int iter = 0; iter < maxIter; iter++)
            {
                bool allSmall = true;
                for (int i = 0; i < n; i++)
                {
                    System.Numerics.Complex num = EvalMonic(q, y[i]); // q is monic, highest-first
                    System.Numerics.Complex den = System.Numerics.Complex.One;
                    for (int j = 0; j < n; j++)
                        if (i != j) den *= (y[i] - y[j]);

                    // Guard against accidental collisions
                    if (den.Magnitude == 0) den = new System.Numerics.Complex(1e-30, 0);

                    System.Numerics.Complex yNew = y[i] - num / den;
                    if ((yNew - y[i]).Magnitude > tol) allSmall = false;
                    y[i] = yNew;
                }
                if (allSmall) break;
            }

            // 4) Map back to x = R * y
            for (int i = 0; i < n; i++) y[i] *= R;
            return y;
        }

        // Convenience: extract real roots within imag tolerance, dedup with gap
        public static List<double> RealRoots(double[] coeffs, double imagTol = 1e-10, double mergeTol = 1e-8)
        {
            var roots = DurandKernerAll(coeffs);
            var reals = roots
                .Where(z => Math.Abs(z.Imaginary) <= imagTol)
                .Select(z => z.Real)
                .OrderBy(x => x)
                .ToList();

            // Merge near-duplicates
            var unique = new List<double>();
            foreach (var r in reals)
            {
                if (unique.Count == 0 || Math.Abs(r - unique[^1]) > mergeTol)
                    unique.Add(r);
            }
            return unique;
        }

        // Cauchy bound: all roots satisfy |x| <= 1 + max_{k<n} |a_k/a_n|
        private static double CauchyRootBound(double[] coeffs)
        {
            double a_n = Math.Abs(coeffs[0]);
            double max = 0.0;
            for (int k = 1; k < coeffs.Length; k++)
                max = Math.Max(max, Math.Abs(coeffs[k]));
            return 1.0 + (a_n == 0 ? 0 : max / a_n);
        }

        // Evaluate monic polynomial given highest-first coeffs q: q[0]=1, q[1]=b_{n-1}, ..., q[n]=b0
        private static System.Numerics.Complex EvalMonic(double[] q, System.Numerics.Complex x)
        {
            System.Numerics.Complex acc = q[0]; // == 1
            for (int i = 1; i < q.Length; i++)
                acc = acc * x + q[i];
            return acc;
        }

        #endregion
        
        #region RNG

        public static float NextFloat(this Random r)
        {
            return (float)r.NextDouble();
        }

        public static float NextFloat(this Random r, float min, float max)
        {
            return r.NextFloat() * (max - min) + min;
        }
        
        /// <summary>
        /// Generate a weighted random number within a specified range
        /// example : GenerateWeightedRandom(0,100,0.3,0.6) mostly returns values within 30-60 range
        /// </summary>
        /// <param name="min">minimum value</param>
        /// <param name="max">maximum value</param>
        /// <param name="weightStart">weight% floor</param>
        /// <param name="weightEnd">weight% ceiling</param>
        /// <returns>float</returns>
        public static float GenerateWeightedRandom(float min, float max, float weightStart, float weightEnd)
        {
            float randomValue = Main.rand.NextFloat(); // Random value between 0 and 1
            float lerpValue = MathHelper.Lerp(weightStart, weightEnd, randomValue); // Interpolate based on random value

            // Map the interpolated value to the specified range
            return MathHelper.Lerp(min, max, lerpValue);
        }

        /// <summary>
        /// Returns 1 or -1 randomly
        /// </summary>
        public static int Coinflip()
        {
            int result = Main.rand.Next(0, 2);
            if (result == 0)
            {
                return -1;
            }
            return 1;
        }

        /// <summary>
        /// Returns a random coordinate on a given rectangle
        /// </summary>
        public static Vector2 RandomPointOnRectangle(this Rectangle rectangle)
        {
            return new Vector2(Main.rand.Next(0, rectangle.Width + 1), Main.rand.Next(0, rectangle.Height + 1));
        }

        #endregion

        #region Geometry

        public static Rectangle SimpleSquare(Point center, int sideLength)
        {
            int halfSide = sideLength >> 1;
            return new Rectangle(center.X - halfSide, center.Y - halfSide, center.X + halfSide, center.Y + halfSide);
        }

        #endregion
    }
}