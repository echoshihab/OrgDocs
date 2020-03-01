using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrgDocs.Data;
using OrgDocs.Models;

namespace OrgDocs.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly OrgDocsContext _context;
        public readonly IWebHostEnvironment _hostEnvironment;

        public DocumentsController(OrgDocsContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Documents
        public async Task<IActionResult> Index()
        {
            var orgDocsContext = _context.Documents.Include(d => d.Category).Include(d => d.Dept);
            return View(await orgDocsContext.ToListAsync());
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
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["DeptId"] = new SelectList(_context.Depts, "Id", "Department");
            return View();
        }

        // POST: Documents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,LastUpdate,CategoryId,DeptId")] Document document)
        {
            if (id != document.Id)
            {
                return NotFound();
            }
            document.LastUpdate = DateTime.Now;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(document);
                    await _context.SaveChangesAsync();
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

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }
    }
}
