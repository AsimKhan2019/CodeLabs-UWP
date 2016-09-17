using Microsoft.Labs.SightsToSee.Library.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace Microsoft.Labs.SightsToSee.Services.TilesNotificationsService
{
    internal class TileHelper
    {
        public static void SetInteractiveTilesForTrip(Trip trip)
        {
            // Enable notification cycling
            // Call only needs to be made once while the app is running, though there is no harm in calling it again.
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);

            if (trip.Sights.Count > 0)
            {
                int sightCount = 0;
                foreach (var sight in trip.Sights)
                {
                    if (sight.IsMySight)
                    {
                        sightCount++;
                        // NotificationQueue is limited to 5, so limit to only the first 5 sights
                        if (sightCount > 4) break;


                        TileContent content = new TileContent()
                        {
                            Visual = new TileVisual()
                            {
                                Branding = TileBranding.NameAndLogo,

                                // For the small tile, just put the trip name on the tile
                                TileSmall = new TileBinding()
                                {
                                    Content = new TileBindingContentAdaptive()
                                    {
                                        Children =
                                        {
                                            new AdaptiveText()
                                            {
                                                Text = trip.Name,
                                                HintStyle = AdaptiveTextStyle.Caption,
                                                HintWrap = true
                                            }
                                        }
                                    }
                                },

                                // Medium tile gets image and some small text
                                TileMedium = new TileBinding()
                                {
                                    Content = new TileBindingContentAdaptive()
                                    {
                                        PeekImage = new TilePeekImage()
                                        {
                                            Source = sight.ImagePath,
                                            HintOverlay = 20
                                        },
                                        Children =
                                        {
                                            new AdaptiveText()
                                            {
                                                Text = sight.Name,
                                                HintStyle = AdaptiveTextStyle.Caption
                                            },
                                            new AdaptiveText()
                                            {
                                                Text = sight.Description,
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                HintWrap = true,
                                                HintMaxLines = 2
                                            }
                                        }
                                    }
                                },

                                // Wide tile gets the sight image and larger text
                                TileWide = new TileBinding()
                                {
                                    Content = new TileBindingContentAdaptive()
                                    {
                                        PeekImage = new TilePeekImage()
                                        {
                                            Source = sight.ImagePath,
                                            HintOverlay = 20
                                        },
                                        Children =
                                        {
                                            new AdaptiveText()
                                            {
                                                Text = sight.Name,
                                                HintStyle = AdaptiveTextStyle.Subtitle
                                            },
                                            new AdaptiveText()
                                            {
                                                Text = sight.Description,
                                                HintStyle = AdaptiveTextStyle.BodySubtle,
                                                HintWrap = true,
                                                HintMaxLines = 2
                                            }
                                        }
                                    }
                                },

                                // Large tile is used on desktop only, but no harm in sending the notification on other devie families
                                TileLarge = new TileBinding()
                                {
                                    Content = new TileBindingContentAdaptive()
                                    {
                                        PeekImage = new TilePeekImage()
                                        {
                                            Source = sight.ImagePath,
                                            HintOverlay = 20
                                        },
                                        Children =
                                        {
                                            new AdaptiveText()
                                            {
                                                Text = sight.Name,
                                                HintStyle = AdaptiveTextStyle.Subtitle
                                            },
                                            new AdaptiveText()
                                            {
                                                Text = sight.Description,
                                                HintStyle = AdaptiveTextStyle.BodySubtle,
                                                HintWrap = true,
                                                HintMaxLines = 3
                                            }
                                        }
                                    }
                                }
                            }
                        };

                        XmlDocument doc = content.GetXml();

                        // Generate WinRT notification
                        var tileNotification = new TileNotification(doc);
                        TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
                    }
                }
            }
        }

        //private static async Task<string> CreateScaledImageAsync(Sight sight)
        //{
        //    // maximum size of a Tile image is 1024 x 1024 pixels, and a file size of 200 KB
        //    // If the current image size is too big, we will have to create a scaled down copy
        //    if (sight.ImageUri.PixelWidth > 1024 || sight.ImageUri.PixelHeight > 1024)
        //    {
        //        // Copy the image file to temp folder


        //        // Create a BitmapDecoder
        //        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(inputStream);

        //        // create a new stream and encoder for the new image
        //        InMemoryRandomAccessStream ras = new InMemoryRandomAccessStream();
        //        BitmapEncoder enc = await BitmapEncoder.CreateForTranscodingAsync(ras, decoder);

        //        // convert the entire bitmap to a 100px by 100px bitmap
        //        enc.BitmapTransform.ScaledHeight = 100;
        //        enc.BitmapTransform.ScaledWidth = 100;

        //        //BitmapBounds bounds = new BitmapBounds();
        //        //bounds.Height = 100;
        //        //bounds.Width = 100;
        //        //bounds.X = 0;
        //        //bounds.Y = 0;
        //        //enc.BitmapTransform.Bounds = bounds;

        //        // write out to the stream
        //        await enc.FlushAsync();

        //    }
        //}
    }
}
