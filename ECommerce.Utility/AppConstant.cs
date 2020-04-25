using System;

namespace ECommerce.Utility
{
    public class AppConstant
    {
        public const string ROLE_ADMIN = "Admin";
        public const string ROLE_CUSTOMER = "Customer";

        public const string CART_SESSION_KEY = "EMallCartSession";
        public const string FULLNAME_SESSION_KEY = "FullName";

        public const string CONTENT_JSON = "application/json";

        public static class PaymentStatus
        {
            public const string PENDING = "Pending";
            public const string APPROVED = "Approved";
            public const string REJECTED = "Rejected";
            public const string REFUNDED = "Refunded";
            public const string CANCELLED = "Cancelled";
        }

        public static class OrderStatus
        {
            public const string PENDING = "Pending";
            public const string APPROVED = "Approved";
            public const string PROCESSING = "Processing";
            public const string SHIPPED = "Shipped";
            public const string CANCELLED = "Cancelled";
            public const string COMPLETE = "Completed";
            public const string REFUNDED = "Refunded";
        }

        public static class StripeChargeStatus
        {
            public const string SUCCEEDED = "succeeded";
            public const string PENDING = "pending";
            public const string FAILED = "failed";
        }
    }
}
