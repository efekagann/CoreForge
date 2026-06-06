namespace CoreForge.Application.Common.Localization;

public static class ResourceKeys
{
    public static class Validation
    {
        public const string EmailRequired    = "Validation_EmailRequired";
        public const string EmailInvalid     = "Validation_EmailInvalid";
        public const string PasswordRequired = "Validation_PasswordRequired";
        public const string PasswordTooShort = "Validation_PasswordTooShort";
        public const string FieldRequired    = "Validation_FieldRequired";
        public const string FieldTooLong     = "Validation_FieldTooLong";
        public const string SlugInvalid      = "Validation_SlugInvalid";
    }

    public static class Auth
    {
        public const string InvalidCredentials  = "Auth_InvalidCredentials";
        public const string UserNotFound        = "Auth_UserNotFound";
        public const string UserAlreadyExists   = "Auth_UserAlreadyExists";
        public const string InvalidToken        = "Auth_InvalidToken";
        public const string TokenExpired        = "Auth_TokenExpired";
        public const string RoleAssignmentFailed = "Auth_RoleAssignmentFailed";
        public const string AccountDisabled     = "Auth_AccountDisabled";
    }

    public static class Payment
    {
        public const string CheckoutFailed        = "Payment_CheckoutFailed";
        public const string WebhookInvalid        = "Payment_WebhookInvalid";
        public const string SubscriptionNotFound  = "Payment_SubscriptionNotFound";
        public const string PlanNotAvailable      = "Payment_PlanNotAvailable";
    }

    public static class Tenant
    {
        public const string NotFound      = "Tenant_NotFound";
        public const string AlreadyExists = "Tenant_AlreadyExists";
        public const string Deactivated   = "Tenant_Deactivated";
    }

    public static class Errors
    {
        public const string NotFound       = "Error_NotFound";
        public const string Unauthorized   = "Error_Unauthorized";
        public const string Forbidden      = "Error_Forbidden";
        public const string Conflict       = "Error_Conflict";
        public const string BadRequest     = "Error_BadRequest";
        public const string InternalServer = "Error_InternalServer";
    }
}
