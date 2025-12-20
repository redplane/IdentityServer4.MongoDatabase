using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Attributes
{
    public class LenientAuthorizationAttribute : Attribute, IFilterMetadata
    {
    }
}