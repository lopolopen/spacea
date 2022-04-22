// using System;
// using System.Collections.Generic;
// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using Microsoft.IdentityModel.Tokens;
// using Swashbuckle.AspNetCore.SwaggerGen;
// using Swashbuckle.AspNetCore.Swagger;
// using Microsoft.OpenApi.Models;
// using Microsoft.OpenApi.Any;

// namespace SpaceA.WebApi.Filters
// {
//     public class AuthorizationHeaderParameterFilter : IOperationFilter
//     {
//         public void Apply(OpenApiOperation operation, OperationFilterContext context)
//         {
//             if (operation.Parameters == null)
//                 operation.Parameters = new List<OpenApiParameter>();

//             operation.Parameters.Add(new OpenApiSecurityScheme
//             {
//                 Name = "",
//                 In = ParameterLocation.Header,
//                 Description = "",
//                 Required = false,
//                 Schema = new OpenApiSchema
//                 {
//                     Type = "String",
//                     Default = new OpenApiString("")
//                 }
//             });
//         }
//     }
// }