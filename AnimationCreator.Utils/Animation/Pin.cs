using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimationCreator.Utils.Animation
{
    public class Pin
    {
        private static double HeadRadius = 0.03;
        private static double PinRadius = 0.01;
        private static double PinLength = 1.5;
        private static int FallbackStart = 50;
        private static double FallbackDistance = 0.2;


        public int Untouched { get; set; }


        public double X { get; set; }
        public double Y { get; set; }

        private double m_Z;

        public double Z
        {
            get { return m_Z; }
            set
            {
                m_Z = value;
                Untouched = 0;
            }
        }

        public Pin()
        {
            Untouched = 0;
        }


        public void Frame()
        {
            if (m_Z > 0 && Untouched > FallbackStart)
            {
                m_Z -= FallbackDistance;
                if (m_Z < 0) m_Z = 0;
            }

            Untouched++;
        }


        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(string.Format(
                    "object {{ sphere <{0:0.000}, {1:0.000}, {2:0.000}>, {3:0.000} chrome }}\r\n",
                    X,
                    Y,
                    m_Z,
                    HeadRadius
                    ));

            builder.Append(string.Format(
                "object {{ cylinder <{0:0.000}, {1:0.000}, {2:0.000}>, <{3:0.000}, {4:0.000}, {5:0.000}>, {6:0.000} chrome }}\r\n",
                X, Y, m_Z, X, Y, m_Z - PinLength, PinRadius));

            return builder.ToString();

        }

    }
}
