using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.OpenApi;

namespace FarmerOps.Api.OpenApi;

/// <summary>
/// .NET 10 changed how JWT bearer auth is surfaced in generated OpenAPI documents — the old
/// .NET 9 <c>OpenApiReference</c>-based pattern for wiring the "Authorize" button in Scalar no
/// longer compiles against the new Microsoft.OpenApi object model. This transformer adds the
/// Bearer security scheme once and requires it on every operation, which is what makes the
/// Authorize button in <c>/scalar/v1</c> actually attach a token to requests.
/// </summary>
internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.All(scheme => scheme.Name != JwtBearerDefaults.AuthenticationScheme))
            return;

        const string schemeId = "Bearer";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[schemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            In = ParameterLocation.Header,
            BearerFormat = "JWT",
            Description = "Enter the JWT access token returned by POST /auth/login."
        };

        var securityRequirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(schemeId, document)] = []
        };

        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations!.Values))
            operation.Security ??= [securityRequirement];
    }
}
