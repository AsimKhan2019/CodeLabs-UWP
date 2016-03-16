namespace Microsoft.Labs.SightsToSee.Library.Services.DataModelService
{
    public static class DataModelServiceFactory
    {
        private static IDataModelService _dataModelService;

        public static IDataModelService CurrentDataModelService()
        {
            if (_dataModelService == null)
            {
                _dataModelService = new SqliteDataModelService();
            }

            return _dataModelService;
        }
    }
}