using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GPSGateRecruitment.Common;

namespace GPSGateRecruitment.UnsafeCanvas;

public class Canvas
{
    public EventHandler<Position> MouseLeftButtonDownHandler;

    private readonly Image _image;
    private readonly WriteableBitmap _writeableBitmap;

    public Canvas(int width, int height, Color backgroundColor)
    {
        _image = new Image();
        RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.NearestNeighbor);
        RenderOptions.SetEdgeMode(_image, EdgeMode.Aliased);

        var window = new Window();
        window.Content = _image;
        window.SizeToContent = SizeToContent.WidthAndHeight;
        window.Show();

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
    
    // The DrawPixels method updates the WriteableBitmap by using
    // unsafe code to write pixels into the back buffer.
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
                    // Find the address of the pixel to draw.
                    pBackBuffer += pixels[i].Y * _writeableBitmap.BackBufferStride;
                    pBackBuffer += pixels[i].X * 4;

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