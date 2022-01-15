using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GPSGateRecruitment.Core;
using GPSGateRecruitment.UnsafeCanvas;
using Point = GPSGateRecruitment.Core.Point;

namespace GPSGateRecruitment;

public class Program : Application
{
    private static PathFindingDispatcher _pathFindingDispatcher;
    private static CanvasWindow _canvasWindow;
    private static int _numberOfPointsEnqueued = 0;
    private static string _windowTitleWithoutCalcStatus;
    private static Queue<Color> _colorsForLines = new();

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

        var app = new Application();
        app.Run();
    }

    private static void OnMouseLeftButtonDown(object sender, Point position)
    {
        _numberOfPointsEnqueued++;
        Color pointColor = default;
        
        if (_numberOfPointsEnqueued % 2 != 0)
        {
            // Requesting new line, generate a random color for it
            pointColor = _canvasWindow.GenerateRandomColor();
            _colorsForLines.Enqueue(pointColor);
        }
        else
        {
            pointColor = _colorsForLines.Last();
        }
        
        _canvasWindow.DrawPixels(pointColor, CanvasWindow.CreateCircle(position, 2f).ToArray());
        
        UpdateWindowTitle();
        _pathFindingDispatcher.AddPoint(position);
    }

    private static void OnLineCreated(object sender, IEnumerable<Point> pathPixels)
    {
        _numberOfPointsEnqueued -= 2;
        UpdateWindowTitle();
        _canvasWindow.DrawPixels(_colorsForLines.Dequeue(), pathPixels.ToArray());
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
        
        var calculatingStatus = _numberOfPointsEnqueued >= 2 ? $"(Calculating {_numberOfPointsEnqueued / 2} in the background)" : "";
        _canvasWindow.Title = $"{_windowTitleWithoutCalcStatus} {calculatingStatus}";
    }
}