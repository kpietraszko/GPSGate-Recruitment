using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GPSGateRecruitment.Common;
using GPSGateRecruitment.UnsafeCanvas;

namespace GPSGateRecruitment;

public class Program : Application
{
    private static Canvas _canvas;

    // Taken partially from https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.writeablebitmap
    // Using this to avoid XAML boilerplate and straight-up draw to a canvas and display it in a window
    [STAThread]
    public static void Main(string[] args)
    {
        _canvas = new Canvas(1280, 720, Colors.White);
        _canvas.MouseLeftButtonDownHandler += OnMouseLeftButtonDown;

        Application app = new Application();
        app.Run();
    }

    /*static void ErasePixel(MouseEventArgs e)
    {
        byte[] ColorData = { 0, 0, 0, 0 }; // B G R

        Int32Rect rect = new Int32Rect(
            (int)(e.GetPosition(i).X),
            (int)(e.GetPosition(i).Y),
            1,
            1);

        _writeableBitmap.WritePixels(rect, ColorData, 4, 0);
    }*/

    static void OnMouseLeftButtonDown(object sender, Position position)
    {
        _canvas.DrawPixels(Colors.Black, position);
    }
}