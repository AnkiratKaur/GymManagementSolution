using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GymManagement.Data;
using GymManagement.Models;
using GymManagement.CustomControllers;
using GymManagement.Utilities;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Authorization;

namespace GymManagement.Controllers
{
    [Authorize]
    public class InstructorDocumentController : ElephantController
    {
        private readonly GymContext _context;

        public InstructorDocumentController(GymContext context)
        {
            _context = context;
        }

        // GET: InstructorDocument
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Index(string? SearchString, int? InstructorID, string? SearchFileName, int? page, int? pageSizeID)
        {
            //filter Select List
            ViewData["InstructorID"] = InstructorSelectList(null);

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied

            var instructorDocuments = _context.InstructorDocuments
                .Include(i => i.Instructor)
                .AsNoTracking();

            //Add as many filters as needed
            if (InstructorID.HasValue)
            {
                instructorDocuments = instructorDocuments.Where(p => p.InstructorID == InstructorID);
                numberFilters++;
            }
            if (!System.String.IsNullOrEmpty(SearchFileName))
            {
                instructorDocuments = instructorDocuments.Where(p => p.FileName.ToUpper().Contains(SearchFileName.ToUpper()));
                numberFilters++;
            }
            if (!System.String.IsNullOrEmpty(SearchString))//Seach Description
            {
                instructorDocuments = instructorDocuments.Where(p => p.Description.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
            }
            //Give feedback about the state of the filters
            if (numberFilters != 0)
            {
                //Toggle the Open/Closed state of the collapse depending on if we are filtering
                ViewData["Filtering"] = " btn-danger";
                //Show how many filters have been applied
                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
                //Keep the Bootstrap collapse open
                //@ViewData["ShowFilter"] = " show";
            }
            // Always sort by InstructorDocument Name
            instructorDocuments = instructorDocuments
                        .OrderBy(p => p.FileName);

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<InstructorDocument>.CreateAsync(instructorDocuments, page ?? 1, pageSize);
            return View(pagedData);
        }

        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Download(int id)
        {
            var theFile = await _context.UploadedFiles
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();

            if (theFile?.FileContent?.Content == null || theFile.MimeType == null)
            {
                return NotFound();
            }

            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }



        // GET: InstructorDocument/Edit/5
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructorDocument = await _context.InstructorDocuments
                .Include(d => d.Instructor).FirstOrDefaultAsync(d => d.ID == id);

            if (instructorDocument == null)
            {
                return NotFound();
            }

            return View(instructorDocument);
        }

        // POST: InstructorDocument/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var instructorDocumentToUpdate = await _context.InstructorDocuments
        .Include(d => d.Instructor).FirstOrDefaultAsync(d => d.ID == id);

            //Check that you got it or exit with a not found error
            if (instructorDocumentToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<InstructorDocument>(instructorDocumentToUpdate, "",
                    d => d.FileName, d => d.Description))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstructorDocumentExists(instructorDocumentToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (RetryLimitExceededException /* dex */)//This is a Transaction in the Database!
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. " +
                        "Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }

            }

            return View(instructorDocumentToUpdate);
        }

        // GET: InstructorDocument/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructorDocument = await _context.InstructorDocuments
                .Include(i => i.Instructor)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructorDocument == null)
            {
                return NotFound();
            }

            return View(instructorDocument);
        }

        // POST: InstructorDocument/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.InstructorDocuments == null)
            {
                return Problem("Entity set 'Instructor Documents'  is null.");
            }
            var instructorDocument = await _context.InstructorDocuments
                .Include(f => f.Instructor)
                .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (instructorDocument != null)
                {
                    _context.InstructorDocuments.Remove(instructorDocument);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(instructorDocument);
        }

        private SelectList InstructorSelectList(int? selectedId)
        {
            var qry = _context.Instructors
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName)
                .AsNoTracking();
            return new SelectList(qry, "ID", "FormalName", selectedId);
        }

        private bool InstructorDocumentExists(int id)
        {
            return _context.InstructorDocuments.Any(e => e.ID == id);
        }
    }
}
