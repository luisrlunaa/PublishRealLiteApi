using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Application.Services.Interfaces;

namespace PublishRealLiteApi.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected readonly ICurrentUserService CurrentUser;

        protected ApiControllerBase(ICurrentUserService currentUser)
        {
            CurrentUser = currentUser;
        }
    }
}
