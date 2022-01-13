using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GPSGateRecruitment.Common;

namespace GPSGateRecruitment.UnsafeCanvas;

// Taken partially from https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.writeablebitmap
// Doing it manually to avoid XAML boilerplate and straight-up draw to a canvas and display it in a window
public class Window
{
    public EventHandler<Position> MouseLeftButtonDownHandler;

    public string Title
    {
        get => _window.Title;
        set => _window.Title = value;
    }

    private readonly Image _image;
    private readonly WriteableBitmap _writeableBitmap;
    private System.Windows.Window _window;

    public Window(int width, int height, Color backgroundColor)
    {
        _image = new Image();
        RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.NearestNeighbor);
        RenderOptions.SetEdgeMode(_image, EdgeMode.Aliased);

        _window = new System.Windows.Window();
        _window.Content = _image;
        _window.SizeToContent = SizeToContent.WidthAndHeight;
        _window.Show();

        _writeableBitmap = new WriteableBitmap(
            width,
            height,
            96,
            96,
            PixelFormats.Bgr32,
            null);
        
        Fill(backgroundColor);

        _image.Source = _writeableBitmap;

        _image.Stretch = Stretch.None;
        _image.HorizontalAlignment = HorizontalAlignment.Left;
        _image.VerticalAlignment = VerticalAlignment.Top;
        
        _image.MouseLeftButtonDown += HandleMouseLeftDownWithMappedPosition;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="color">Color to draw the pixels with</param>
    /// <param name="pixels">Positions of the pixels to draw</param>
    /// <remarks>This method updates the WriteableBitmap by using unsafe code to write pixels into the back buffer.
    /// Changes are drawn in the window immediately</remarks>
    public void DrawPixels(Color color, params Position[] pixels)
    {
        try
        {
            // Reserve the back buffer for updates.
            _writeableBitmap.Lock();
            unsafe
            {
                // Get a pointer to the back buffer.
                for (int i = 0; i < pixels.Length; i++)
                {
                    var pBackBuffer = _writeableBitmap.BackBuffer;
                    var pixel = pixels[i];
                    
                    if (pixel.X  < 0 || pixel.X > _writeableBitmap.Width ||
                        pixel.Y < 0 || pixel.Y > _writeableBitmap.Height)
                    {
                        throw new ArgumentException("Pixel out of bounds");
                    }
                    
                    // Find the address of the pixel to draw.
                    pBackBuffer += pixel.Y * _writeableBitmap.BackBufferStride;
                    pBackBuffer += pixel.X * 4;

                    // Pack the pixel color into an int
                    int colorData = color.R << 16; // R
                    colorData |= color.G << 8; // G
                    colorData |= color.B << 0; // B

                    // Assign the color data to the pixel.
                    *((int*)pBackBuffer) = colorData;
                }
            }

            // Mark whole canvas as dirty
            _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, (int) _writeableBitmap.Width, (int) _writeableBitmap.Height));
        }
        finally
        {
            // Release the back buffer and make it available for display.
            _writeableBitmap.Unlock();
        }
    }
    
    /// <param name="center">Center of the circle</param>
    /// <param name="radius">Radius of the circle</param>
    /// <returns>IEnumerable of pixels' positions that make up the circle</returns>
    public static IEnumerable<Position> CreateCircle(Position center, float radius)
    {
        var minX = center.X - radius;
        var maxX = center.X + radius + 1; 
        var minY = center.Y - radius; 
        var maxY = center.Y + radius + 1;

        for (var y = minY; y < maxY; y++)
        {
            for (var x = minX; x < maxX; x++)
            {
                var distance = (float) Math.Sqrt(Math.Pow(x - center.X, 2) + Math.Pow(y - center.Y, 2));
                if (distance <= radius + 0.5)
                {
                    yield return new Position((int)x, (int)y);
                }
            }
        }
    }

    private void Fill(Color color)
    {
        var width = (int) _writeableBitmap.Width;
        var height = (int) _writeableBitmap.Height;
        
        var pixels = Enumerable.Range(0, width * height)
            .Select(i => new Position(i % width, i / width)); // get 2D position from 1D index
        
        DrawPixels(color, pixels.ToArray());
    }

    private void HandleMouseLeftDownWithMappedPosition(object sender, MouseButtonEventArgs mouseEventArgs)
    {
        if (MouseLeftButtonDownHandler != null)
        {
            MouseLeftButtonDownHandler(this, new Position((int)mouseEventArgs.GetPosition(_image).X, (int)mouseEventArgs.GetPosition(_image).Y));
        }
    }
}