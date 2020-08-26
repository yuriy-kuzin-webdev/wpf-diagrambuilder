using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace MVVMDiagramBuilder.Models
{
    public class DiagramModel : ObservableObject
    {
        private Canvas _content;
        private int _quantity = 100;
        public DiagramModel()
            => _content = new Canvas();

        public Canvas Content
        {
            get => _content;
            set => OnPropertyChanged(ref _content, value);
        }

        public int Quantity
        {
            get => _quantity;
            set => OnPropertyChanged(ref _quantity, value);
        }

        public void UpdateData((double x, double y)[] dataset)
        {
            Canvas canvas = new Canvas();

            double
                maxY = dataset.Max(val => val.y),                                       // Максимальное значение по Y
                minY = dataset.Min(val => val.y);
            minY = Double.IsNaN(minY) ? maxY : minY;                                    // Минимальное значение по Y

            double
                xOffset = 50,
                yOffset = 20,
                yHeight = (maxY > Math.Abs(minY)) ? maxY * 2 : Math.Abs(minY) * 2,      // Высота оси Y
                maxX = dataset.Last().x,                                                // Максимальное значение по X
                width = _content.ActualWidth - xOffset * 2,                             // Ширина контейнера
                height = _content.ActualHeight - yOffset * 2,                           // Высота контейнера
                yStart = height / 2 + yOffset,                                            // Центр по Y 
                xScale = width / maxX,                                                  // Пропорция по X
                yScale = height / yHeight;                                              // Пропорция по Y

            //Трансформируем результат в координаты канваса
            Point[] points = dataset.Select(d => new Point(
               d.x * xScale + xOffset,
               yStart - d.y * yScale
               )).ToArray();
            #region axis
            //Oсь X
            canvas.Children.Add(new Line() {
                X1 = xOffset, Y1 = yStart,
                X2 = width + xOffset, Y2 = yStart,
                Stroke = Brushes.Gray,
                StrokeThickness = 1 });
            canvas.Children.Add(GetText(25, yStart - 9, "0"));
            //Oсь Y
            canvas.Children.Add(new Line()
            {
                X1 = xOffset, Y1 = yOffset,
                X2 = xOffset, Y2 = yOffset + height,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            });
            #endregion axis
            #region help
            //Вспомогательные линии и текст
            double
                posY = Math.Round(maxY),
                stair = posY / 5.0;
            while (posY >= stair)
            {
                double topLineY = yStart - posY * yScale;
                double botLineY = yStart + posY * yScale;

                canvas.Children.Add(new Line()
                {
                    X1 = xOffset, Y1 = topLineY,
                    X2 = width + xOffset, Y2 = topLineY,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection() { 10 }
                });
                canvas.Children.Add(new Line()
                {
                    X1 = xOffset, Y1 = botLineY,
                    X2 = width + xOffset, Y2 = botLineY,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection() { 10 }
                });
                canvas.Children.Add(GetText(20, topLineY - 9, $"{posY:F2}"));
                canvas.Children.Add(GetText(15, botLineY - 9, $"{-posY:F2}"));
                posY -= stair;
            }
            //Отрисовка по Х
            double
                posX = Math.Round(maxX);
            stair = posX / 15.0;
            while (posX >= stair)
            {
                double x = posX * xScale + xOffset;
                canvas.Children.Add(GetEllipse(x, yStart));
                canvas.Children.Add(GetText(x - 5, yStart, $"{posX:F1}", 10));
                posX -= stair;
            }
            #endregion help
            #region curve
            //Отрисовка
            IEnumerable<Point> pts = points.Where(p => !Double.IsNaN(p.X) && !Double.IsNaN(p.Y));
            Point fp = pts.First();
            Polyline polyline = new Polyline()
            {
                Stroke = Brushes.LightPink,
                StrokeThickness = 3,
                Points = new PointCollection(pts),
            };
            canvas.Children.Add(polyline);
            #endregion curve
            //Aнимация
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            IEnumerable<Point> offsetPts = pts.Select(pnt => new Point(pnt.X - 4, pnt.Y - 4));
            pathFigure.StartPoint = offsetPts.First();
            PolyLineSegment polyLineSegment = new PolyLineSegment(offsetPts, true);
            pathFigure.IsClosed = false;
            pathFigure.Segments.Add(polyLineSegment);
            pathGeometry.Figures.Add(pathFigure);
            pathGeometry.Freeze();


            Border animableEllipse = GetAnimable(fp.X, fp.Y);
            canvas.Children.Add(animableEllipse);


            DoubleAnimation widthAnimation = new DoubleAnimation()
            {
                From = 5,
                To = 10,
                Duration = TimeSpan.FromSeconds(0.75),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            DoubleAnimation heightAnimation = new DoubleAnimation()
            {
                From = 5,
                To = 10,
                Duration = TimeSpan.FromSeconds(0.75),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            DoubleAnimationUsingPath pathAnimationX = new DoubleAnimationUsingPath()
            {
                PathGeometry = pathGeometry,
                Duration = TimeSpan.FromSeconds(10),
                Source = PathAnimationSource.X,
                RepeatBehavior = RepeatBehavior.Forever
            };
            DoubleAnimationUsingPath pathAnimationY = new DoubleAnimationUsingPath()
            {
                PathGeometry = pathGeometry,
                Duration = TimeSpan.FromSeconds(10),
                Source = PathAnimationSource.Y,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTargetProperty(pathAnimationX, new PropertyPath(Canvas.LeftProperty));
            Storyboard.SetTarget(pathAnimationX, animableEllipse);

            Storyboard.SetTargetProperty(pathAnimationY, new PropertyPath(Canvas.TopProperty));
            Storyboard.SetTarget(pathAnimationY, animableEllipse);

            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(Border.WidthProperty));
            Storyboard.SetTarget(widthAnimation, animableEllipse);

            Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(Border.HeightProperty));
            Storyboard.SetTarget(heightAnimation, animableEllipse);

            Storyboard s = new Storyboard();
            s.Children.Add(widthAnimation);
            s.Children.Add(heightAnimation);
            s.Children.Add(pathAnimationX);
            s.Children.Add(pathAnimationY);
            s.Begin();


            
            //Замена
            Content = canvas;
        }
        private TextBlock GetText(double x, double y, string text,double fontsize = 12)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.FontSize = fontsize;
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            return textBlock;
        }
        private Ellipse GetEllipse(double x, double y)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Width = 1;
            ellipse.Height = 7;
            ellipse.Fill = Brushes.Gray;
            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, y - 3);
            return ellipse;
        }

        private Border GetAnimable(double x, double y)
        {
            Ellipse animableEllipse = new Ellipse()
            {
                Width = 5,
                Height = 5,
                Fill = Brushes.Red,
                StrokeThickness = 1,
                Stroke = Brushes.OrangeRed
            };
            

            Border border = new Border()
            {
                Width = 5,
                Height = 5,
                BorderBrush = Brushes.White,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(2),
                Opacity = 1,
                CornerRadius = new CornerRadius(5),
                Effect = new DropShadowEffect()
                {
                    ShadowDepth = 0,
                    Color = Color.FromRgb(255, 255, 255),
                    Opacity = 1,
                    BlurRadius = 5
                },
                Child = animableEllipse,
            };

            Canvas.SetLeft(border, x);
            Canvas.SetTop(border, y - 3);

            return border;
        }
    }
}
