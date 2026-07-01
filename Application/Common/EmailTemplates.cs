namespace Application.Common
{
    public static class EmailTemplates
    {
        private static string BaseTemplate(string content) => $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; background: #f4f4f4; margin: 0; padding: 0; }}
                .container {{ max-width: 600px; margin: 30px auto; background: #fff; border-radius: 8px; overflow: hidden; }}
                .header {{ background: #4F46E5; padding: 20px; text-align: center; }}
                .header h1 {{ color: #fff; margin: 0; font-size: 24px; }}
                .body {{ padding: 30px; color: #333; line-height: 1.6; }}
                .button {{ display: inline-block; padding: 12px 24px; background: #4F46E5; color: #fff; 
                           text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                .footer {{ background: #f4f4f4; padding: 15px; text-align: center; 
                           font-size: 12px; color: #999; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'><h1>Book Subscription App</h1></div>
                <div class='body'>{content}</div>
                <div class='footer'>© {DateTime.Now.Year} Book subscription App. All rights reserved.</div>
            </div>
        </body>
        </html>";

        public static string WelcomeEmail(string fullName) => BaseTemplate($@"
            <h2>Welcome, {fullName}! 🎉</h2>
            <p>Thank you for registering on Book Subscription App. Your account has been created successfully.</p>
            <p>You can now browse books, fund your wallet and subscribe to a plan.</p>
            <p>If you have any questions, feel free to reach out to our support team.</p>
            <br/>
            <p>Best regards,<br/><strong>Book Subscription App Team</strong></p>");

        public static string WalletFundedEmail(string fullName, decimal amount, decimal newBalance) => BaseTemplate($@"
            <h2>Wallet Funded Successfully ✅</h2>
            <p>Hi <strong>{fullName}</strong>,</p>
            <p>Your wallet has been credited with <strong>₦{amount:N2}</strong>.</p>
            <table style='width:100%; border-collapse:collapse; margin:20px 0;'>
                <tr style='background:#f4f4f4;'>
                    <td style='padding:10px; border:1px solid #ddd;'>Amount Credited</td>
                    <td style='padding:10px; border:1px solid #ddd;'><strong>₦{amount:N2}</strong></td>
                </tr>
                <tr>
                    <td style='padding:10px; border:1px solid #ddd;'>New Balance</td>
                    <td style='padding:10px; border:1px solid #ddd;'><strong>₦{newBalance:N2}</strong></td>
                </tr>
            </table>
            <p>If you did not initiate this transaction, please contact support immediately.</p>
            <br/>
            <p>Best regards,<br/><strong>EcommerceApp Team</strong></p>");

        public static string ForgotPasswordEmail(string fullName, string token) => BaseTemplate($@"
    <h2>Password Reset Code 🔐</h2>
    <p>Hi <strong>{fullName}</strong>,</p>
    <p>We received a request to reset your password. Use the code below:</p>
    <div style='text-align:center; margin:30px 0;'>
        <div style='display:inline-block; background:#e53e3e; color:#fff;
                    font-size:36px; font-weight:bold; letter-spacing:10px;
                    padding:20px 40px; border-radius:8px;'>
            {token}
        </div>
    </div>
    <p style='text-align:center; color:#e53e3e;'>
        ⚠️ This code expires in <strong>5 minutes</strong>
    </p>
    <p>If you did not request a password reset please ignore this email.</p>
    <br/>
    <p>Best regards,<br/><strong>EcommerceApp Team</strong></p>");
        public static string WithdrawalEmail(string fullName, decimal amount, string accountNumber, string bankName, decimal newBalance) => BaseTemplate($@"
            <h2>Withdrawal Initiated 🏦</h2>
            <p>Hi <strong>{fullName}</strong>,</p>
            <p>Your withdrawal request has been initiated successfully.</p>
            <table style='width:100%; border-collapse:collapse; margin:20px 0;'>
                <tr style='background:#f4f4f4;'>
                    <td style='padding:10px; border:1px solid #ddd;'>Amount</td>
                    <td style='padding:10px; border:1px solid #ddd;'><strong>₦{amount:N2}</strong></td>
                </tr>
                <tr>
                    <td style='padding:10px; border:1px solid #ddd;'>Account Number</td>
                    <td style='padding:10px; border:1px solid #ddd;'>{accountNumber}</td>
                </tr>
                <tr style='background:#f4f4f4;'>
                    <td style='padding:10px; border:1px solid #ddd;'>Bank</td>
                    <td style='padding:10px; border:1px solid #ddd;'>{bankName}</td>
                </tr>
                <tr>
                    <td style='padding:10px; border:1px solid #ddd;'>New Balance</td>
                    <td style='padding:10px; border:1px solid #ddd;'><strong>₦{newBalance:N2}</strong></td>
                </tr>
            </table>
            <p>If you did not initiate this transaction, please contact support immediately.</p>
            <br/>
            <p>Best regards,<br/><strong>EcommerceApp Team</strong></p>");

        public static string AdminNewUserEmail(string fullName, string email) => BaseTemplate($@"
            <h2>New User Registration 👤</h2>
            <p>A new user has just registered on EcommerceApp.</p>
            <table style='width:100%; border-collapse:collapse; margin:20px 0;'>
                <tr style='background:#f4f4f4;'>
                    <td style='padding:10px; border:1px solid #ddd;'>Full Name</td>
                    <td style='padding:10px; border:1px solid #ddd;'>{fullName}</td>
                </tr>
                <tr>
                    <td style='padding:10px; border:1px solid #ddd;'>Email</td>
                    <td style='padding:10px; border:1px solid #ddd;'>{email}</td>
                </tr>
                <tr style='background:#f4f4f4;'>
                    <td style='padding:10px; border:1px solid #ddd;'>Date</td>
                    <td style='padding:10px; border:1px solid #ddd;'>{DateTime.Now:dd MMM yyyy, hh:mm tt}</td>
                </tr>
            </table>");

        public static string VerificationEmail(string fullName, string token) => BaseTemplate($@"
            <h2>Verify Your Email Address 📧</h2>
            <p>Hi <strong>{fullName}</strong>,</p>
            <p>Thank you for registering! Please use the code below to verify your email address.</p>
            <div style='text-align:center; margin:30px 0;'>
                <div style='display:inline-block; background:#4F46E5; color:#fff;
                            font-size:36px; font-weight:bold; letter-spacing:10px;
                            padding:20px 40px; border-radius:8px;'>
                    {token}
                </div>
            </div>
            <p style='text-align:center; color:#e53e3e;'>
                ⚠️ This code expires in <strong>5 minutes</strong>
            </p>
            <p>If you did not create an account please ignore this email.</p>
            <br/>
            <p>Best regards,<br/><strong>EcommerceApp Team</strong></p>");

        public static string AdminNewTransactionEmail(string fullName, string type, decimal amount, string reference) => BaseTemplate($@"
            <h2>New Transaction Alert 💰</h2>
            <p>A new transaction has been recorded.</p>
            <table style='width:100%; border-collapse:collapse; margin:20px 0;'>
                <tr style='background:#f4f4f4;'>
                    <td style='padding:10px; border:1px solid #ddd;'>Customer</td>
                    <td style='padding:10px; border:1px solid #ddd;'>{fullName}</td>
                </tr>
                <tr>
                    <td style='padding:10px; border:1px solid #ddd;'>Type</td>
                    <td style='padding:10px; border:1px solid #ddd;'>{type}</td>
                </tr>
                <tr style='background:#f4f4f4;'>
                    <td style='padding:10px; border:1px solid #ddd;'>Amount</td>
                    <td style='padding:10px; border:1px solid #ddd;'><strong>₦{amount:N2}</strong></td>
                </tr>
                <tr>
                    <td style='padding:10px; border:1px solid #ddd;'>Reference</td>
                    <td style='padding:10px; border:1px solid #ddd;'>{reference}</td>
                </tr>
                <tr style='background:#f4f4f4;'>
                    <td style='padding:10px; border:1px solid #ddd;'>Date</td>
                    <td style='padding:10px; border:1px solid #ddd;'>{DateTime.Now:dd MMM yyyy, hh:mm tt}</td>
                </tr>
            </table>");
    }
}
