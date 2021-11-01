using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Frameworker.Scorponok.Reading.Database.Impl;

namespace Frameworker.Scorponok.Reading.Database
{
    public interface IApplicationReadDb
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql">A plain string sql query</param>
        /// <param name="param">represents a parameterized the query</param>
        /// <param name="transaction">Represents a transaction to be performed at a data source, and is implemented by .NET data providers that access relational databases.</param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlCount"></param>
        /// <param name="sqlRows"></param>
        /// <param name="pageNumber">The page index to fetch. 0-based (Starts with 0)</param>
        /// <param name="pageSize">The page size. Must be a positive number</param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<PagedList<T>> QueryToPagedListAsync<T>(string sqlCount, string sqlRows, int pageNumber = 0, int pageSize = 10, object param = null, IDbTransaction transaction = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql">A plain string sql query</param>
        /// <param name="param">represents a parameterized the query</param>
        /// <param name="transaction">Represents a transaction to be performed at a data source, and is implemented by .NET data providers that access relational databases.</param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql">A plain string sql query</param>
        /// <param name="param">represents a parameterized the query</param>
        /// <param name="transaction">Represents a transaction to be performed at a data source, and is implemented by .NET data providers that access relational databases.</param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> QuerySingleAsync<T>(string sql, object param = null, IDbTransaction transaction = null,
            CancellationToken cancellationToken = default);
    }
}