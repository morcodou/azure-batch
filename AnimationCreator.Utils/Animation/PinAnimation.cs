using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimationCreator.Utils.Animation
{
    public class PinAnimation
    {
        public int PinsWidth = 100;
        public int PinsHeight = 60;
        public double PinSpacing = 0.1;


        public double PinBoardX = -5;
        public double PinBoardY = -1;

        public double CameraX = 0;
        public double CameraY = 0;
        public double CameraZ = 0;

        public double CameraAtX = 0;
        public double CameraAtY = 0;
        public double CameraAtZ = 0;


        public int XResolution = 800;
        public int YResolution = 600;

        public static double TableHeight = -2.0;

        public static double CameraRotateSpeed = 0.1;
        public static double CameraRadius = 8.0;

        public static double FrameTime = 0.04;

        public double CameraAngle = -0.5;


        public double PinDepthScale = 0;


        public List<Pin> Pins { get; set; }
        public int FrameNumber { get; set; }

        public PinAnimation(int width, int height)
        {
            XResolution = width;
            YResolution = height;

            Pins = new List<Pin>();

            FrameNumber = 0;

            // Create the first array of pins
            for (int pinX = 0; pinX < PinsWidth; pinX++)
            {
                for (int pinY = 0; pinY < PinsHeight; pinY++)
                {
                    Pin pin = new Pin() { X = PinBoardX + (pinX * PinSpacing), Y = PinBoardY + (pinY * PinSpacing), Z = 0 };
                    Pins.Add(pin);
                }
            }

            // Create the intermediary pins
            for (int pinX = 0; pinX < PinsWidth - 1; pinX++)
            {
                for (int pinY = 0; pinY < PinsHeight - 1; pinY++)
                {
                    Pin pin = new Pin() { X = PinBoardX + (PinSpacing / 2) + (pinX * PinSpacing), Y = PinBoardY + (PinSpacing / 2) + (pinY * PinSpacing), Z = 0 };
                    Pins.Add(pin);
                }
            }

        }

        public void SetPinDepth(string imageFile)
        {
            int left = 0;
            int top = 0;
            int right = 639;
            int bottom = 479;


            if (FrameNumber >= 0)
            {
                Bitmap image = new Bitmap(imageFile);


                right = image.Width;
                bottom = image.Height;

                foreach (Pin pin in Pins)
                {
                    int PixelX = left + (int)((pin.X - PinBoardX) * ((double)(right - left) / ((double)PinsWidth) / PinSpacing));
                    int PixelY = top + (int)((pin.Y - PinBoardY) * ((double)(bottom - top) / ((double)PinsHeight) / PinSpacing));


                    if (PixelX > 0)
                    {
                        try
                        {
                            double pinZ = ((double)(image.GetPixel(image.Width - PixelX - 1, image.Height - PixelY - 1).B) / 255);
                            if (pinZ > pin.Z)
                            {
                                pin.Z = pinZ;
                            }
                        }
                        catch
                        {
                        }

                    }
                    else
                    {
                    }

                    pin.Frame();
                }
            }
        }

        public void AdvanceFrame()
        {
            FrameNumber++;
            // Advance the Camera...
            CameraAngle += CameraRotateSpeed * FrameTime;

            CameraX = Math.Sin(CameraAngle) * CameraRadius;
            CameraZ = Math.Cos(CameraAngle) * CameraRadius;
        }



        public override string ToString()
        {


            StringBuilder builder = new StringBuilder();

            builder.Append("background Midnight_Blue\r\n");
            builder.Append("static define matte surface { ambient 0.1 diffuse 0.7 }\r\n");
            builder.Append("define matte_white texture { matte { color white } }\r\n");
            builder.Append("define matte_black texture { matte { color dark_slate_gray } }\r\n");

            builder.Append("define position_cylindrical 3\r\n");
            builder.Append("define lookup_sawtooth 1\r\n");

            builder.Append("define light_wood <0.6, 0.24, 0.1>\r\n");
            builder.Append("define median_wood <0.3, 0.12, 0.03>\r\n");
            builder.Append("define dark_wood <0.05, 0.01, 0.005>\r\n");
            builder.Append("define wooden texture { noise surface { ambient 0.2  diffuse 0.7  specular white, 0.5 microfacet Reitz 10 position_fn position_cylindrical position_scale 1  lookup_fn lookup_sawtooth octaves 1 turbulence 1 color_map( [0.0, 0.2, light_wood, light_wood] [0.2, 0.3, light_wood, median_wood] [0.3, 0.4, median_wood, light_wood] [0.4, 0.7, light_wood, light_wood] [0.7, 0.8, light_wood, median_wood] [0.8, 0.9, median_wood, light_wood] [0.9, 1.0, light_wood, dark_wood]) } }\r\n");
            builder.Append("define glass texture { surface { ambient 0 diffuse 0 specular 0.2 reflection white, 0.1 transmission white, 1, 1.5 }}\r\n");
            builder.Append("define shiny surface { ambient 0.1 diffuse 0.6 specular white, 0.6 microfacet Phong 7  }\r\n");
            builder.Append("define steely_blue texture { shiny { color black } }\r\n");

            builder.Append("define chrome texture { surface { color white ambient 0.0 diffuse 0.2 specular 0.4 microfacet Phong 10 reflection 0.8 } }\r\n");


            builder.Append(string.Format("viewpoint {{ from <{0:0.000}, {1:0.000}, {2:0.000}> at <{3:0.000}, {4:0.000}, {5:0.000}> up <0, 1, 0> angle 60 resolution {6}, {7} aspect 1.78 image_format 0 }}\r\n",
                CameraX, CameraY, CameraZ,
                CameraAtX, CameraAtY, CameraAtZ,
                XResolution, YResolution));




            builder.Append("light <-10, 30, 20>\r\n");
            builder.Append("light <-10, 30, -20>\r\n");
            builder.Append(string.Format("object {{ disc <0, {0}, 0>, <0, 1, 0>, 30 wooden }}\r\n", TableHeight));


            builder.AppendLine("object { object { sphere < 0, 0, 5.4 >, 6 glass } * object { sphere < 0, 0, -5.4 >, 6 glass } + object { cylinder <0, 0, -0.3>, <0, 0, 0.3>, 2.65 chrome } + object { cylinder <0, 0, -0.3>, <0, 0, 0.3>, 2.6 chrome } + object { cylinder <0, -4, 0>, <0, -2.65, 0>, 0.25 steely_blue } translate < " + (5 - (double)FrameNumber / 50) + ", 2, 3 > }");


            //// Frame
            builder.Append(string.Format("object {{ box <{0}, {1}, {2}>, <{3}, {4}, {5}> glass }}\r\n",
                 PinBoardX - 0.5, PinBoardY - 0.5, 1.05,
                 PinBoardX + (PinsWidth * PinSpacing) + 0.5, PinBoardY + (PinsHeight * PinSpacing) + 0.5, 1.25));

            builder.Append(string.Format("object {{ box <{0}, {1}, {2}>, <{3}, {4}, {5}> steely_blue }}\r\n",
                PinBoardX - 0.5, PinBoardY - 0.5, -0.04,
                PinBoardX + (PinsWidth * PinSpacing) + 0.5, PinBoardY + (PinsHeight * PinSpacing) + 0.5, -0.09));

            builder.Append(string.Format("object {{ box <{0}, {1}, {2}>, <{3}, {4}, {5}> steely_blue }}\r\n",
                PinBoardX - 0.5, PinBoardY - 0.5, -0.52,
                PinBoardX + (PinsWidth * PinSpacing) + 0.5, PinBoardY + (PinsHeight * PinSpacing) + 0.5, -0.59));



            builder.Append(string.Format("object {{ cylinder <{0}, {1}, {2}>, <{3}, {4}, {5}>, 0.2 steely_blue }}\r\n",
                PinBoardX - 0.2, PinBoardY - 0.2, 1.4,
                PinBoardX - 0.2, PinBoardY - 0.2, -0.74
                ));

            builder.Append(string.Format("object {{ cylinder <{0}, {1}, {2}>, <{3}, {4}, {5}>, 0.2 steely_blue }}\r\n",
                PinBoardX + (PinsWidth * PinSpacing) + 0.2, PinBoardY - 0.2, 1.4,
                PinBoardX + (PinsWidth * PinSpacing) + 0.2, PinBoardY - 0.2, -0.74
                ));

            builder.Append(string.Format("object {{ cylinder <{0}, {1}, {2}>, <{3}, {4}, {5}>, 0.2 steely_blue }}\r\n",
                PinBoardX - 0.2, PinBoardY + (PinsHeight * PinSpacing) + 0.2, 1.4,
                PinBoardX - 0.2, PinBoardY + (PinsHeight * PinSpacing) + 0.2, -0.74
                ));

            builder.Append(string.Format("object {{ cylinder <{0}, {1}, {2}>, <{3}, {4}, {5}>, 0.2 steely_blue }}\r\n",
                PinBoardX + (PinsWidth * PinSpacing) + 0.2, PinBoardY + (PinsHeight * PinSpacing) + 0.2, 1.4,
                PinBoardX + (PinsWidth * PinSpacing) + 0.2, PinBoardY + (PinsHeight * PinSpacing) + 0.2, -0.74
                ));


            builder.Append(string.Format("object {{ cylinder <{0}, {1}, {2}>, <{3}, {4}, {5}>, 0.2 steely_blue }}\r\n",
                PinBoardX + (PinsWidth * PinSpacing / 2), PinBoardY - 0.2, 1.4,
                PinBoardX + (PinsWidth * PinSpacing / 2), PinBoardY - 0.2, -0.74
                ));

            builder.Append(string.Format("object {{ cylinder <{0}, {1}, {2}>, <{3}, {4}, {5}>, 0.2 steely_blue }}\r\n",
                PinBoardX + (PinsWidth * PinSpacing / 2), PinBoardY + (PinsHeight * PinSpacing) + 0.2, 1.4,
                PinBoardX + (PinsWidth * PinSpacing / 2), PinBoardY + (PinsHeight * PinSpacing) + 0.2, -0.74
                ));




            foreach (Pin pin in Pins)
            {
                builder.Append(pin.ToString());
            }



            return builder.ToString();
        }



    }
}
