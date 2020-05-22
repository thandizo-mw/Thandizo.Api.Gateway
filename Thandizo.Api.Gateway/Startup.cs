using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Thandizo.Api.Gateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerForOcelot(Configuration);

            var authenticationProviderKey = Configuration.GetValue<string>("AuthenticationKey");
   
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(authenticationProviderKey, options =>
            {
                // base-address of your identityserver
                options.Authority = Configuration.GetValue<string>("IdentityServer");

                // name of the API resource
                options.Audience = "api_gateway";
                options.RequireHttpsMetadata = false;
            });

            services.AddOcelot()
                .AddCacheManager(x =>
                {
                    x.WithDictionaryHandle();
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod());
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseSwaggerForOcelotUI(Configuration, opt =>
            {
                opt.PathToSwaggerGenerator = "/swagger/docs";
            });
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseOcelot()
                .Wait();
        }
    }
}
