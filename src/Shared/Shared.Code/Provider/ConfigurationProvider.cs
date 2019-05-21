using Shared.Code.Provider;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Code.Provider
{

    public class DataConfigurationProvider : IDataConfigurationProvider
    {
        private readonly string _connectionString;

        public string ConnectionString => _connectionString;

        public DataConfigurationProvider(string connectionString)
            => _connectionString = connectionString;
    }
}
