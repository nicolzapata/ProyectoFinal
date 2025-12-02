using System;
using System.Linq;
using GestionUsuarios.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestionUsuarios.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // Crear scope propio para resolver servicios correctamente
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Aplicar migraciones (opcional; útil en desarrollo)
            await context.Database.MigrateAsync();

            // Crear Roles
            string[] roleNames = { "Administrador", "Usuario", "Cliente" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException($"No se pudo crear el rol '{roleName}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

            // Crear o asegurar Usuario Administrador
            var adminEmail = "admin@gmail.com";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NombreCompleto = "Administrador del Sistema",
                    EmailConfirmed = true,
                    Activo = true,
                    FechaRegistro = DateTime.Now
                };

                var createResult = await userManager.CreateAsync(admin, "Admin123!");
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"No se pudo crear el usuario administrador: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            // Asegurar que el admin está en el rol Administrador (si no, añadirlo)
            if (!await userManager.IsInRoleAsync(admin, "Administrador"))
            {
                var addRoleResult = await userManager.AddToRoleAsync(admin, "Administrador");
                if (!addRoleResult.Succeeded)
                {
                    throw new InvalidOperationException($"No se pudo asignar el rol 'Administrador' a {adminEmail}: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                }
            }

            // Asignar permisos a rol Administrador (si hay permisos en la tabla)
            var adminRole = await roleManager.FindByNameAsync("Administrador");
            if (adminRole != null)
            {
                var permisos = context.Permisos.ToList();
                foreach (var permiso in permisos)
                {
                    if (!context.RolPermisos.Any(rp => rp.RolId == adminRole.Id && rp.PermisoId == permiso.PermisoId))
                    {
                        context.RolPermisos.Add(new RolPermiso
                        {
                            RolId = adminRole.Id,
                            PermisoId = permiso.PermisoId
                        });
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }
}