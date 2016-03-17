namespace Microsoft.Labs.SightsToSee.Library.Services.DataModelService
{
    public static class DataModelServiceFactory
    {
        private static IDataModelService _dataModelService;

        public static IDataModelService CurrentDataModelService()
        {
            if (_dataModelService == null)
            {
#if SQLITE
                _dataModelService = new SqliteDataModelService();
#else
                _dataModelService = new AzureDataModelService();
#endif
            }

            return _dataModelService;
        }
    }
}