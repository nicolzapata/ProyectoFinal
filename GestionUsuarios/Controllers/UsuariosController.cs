using GestionUsuarios.Data;
using GestionUsuarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionUsuarios.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsuariosController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var usuarios = await _userManager.Users.ToListAsync();
            return View(usuarios);
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(usuario);
            ViewBag.Roles = roles;

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser usuario, string password, string rolSeleccionado)
        {
            if (ModelState.IsValid)
            {
                usuario.UserName = usuario.Email;
                var result = await _userManager.CreateAsync(usuario, password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(rolSeleccionado))
                    {
                        await _userManager.AddToRoleAsync(usuario, rolSeleccionado);
                    }

                    // Registrar auditoría
                    _context.AuditoriaUsuarios.Add(new AuditoriaUsuario
                    {
                        UsuarioId = usuario.Id,
                        Accion = "Crear",
                        Descripcion = $"Usuario {usuario.Email} creado",
                        DireccionIP = HttpContext.Connection.RemoteIpAddress?.ToString()
                    });
                    await _context.SaveChangesAsync();

                    TempData["Mensaje"] = "Usuario creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            ViewBag.Roles = _roleManager.Roles.ToList();
            ViewBag.RolesUsuario = await _userManager.GetRolesAsync(usuario);

            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser usuario, string rolSeleccionado)
        {
            if (id != usuario.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioDb = await _userManager.FindByIdAsync(id);
                    if (usuarioDb == null) return NotFound();

                    usuarioDb.NombreCompleto = usuario.NombreCompleto;
                    usuarioDb.NumeroDocumento = usuario.NumeroDocumento;
                    usuarioDb.PhoneNumber = usuario.PhoneNumber;
                    usuarioDb.Activo = usuario.Activo;
                    usuarioDb.Observaciones = usuario.Observaciones;

                    var result = await _userManager.UpdateAsync(usuarioDb);

                    if (result.Succeeded)
                    {
                        // Actualizar rol
                        var rolesActuales = await _userManager.GetRolesAsync(usuarioDb);
                        await _userManager.RemoveFromRolesAsync(usuarioDb, rolesActuales);

                        if (!string.IsNullOrEmpty(rolSeleccionado))
                        {
                            await _userManager.AddToRoleAsync(usuarioDb, rolSeleccionado);
                        }

                        // Registrar auditoría
                        _context.AuditoriaUsuarios.Add(new AuditoriaUsuario
                        {
                            UsuarioId = usuarioDb.Id,
                            Accion = "Editar",
                            Descripcion = $"Usuario {usuarioDb.Email} actualizado",
                            DireccionIP = HttpContext.Connection.RemoteIpAddress?.ToString()
                        });
                        await _context.SaveChangesAsync();

                        TempData["Mensaje"] = "Usuario actualizado exitosamente";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UsuarioExists(usuario.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.Roles = _roleManager.Roles.ToList();
            ViewBag.RolesUsuario = await _userManager.GetRolesAsync(usuario);
            return View(usuario);
        }

        // POST: Usuarios/CambiarEstado/5
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            usuario.Activo = !usuario.Activo;
            await _userManager.UpdateAsync(usuario);

            // Registrar auditoría
            _context.AuditoriaUsuarios.Add(new AuditoriaUsuario
            {
                UsuarioId = usuario.Id,
                Accion = "Cambiar Estado",
                Descripcion = $"Usuario {usuario.Email} {(usuario.Activo ? "activado" : "desactivado")}",
                DireccionIP = HttpContext.Connection.RemoteIpAddress?.ToString()
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> UsuarioExists(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            return usuario != null;
        }
    }
}