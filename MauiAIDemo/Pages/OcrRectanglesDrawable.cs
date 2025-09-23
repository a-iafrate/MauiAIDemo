using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace MauiAIDemo
{
    public class OcrRectanglesDrawable : IDrawable
    {
        public List<RectF> Rectangles { get; set; } = new();

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.Red;
            canvas.StrokeSize = 3;
            foreach (var rect in Rectangles)
            {
                canvas.DrawRectangle(rect);
            }
        }
    }
}
