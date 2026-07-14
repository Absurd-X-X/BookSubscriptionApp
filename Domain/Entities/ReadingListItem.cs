using Domain.Enums;

namespace Domain.Entities
{
    public class ReadingListItem
    {
        public Guid Id { get; set; }

        public Guid ReaderId { get; set; }
        public Reader Reader { get; set; } = default!;

        public Guid BookId { get; set; }
        public Book Book { get; set; } = default!;

        public BookListType ListType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}