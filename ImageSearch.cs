using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using OpenCV.Net;

using System.Diagnostics;
namespace OpenCVImageSearch
{
    class ImageSearch
    {
        public static Methods MatchingMethod = Methods.CorrelationCoefficientNormed;

        public static MatchPoint Find(Bitmap small, Bitmap big, bool returnCenter = true)
        {
            MatchPoint result;

            using (Mat sm = BitmapToMat(small))
            using (Mat bm = BitmapToMat(big))
            using (Mat Mresult = Match(sm, bm))
            {
                double minVal, maxVal, MatchVal;
                OpenCV.Net.Point minLoc, maxLoc, MatchLoc;
                CV.MinMaxLoc(Mresult, out  minVal, out maxVal, out minLoc, out maxLoc);

                //For SquareDifference and SquareDifferenceNormed, the best matches are lower values. 
                //For all the other methods, the higher the better
                MatchVal = (MatchingMethod == Methods.SquareDifference || MatchingMethod == Methods.SquareDifferenceNormed) ? minVal : maxVal;
                MatchLoc = (MatchingMethod == Methods.SquareDifference || MatchingMethod == Methods.SquareDifferenceNormed) ? minLoc : maxLoc;

                if (returnCenter)
                {
                    MatchLoc.X += small.Width / 2;
                    MatchLoc.Y += small.Height / 2;
                }

                result = new MatchPoint(MatchLoc, MatchVal);

            }

            return result;
        }



        /// <summary>
        /// Finds all matches of small bmp in the big bmp
        /// </summary>
        /// <param name="small">Small image that will be searched</param>
        /// <param name="big">Big image that small image will be searched in</param>
        /// <param name="MaxValue">max number of matches</param>
        /// <param name="punctuality">Must be ranged 0.00~1.00</param>
        /// <param name="returnCenter">middle of small bmp</param>
        /// <returns>List of MatchPoint based on found positions</returns>
        public static List<MatchPoint> FindAll(Bitmap small, Bitmap big, int MaxValue, double punctuality, bool returnCenter = true)
        {
            var result = new List<MatchPoint>();
            double minVal, maxVal, MatchVal;
            OpenCV.Net.Point minLoc, maxLoc, MatchLoc;

            //For SquareDifference and SquareDifferenceNormed, the best matches are lower values. 
            //For all the other methods, the higher the better
            bool boolMin = (MatchingMethod == Methods.SquareDifference || MatchingMethod == Methods.SquareDifferenceNormed);

            using (Mat sm = BitmapToMat(small))
            using (Mat bm = BitmapToMat(big))
            using (Mat Mresult = Match(sm, bm))
            {
                while (MaxValue > 0)
                {

                    CV.MinMaxLoc(Mresult, out  minVal, out maxVal, out minLoc, out maxLoc);


                    MatchVal = boolMin ? minVal : maxVal;
                    bool pass = (MatchVal >= punctuality);

                    if (boolMin) pass = (MatchVal <= punctuality);


                    if (pass)
                    {
                        MatchLoc = boolMin ? minLoc : maxLoc;

                        // Fill results array with hi or lo vals, so we don't match this same location
                        CV.FloodFill(Mresult, MatchLoc, new Scalar(Convert.ToDouble(boolMin)), new Scalar(0.1), new Scalar(1.0));


                        if (returnCenter)
                        {
                            MatchLoc.X += small.Width / 2;
                            MatchLoc.Y += small.Height / 2;
                        }
                        result.Add(new MatchPoint(MatchLoc, MatchVal));
                    }
                    else
                    {
                        break;
                    }

                    MaxValue--;
                }
            }


            return result;
        }


        private static Mat Match(Mat small, Mat big)
        {
            int rows = big.Rows - small.Rows + 1;
            int cols = big.Cols - small.Cols + 1;
            Mat result = new Mat(rows, cols, Depth.F32, 1);//CV_32FC1

            CV.MatchTemplate(big, small, result, (TemplateMatchingMethod)MatchingMethod);
            
           // NamedWindow wnd = new NamedWindow("Debug");
             //wnd.ShowImage(result);
           // CV.WaitKey();

            //CV.Threshold(result, result, 0.90d, 1.00d, ThresholdTypes.ToZero);

            // wnd.ShowImage(result);
            //CV.WaitKey();

            return result;

        }



        private static Mat BitmapToMat(Bitmap bitmap)
        {
            BitmapData bmData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var tmp = new IplImage(new OpenCV.Net.Size(bitmap.Width, bitmap.Height), IplDepth.U8, 3, bmData.Scan0);
            bitmap.UnlockBits(bmData);

            return tmp.GetMat();
        }

    }


    public enum Methods //copied from TemplateMatchingMethod
    {
        SquareDifference = 0,
        SquareDifferenceNormed = 1,
        CrossCorrelation = 2,
        CrossCorrelationNormed = 3,
        CorrelationCoefficient = 4,
        CorrelationCoefficientNormed = 5,
    }
    public class MatchPoint
    {
        public int X;
        public int Y;
        public double Punctuality;

        public MatchPoint() : this(0, 0, 0.0) { }

        public MatchPoint(int x, int y, double val)
        {
            X = x;
            Y = y;
            Punctuality = val;
        }

        public MatchPoint(OpenCV.Net.Point pt, double val)
        {
            X = pt.X;
            Y = pt.Y;
            Punctuality = val;
        }

        public System.Drawing.Point ToPoint()
        {
            return new System.Drawing.Point(X, Y);
        }

        public OpenCV.Net.Point ToCVPoint()
        {
            return new OpenCV.Net.Point(X, Y);
        }




    }
}
