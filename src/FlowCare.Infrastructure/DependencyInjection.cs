using FlowCare.Application.Interfaces;
using FlowCare.Domain.Enums;
using FlowCare.Infrastructure.Auth;
using FlowCare.Infrastructure.Data;
using FlowCare.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlowCare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FlowCareDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddAuthentication(BasicAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
                BasicAuthenticationHandler.SchemeName, null);

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", p => p.RequireRole(nameof(UserRole.Admin)))
            .AddPolicy("ManagerOrAdmin", p => p.RequireRole(
                nameof(UserRole.Admin), nameof(UserRole.BranchManager)))
            .AddPolicy("StaffOrAbove", p => p.RequireRole(
                nameof(UserRole.Admin), nameof(UserRole.BranchManager), nameof(UserRole.Staff)))
            .AddPolicy("CustomerOnly", p => p.RequireRole(nameof(UserRole.Customer)));

        services.AddScoped<IBranchAuthorizationService, BranchAuthorizationService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IAppointmentAttachmentService, AppointmentAttachmentService>();
        services.AddScoped<ISlotService, SlotService>();
        services.AddScoped<IStaffManagementService, StaffManagementService>();
        services.AddScoped<ICustomerService, CustomerService>();
        // services.AddScoped<IAuditLogQueryService, AuditLogQueryService>();

        return services;
    }
}
