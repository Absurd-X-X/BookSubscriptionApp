namespace Infrastructure.Services
{
    public class EmailResult
    {
        public bool Success { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static EmailResult Ok() => new EmailResult { Success = true };

        public static EmailResult Fail(string errorMessage) =>
            new EmailResult { Success = false, ErrorMessage = errorMessage };
    }
}
