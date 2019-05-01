using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PeopleLookup.Mvc.Models;

namespace PeopleLookup.Mvc.Services
{
    public interface IPermissionService
    {
        bool CanSeeSensitiveInfo();
    }

    public class PermissionService : IPermissionService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly AuthSettings _authSettings;

        public PermissionService(IHttpContextAccessor contextAccessor, IOptions<AuthSettings> authSettings)
        {
            _contextAccessor = contextAccessor;
            _authSettings = authSettings.Value;
        }

        public bool CanSeeSensitiveInfo()
        {
            return _authSettings.AllowSearchStudent.Split(',').Contains(_contextAccessor.HttpContext.User.Identity.Name);
        }
    }
}
