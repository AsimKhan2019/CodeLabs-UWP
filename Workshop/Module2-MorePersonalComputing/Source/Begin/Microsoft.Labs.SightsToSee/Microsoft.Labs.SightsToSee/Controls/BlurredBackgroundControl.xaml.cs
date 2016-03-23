using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Microsoft.Labs.SightsToSee.Controls
{
    public sealed partial class BlurredBackgroundControl : UserControl
    {
        public static readonly DependencyProperty BlurFactorProperty = DependencyProperty.Register(
            "BlurFactor", typeof (float), typeof (BlurredBackgroundControl), new PropertyMetadata(20.0f));

        public static readonly DependencyProperty BackgroundImageSourceProperty = DependencyProperty.Register(
            "BackgroundImageSource", typeof (ImageSource), typeof (BlurredBackgroundControl),
            new PropertyMetadata(default(ImageSource), PropertyChangedCallback));

        private CanvasBitmap _backgroundBitmap;
        private GaussianBlurEffect _blurEffect;

        public BlurredBackgroundControl()
        {
            InitializeComponent();
            BlurredImage.CreateResources += BlurredImage_CreateResources;
            BlurredImage.Draw += BlurredImage_Draw;
        }

        public float BlurFactor
        {
            get { return (float) GetValue(BlurFactorProperty); }
            set { SetValue(BlurFactorProperty, value); }
        }

        public ImageSource BackgroundImageSource
        {
            get { return (ImageSource) GetValue(BackgroundImageSourceProperty); }
            set { SetValue(BackgroundImageSourceProperty, value); }
        }

        private static async void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = dependencyObject as BlurredBackgroundControl;
            if (control?.BackgroundImageSource == null) return;

            var imageSource = control.BackgroundImageSource;


            try
            {
                while (!control.BlurredImage.ReadyToDraw)
                {
                    await Task.Delay(100);
                }

                if (imageSource == null)
                {
                    return;
                }

                var uriPath = ((BitmapImage) imageSource).UriSource;
                var path = uriPath.AbsoluteUri;
                if (path.StartsWith("ms-appx"))
                {
                    //Add the Preprocessor directive EFCOREHACK to enable generating migrations
                    //Pending issue https://github.com/aspnet/EntityFramework/issues/4683
#if !EFCOREHACK
                    control._backgroundBitmap =
                        await CanvasBitmap.LoadAsync(control.BlurredImage, ((BitmapImage) imageSource).UriSource);
#endif
                }
                else if (path.StartsWith("ms-appdata"))
                {
#if !EFCOREHACK
                    control._backgroundBitmap = 
                        await CanvasBitmap.LoadAsync(control.BlurredImage,((BitmapImage)imageSource).UriSource);
#endif
                }
                else
                {
#if !EFCOREHACK
                    control._backgroundBitmap = 
                        await CanvasBitmap.LoadAsync(control.BlurredImage, uriPath.AbsolutePath);
#endif
                }

                control._blurEffect = new GaussianBlurEffect
                {
                    Source = control._backgroundBitmap,
                    BlurAmount = control.BlurFactor
                };

                var fadeOut = new Storyboard();
                var daOut = new DoubleAnimation
                {
                    From = 0.6,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                    AutoReverse = false
                };
                Storyboard.SetTarget(daOut, control.OpacityLayer);
                Storyboard.SetTargetProperty(daOut, "Opacity");
                fadeOut.Children.Add(daOut);
                var fadeIn = new Storyboard();
                var daIn = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.6,
                    Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                    AutoReverse = false
                };
                Storyboard.SetTarget(daIn, control.OpacityLayer);
                Storyboard.SetTargetProperty(daIn, "Opacity");
                fadeIn.Children.Add(daIn);
                fadeOut.Begin();
                fadeOut.Completed += (sender, o) =>
                {
                    control.BlurredImage.Invalidate();
                    fadeIn.Begin();
                };
            }
            catch (ArgumentException)
            {
                // swallow if we don't have the canvas device
            }
        }

        private void BlurredImage_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_backgroundBitmap != null)
            {
                var imageHeight = _backgroundBitmap.Bounds.Height;
                var imageWidth = _backgroundBitmap.Bounds.Width;

                var scale = Math.Min(BlurredImage.ActualWidth/imageWidth, BlurredImage.ActualHeight/imageHeight);

                double xOffset = 0, yOffset = 0;
                if (Math.Abs(imageWidth*scale - BlurredImage.ActualWidth) < 1)
                {
                    // Basically the same width, we need to scale up for the height to fit
                    var newScale = BlurredImage.ActualHeight/(imageHeight*scale);
                    scale *= newScale;
                }
                else
                {
                    var newScale = BlurredImage.ActualWidth/(imageWidth*scale);
                    scale *= newScale;
                }

                yOffset = (BlurredImage.ActualHeight - imageHeight*scale)/2.0;
                xOffset = (BlurredImage.ActualWidth - imageWidth*scale)/2.0;

                args.DrawingSession.DrawImage(_blurEffect,
                    new Rect(xOffset, yOffset, imageWidth*scale, imageHeight*scale),
                    _backgroundBitmap.Bounds);
            }
        }

        private void BlurredImage_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        private async Task CreateResourcesAsync(CanvasControl sender)
        {
            // Comment the following line when running migrations
            if (BackgroundImageSource != null)
            {
#if !EFCOREHACK
                _backgroundBitmap =
                    await CanvasBitmap.LoadAsync(sender, ((BitmapImage) BackgroundImageSource).UriSource);
                _blurEffect = new GaussianBlurEffect
                {
                    Source = _backgroundBitmap,
                    BlurAmount = BlurFactor
                };
#endif
            }
        }
    }
}