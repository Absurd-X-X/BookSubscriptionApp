namespace Application.Common.Dtos
{
    public class PaystackInitializeResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; } = default!;
        public string AuthorizationUrl { get; set; } = default!;
        public string Reference { get; set; } = default!;
    }

    public class PaystackVerifyResponse
    {
        public bool Status { get; set; }
        public string PaymentStatus { get; set; } = default!; // "success", "failed"
        public decimal Amount { get; set; }
        public string Reference { get; set; } = default!;
    }

    public class PaystackAccountResponse
    {
        public bool Status { get; set; }
        public string AccountName { get; set; } = default!;
        public string AccountNumber { get; set; } = default!;
    }

    public class PaystackTransferResponse
    {
        public bool Status { get; set; }
        public string TransferStatus { get; set; } = default!; // "pending", "success", "failed"
        public string Reference { get; set; } = default!;
    }

    public class PaystackBank
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
    }
}
