namespace Application.Common.Dtos
{
    public class Result<T>
    {
        public string Message { get; set; } = default!;
        public T? Data { get; set; }
        public bool Status { get; set; }

        public static Result<T> Success(T data, string message)
        {
            return new Result<T>
            {
                Data = data,
                Status = true,
                Message = message
            };
        }

        public static Result<T> Failure(string message)
        {
            return new Result<T>
            {
                Status = false,
                Message = message
            };
        }
    }
}
