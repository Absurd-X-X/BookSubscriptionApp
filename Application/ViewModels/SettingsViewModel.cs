namespace Application.ViewModels
{
    public class SettingsViewModel
    {
        // ==========================
        // General Settings
        // ==========================
        public string ApplicationName { get; set; } = string.Empty;
        public string Tagline { get; set; } = string.Empty;
        public string DefaultCurrency { get; set; } = string.Empty;
        public string DefaultLanguage { get; set; } = string.Empty;
        public string DateFormat { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public string LogoPath { get; set; } = string.Empty;

        // ==========================
        // Business Information
        // ==========================
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessEmail { get; set; } = string.Empty;
        public string BusinessPhone { get; set; } = string.Empty;
        public string BusinessAddress { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;

        // ==========================
        // System Preferences
        // ==========================
        public bool AllowNewRegistrations { get; set; }
        public bool RequireEmailVerification { get; set; }
        public bool AutoApproveLibraries { get; set; }
        public bool MaintenanceMode { get; set; }
        public int ItemsPerPage { get; set; }

        // ==========================
        // Payment Settings
        // ==========================
        public string PaymentGateway { get; set; } = string.Empty;
        public string PaystackPublicKey { get; set; } = string.Empty;
        public string PaystackSecretKey { get; set; } = string.Empty;
        public string PaymentSuccessUrl { get; set; } = string.Empty;
        public string PaymentCancelUrl { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;

        // ==========================
        // Security
        // ==========================
        public int SessionTimeout { get; set; }
        public int PasswordMinLength { get; set; }
        public int LoginAttemptsLimit { get; set; }
        public bool EnableTwoFactor { get; set; }
        public int PasswordExpiry { get; set; }

        // ==========================
        // Notifications
        // ==========================
        public bool EmailNotifications { get; set; }
        public bool SubscriptionNotifications { get; set; }
        public bool PaymentNotifications { get; set; }
        public bool MaintenanceNotifications { get; set; }

        // ==========================
        // System
        // ==========================
        public string DashboardView { get; set; } = string.Empty;
        public string TimeFormat { get; set; } = string.Empty;
        public string FirstDayOfWeek { get; set; } = string.Empty;
        public string CacheDriver { get; set; } = string.Empty;
        public int CacheDuration { get; set; }
        public string MaxUploadSize { get; set; } = string.Empty;
        public string AllowedFileTypes { get; set; } = string.Empty;
        public string StorageDisk { get; set; } = string.Empty;
        public int ImageQuality { get; set; }
        public string LogLevel { get; set; } = string.Empty;
        public string LogRetention { get; set; } = string.Empty;
        public bool ActivityLogging { get; set; }

        // ==========================
        // Backup
        // ==========================
        public bool EnableAutomaticBackups { get; set; }
        public string BackupFrequency { get; set; } = string.Empty;
        public string BackupRetention { get; set; } = string.Empty;
        public DateTime LastBackup { get; set; }
        public string BackupStatus { get; set; } = string.Empty;
    }
}
