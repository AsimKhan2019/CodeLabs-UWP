using Microsoft.Labs.SightsToSee.Library.Models;
using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Microsoft.Labs.SightsToSee.Services.TilesNotificationsService
{
    public static class ScheduledNotificationService
    {
        public static void AddToastReminder(Sight sight)
        {
            var manager = ToastNotificationManager.CreateToastNotifier();
            manager.AddToSchedule(new ScheduledToastNotification(GenerateXMLContent(sight), DateTime.Now.AddSeconds(30)));
        }

        public static XmlDocument GenerateXMLContent(Sight sight)
        {
            var xml = $@"<?xml version='1.0'?>
                <toast launch='SightReminder' scenario='reminder'>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>Visit {sight.Name}</text>
                            <text>{sight.Trip.Name}</text>
                            <text>{sight.VisitDate.Value.ToString("hh:mm tt")}</text>
                            <image src='{sight.ImagePath}'/>
                        </binding>
                    </visual>
                    <actions hint-systemCommands = 'SnoozeAndDismiss' >
                        <action content='View Sight' activationType='foreground' arguments='View:{sight.Id.ToString()}' />
                        <action content='Remove from plan' activationType='foreground' arguments='Remove:{sight.Id.ToString()}' />
                    </actions>
                </toast>";

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            return xmlDoc;
        }

        //Not Used to due inability to specify action hint-systemCommands with subactions in NotificationsExtensions
        public static XmlDocument GenerateContent(Sight sight)
        {
            var content = new ToastContent()
            {
                Launch = "SightReminder",

                Visual = new ToastVisual()
                {
                    TitleText = new ToastText()
                    {
                        Text = $"Visit {sight.Name}"
                    },

                    BodyTextLine1 = new ToastText()
                    {
                        Text = $"{sight.Location}"
                    },

                    BodyTextLine2 = new ToastText()
                    {
                        Text = $"{sight.VisitDate.Value.ToString("hh:mm tt")}"
                    },
                    InlineImages =
                    {
                        new ToastImage()
                        {
                            Source = new ToastImageSource(sight.ImagePath)
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButtonSnooze(),
                        new ToastButtonDismiss(),
                        new ToastButton("View", $"View:{sight.Id.ToString()}")
                        {
                            ActivationType = ToastActivationType.Foreground
                        },
                        new ToastButton("Remove", $"Remove:{sight.Id.ToString()}")
                        {
                            ActivationType = ToastActivationType.Foreground
                        }
                    }
                },
            };

            return content.GetXml();
        }
    }
}
