using GestionUsuarios.Data;
using GestionUsuarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionUsuarios.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RolesController(RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        // GET: Roles/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            // Obtener permisos del rol
            var permisos = await _context.RolPermisos
                .Where(rp => rp.RolId == id)
                .Include(rp => rp.Permiso)
                    .ThenInclude(p => p.Modulo)
                .ToListAsync();

            ViewBag.Permisos = permisos;

            return View(role);
        }

        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("", "El nombre del rol es requerido");
                return View();
            }

            var roleExists = await _roleManager.RoleExistsAsync(name);
            if (roleExists)
            {
                ModelState.AddModelError("", "El rol ya existe");
                return View();
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(name));
            if (result.Succeeded)
            {
                TempData["Mensaje"] = "Rol creado exitosamente";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            return View(role);
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string name)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("", "El nombre del rol es requerido");
                return View(role);
            }

            role.Name = name;
            role.NormalizedName = name.ToUpper();

            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                TempData["Mensaje"] = "Rol actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(role);
        }

        // GET: Roles/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            return View(role);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            // Verificar si hay usuarios con este rol
            var usersInRole = await _context.UserRoles.AnyAsync(ur => ur.RoleId == id);
            if (usersInRole)
            {
                TempData["Error"] = "No se puede eliminar el rol porque tiene usuarios asignados";
                return RedirectToAction(nameof(Index));
            }

            // Eliminar permisos asociados
            var permisos = _context.RolPermisos.Where(rp => rp.RolId == id);
            _context.RolPermisos.RemoveRange(permisos);
            await _context.SaveChangesAsync();

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["Mensaje"] = "Rol eliminado exitosamente";
            }
            else
            {
                TempData["Error"] = "Error al eliminar el rol";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Roles/AsignarPermisos/5
        public async Task<IActionResult> AsignarPermisos(string id)
        {
            if (id == null) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            // Obtener todos los módulos con sus permisos
            var modulos = await _context.Modulos
                .Include(m => m.Permisos)
                .Where(m => m.Activo)
                .OrderBy(m => m.Orden)
                .ToListAsync();

            // Obtener permisos ya asignados al rol
            var permisosAsignados = await _context.RolPermisos
                .Where(rp => rp.RolId == id)
                .Select(rp => rp.PermisoId)
                .ToListAsync();

            ViewBag.Role = role;
            ViewBag.PermisosAsignados = permisosAsignados;

            return View(modulos);
        }

        // POST: Roles/AsignarPermisos/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarPermisos(string id, List<int> permisos)
        {
            if (id == null) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();

            // Eliminar permisos existentes
            var permisosExistentes = _context.RolPermisos.Where(rp => rp.RolId == id);
            _context.RolPermisos.RemoveRange(permisosExistentes);

            // Agregar nuevos permisos
            if (permisos != null && permisos.Any())
            {
                foreach (var permisoId in permisos)
                {
                    _context.RolPermisos.Add(new RolPermiso
                    {
                        RolId = id,
                        PermisoId = permisoId,
                        FechaAsignacion = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Permisos asignados exitosamente";

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}