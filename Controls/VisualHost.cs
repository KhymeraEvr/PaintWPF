using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using Paint.WPF.Tools;

namespace Paint.WPF.Controls
{
    /// <summary>
    /// Класс реалізучий DrawingVisual і відповідаючий за малювання
    /// </summary>
    public class VisualHost : FrameworkElement
    {   
        public new bool IsFocused { get; set; }

        // Колекція для збереження DrawingVisual
        private readonly VisualCollection _visuals;

        //Властивості для збереження стану про корінневомий екземпляр коллекції _visuals
        private Brush FillBrush { get; set; }
        private Point Position { get; set; }
        public Size SpaceSize { get; private set; }

        public VisualHost()
        {
            _visuals = new VisualCollection(this);
            _visuals.Add(ClearVisualSpace());

            this.MouseLeftButtonUp += new MouseButtonEventHandler(VisualHost_MouseLeftButtonUp);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(VisualHost_MouseLeftButtonDown);
            this.MouseMove += new MouseEventHandler(VisualHost_MouseMove);
        }  

        /// <summary>
        /// Створення кореневого елементу VisualCollection
        /// </summary>
        /// <param name="borderBrush">Колір границі</param>
        /// <param name="backgroundBrush">Заливка</param>
        /// <param name="position">Початкове положення холста</param>
        /// <param name="size">Розмір холста</param>
        /// <returns></returns>
        private DrawingVisual CreateDrawingVisualSpace(Brush borderBrush, Brush backgroundBrush, Point position, Size size)
        {
            FillBrush = backgroundBrush;
            Position = position;
            SpaceSize = size;

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                Rect rect = new Rect(Position, SpaceSize);
                Pen pen = new Pen(borderBrush, 1);

                drawingContext.DrawRectangle(FillBrush, pen, rect);
            }

            return drawingVisual;
        }

        #region Методи редакції роботчої області

        private DrawingVisual ClearVisualSpace()
        {
            return CreateDrawingVisualSpace(Brushes.Silver, Brushes.Transparent, new Point(0, 0), new Size(300, 300));
        }

        public void FocusSpace()
        {
            _visuals[0] = null;
            _visuals[0] = CreateDrawingVisualSpace(Brushes.DimGray, FillBrush, Position, SpaceSize);
        }

        public void UnFocusSpace()
        {
            _visuals[0] = null;
            _visuals[0] = CreateDrawingVisualSpace(Brushes.Silver, FillBrush, Position, SpaceSize);
        }

        public void ChangeFill(Brush backgroundBrush)
        {
            _visuals[0] = null;
            _visuals[0] = CreateDrawingVisualSpace(Brushes.DimGray, backgroundBrush, Position, SpaceSize);
        }

        public void ChangeSize(Size newSize)
        {
            _visuals[0] = null;
            _visuals[0] = CreateDrawingVisualSpace(Brushes.DimGray, FillBrush, Position, newSize);
        }

        public void HideWorkSpace()
        {
            _visuals[0] = null;
        }

        public void RestoreWorkSpace()
        {
            if (_visuals[0] == null)
                _visuals[0] = CreateDrawingVisualSpace(Brushes.Silver, FillBrush, Position, SpaceSize);
        }
        #endregion

        /// <summary>
        /// Оприділення стану для початку малювання
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void VisualHost_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GlobalState.PressLeftButton = true;
            VisualHost_MouseMove(sender, e);
        }

        /// <summary>
        /// Закінчення малювання і оприділення кінцевих координат
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void VisualHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GlobalState.PressLeftButton = false;
            Point pt = e.GetPosition((UIElement)sender);
        }

   
        /// <summary>
        /// Метод для малювання точок кистю
        /// </summary>
        /// <param name="pt"></param>
        private void DrawPoint(Point pt)
        {
            var drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                Rect rect = new Rect(pt, GlobalState.BrushSize);
                drawingContext.DrawRoundedRectangle(GlobalState.Color, null, rect, GlobalState.BrushSize.Width, GlobalState.BrushSize.Height);
            }
            _visuals.Add(drawingVisual);
        }

        /// <summary>
        /// Івент для опреділення координат малювання
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void VisualHost_MouseMove(object sender, MouseEventArgs e)
        {
            switch (GlobalState.CurrentTool)
            {
                case Instruments.Arrow:
                    break;
                case Instruments.Brush:
 
                    if (GlobalState.PressLeftButton && this.IsFocused)
                    {
                        Point pt = e.GetPosition((UIElement)sender);
                        DrawPoint(pt);
                    }
                    break;
            }
        }

        #region Перевизначення методів класа FrameworkElement

        protected override int VisualChildrenCount
        {
            get { return _visuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _visuals.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _visuals[index];
        }
        #endregion
    }
}
