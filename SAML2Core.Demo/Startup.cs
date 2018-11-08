﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SamlCore.AspNetCore.Authentication.Saml2.Metadata;

namespace Saml2Authentication
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
            services.AddHttpContextAccessor(); //add for Saml2Core
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddSamlCore(options =>
            {
                // SignOutPath (REQUIRED) - The endpoint for the idp to perform its signout action
                options.SignOutPath = "/signedout";

                // EntityId (REQUIRED) - The Relying Party Identifier e.g. https://my.la.gov.local
                options.ServiceProvider.EntityId = Configuration["AppConfiguration:ServiceProvider:EntityId"];

                // There are two ways to provide FederationMetadata
                // Option 1 - A FederationMetadata.xml already exists for your application
                // options.MetadataAddress = @"FederationMetadata.xml"; //idp metadata file instead of a url address
                // options.CreateMetadataFile = false

                // Option 2 - Have the middleware generate the FederationMetadata.xml file for you
                options.MetadataAddress = Configuration["AppConfiguration:IdentityProvider:MetadataAddress"]; //idp metadata url address  
                options.CreateMetadataFile = true;

                // Optional Properties
                //options.ServiceProvider.CertificateIdentifierType = X509FindType.FindBySerialNumber; // the default is 'X509FindType.FindBySerialNumber'. Change value of 'options.ServiceProvider.SigningCertificateX509TypeValue' if this changes
                //options.ServiceProvider.SigningCertificateX509TypeValue = Configuration["AppConfiguration:ServiceProvider:CertificateSerialNumber"]; //your certifcate serial number (default type which can be chnaged by ) that is in your certficate store

                // Force Authentication (optional) - Is authentication required?
                options.ForceAuthn = true;

                // Service Provider Properties (optional) - These set the appropriate tags in the metadata.xml file
                options.ServiceProvider.ServiceName = "My Test Site";
                options.ServiceProvider.Language = "en-US";
                options.ServiceProvider.OrganizationDisplayName = "Louisiana State Government";
                options.ServiceProvider.OrganizationName = "Louisiana State Government";
                options.ServiceProvider.OrganizationURL = "https://my.test.site.gov";
                options.ServiceProvider.ContactPerson = new ContactType()
                {
                    Company = "Louisiana State Government - OTS",
                    GivenName = "Dina Heidar",
                    EmailAddress = new[] { "dina.heidar@la.gov" },
                    contactType = ContactTypeType.technical,
                    TelephoneNumber = new[] { "+1 234 5678" }
                };

                // Events - Modify events below if you want to log errors, add custom claims, etc.

                //options.Events.OnRemoteFailure = context =>
                //{
                //    return Task.FromResult(0);
                //};              
                //options.Events.OnTicketReceived = context =>
                //{  //TODO: add custom claims here
                //    return Task.FromResult(0);
                //};               
            })
            .AddCookie();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication(); //add for Saml2Core

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}