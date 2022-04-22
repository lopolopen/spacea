using SpaceA.WebApi.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceA.WebApi
{
    public static class AuthenticationBuilderExts
    {
        private readonly static Action<ApikeyOptions> _noop = _ => { };

        public static AuthenticationBuilder AddApikey(this AuthenticationBuilder builder, Action<ApikeyOptions> configure = null)
        {
            return builder.AddScheme<ApikeyOptions, ApikeyAuthenticationHandler>(ApikeyDefaults.AuthenticationScheme, configure ?? _noop);
        }
    }
}
