using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.ReaderBooks.Queries.GetReaderBooksPage
{
    public sealed record GetReaderBooksPageQuery(Guid ReaderId) : IRequest<ReaderBooksPageVm>;

    public sealed class ReaderBooksPageVm
    {
        public HeroBookVm? HeroBook { get; init; }
        public List<CategoryVm> Categories { get; init; } = [];
        public List<BookCardVm> NewArrivals { get; init; } = [];
        public List<BookCardVm> Recommended { get; init; } = [];
        public List<BookCardVm> Popular { get; init; } = [];
    }

    public sealed class HeroBookVm
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = default!;
        public string Author { get; init; } = default!;
        public string CoverImageUrl { get; init; } = default!;
        public string About { get; init; } = default!;
        public double AverageRating { get; init; }
        public int ReviewCount { get; init; }
    }

    public sealed class CategoryVm
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public int BookCount { get; init; }
    }

    public sealed class BookCardVm
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = default!;
        public string Author { get; init; } = default!;
        public string CoverImageUrl { get; init; } = default!;
        public double AverageRating { get; init; }
        public bool IsNew { get; init; }
        public bool IsInReadingList { get; init; } // drives the save icon
        public bool IsFavorite { get; init; }       // drives "Add to List" state
    }

    public sealed class GetReaderBooksPageQueryHandler(
        IBookRepository bookRepository,
        IReviewRepository reviewRepository,
        ICategoryRepository categoryRepository,
        IReadingListRepository readingListRepository,
        IFavoriteRepository favoriteRepository)
        : IRequestHandler<GetReaderBooksPageQuery, ReaderBooksPageVm>
    {
        public async Task<ReaderBooksPageVm> Handle(GetReaderBooksPageQuery request, CancellationToken ct)
        {
            var topReadPage = await bookRepository.GetAllAsync(
                new PageRequest { Page = 1, PageSize = 11 }, usePaging: true);

            var topRead = topReadPage.Items.ToList();
            var heroBook = topRead.FirstOrDefault();
            var popularBooks = topRead.Skip(1).Take(10).ToList();

            var newArrivals = await bookRepository.GetNewArrivalsAsync(10);
            var recommended = await bookRepository.GetRecommendedForReaderAsync(request.ReaderId, null, 10);
            var categories = await categoryRepository.GetAllCategoriesAsync();

            return new ReaderBooksPageVm
            {
                HeroBook = heroBook is null ? null : await BuildHeroVm(heroBook),
                Categories = categories
                    .Select(c => new CategoryVm
                    {
                        Id = c.Id,
                        Name = c.Name,
                        BookCount = c.Books.Count(b => !b.IsDeleted && b.IsPublished)
                    })
                    .ToList(),
                NewArrivals = await BuildBookCards(newArrivals, request.ReaderId),
                Recommended = await BuildBookCards(recommended, request.ReaderId),
                Popular = await BuildBookCards(popularBooks, request.ReaderId)
            };
        }

        private async Task<HeroBookVm> BuildHeroVm(Book book)
        {
            var avgRating = await reviewRepository.GetAverageRatingForBookAsync(book.Id);
            var reviewCount = await reviewRepository.CountByBookIdAsync(book.Id);

            return new HeroBookVm
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                CoverImageUrl = book.BookCoverUrl,
                About = book.About,
                AverageRating = avgRating,
                ReviewCount = reviewCount
            };
        }

        private async Task<List<BookCardVm>> BuildBookCards(IEnumerable<Book> books, Guid readerId)
        {
            var newArrivalsCutoff = DateTime.UtcNow.AddDays(-30);
            var cards = new List<BookCardVm>();

            foreach (var book in books)
            {
                var avgRating = await reviewRepository.GetAverageRatingForBookAsync(book.Id);
                var inReadingList = await readingListRepository.IsInReadingListAsync(readerId, book.Id);
                var isFavorite = await favoriteRepository.IsFavoriteAsync(readerId, book.Id);

                cards.Add(new BookCardVm
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    CoverImageUrl = book.BookCoverUrl,
                    AverageRating = avgRating,
                    IsNew = book.DateCreated >= newArrivalsCutoff,
                    IsInReadingList = inReadingList,
                    IsFavorite = isFavorite
                });
            }

            return cards;
        }
    }
}