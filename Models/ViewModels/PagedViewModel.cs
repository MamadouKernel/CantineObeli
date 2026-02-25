namespace Obeli_K.Models.ViewModels
{
    public class PagedViewModel<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public PaginationViewModel? Pagination { get; set; }
    }
}
