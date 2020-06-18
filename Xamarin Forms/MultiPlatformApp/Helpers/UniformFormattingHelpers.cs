using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MultiPlatformApp.Helpers
{
    public static class UniformFormattingHelpers
    {
        private static double _fontSizeRatio = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width / Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density / 400;
        private static string _normalFont = Application.Current.Resources["NormalFont"] as string;
        private static string _boldFont = Application.Current.Resources["BoldFont"] as string;

        public static void UniversalAdjustments(Xamarin.Forms.View startingView)
        {
            if (startingView.GetType() == typeof(Grid))
            {
                var grid = (Grid)startingView;
                if (grid.RowDefinitions != null)
                {
                    foreach (var rowDef in grid.RowDefinitions)
                    {
                        if (rowDef.Height.IsAbsolute)
                            rowDef.Height = rowDef.Height.Value * _fontSizeRatio;
                    }
                }
                if (grid.ColumnDefinitions != null)
                {
                    foreach (var colDef in grid.ColumnDefinitions)
                    {
                        if (colDef.Width.IsAbsolute)
                            colDef.Width = colDef.Width.Value * _fontSizeRatio;
                    }
                }
                foreach (var item in grid.Children)
                {
                    if (item.GetType() == typeof(StackLayout))
                        StackLayoutAdjustments((StackLayout)item);
                    else if (item.GetType() == typeof(Button))
                        SetButtonAttributes((Button)item);
                    else if (item.GetType() == typeof(Label))
                        SetLabelFontAttributes((Label)item);
                    else if (item.GetType() == typeof(Entry))
                        EntryAdjustments((Entry)item);
                    else if (item.GetType() == typeof(Grid))
                        UniversalAdjustments(item);
                }
            }
            else if (startingView.GetType() == typeof(StackLayout))
                StackLayoutAdjustments((StackLayout)startingView);
            else if (startingView.GetType() == typeof(Frame))
            {
                foreach (var ctl in ((Frame)startingView).Children)
                {
                    UniversalAdjustments((Xamarin.Forms.View)ctl);
                }
            }
            else if (startingView.GetType() == typeof(AbsoluteLayout))
            {
                foreach (var ctl in ((AbsoluteLayout)startingView).Children)
                {
                    UniversalAdjustments(ctl);
                }
            }
            else if (startingView.GetType() == typeof(ContentView))
            {
                foreach (var ctl in ((ContentView)startingView).Children)
                {
                    UniversalAdjustments((Xamarin.Forms.View)ctl);
                }
            }
        }

        public static double SizeByFontRatio(double originalSize)
        {
            return originalSize * _fontSizeRatio;
        }

        public static void StackLayoutAdjustments(StackLayout stackLayout)
        {
            if (stackLayout.HeightRequest != -1)
                stackLayout.HeightRequest = stackLayout.HeightRequest * _fontSizeRatio;
            if (stackLayout.WidthRequest != -1)
                stackLayout.WidthRequest = stackLayout.WidthRequest * _fontSizeRatio;
            foreach (var ctl in stackLayout.Children)
            {
                if (ctl.GetType() == typeof(Label))
                    SetLabelFontAttributes((Label)ctl);
                else if (ctl.GetType() == typeof(Button))
                    SetButtonAttributes((Button)ctl);
                else if (ctl.GetType() == typeof(Frame))
                    SetFrameAttributes((Frame)ctl);
                else if (ctl.GetType() == typeof(Grid))
                    UniversalAdjustments(ctl);
                else if (ctl.GetType() == typeof(StackLayout))
                    StackLayoutAdjustments((StackLayout)ctl);
                else if (ctl.GetType() == typeof(Image))
                    ImageAdjustments((Image)ctl);
                else if (ctl.GetType() == typeof(Entry))
                    EntryAdjustments((Entry)ctl);
                else
                    UniversalAdjustments(ctl);
            }
        }

        public static void EntryAdjustments(Entry entry)
        {
            var height = entry.HeightRequest > 0 ? entry.HeightRequest : entry.Height;
            var width = entry.WidthRequest > 0 ? entry.WidthRequest : entry.Width;
            if (height != -1)
                entry.HeightRequest = height * _fontSizeRatio;
            if (width != -1)
                entry.WidthRequest = width * _fontSizeRatio;
            entry.FontSize = entry.FontSize * _fontSizeRatio;
            if (entry.FontAttributes == FontAttributes.Bold)
                entry.FontFamily = _boldFont;
            else
                entry.FontFamily = _normalFont;
        }

        public static void ImageAdjustments(Image image)
        {
            var height = image.HeightRequest > 0 ? image.HeightRequest : image.Height;
            var width = image.WidthRequest > 0 ? image.WidthRequest : image.Width;
            image.HeightRequest = height * _fontSizeRatio;
            image.WidthRequest = width * _fontSizeRatio;
        }

        public static void SetFrameAttributes(Frame frame)
        {
            frame.HeightRequest = frame.HeightRequest * _fontSizeRatio;
            frame.WidthRequest = frame.WidthRequest * _fontSizeRatio;
            frame.CornerRadius = (float)(frame.CornerRadius * _fontSizeRatio);
            foreach (Xamarin.Forms.View item in frame.Children)
            {
                if (item.GetType() == typeof(Label))
                    SetLabelFontAttributes((Label)item);
                else
                    UniversalAdjustments(item);
            }
        }

        public static void SetButtonAttributes(Button button)
        {
            button.FontSize = button.FontSize * _fontSizeRatio;
            if (button.FontAttributes == FontAttributes.Bold)
                button.FontFamily = _boldFont;
            else
                button.FontFamily = _normalFont;
            var height = button.HeightRequest > 0 ? button.HeightRequest : button.Height;
            var width = button.WidthRequest > 0 ? button.WidthRequest : button.Width;
            button.HeightRequest = height * _fontSizeRatio;
            button.WidthRequest = width * _fontSizeRatio;
            if (button.WidthRequest > Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width / Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density)
                button.WidthRequest = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width / Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;
            button.CornerRadius = (int)(Math.Min(button.HeightRequest / 5, 5 * _fontSizeRatio));
        }

        public static void SetLabelFontAttributes(Label label)
        {
            label.FontSize = label.FontSize * _fontSizeRatio;
            if (label.FontAttributes == FontAttributes.Bold)
                label.FontFamily = _boldFont;
            else
                label.FontFamily = _normalFont;
            if (label.FormattedText != null)
            {
                foreach (var span in label.FormattedText.Spans)
                {
                    span.FontSize = span.FontSize * _fontSizeRatio;
                    if (span.FontAttributes == FontAttributes.Bold)
                        span.FontFamily = _boldFont;
                    else
                        span.FontFamily = _normalFont;
                }
            }
        }

        public static void HeightRequest(Xamarin.Forms.View item, double heightRequest)
        {
            item.HeightRequest = heightRequest * _fontSizeRatio;
        }
        public static void WidthRequest(Xamarin.Forms.View item, double widthRequest)
        {
            item.WidthRequest = widthRequest * _fontSizeRatio;
        }
        public static void HeightAndWidthRequest(Xamarin.Forms.View item, double heightAndWidthRequest)
        {
            item.HeightRequest = heightAndWidthRequest * _fontSizeRatio;
            item.WidthRequest = heightAndWidthRequest * _fontSizeRatio;
        }
    }
}
