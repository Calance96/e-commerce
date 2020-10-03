// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections.Generic;
using System.Security.Claims;

namespace ECommerce.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
                   new IdentityResource[]
                   {
                        new IdentityResources.OpenId(),
                        new IdentityResources.Profile()
                   };

        public static IEnumerable<ApiResource> ApiResources =>
                new ApiResource[]
                {
                    new ApiResource("ecommerce_api")
                    {
                        Scopes = { "main_api" }
                    }
                };
            

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
               new ApiScope("main_api"),
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientId = "ecommerce-client",
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RedirectUris =
                    {
                        "https://localhost:44394/signin-oidc"
                    },
                    PostLogoutRedirectUris =
                    {
                        "https://localhost:44394/signout-callback-oidc"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "main_api"
                    },
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    UpdateAccessTokenClaimsOnRefresh = true
                },
                new Client
                {
                    ClientId = "ecommerce-postman",
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    RequirePkce = false,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "main_api"
                    },
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    UpdateAccessTokenClaimsOnRefresh = true
                }
            };
    }
}