using System;
using System.Collections.Generic;

namespace Application.DTOs.Common
{
    public class PageResultDTO<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }      
        public int PageSize { get; set; }       
        public int TotalPages { get; set; }     
        public int TotalCount { get; set; }      
    }
}
