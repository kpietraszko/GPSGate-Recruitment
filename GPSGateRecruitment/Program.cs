using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GPSGateRecruitment.Common;
using GPSGateRecruitment.UnsafeCanvas;
using Point = System.Drawing.Point;

namespace GPSGateRecruitment;

public class Program : Application
{
    private static PathFindingDispatcher _pathFindingDispatcher;
    private static CanvasWindow _canvasWindow;
    private static int _numberOfPointsEnqueued = 0;
    private static string _windowTitleWithoutCalcStatus;

    [STAThread]
    public static void Main(string[] args)
    {
        const int width = 1280;
        const int height = 720;
        _canvasWindow = new CanvasWindow(width, height, Colors.White);
        _canvasWindow.MouseLeftButtonDownHandler += OnMouseLeftButtonDown;
        const string windowTitlePrefix = "GPSGate Recruitment Task";
        UpdateWindowTitle(windowTitlePrefix);

        PathFindingDispatcher.WaitingForStartPoint += (_, _) =>
            UpdateWindowTitle($"{windowTitlePrefix} - Please click where you want the line to start");

        PathFindingDispatcher.WaitingForEndPoint += (_, _) =>
            UpdateWindowTitle($"{windowTitlePrefix} - Please click where you want the line to end");
        
        _pathFindingDispatcher = new PathFindingDispatcher(new AStarPathFinder(width, height));
        
        _pathFindingDispatcher.LineCreated += OnLineCreated;
        _pathFindingDispatcher.PathFindingFailed += OnPathFindingFailed;

        Application app = new Application();
        app.Run();
    }

    private static void OnMouseLeftButtonDown(object sender, Point position)
    {
        _canvasWindow.DrawPixels(Colors.Blue, CanvasWindow.CreateCircle(position, 3f).ToArray());
        _numberOfPointsEnqueued++;
        UpdateWindowTitle();
        _pathFindingDispatcher.AddPoint(position);
    }
    
    private static void OnLineCreated(object sender, IEnumerable<Point> pathPixels)
    {
        _numberOfPointsEnqueued -= 2;
        UpdateWindowTitle();
        _canvasWindow.DrawPixels(pathPixels.ToArray());
    }
    
    private static void OnPathFindingFailed(object _, Exception e)
    {
        _numberOfPointsEnqueued -= 2;
        UpdateWindowTitle();
        MessageBox.Show($"Error: {e.Message}");
    }

    private static void UpdateWindowTitle(string newTitle = null)
    {
        if (newTitle != null)
        {
            _windowTitleWithoutCalcStatus = newTitle;
        }

        // const string calculatingStatusText = "Calculating...";
        var calculatingStatus = _numberOfPointsEnqueued > 0 ? "(Calculating in the background)" : "";
        // var format = $"{{0:-80}}{{1}}";
        // _canvasWindow.Title = string.Format(format, _windowTitleWithoutCalcStatus, calculatingStatus);
        _canvasWindow.Title = $"{_windowTitleWithoutCalcStatus} {calculatingStatus}";
    }
}