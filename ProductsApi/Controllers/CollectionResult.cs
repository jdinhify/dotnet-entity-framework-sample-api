using System.Collections.Generic;

namespace ProductsApi.Controllers
{
    public readonly struct CollectionResult<T>
    {
        public List<T> Items { get; }

        public CollectionResult(List<T> items)
        {
            Items = items;
        }
    }
}