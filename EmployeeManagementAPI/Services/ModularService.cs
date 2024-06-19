namespace EmployeeManagementAPI.Services
{
    public static class ModularService
    {
        public static IServiceCollection AddFeatures(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddJsonServices();
            services.AddCorsService(builder);
            services.AddCustomServices();
            return services;
        }

        private static IServiceCollection AddJsonServices(this IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            return services;
        }

        private static IServiceCollection AddCorsService(this IServiceCollection services, WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost3000",
                    builder => builder.WithOrigins("http://localhost:3000")
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });
            return services;
        }

        private static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            services.AddScoped<AdoDotNetServices>();
            return services;
        }
    }
}
