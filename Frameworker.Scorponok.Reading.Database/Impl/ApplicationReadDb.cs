using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Frameworker.Scorponok.Reading.Database.Impl
{
    public class ApplicationReadDb : IDisposable, IApplicationReadDb
    {
        private static readonly Func<string, int, int, string> PagedSql = (sql, pageIndex, pageSize) => $"{sql} LIMIT {(pageIndex - 1) * pageSize}, {pageSize}";

        private static Func<int, int, int> GetOffset = (pageSize, pageNumber) => (pageNumber - 1) * pageSize;

        /// <summary>
        /// Stores the DB connection string from appSettings.json
        /// </summary>
        private readonly IDbConnection connection;

        public ApplicationReadDb(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            connection = new SqliteConnection(connectionString);
        }

        public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object param = null,
            IDbTransaction transaction = null, CancellationToken cancellationToken = default)
            => (await connection.QueryAsync<T>(sql, param, transaction)).AsList();

        public async Task<PagedList<T>> QueryToPagedListAsync<T>(string sqlCount, string sqlRows, int pageNumber = 0, int pageSize = 10, object param = null, IDbTransaction transaction = null,
            CancellationToken cancellationToken = default)
        {
            var offset = GetOffset(pageSize, pageNumber);
            var sqlPaged = PagedSql(sqlRows, pageNumber, pageSize);
            var sql = $"{sqlCount}; {sqlPaged};";
            var multi = await connection.QueryMultipleAsync(sql, new { pageSize, offset });
            var totalRowCount = multi.Read<int>().Single();
            var gridDataRows = multi.Read<T>().ToList();
            return PagedList<T>.Factory.Create(gridDataRows, pageSize, pageNumber, totalRowCount);
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null,
            IDbTransaction transaction = null, CancellationToken cancellationToken = default)
            => await connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction);

        public async Task<T> QuerySingleAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
            => await connection.QuerySingleAsync<T>(sql, param, transaction);

        public void Dispose() => connection.Dispose();

    }
}