namespace Microsoft.Labs.SightsToSee.Services.DataModelService
{
    public static class DataModelServiceFactory
    {
        private static IDataModelService _dataModelService;

        public static IDataModelService CurrentDataModelService()
        {
            if (_dataModelService == null)
            {
#if EFCORE
                _dataModelService = new EfCoreDataModelService();
#else
                _dataModelService = new AzureDataModelService();
#endif
            }

            return _dataModelService;
        }
    }
}