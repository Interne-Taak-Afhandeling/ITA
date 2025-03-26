﻿using InterneTaakAfhandeling.Web.Server.Features;

namespace InterneTaakAfhandeling.Web.Server.Config
{
    public static class SecurityHeaders
    {
        public static IApplicationBuilder UseItaSecurityHeaders(this WebApplication app)
        {
            var resourcesConfig = app.Services.GetRequiredService<ResourcesConfig>();

            var connectSources = new List<string?> {
                "'self'"
            };

            var styleSources = new List<string?> {
                "'self'",
                resourcesConfig.TokensUrl
            };

            var imgSources = new List<string?> {
                "'self'",
                resourcesConfig.FaviconUrl
            };

            var fontSources = new List<string?> {
                "'self'",
                resourcesConfig.FontSources
            };

            // Add svg logo to connectSources to be able to fetch through js
            var logoUrl = resourcesConfig.LogoUrl;

            if (!string.IsNullOrEmpty(logoUrl))
            {
                if (logoUrl.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                {
                    connectSources.Add(logoUrl);
                }
                else
                {
                    imgSources.Add(logoUrl);
                }
            }

            return app.UseSecurityHeaders(x => x
                .AddDefaultSecurityHeaders()
                .AddCrossOriginOpenerPolicy(x =>
                {
                    x.SameOrigin();
                })
                .AddCrossOriginEmbedderPolicy(x =>
                {
                    x.RequireCorp();
                })
                .AddCrossOriginResourcePolicy(x =>
                {
                    x.SameOrigin();
                })
                .AddContentSecurityPolicy(csp =>
                {
                    csp.AddUpgradeInsecureRequests();
                    csp.AddDefaultSrc().None();
                    csp.AddConnectSrc().From(string.Join(" ", connectSources.Where(src => !string.IsNullOrWhiteSpace(src))));
                    csp.AddScriptSrc().Self();
                    csp.AddStyleSrc().From(string.Join(" ", styleSources.Where(src => !string.IsNullOrWhiteSpace(src))));
                    csp.AddImgSrc().From(string.Join(" ", imgSources.Where(src => !string.IsNullOrWhiteSpace(src))));
                    csp.AddFontSrc().From(string.Join(" ", fontSources.Where(src => !string.IsNullOrWhiteSpace(src))));
                    csp.AddFrameAncestors().None();
                    csp.AddFormAction().Self();
                    csp.AddBaseUri().None();
                })
                .AddPermissionsPolicy(permissions =>
                {
                    permissions.AddAccelerometer().None();
                    permissions.AddAmbientLightSensor().None();
                    permissions.AddAutoplay().None();
                    permissions.AddCamera().None();
                    permissions.AddEncryptedMedia().None();
                    permissions.AddFullscreen().None();
                    permissions.AddGeolocation().None();
                    permissions.AddGyroscope().None();
                    permissions.AddMagnetometer().None();
                    permissions.AddMicrophone().None();
                    permissions.AddMidi().None();
                    permissions.AddPayment().None();
                    permissions.AddPictureInPicture().None();
                    permissions.AddSpeaker().None();
                    permissions.AddSyncXHR().None();
                    permissions.AddUsb().None();
                    permissions.AddVR().None();
                }));
        }
    }
}
