using Microsoft.AspNetCore.Mvc;
using PublishRealLiteApi.Common;

namespace PublishRealLiteApi.Controllers;

public abstract class ApiControllerBase(ICurrentUserService currentUser) : ControllerBase
{
    protected readonly ICurrentUserService CurrentUser = currentUser;
}
