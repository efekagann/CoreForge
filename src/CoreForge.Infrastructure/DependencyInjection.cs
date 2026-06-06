using CoreForge.Application.Common.Interfaces;
using CoreForge.Domain.Common;
using CoreForge.Domain.Interfaces;
using CoreForge.Infrastructure.BackgroundJobs;
using CoreForge.Infrastructure.Email;
using CoreForge.Infrastructure.Identity;
using CoreForge.Infrastructure.Payments;
using CoreForge.Infrastructure.Persistence;
using CoreForge.Infrastructure.Repositories;
using CoreForge.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoreForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["DatabaseProvider"] ?? "PostgreSQL";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                options.UseSqlServer(connectionString);
            else
                options.UseNpgsql(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        // Payment provider — switch between Mock and Stripe via appsettings.json
        var paymentProvider = configuration["PaymentProvider"] ?? "Mock";
        if (paymentProvider.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
        {
            services.Configure<StripeSettings>(configuration.GetSection(StripeSettings.SectionName));
            services.AddScoped<IPaymentService, StripePaymentService>();
        }
        else
        {
            services.AddScoped<IPaymentService, MockPaymentService>();
        }

        services.AddSingleton<IFeatureService, FeatureService>();

        // Email provider
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        var emailProvider = configuration[$"{EmailSettings.SectionName}:Provider"] ?? "Mock";
        if (emailProvider.Equals("MailKit", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<IEmailService, MailKitEmailService>();
        else
            services.AddScoped<IEmailService, MockEmailService>();

        // Storage provider
        services.Configure<StorageSettings>(configuration.GetSection(StorageSettings.SectionName));
        var storageProvider = configuration[$"{StorageSettings.SectionName}:Provider"] ?? "Mock";
        if (storageProvider.Equals("Local", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<IStorageService, LocalStorageService>();
        else
            services.AddScoped<IStorageService, MockStorageService>();

        // Background job queue
        services.AddSingleton<IBackgroundJobQueue, BackgroundJobQueue>();
        services.AddHostedService<BackgroundJobProcessor>();

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }
}
