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