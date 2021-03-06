﻿using System;
using Windows.Storage;

namespace Microsoft.Labs.SightsToSee.Models
{
    public static class AppSettings
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        public static bool HasRun
        {
            get { return GetTyped<bool>(nameof(HasRun)); }
            set { LocalSettings.Values[nameof(HasRun)] = value; }
        }

        public static string MapServiceToken =>
        "7H7lMjEkAfP3PeOrrPVO~IRTU1f4lP6GTdpBxi4gqoQ~AvvbAnSGbHtsowQ98zRfwvaw6PdCgo2vq3x75R3_SbvN2zb7-YcaM_UIPNtNWOWK";


        public static Guid LastTripId
        {
            get { return GetTyped<Guid>(nameof(LastTripId)); }
            set { LocalSettings.Values[nameof(LastTripId)] = value; }
        }

        private static T GetTyped<T>(string key)
        {
            var value = LocalSettings.Values[key];

            if (value is T)
            {
                return (T) value;
            }
            return default(T);
        }
    }
}