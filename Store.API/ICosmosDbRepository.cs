using Azure;
using Microsoft.Azure.Cosmos;
using P.Pager;
using System.Linq.Expressions;

public interface ICosmosDbRepository
{
    Task AddItemAsync<T>(T item, string partitionKey) where T : class;
    Task DeleteItemAsync<T>(string id, string partitionKey) where T : class;
    Task<T> GetItemAsync<T>(string id, string partitionKey) where T : class;
    Task<IEnumerable<T>> GetItemsAsync<T>(string queryString, string partitionKey = null) where T : class;
    Task<IEnumerable<T>> GetItemsAsync<T>(QueryDefinition queryDefinition, string partitionKey = null) where T : class;
    Task<IEnumerable<T>> GetItemsAsync<T>(Expression<Func<T, bool>> predicate, int? skip = null, int? take = null,
        string partitionKey = null) where T : class;
    Task<IPager<T>> GetItemsWithPagingAsync<T>(Expression<Func<T, bool>> predicate, int pageIndex, int pageSize,
        string partitionKey = null) where T : class;
    Task UpdateItemAsync<T>(T item, string partitionKey) where T : class;
}