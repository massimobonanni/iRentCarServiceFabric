using System;
using System.Collections.Generic;

namespace iRentCar.FrontEnd.Models
{
    public class SearchResultModel<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public int TotalItems { get; private set; }

        public SearchResultModel(IEnumerable<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalItems = count;
            if (pageSize == 0)
                TotalPages = 1;
            else
                TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }


        public bool HasPreviousPage => (PageIndex >= 1);

        public bool HasNextPage => (PageIndex < TotalPages-1);

        public static SearchResultModel<T> Create(IEnumerable<T> items, int count, int pageIndex, int pageSize)
        {
            return new SearchResultModel<T>(items, count, pageIndex, pageSize);
        }


    }
}