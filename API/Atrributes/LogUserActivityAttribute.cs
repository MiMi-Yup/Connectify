using API.Core.Contracts;
using API.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Atrributes
{
    public class LogUserActivityAttribute : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            bool? isAuth = resultContext.HttpContext.User.Identity?.IsAuthenticated;
            if (isAuth == null || isAuth == false) return;

            var userId = resultContext.HttpContext.User.GetUserId();
            var uow = resultContext.HttpContext.RequestServices.GetService<IUnitOfWork>();
            //GetService: Microsoft.Extensions.DependencyInjection
            var user = await uow.User.GetByID(userId, true);
            user.LastActive = DateTime.Now;
            await uow.SaveAsync();//add this: services.AddScoped<LogUserActivity>(); [ServiceFilter(typeof(LogUserActivity))] dat truoc controller base
        }
    }
}
