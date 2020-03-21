using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrgDocs.Data;
using OrgDocs.Models;
using OrgDocs.Utility;

namespace OrgDocs.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly OrgDocsContext _context;
        public readonly IWebHostEnvironment _hostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public DocumentsController(OrgDocsContext context, IWebHostEnvironment hostEnvironment, UserManager<IdentityUser> userManager, 
            IEmailSender emailSender)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: Documents
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string catFilter,
            string deptFilter,
            string searchString,
            int? pageNumber)
        {

            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortParam"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["LastUpdateSortParam"] = sortOrder == "LastUpdate" ? "last_update_desc" : "LastUpdate";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["DeptFilter"] = deptFilter;
            ViewData["CatFilter"] = catFilter;
            ViewData["UserID"] = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //get list of Categories
            IQueryable<string> catQuery = from category in _context.Categories orderby category.Name select category.Name;


            //get list of depts
            IQueryable<string> deptQuery = from dept in _context.Depts orderby dept.Department select dept.Department;

            //get documents
            var documents = from document in _context.Documents
                            .Include(d => d.Category)
                            .Include(d => d.Dept)
                            .Include(d=> d.Subscriptions)
                            select document;



            //filter logic

            if (!String.IsNullOrEmpty(searchString))
            {
                documents = documents.Where(document => document.Title.Contains(searchString));

            }

            if (!String.IsNullOrEmpty(catFilter))
            {
                documents = documents.Where(document => document.Category.Name.Contains(catFilter));
            }

            if (!String.IsNullOrEmpty(deptFilter))
            {
                documents = documents.Where(document => document.Dept.Department.Contains(deptFilter));
            }




            //sort logic

            switch (sortOrder)
            {
                case "title_desc":
                    documents = documents.OrderByDescending(d => d.Title);
                    break;
                case "LastUpdate":
                    documents = documents.OrderBy(d => d.LastUpdate);
                    break;
                case "last_update_desc":
                    documents = documents.OrderByDescending(d => d.LastUpdate);
                    break;
                default:
                    documents = documents.OrderBy(d => d.Title);
                    break;
            }



            //pagination logic
            int pageSize = 3;
            var docFiltersVM = new DocFiltersVM
            {
                Categories = new SelectList(await catQuery.ToListAsync(), catFilter),
                Depts = new SelectList(await deptQuery.ToListAsync(), deptFilter),
                Documents = await PaginatedList<Document>.CreateAsync(
                    documents.AsNoTracking(), pageNumber ?? 1, pageSize),

            };

            return View(docFiltersVM);
        }

        // GET: Documents/Details/5

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Category)
                .Include(d => d.Dept)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // GET: Documents/Create
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["DeptId"] = new SelectList(_context.Depts, "Id", "Department");
            return View();
        }

        // POST: Documents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,LastUpdate,CategoryId,DeptId")] Document document)
        {
            document.LastUpdate = DateTime.Now;
            if (ModelState.IsValid)
            {

                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                if (files.Count > 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"documents\uploads");
                    var extension = Path.GetExtension(files[0].FileName);
                    if (extension != ".pdf")  //not processing request if it is not a pdf
                    {
                        return View(document);
                    }

                    using (FileStream fileStreams = new FileStream(Path.Combine(uploads, fileName + extension),
                        FileMode.Create))
                    {
                        files[0].CopyTo(fileStreams);
                    }
                    document.PdfUrl = @"\documents\uploads\" + fileName + extension;
                }


                _context.Add(document);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", document.CategoryId);
            ViewData["DeptId"] = new SelectList(_context.Depts, "Id", "Department", document.DeptId);
            return View(document);
        }

        // GET: Documents/Edit/5
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", document.CategoryId);
            ViewData["DeptId"] = new SelectList(_context.Depts, "Id", "Department", document.DeptId);
            return View(document);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,LastUpdate,CategoryId,DeptId,PdfUrl")] Document document)
        {
            if (id != document.Id)
            {
                return NotFound();
            }

           

            if (ModelState.IsValid)
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                bool uploadedNewFile = false;

                if (files.Count > 0) //a new file has been uploded in edit
                {
                    uploadedNewFile = true;
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"documents\uploads");
                    var extension = Path.GetExtension(files[0].FileName);
                    var pdfPath = Path.Combine(webRootPath, document.PdfUrl.TrimStart('\\'));
                    if (extension != ".pdf")  //not processing request if it is not a pdf
                    {
                        return View(document);
                    }

                    if (System.IO.File.Exists(pdfPath))
                    {
                        System.IO.File.Delete(pdfPath);
                    }

                    using (FileStream fileStreams = new FileStream(Path.Combine(uploads, fileName + extension),
                       FileMode.Create))
                    {
                        files[0].CopyTo(fileStreams);
                    }
                    document.PdfUrl = @"\documents\uploads\" + fileName + extension;
                    document.LastUpdate = DateTime.Now; //last update only updated if new document is uploaded
                }


                

               

              
                try
                {
                    
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                    //send email update post-document update to subscribed users.
                    if (uploadedNewFile)
                    {
                        List<Subscription> listofSubscriptions = _context.Subscriptions.Include(s => s.ApplicationUser).Where(d => d.DocumentID == id).ToList();
                        foreach (Subscription sub in listofSubscriptions)
                        {
                            await _emailSender.SendEmailAsync(sub.ApplicationUser.Email, "OrgDocs: Subscribed document updated!",
                                $"This notification is to inform you that the document titled {document.Title} has been updated");
                        }
                    }


                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }


          

                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", document.CategoryId);
            ViewData["DeptId"] = new SelectList(_context.Depts, "Id", "Department", document.DeptId);
            return View(document);
        }

        // GET: Documents/Delete/5
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Category)
                .Include(d => d.Dept)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: Documents/Delete/5
        [Authorize(Roles = SD.Role_Admin)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string webRootPath = _hostEnvironment.WebRootPath;

            var document = await _context.Documents.FindAsync(id);

            //delete image
            var pdfPath = Path.Combine(webRootPath, document.PdfUrl.TrimStart('\\'));

            if (System.IO.File.Exists(pdfPath))
            {
                System.IO.File.Delete(pdfPath);
            }
            //delete instance
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = SD.Role_Admin + ", "+ SD.Role_Employee)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(int documentID, string userID)
        {

            Subscription subscription = _context.Subscriptions
                .Where(s => s.DocumentID == documentID & s.ApplicationUserID == userID).FirstOrDefault();

            if (subscription == null)
            {
                subscription = new Subscription
                {
                    DocumentID = documentID,
                    ApplicationUserID = userID
                };
                _context.Subscriptions.Add(subscription);
            }
            else
            {
                _context.Subscriptions.Remove(subscription);
            }
               

            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }

        private bool SubscriptionExists(int documentID, string userID)
        {
            return _context.Subscriptions.Any(e => e.DocumentID == documentID & e.ApplicationUserID == userID);
        }
    }
}
