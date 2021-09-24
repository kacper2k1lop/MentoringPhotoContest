using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MentoringPhotoContest.Infrastructure.Configuration
{
    public class ConfigurationBuilder
    {
        //dto - obiekt ktory niesie dane,
        public Configuration BuildConfiguration()
        {
            var result = new Configuration();
            result.DbConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

            return result;
        }
    }
}
