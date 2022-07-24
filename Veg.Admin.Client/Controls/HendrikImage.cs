using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Veg.API.Client;

namespace Veg.Admin.Client.Controls
{
    public class HendrikImage : Image
    {
        static HendrikImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HendrikImage), new FrameworkPropertyMetadata(typeof(HendrikImage)));
        }

        public static readonly DependencyProperty ImageNameProperty =
           DependencyProperty.Register("ImageName", typeof(string), typeof(HendrikImage), new
              PropertyMetadata("", new PropertyChangedCallback(OnImageNameChanged)));

        public string ImageName
        {
            get { return (string)GetValue(ImageNameProperty); }
            set { SetValue(ImageNameProperty, value); }
        }

        private static void OnImageNameChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            HendrikImage UserControl1Control = d as HendrikImage;
            UserControl1Control.OnImageNameOrSizeChanged(e);
        }

        public static readonly DependencyProperty ImageSizeProperty =
         DependencyProperty.Register("ImageSize", typeof(string), typeof(HendrikImage), new
            PropertyMetadata("512", new PropertyChangedCallback(OnImageSizeChanged)));

        public string ImageSize
        {
            get { return (string)GetValue(ImageSizeProperty); }
            set { SetValue(ImageSizeProperty, value); }
        }

        private static void OnImageSizeChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            HendrikImage UserControl1Control = d as HendrikImage;
            UserControl1Control.OnImageNameOrSizeChanged(e);
        }

        private void OnImageNameOrSizeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ImageSize))
            {
                var fullFilePath = GetImagePath(ImageSize, ImageName);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullFilePath, UriKind.Absolute);
                bitmap.EndInit();

                Source = bitmap;
            }
        }

        public string GetImagePath(string imageSize, string imageName)
        {
            return GetAPIUrl() + @"imagestore/" + imageSize + "/" + (imageName != null ? imageName : "NoImage.png");
        }

        public string GetAPIUrl()
        {
            return "http://localhost:5003/";
        }
    }
}
