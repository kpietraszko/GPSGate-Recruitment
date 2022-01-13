using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GPSGateRecruitment.Common;
using GPSGateRecruitment.UnsafeCanvas;
using Window = GPSGateRecruitment.UnsafeCanvas.Window;

namespace GPSGateRecruitment;

public class Program : Application
{
    private static ApplicationState _applicationState;
    private static Window _window;

    [STAThread]
    public static void Main(string[] args)
    {
        const int width = 1280;
        const int height = 720;
        _window = new Window(width, height, Colors.White);
        _window.MouseLeftButtonDownHandler += OnMouseLeftButtonDown;
        _window.Title = "GPSGate Recruitment Task";

        _applicationState = new ApplicationState(new AStarPathFinder(width, height));
        _applicationState.LineCreated += OnLineCreated;
        _applicationState.PathFindingFailed += (_, e) => MessageBox.Show(e.ToString());

        Application app = new Application();
        app.Run();
    }

    static void OnMouseLeftButtonDown(object sender, Position position)
    {
        _window.DrawPixels(Colors.Blue, Window.CreateCircle(position, 4f).ToArray());
        _applicationState.AddPoint(position);
    }
    
    private static void OnLineCreated(object sender, IEnumerable<Position> pathPixels)
    {
        _window.DrawPixels(Colors.Black, pathPixels.ToArray());
    }
}