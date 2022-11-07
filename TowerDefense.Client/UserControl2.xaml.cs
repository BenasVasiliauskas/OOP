﻿using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using TowerDefense.Server.Models;
using TowerDefense.Server.Models.Maps;

namespace TowerDefense.Client
{
    public class RectangleEnemy
    {
        public Rectangle Rectangle { get; set; }
        public Unit Enemy { get; set; }
    }

    /// <summary>
    /// Interaction logic for UserControl2.xaml
    /// </summary>
    public partial class UserControl2 : UserControl
    {
        private readonly List<MovePoint> _path;
        private HubConnection _connection;
        private bool _towerBuildSelected = false;
        private List<Rectangle> _rectangles = new();
        private List<Rectangle> _towers = new();
        private List<(DoubleAnimationUsingPath, DoubleAnimationUsingPath)> _animations = new();
        private List<Storyboard> storyboards = new();
        
        DispatcherTimer gameTimer = new();

        public UserControl2(HubConnection connection, List<MovePoint> path)
        {
            InitializeComponent();
            _connection = connection;
            _path = path;

            gameTimer.Tick += delegate (object sender, EventArgs e)
            {
                GameTimerEvent(sender, e);
            };

            gameTimer.Interval = TimeSpan.FromMilliseconds(100);
            gameTimer.Start();
        }

        private async void GameTimerEvent(object sender, EventArgs e)
        {
            await _connection.InvokeAsync("GameTimerTick");
        }

        private void ActionLoaded(object sender, RoutedEventArgs e)
        {
            _connection.On<List<Unit>>("Ticked", async (enemies) =>
            {
                for (int i = 0; i < _rectangles.Count; i++)
                {
                    double x = Canvas.GetLeft(_rectangles[i]);
                    double y = Canvas.GetTop(_rectangles[i]);

                    for(int j = 0; j < _towers.Count; j++)
                    {
                        double towerX = Canvas.GetLeft(_towers[j]);
                        double towerY = Canvas.GetTop(_towers[j]);

                        var actualDistance = Math.Abs(x - towerX) + Math.Abs(y - towerY);
                        distance.Content = actualDistance.ToString();

                        if (actualDistance < 64)
                        {
                            await _connection.InvokeAsync("NearTower", i, j, _connection.ConnectionId);
                            coordinate.Content = enemies[0].Health.ToString();
                        }
                    }
                }

            });

            _connection.On<int>("EnemyDeath", (index) =>
            {
                canvas.Children.Remove(_rectangles[index]);
                enemyCanvas.Children.Remove(_rectangles[index]);
            });

            _connection.On<Unit, Player, string>("EnemyCreated", (unit, player, contextId) =>
            {               
                var rect = new Rectangle
                {
                    Width = 32,
                    Height = 32,
                    Fill = new ImageBrush
                    {
                        ImageSource = new BitmapImage(new Uri($"pack://application:,,,{unit.ImageSource}"))
                    }
                };
                Canvas.SetTop(rect, 96);
                Canvas.SetLeft(rect, 0);
                _rectangles.Add(rect);

                if (player.ConnectionId == contextId) 
                {
                    enemyCanvas.Children.Add(rect);
                }
                else
                {
                    canvas.Children.Add(rect);
                }


                Path path = new Path();
                PathFigure pathFigure = new PathFigure();
                pathFigure.StartPoint = new Point(_path[0].X, _path[0].Y);

                for (int i = 1; i < _path.Count; i++)
                {
                    LineSegment segment = new LineSegment();
                    segment.Point = new Point(_path[i].X, _path[i].Y);
                    pathFigure.Segments.Add(segment);
                }

                PathGeometry pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(pathFigure);

                path.Data = pathGeometry;

                var animX = new DoubleAnimationUsingPath();
                animX.Duration = TimeSpan.FromSeconds(10);
                animX.PathGeometry = pathGeometry;
                animX.Source = PathAnimationSource.X;

                var animY = new DoubleAnimationUsingPath();
                animY.Duration = TimeSpan.FromSeconds(10);
                animY.PathGeometry = pathGeometry;
                animY.Source = PathAnimationSource.Y;

                _animations.Add((animX, animY));

                Storyboard storyboard = new Storyboard();

                Storyboard.SetTarget(animX, rect);
                Storyboard.SetTargetProperty(animX, new PropertyPath(Canvas.LeftProperty));
                Storyboard.SetTarget(animY, rect);
                Storyboard.SetTargetProperty(animY, new PropertyPath(Canvas.TopProperty));

                storyboard.Children.Add(animX);
                storyboard.Children.Add(animY);
                storyboards.Add(storyboard);

                storyboard.Begin();
            });

            _connection.On<Unit, Player, string, int, int>("TowerBuilt", (unit, player, contextId, x, y) =>
            {
                BrushConverter bc = new();

                var tower = new Rectangle
                {
                    Width = 32,
                    Height = 32,
                    Fill = Brushes.Black
                };

                Canvas.SetLeft(tower, x);
                Canvas.SetTop(tower, y);

                _towers.Add(tower);

                if (player.ConnectionId == contextId)
                {
                    enemyCanvas.Children.Add(tower);
                }
                else
                {
                    canvas.Children.Add(tower);
                }
            });


            _connection.On("PoweredUp", () =>
            {
                foreach (var item in storyboards)
                {
                    item.SetSpeedRatio(item.SpeedRatio += 0.1);
                }
            });
        }
        private async void Create_Shooting_Enemy_Button_Click(object sender, RoutedEventArgs e)
        {
            await _connection.InvokeAsync("CreateEnemy", "S");
        }
        private async void Create_Healing_Enemy_Button_Click(object sender, RoutedEventArgs e)
        {
            await _connection.InvokeAsync("CreateEnemy", "H");
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            await _connection.InvokeAsync("ChangeLevel");
        }

        private void TowerButton_Click(object sender, RoutedEventArgs e)
        {
            _towerBuildSelected = true;
        }

        private async void Canvas_Click(object sender, MouseButtonEventArgs e)
        {
            if (_towerBuildSelected)
            {
                var point = e.GetPosition(canvas);

                int newX = ((int)point.X / 32) * 32;
                int newY = ((int)point.Y / 32) * 32;
                _towerBuildSelected = false;

                await _connection.InvokeAsync("CreateTower", "S", newX, newY);
            }
        }

        private async void PowerUp_Click(object sender, RoutedEventArgs e)
        {

            await _connection.InvokeAsync("PowerUp");
        }
    }
}
