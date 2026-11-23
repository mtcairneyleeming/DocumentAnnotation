using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DocumentAnnotation.Extensions;
using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace DocumentAnnotation.Authorization
{
    /// <summary>The operations a user can be authorised to perform on a <see cref="Document"/>.</summary>
    public static class DocumentOperations
    {
        public static readonly OperationAuthorizationRequirement View = new OperationAuthorizationRequirement {Name = nameof(View)};
        public static readonly OperationAuthorizationRequirement Edit = new OperationAuthorizationRequirement {Name = nameof(Edit)};
    }

    /// <summary>
    /// The single place that decides who may see or change a document (and so its annotations and highlights); used by
    /// the Annotate pages and every /api controller.
    ///   View: Public documents (anyone, including anonymous users); otherwise the owner, and for Limited documents the
    ///         users whose email is in AllowedUsers ("shared with you").
    ///   Edit: the owner only.
    /// </summary>
    public class DocumentAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Document>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement, Document document)
        {
            var userId = context.User.GetUserId();
            var isAuthenticated = context.User.Identity?.IsAuthenticated == true && userId != null;
            var isOwner = isAuthenticated && document.UserId == userId;

            bool allowed;
            if (requirement == DocumentOperations.Edit)
            {
                allowed = isOwner;
            }
            else if (requirement == DocumentOperations.View)
            {
                allowed = document.Visibility == DocumentVisibility.Public || isOwner ||
                          (isAuthenticated && document.Visibility == DocumentVisibility.Limited &&
                           IsInAllowedUsers(document, context.User.FindFirst(ClaimTypes.Email)?.Value));
            }
            else
            {
                allowed = false;
            }

            if (allowed)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private static bool IsInAllowedUsers(Document document, string email)
        {
            return !string.IsNullOrEmpty(email) && document.AllowedUsers != null &&
                   document.AllowedUsers.Any(a => string.Equals(a, email, StringComparison.OrdinalIgnoreCase));
        }
    }

    public enum DocumentAccess
    {
        Allowed,
        /// <summary>The caller is anonymous and must log in (401 / redirect to login).</summary>
        Unauthenticated,
        /// <summary>Authenticated, can see the document but not do this to it (403).</summary>
        Forbidden,
        /// <summary>Document missing, or the caller may not even see it: indistinguishable on purpose (404).</summary>
        NotFound
    }

    public static class DocumentAuthorizationExtensions
    {
        /// <param name="document">May be null (document does not exist).</param>
        public static async Task<DocumentAccess> CheckDocumentAsync(this IAuthorizationService authorization,
            ClaimsPrincipal user, Document document, OperationAuthorizationRequirement operation)
        {
            if (document != null && (await authorization.AuthorizeAsync(user, document, operation)).Succeeded)
            {
                return DocumentAccess.Allowed;
            }

            // Not allowed (or no such document). Anonymous callers are asked to log in whatever the reason, so the API
            // does not reveal which document ids exist.
            if (user.Identity?.IsAuthenticated != true)
            {
                return DocumentAccess.Unauthenticated;
            }

            if (document != null && operation != DocumentOperations.View &&
                (await authorization.AuthorizeAsync(user, document, DocumentOperations.View)).Succeeded)
            {
                return DocumentAccess.Forbidden;
            }

            return DocumentAccess.NotFound;
        }

        /// <summary>The API response for a denied request, or null if access is allowed.</summary>
        public static IActionResult ToApiResult(this DocumentAccess access)
        {
            switch (access)
            {
                case DocumentAccess.Allowed: return null;
                case DocumentAccess.Unauthenticated: return new StatusCodeResult(StatusCodes.Status401Unauthorized);
                case DocumentAccess.Forbidden: return new StatusCodeResult(StatusCodes.Status403Forbidden);
                default: return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
        }
    }
}
