namespace Catalog.Queries;

public class PagingModel 
{  
    const int maxPageSize = 20;  
  
    public int PageNumber { get; set; }
  
    public int PageSize { get; set; } 
}