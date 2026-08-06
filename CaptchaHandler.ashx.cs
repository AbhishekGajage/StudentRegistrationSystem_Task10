using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Web;
using System.Web.SessionState;

/// <summary>
/// Generates a simple distorted-text CAPTCHA image and stores the
/// expected value in Session["CaptchaCode"] for the Login page to
/// validate against.
/// </summary>
public class CaptchaHandler : IHttpHandler, IRequiresSessionState
{
    private const int CodeLength = 5;
    private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O/1/I to avoid confusion

    public void ProcessRequest(HttpContext context)
    {
        string code = GenerateRandomCode();
        context.Session["CaptchaCode"] = code;

        context.Response.ContentType = "image/png";
        context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        context.Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

        using (Bitmap bmp = new Bitmap(160, 60))
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(240, 244, 248));

            Random rnd = new Random();

            // Noise lines for basic anti-bot distortion
            for (int i = 0; i < 6; i++)
            {
                using (Pen pen = new Pen(Color.FromArgb(rnd.Next(150, 210), rnd.Next(150, 210), rnd.Next(150, 210)), 1))
                {
                    g.DrawLine(pen,
                        rnd.Next(bmp.Width), rnd.Next(bmp.Height),
                        rnd.Next(bmp.Width), rnd.Next(bmp.Height));
                }
            }

            using (Font font = new Font("Arial", 26, FontStyle.Bold))
            {
                float x = 10;
                foreach (char c in code)
                {
                    float y = rnd.Next(5, 15);
                    float angle = rnd.Next(-20, 20);

                    g.TranslateTransform(x + 12, y + 18);
                    g.RotateTransform(angle);
                    g.DrawString(c.ToString(), font,
                        new SolidBrush(Color.FromArgb(rnd.Next(30, 90), rnd.Next(30, 90), rnd.Next(90, 150))),
                        -12, -18);
                    g.ResetTransform();

                    x += 28;
                }
            }

            // Noise dots
            for (int i = 0; i < 60; i++)
            {
                bmp.SetPixel(rnd.Next(bmp.Width), rnd.Next(bmp.Height),
                    Color.FromArgb(rnd.Next(150, 220), rnd.Next(150, 220), rnd.Next(150, 220)));
            }

            bmp.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Png);
        }
    }

    private string GenerateRandomCode()
    {
        Random rnd = new Random(Guid.NewGuid().GetHashCode());
        char[] result = new char[CodeLength];
        for (int i = 0; i < CodeLength; i++)
        {
            result[i] = AllowedChars[rnd.Next(AllowedChars.Length)];
        }
        return new string(result);
    }

    public bool IsReusable => false;
}
