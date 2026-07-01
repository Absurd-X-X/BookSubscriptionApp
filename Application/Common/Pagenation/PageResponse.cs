namespace Application.Common.Pagenation
{
    public class PagenatedList<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public long TotalCount { get; set; }
        public int Page {  get; set; }
        public int PageSize { get; set; }
        public long PageCount  => (long)Math.Ceiling(TotalCount / (decimal)PageSize);
    }
}
