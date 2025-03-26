using GymManagement.CustomControllers;
using GymManagement.Data;
using GymManagement.Models;
using GymManagement.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace GymManagement.Controllers
{
    [Authorize(Roles = "Staff, Supervisor, Admin")]
    public class InstructorController : ElephantController
    {
        private readonly GymContext _context;

        public InstructorController(GymContext context)
        {
            _context = context;
        }

        // GET: Instructor
        public async Task<IActionResult> Index(string? SearchString, string? SearchPhone, 
            bool? ActiveStatus, int? page, int? pageSizeID, string? actionButton, 
            string sortDirection = "asc", string sortField = "Instructor")
        {
            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Seniority", "Instructor", "Phone", "Email" };

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied

            //Start with Includes but make sure your expression returns an
            //IQueryable<T> so we can add filter and sort 
            //options later.
            var instructors = _context.Instructors
                .Include(d => d.InstructorDocuments)
                .AsNoTracking();

            if (!String.IsNullOrEmpty(SearchString))
            {
                instructors = instructors.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || p.FirstName.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchPhone))
            {
                instructors = instructors.Where(p => p.Phone.Contains(SearchPhone));
                numberFilters++;
            }
            if (ActiveStatus.HasValue)
            {
                instructors = instructors.Where(p => p.IsActive==ActiveStatus.GetValueOrDefault());
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
                @ViewData["ShowFilter"] = " show";

            }
            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
                page = 1;//Reset page to start

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            //Now we know which field and direction to sort by
            if (sortField == "Seniority")
            {
                if (sortDirection == "asc")
                {
                    instructors = instructors
                        .OrderByDescending(p => p.HireDate)
                        .ThenBy(p => p.LastName)
                        .ThenBy(p => p.FirstName); 
                }
                else
                {
                    instructors = instructors
                        .OrderBy(p => p.HireDate)
                        .ThenBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                }
            }
            else if (sortField == "Phone")
            {
                if (sortDirection == "asc")
                {
                    instructors = instructors
                        .OrderBy(p => p.Phone)
                        .ThenBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                }
                else
                {
                    instructors = instructors
                        .OrderByDescending(p => p.Phone)
                        .ThenBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                }
            }
            else if (sortField == "Email")
            {
                if (sortDirection == "asc")
                {
                    instructors = instructors
                        .OrderBy(p => p.Email)
                        .ThenBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                }
                else
                {
                    instructors = instructors
                        .OrderByDescending(p => p.Email)
                        .ThenBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                }
            }
            else //Sorting by Instructor Name
            {
                if (sortDirection == "asc")
                {
                    instructors = instructors
                        .OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName);
                }
                else
                {
                    instructors = instructors
                        .OrderByDescending(p => p.LastName)
                        .ThenByDescending(p => p.FirstName);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Instructor>.CreateAsync(instructors.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Instructor/Details/5
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .Include(c => c.GroupClasses).ThenInclude(c => c.FitnessCategory)
                .Include(c => c.GroupClasses).ThenInclude(c => c.ClassTime)
                .Include(d => d.InstructorDocuments)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // GET: Instructor/Create
        [Authorize(Roles = "Supervisor, Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Instructor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Create([Bind("FirstName,MiddleName,LastName,HireDate,Phone," +
            "Email,IsActive")] Instructor instructor, List<IFormFile> theFiles)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await AddDocumentsAsync(instructor, theFiles);
                    _context.Add(instructor);
                    await _context.SaveChangesAsync();
                    var returnUrl = ViewData["returnURL"]?.ToString();
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    return Redirect(returnUrl);
                }
            }
            catch (DbUpdateException dex)
            {
                string message = dex.GetBaseException().Message;
                if (message.Contains("UNIQUE") && message.Contains("Email"))
                {
                    ModelState.AddModelError("Email", "Unable to save changes. Remember, " +
                        "you cannot have duplicate Email addresses for Instructors.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            return View(instructor);
        }

        // GET: Instructor/Edit/5
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .Include(d => d.InstructorDocuments)
                .FirstOrDefaultAsync(m => m.ID == id); 
            if (instructor == null)
            {
                return NotFound();
            }
            return View(instructor);
        }

        // POST: Instructor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Edit(int id, List<IFormFile> theFiles)
        {
            //Go get the record to update
            var instructorToUpdate = await _context.Instructors
                .Include(d => d.InstructorDocuments)
                .FirstOrDefaultAsync(p => p.ID == id);

            //Check that you got it or exit with a not found error
            if (instructorToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<Instructor>(instructorToUpdate, "",
                p => p.FirstName, p => p.MiddleName, p => p.LastName, p => p.HireDate,
                p => p.Phone, p => p.Email, p => p.IsActive))
            {
                try
                {
                    await AddDocumentsAsync(instructorToUpdate, theFiles);
                    await _context.SaveChangesAsync();
                    var returnUrl = ViewData["returnURL"]?.ToString();
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    return Redirect(returnUrl);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstructorExists(instructorToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException dex)
                {
                    string message = dex.GetBaseException().Message;
                    if (message.Contains("UNIQUE") && message.Contains("Email"))
                    {
                        ModelState.AddModelError("Email", "Unable to save changes. Remember, " +
                            "you cannot have duplicate Email addresses for Instructors.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }

            }
            return View(instructorToUpdate);
        }

        // GET: Instructor/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .Include(d => d.InstructorDocuments)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // POST: Instructor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instructor = await _context.Instructors
                .Include(d => d.InstructorDocuments)
                .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (instructor != null)
                {
                    _context.Instructors.Remove(instructor);
                }

                await _context.SaveChangesAsync();
                var returnUrl = ViewData["returnURL"]?.ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction(nameof(Index));
                }
                return Redirect(returnUrl);
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Instructor. Remember, you cannot delete a Instructor that teaches Group Classes.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(instructor);

        }

        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFiles
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();

            if (theFile?.FileContent?.Content == null || theFile.MimeType == null)
            {
                return new FileContentResult(Array.Empty<byte>(), "application/octet-stream");
            }

            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }

        private async Task AddDocumentsAsync(Instructor instructor, List<IFormFile> theFiles)
        {
            foreach (var f in theFiles)
            {
                if (f != null)
                {
                    string mimeType = f.ContentType;
                    string fileName = Path.GetFileName(f.FileName);
                    long fileLength = f.Length;
                    if (!(fileName == "" || fileLength == 0))
                    {
                        InstructorDocument d = new InstructorDocument
                        {
                            FileContent = new FileContent()
                        };
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.MimeType = mimeType;
                        d.FileName = fileName;
                        instructor.InstructorDocuments.Add(d);
                    }
                }
            }
        }
        private bool InstructorExists(int id)
        {
            return _context.Instructors.Any(e => e.ID == id);
        }
    }
}
