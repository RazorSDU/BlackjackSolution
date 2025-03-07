using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using System;

namespace Blackjack.MAUI.Helpers
{
    public enum HorizontalAlign { Left, Center, Right }
    public enum VerticalAlign { Top, Center, Bottom }

    public static class GridPosition
    {
        // Attached properties
        public static readonly BindableProperty ColumnProperty =
            BindableProperty.CreateAttached(
                "Column",
                typeof(int),
                typeof(GridPosition),
                0,
                propertyChanged: OnPositionChanged);

        public static readonly BindableProperty RowProperty =
            BindableProperty.CreateAttached(
                "Row",
                typeof(int),
                typeof(GridPosition),
                0,
                propertyChanged: OnPositionChanged);

        public static readonly BindableProperty HorizontalAlignmentProperty =
            BindableProperty.CreateAttached(
                "HorizontalAlignment",
                typeof(HorizontalAlign),
                typeof(GridPosition),
                HorizontalAlign.Left,
                propertyChanged: OnPositionChanged);

        public static readonly BindableProperty VerticalAlignmentProperty =
            BindableProperty.CreateAttached(
                "VerticalAlignment",
                typeof(VerticalAlign),
                typeof(GridPosition),
                VerticalAlign.Top,
                propertyChanged: OnPositionChanged);

        // Getters/Setters
        public static int GetColumn(BindableObject view) =>
            (int)view.GetValue(ColumnProperty);

        public static void SetColumn(BindableObject view, int value) =>
            view.SetValue(ColumnProperty, value);

        public static int GetRow(BindableObject view) =>
            (int)view.GetValue(RowProperty);

        public static void SetRow(BindableObject view, int value) =>
            view.SetValue(RowProperty, value);

        public static HorizontalAlign GetHorizontalAlignment(BindableObject view) =>
            (HorizontalAlign)view.GetValue(HorizontalAlignmentProperty);

        public static void SetHorizontalAlignment(BindableObject view, HorizontalAlign value) =>
            view.SetValue(HorizontalAlignmentProperty, value);

        public static VerticalAlign GetVerticalAlignment(BindableObject view) =>
            (VerticalAlign)view.GetValue(VerticalAlignmentProperty);

        public static void SetVerticalAlignment(BindableObject view, VerticalAlign value) =>
            view.SetValue(VerticalAlignmentProperty, value);

        // This method is called any time an attached property changes
        private static void OnPositionChanged(BindableObject bindable, object oldVal, object newVal)
        {
            if (bindable is View view)
            {
                // Re-calculate the AbsoluteLayout bounds
                UpdateAbsoluteLayoutPosition(view);
            }
        }

        private static void UpdateAbsoluteLayoutPosition(View view)
        {
            // Read the properties
            int col = GetColumn(view);
            int row = GetRow(view);
            var hAlign = GetHorizontalAlignment(view);
            var vAlign = GetVerticalAlignment(view);

            // We have 17 columns (0..16) and 9 rows (0..8).
            // Convert the (col, row) into proportional X/Y:
            const int maxCols = 16;
            const int maxRows = 8;

            // Proportional range is 0..1
            double x = (double)col / maxCols; // 0..1 horizontally
            double y = (double)row / maxRows; // 0..1 vertically

            // We'll set the element's anchor based on alignment
            // If horizontal is Left, anchorX=0; if Center, anchorX=0.5; if Right, anchorX=1.
            // Same for anchorY with top=0, center=0.5, bottom=1.
            double anchorX = hAlign switch
            {
                HorizontalAlign.Left => 0.0,
                HorizontalAlign.Center => 0.5,
                HorizontalAlign.Right => 1.0,
                _ => 0.0
            };

            double anchorY = vAlign switch
            {
                VerticalAlign.Top => 0.0,
                VerticalAlign.Center => 0.5,
                VerticalAlign.Bottom => 1.0,
                _ => 0.0
            };

            // We set the element's AnchorX/AnchorY for alignment.
            view.AnchorX = anchorX;
            view.AnchorY = anchorY;

            // Then, place the element at (x,y) in proportional space (PositionProportional).
            // The element's top-left corner (or anchor corner) is at that proportion.
            AbsoluteLayout.SetLayoutBounds(view, new Rect(x, y, AutoSize, AutoSize));
            AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.PositionProportional);
        }

        // Helper for "auto sizing" the element
        private static double AutoSize => AbsoluteLayout.AutoSize;
    }
}
