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

        /// <summary>
        /// Execute a query asynchronously
        /// </summary>
        /// <param name="sql">The SQL to execute for the query</param>
        /// <param name="param">The parameters to pass, if there is</param>
        /// <param name="transaction">The transaction to use, if there is</param>
        /// <param name="cancellationToken">Returned by this property will be non-cancelable by default</param>
        /// <typeparam name="T">The type of results to return</typeparam>
        /// <returns>IReadOnlyList<T></returns>
        public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object param = null,
            IDbTransaction transaction = null, CancellationToken cancellationToken = default)
            => (await connection.QueryAsync<T>(sql, param, transaction)).AsList();

        /// <summary>
        /// Execute a query asynchronously paged
        /// </summary>
        /// <param name="sqlCount">The SQL to run for the record total result</param>
        /// <param name="sql">The SQL to execute for the query</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="param">The parameters to pass, if there is</param>
        /// <param name="transaction">The transaction to use, if there is</param>
        /// <param name="cancellationToken">Returned by this property will be non-cancelable by default</param>
        /// <typeparam name="T">The type of results to return</typeparam>
        /// <returns></returns>
        public async Task<IPagedList<T>> QueryToPagedListAsync<T>(string sqlCount, string sql, int pageNumber = 0, int pageSize = 10, object param = null
            , IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        {
            var offset = GetOffset(pageSize, pageNumber);
            var sqlPaged = PagedSql(sql, pageNumber, pageSize);
            var sqlRows = $"{sqlCount}; {sqlPaged};";
            var multi = await connection.QueryMultipleAsync(sqlRows, new { pageSize, offset });
            var totalRowCount = multi.Read<int>().Single();
            var gridDataRows = multi.Read<T>().ToList();
            return PagedList<T>.Factory.Create(gridDataRows, pageSize, pageNumber, totalRowCount);
        }

        /// <summary>
        /// Execute a single-row query asynchronously
        /// </summary>
        /// <param name="sql">The SQL to execute for the query</param>
        /// <param name="param">The parameters to pass, if there is</param>
        /// <param name="transaction">The transaction to use, if there is</param>
        /// <param name="cancellationToken">Returned by this property will be non-cancelable by default</param>
        /// <typeparam name="T">The type of results to return</typeparam>
        /// <returns>Task<T></returns>
        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
            => await connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction);

        /// <summary>
        /// Execute a single-row query asynchronously using
        /// </summary>
        /// <param name="sql">The SQL to execute for the query</param>
        /// <param name="param">The parameters to pass, if there is</param>
        /// <param name="transaction">The transaction to use, if there is</param>
        /// <param name="cancellationToken">Returned by this property will be non-cancelable by default</param>
        /// <typeparam name="T">The type of results to return</typeparam>
        /// <returns>Task<T></returns>
        public async Task<T> QuerySingleAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
            => await connection.QuerySingleAsync<T>(sql, param, transaction);

        public void Dispose()
        {
            connection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}