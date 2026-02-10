using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Warehouse.Web.Operations
{
    public interface ICurrentUser
    {
        Guid UserId { get; }
        string? FullName { get; }   // опционально, но домену не передаем
        long StoreId { get; }
        string? StoreName { get; }   // опционально, но домену не передаем
        IReadOnlyCollection<string> Roles { get; }
    }

    public sealed class HttpContextCurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _ctx;

        public HttpContextCurrentUser(IHttpContextAccessor ctx) => _ctx = ctx;

        public Guid UserId =>
            Guid.Parse(_ctx.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public long StoreId =>
            long.Parse(_ctx.HttpContext!.User.FindFirstValue("StoreId")!);

        public string? FullName =>
            _ctx.HttpContext!.User.FindFirstValue("FullName");

        public string? StoreName => StoreId == 0 ? "Основной" :
            _ctx.HttpContext!.User.FindFirstValue("StoreName");

        public IReadOnlyCollection<string> Roles =>
            _ctx.HttpContext!.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
    }
}