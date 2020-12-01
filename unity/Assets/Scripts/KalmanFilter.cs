using System.Collections.Generic;

namespace PoseTeacher
{
    public class KalmanFilter
    {

        //-----------------------------------------------------------------------------------------
        // Constants:
        //-----------------------------------------------------------------------------------------

        public const double DEFAULT_Q = 0.000001;
        public const double DEFAULT_R = 0.01;

        public const double DEFAULT_P = 1;

        //-----------------------------------------------------------------------------------------
        // Private Fields:
        //-----------------------------------------------------------------------------------------

        private double q;
        private double r;
        private double p = DEFAULT_P;
        private double x;
        private double k;

        //-----------------------------------------------------------------------------------------
        // Constructors:
        //-----------------------------------------------------------------------------------------


        public KalmanFilter() : this(DEFAULT_Q) { }

        public KalmanFilter(double aQ = DEFAULT_Q, double aR = DEFAULT_R)
        {
            q = aQ;
            r = aR;
        }

        //-----------------------------------------------------------------------------------------
        // Public Methods:
        //-----------------------------------------------------------------------------------------

        public double Update(double measurement, double? newQ = null, double? newR = null)
        {

            // update values if supplied.
            if (newQ != null && q != newQ)
            {
                q = (double)newQ;
            }
            if (newR != null && r != newR)
            {
                r = (double)newR;
            }

            // update measurement.
            {
                k = (p + q) / (p + q + r);
                p = r * (p + q) / (r + p + q);
            }

            // filter result back into calculation.
            double result = x + (measurement - x) * k;
            x = result;
            return result;
        }

        public double Update(List<double> measurements, bool areMeasurementsNewestFirst = false, double? newQ = null, double? newR = null)
        {

            double result = 0;
            int i = (areMeasurementsNewestFirst) ? measurements.Count - 1 : 0;

            while (i < measurements.Count && i >= 0)
            {

                // decrement or increment the counter.
                if (areMeasurementsNewestFirst)
                {
                    --i;
                }
                else
                {
                    ++i;
                }

                result = Update(measurements[i], newQ, newR);
            }

            return result;
        }

        public void Reset(double x_default)
        {
            p = 1;
            x = x_default;
            k = 0;
        }
    }
}