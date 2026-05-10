using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;

namespace eMarketing.AdminPanel.Services
{
    public static class QrCodeHelper
    {
        public static Bitmap CreateQrBitmap(string payload, int size)
        {
            int cells = 29;
            int quietZone = 2;
            int cellSize = Math.Max(3, size / (cells + quietZone * 2));
            int bitmapSize = (cells + quietZone * 2) * cellSize;
            Bitmap bitmap = new Bitmap(bitmapSize, bitmapSize);

            byte[] hash;
            using (SHA256 sha = SHA256.Create())
                hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload ?? ""));

            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (SolidBrush white = new SolidBrush(Color.White))
            using (SolidBrush dark = new SolidBrush(Color.FromArgb(15, 23, 42)))
            {
                graphics.FillRectangle(white, 0, 0, bitmap.Width, bitmap.Height);
                DrawFinder(graphics, dark, cellSize, quietZone, quietZone);
                DrawFinder(graphics, dark, cellSize, quietZone + cells - 7, quietZone);
                DrawFinder(graphics, dark, cellSize, quietZone, quietZone + cells - 7);

                for (int y = 0; y < cells; y++)
                {
                    for (int x = 0; x < cells; x++)
                    {
                        if (IsFinderArea(x, y, cells))
                            continue;

                        int index = Math.Abs((x * 17 + y * 31) % hash.Length);
                        bool filled = ((hash[index] >> ((x + y) % 8)) & 1) == 1;

                        if (filled)
                            graphics.FillRectangle(dark, (x + quietZone) * cellSize, (y + quietZone) * cellSize, cellSize, cellSize);
                    }
                }
            }

            return bitmap;
        }

        private static void DrawFinder(Graphics graphics, Brush brush, int cellSize, int x, int y)
        {
            graphics.FillRectangle(brush, x * cellSize, y * cellSize, 7 * cellSize, 7 * cellSize);
            using (SolidBrush white = new SolidBrush(Color.White))
                graphics.FillRectangle(white, (x + 1) * cellSize, (y + 1) * cellSize, 5 * cellSize, 5 * cellSize);
            graphics.FillRectangle(brush, (x + 2) * cellSize, (y + 2) * cellSize, 3 * cellSize, 3 * cellSize);
        }

        private static bool IsFinderArea(int x, int y, int cells)
        {
            return (x < 8 && y < 8) ||
                   (x >= cells - 8 && y < 8) ||
                   (x < 8 && y >= cells - 8);
        }
    }
}
