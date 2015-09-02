using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using System.IdentityModel.Tokens;
using System.Security.Cryptography;
using Newtonsoft.Json;
using MVC6BearerToken.Utility;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.AspNet.Authentication.OAuthBearer;
using MVC6BearerToken.DAL;
using Microsoft.AspNet.Authorization;
using Microsoft.Data.Entity;
using MVC6BearerToken.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Serialization;

namespace MVC6BearerToken {
    public class Startup {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv) {
            // Setup configuration sources.

            var builder = new ConfigurationBuilder(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if(env.IsDevelopment()) {
                // This reads the configuration keys from the secret store.
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services) {
            services.AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));

            services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc().Configure<MvcOptions>(options => {
                var jsonFormatter = options.OutputFormatters.OfType<JsonOutputFormatter>().First();

                jsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });
            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();

            // Bearer Token Stuff
            #region Bearer Token Setup
            RsaSecurityKey key;
            RSACryptoServiceProvider publicAndPrivate = new RSACryptoServiceProvider();
            var savykeyParams = JsonConvert.DeserializeObject<RSAParametersSerializable>(Configuration["rsa-key"]);
            //This if is necessary because migrations don't find environment variables, and so we have to put a blank savy-rsa-key in the config file.
            //That creates a null object. Thus the if.
            if(savykeyParams != null) {
                publicAndPrivate.ImportParameters(savykeyParams.RSAParameters);

                key = new RsaSecurityKey(publicAndPrivate.ExportParameters(true));

                services.AddInstance(new SigningCredentials(key, SecurityAlgorithms.RsaSha256Signature, SecurityAlgorithms.Sha256Digest));

                services.Configure<OAuthBearerAuthenticationOptions>(bearer => {
                    bearer.TokenValidationParameters.IssuerSigningKey = key;
                    bearer.TokenValidationParameters.ValidAudience = "mybearertokenapi";
                    bearer.TokenValidationParameters.ValidIssuer = "mybearertokenapi";
                });
            }

            #endregion

            services.AddScoped<IAuthRepository, AuthRepository>();

            // Authorization policies
            services.ConfigureAuthorization(auth => {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(OAuthBearerAuthenticationDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser()
                    .Build());
            });
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            // Configure the HTTP request pipeline.
            app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc();
            // Add the following route for porting Web API 2 controllers.
            // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");

            // Use OAuth bearer token
            app.UseOAuthBearerAuthentication();

            // Add cookie-based authentication to the request pipeline.
            app.UseIdentity();

            //TODO: Remove this when seeding is native
            var seeder = ActivatorUtilities.CreateInstance<DbSeeder>(app.ApplicationServices);
            seeder.Seed();
        }
    }
}
