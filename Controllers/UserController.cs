using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrgDocs.Data;
using OrgDocs.Models;
using OrgDocs.Utility;

namespace OrgDocs.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly OrgDocsContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public UserController(OrgDocsContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: User
        public  IActionResult Index()
        {
            var userList = _context.ApplicationUsers.ToList();
            var userRoles = _context.UserRoles.ToList();
            var roles = _context.Roles.ToList();

            foreach(var user in userList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;

  
                if(user.Role == null)
                {
                    user.Role = "Not Defined";
                }

            }
            return View(userList);
            
        }

        //GET: User/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = _context.UserRoles.ToList();
            var roles = _context.Roles.ToList();
            var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
            user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
            ViewData["UserRoles"] = new SelectList(roles, "Name", "Name", user.Role);

            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Details")]
        public async Task<IActionResult> DetailsPost(string role, string id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.ApplicationUsers
                .FirstOrDefaultAsync(m => m.Id == id);



            if (user == null)
            {
                return NotFound();
            }

            var currentRoleID = _context.UserRoles.FirstOrDefault(u => u.UserId == id).RoleId;
            var currentRole = _context.Roles.FirstOrDefault(r => r.Id == currentRoleID).Name;

            var newRole = _context.Roles.FirstOrDefault(r => r.Name == role).Name;

            if (newRole == null)
            {
                return NotFound();
            }

            await _userManager.RemoveFromRoleAsync(user, currentRole);
            await _userManager.AddToRoleAsync(user, newRole);
           

            return RedirectToAction(nameof(Index));


        }


        private bool UserExists(string id)
        {
            return _context.ApplicationUsers.Any(e => e.Id == id);
        }
    }
}
