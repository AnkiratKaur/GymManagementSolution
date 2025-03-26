using GymManagement.CustomControllers;
using GymManagement.Data;
using GymManagement.Models;
using GymManagement.Utilities;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;
using System.Numerics;

namespace GymManagement.Controllers
{
    [Authorize]
    public class GroupClassController : ElephantController
    {
        private readonly GymContext _context;

        public GroupClassController(GymContext context)
        {
            _context = context;
        }

        // GET: GroupClass
        public async Task<IActionResult> Index(string? SearchString, int? ClassTimeID, string? DOWFilter,
            int? FitnessCategoryID, int? InstructorID, int? page, int? pageSizeID,
            string? actionButton, string sortDirection = "asc", string sortField = "Class")
        {
            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Class", "Instructor" };

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied

            PopulateDropDownLists();
            //SelectList for the DOW Enum
            if (Enum.TryParse(DOWFilter, out DOW selectedDOW))
            {
                ViewBag.DOWSelectList = DOW.Monday.ToSelectList(selectedDOW);
            }
            else
            {
                ViewBag.DOWSelectList = DOW.Monday.ToSelectList(null);
            }

            //Start with Includes but make sure your expression returns an
            //IQueryable<T> so we can add filter and sort 
            //options later.
            PopulateDropDownLists();
            var groupClasses = _context.GroupClasses
                .Include(g => g.ClassTime)
                .Include(g => g.FitnessCategory)
                .Include(g => g.Instructor)
                .Include(g=>g.Enrollments).ThenInclude(e=>e.Client)
                .AsNoTracking();

            //Add as many filters as needed
            if (ClassTimeID.HasValue)
            {
                groupClasses = groupClasses.Where(p => p.ClassTimeID == ClassTimeID);
                numberFilters++;
            }
            if (FitnessCategoryID.HasValue)
            {
                groupClasses = groupClasses.Where(p => p.FitnessCategoryID == FitnessCategoryID);
                numberFilters++;
            }
            if (InstructorID.HasValue)
            {
                groupClasses = groupClasses.Where(p => p.InstructorID == InstructorID);
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                groupClasses = groupClasses.Where(p => p.Description.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(DOWFilter))
            {
                groupClasses = groupClasses.Where(p => p.DOW == selectedDOW);
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
            if (sortField == "Instructor")
            {
                if (sortDirection == "asc")
                {
                    groupClasses = groupClasses
                        .OrderBy(p => p.Instructor.LastName)
                        .ThenBy(p => p.Instructor.FirstName);
                }
                else
                {
                    groupClasses = groupClasses
                        .OrderByDescending(p => p.Instructor.LastName)
                        .ThenByDescending(p => p.Instructor.FirstName);
                }
            }
            else //Sorting by Class
            {
                if (sortDirection == "asc")
                {
                    groupClasses = groupClasses
                        .OrderBy(p => p.FitnessCategory.Category)
                        .ThenBy(p => p.DOW)
                        .ThenBy(p=>p.ClassTimeID);
                }
                else
                {
                    groupClasses = groupClasses
                        .OrderByDescending(p => p.FitnessCategory.Category)
                        .ThenBy(p => p.DOW)
                        .ThenBy(p => p.ClassTimeID);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<GroupClass>.CreateAsync(groupClasses.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: GroupClass/Details/5
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupClass = await _context.GroupClasses
                .Include(g => g.ClassTime)
                .Include(g => g.FitnessCategory)
                .Include(g => g.Instructor)
                .Include(g => g.Enrollments).ThenInclude(e => e.Client)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (groupClass == null)
            {
                return NotFound();
            }

            return View(groupClass);
        }

        // GET: GroupClass/Create
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public IActionResult Create()
        {
            GroupClass groupClass = new GroupClass();
            PopulateAssignedEnrollmentData(groupClass);
            PopulateDropDownLists(null, true);
            return View();
        }

        // POST: GroupClass/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Create([Bind("Description,DOW,FitnessCategoryID," +
            "InstructorID,ClassTimeID")] GroupClass groupClass, string[] selectedOptions)
        {
            try
            {
                UpdateEnrollments(selectedOptions, groupClass);
                if (ModelState.IsValid)
                {
                    _context.Add(groupClass);
                    await _context.SaveChangesAsync();
                    var returnUrl = ViewData["returnURL"]?.ToString();
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    return Redirect(returnUrl);
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException dex)
            {
                string message = dex.GetBaseException().Message;
                if (message.Contains("UNIQUE") && message.Contains("GroupClasses.InstructorID"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Remember, " +
                        "an Instructor cannot teach two classes at the same time.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            //Validation Error so give the user another chance.
            PopulateAssignedEnrollmentData(groupClass);
            PopulateDropDownLists(groupClass, true);
            return View(groupClass);
        }

        // GET: GroupClass/Edit/5
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupClass = await _context.GroupClasses
                .Include(g => g.Enrollments).ThenInclude(e => e.Client)
                .FirstOrDefaultAsync(m => m.ID == id); 
            if (groupClass == null)
            {
                return NotFound();
            }

            PopulateAssignedEnrollmentData(groupClass);
            PopulateDropDownLists(groupClass);
            return View(groupClass);
        }

        // POST: GroupClass/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int id, string[] selectedOptions, Byte[] RowVersion)
        {
            var groupClassToUpdate = await _context.GroupClasses
                .Include(g => g.Enrollments).ThenInclude(e => e.Client)
                .FirstOrDefaultAsync(p => p.ID == id);

            //Check that you got it or exit with a not found error
            if (groupClassToUpdate == null)
            {
                return NotFound();
            }

            //Update the class enrollments
            UpdateEnrollments(selectedOptions, groupClassToUpdate);

            //Put the original RowVersion value in the OriginalValues collection for the entity
            _context.Entry(groupClassToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<GroupClass>(groupClassToUpdate, "",
                p => p.Description, p => p.DOW, p => p.InstructorID, p => p.FitnessCategoryID,
                p => p.ClassTimeID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    var returnUrl = ViewData["returnURL"]?.ToString();
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    return Redirect(returnUrl);
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException ex)// Added for concurrency
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (GroupClass)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("",
                            "Unable to save changes. The Group Class was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (GroupClass)databaseEntry.ToObject();
                        if (databaseValues.Description != clientValues.Description)
                            ModelState.AddModelError("Description", "Current value: "
                                + databaseValues.Description);
                        if (databaseValues.DOW != clientValues.DOW)
                            ModelState.AddModelError("DOW", "Current value: "
                                + databaseValues.DOW);
                        //For the foreign keys, we need to go to the database to get the information to show
                        if (databaseValues.FitnessCategoryID != clientValues.FitnessCategoryID)
                        {
                            FitnessCategory? databaseFitnessCategory = await _context.FitnessCategories
                                .FirstOrDefaultAsync(i => i.ID == databaseValues.FitnessCategoryID);
                            ModelState.AddModelError("FitnessCategoryID", $"Current value: {databaseFitnessCategory?.Category}");
                        }
                        if (databaseValues.InstructorID != clientValues.InstructorID)
                        {
                            Instructor? databaseInstructor = await _context.Instructors
                                .FirstOrDefaultAsync(i => i.ID == databaseValues.InstructorID);
                            ModelState.AddModelError("InstructorID", $"Current value: {databaseInstructor?.Summary}");
                        }
                        if (databaseValues.ClassTimeID != clientValues.ClassTimeID)
                        {
                            ClassTime? databaseClassTime = await _context.ClassTimes
                                .FirstOrDefaultAsync(i => i.ID == databaseValues.ClassTimeID);
                            ModelState.AddModelError("ClassTimeID", $"Current value: {databaseClassTime?.StartTime}");
                        }
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                + "was modified by another user after you received your values. The "
                                + "edit operation was canceled and the current values in the database "
                                + "have been displayed. If you still want to save your version of this record, click "
                                + "the Save button again. Otherwise click the 'Back to the "
                                + ViewData["ControllerFriendlyName"] + " List' hyperlink.");

                        //Final steps before redisplaying: Update RowVersion from the Database
                        //and remove the RowVersion error from the ModelState
                        groupClassToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
                        ModelState.Remove("RowVersion");
                    }
                }
                catch (DbUpdateException dex)
                {
                    string message = dex.GetBaseException().Message;
                    if (message.Contains("UNIQUE") && message.Contains("GroupClasses.InstructorID"))
                    {
                        ModelState.AddModelError("", "Unable to save changes. Remember, " +
                            "an Instructor cannot teach two classes at the same time.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            PopulateAssignedEnrollmentData(groupClassToUpdate);
            PopulateDropDownLists(groupClassToUpdate);
            return View(groupClassToUpdate);
        }

        // GET: GroupClass/Delete/5
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupClass = await _context.GroupClasses
                .Include(g => g.ClassTime)
                .Include(g => g.FitnessCategory)
                .Include(g => g.Instructor)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (groupClass == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Supervisor"))
            {
                if (groupClass?.CreatedBy != User.Identity?.Name)
                {
                    ModelState.AddModelError("", "You cannot Delete a Group Class you did not enter into the system.");
                    ViewData["NoSubmit"] = "disabled=disabled";
                }
            }

            return View(groupClass);
        }

        // POST: GroupClass/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var groupClass = await _context.GroupClasses
                .Include(g => g.ClassTime)
                .Include(g => g.FitnessCategory)
                .Include(g => g.Instructor)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (User.IsInRole("Supervisor"))
            {
                if (groupClass?.CreatedBy != User.Identity?.Name)
                {
                    ModelState.AddModelError("", "You cannot Delete a Group Class you did not enter into the system.");
                    ViewData["NoSubmit"] = "disabled=disabled";
                }
            }

            try
            {
                if (groupClass != null)
                {
                    _context.GroupClasses.Remove(groupClass);
                }

                await _context.SaveChangesAsync();
                var returnUrl = ViewData["returnURL"]?.ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction(nameof(Index));
                }
                return Redirect(returnUrl);
            }
            catch (DbUpdateException)
            {
                //Note: there is really no reason a delete should fail if you can "talk" to the database.
                ModelState.AddModelError("", "Unable to delete record. Try again, and if the problem persists see your system administrator.");
            }
            return View(groupClass);

        }

        //This is a twist on the PopulateDropDownLists approach
        //  Create methods that return each SelectList separately
        //  and one method to put them all into ViewData.
        //This approach allows for AJAX requests to refresh
        //DDL Data at a later date.
        private SelectList InstructorSelectList(int? selectedId, bool OnlyActive = false)
        {
            //Start with the initial LINQ query
            var qry = _context.Instructors
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName)
                .AsNoTracking();
            //Decide if you want to add the additional restriction (Where Clause)
            if (OnlyActive)
            {
                qry = qry.Where(i => i.IsActive == true);
            }
            return new SelectList(qry, "ID", "FormalName", selectedId);
        }

        [HttpGet]
        public JsonResult GetFitnessCategories(int? id)
        {
            return Json(FitnessCategoryList(id));
        }

        private SelectList FitnessCategoryList(int? selectedId)
        {
            return new SelectList(_context
                .FitnessCategories
                .OrderBy(m => m.Category), "ID", "Category", selectedId);
        }
        private SelectList ClassTimeList(int? selectedId)
        {
            return new SelectList(_context
                .ClassTimes
                .OrderBy(m => m.ID), "ID", "StartTime", selectedId);
        }
        private void PopulateDropDownLists(GroupClass? groupClass = null, bool OnlyActive = false)
        {
            ViewData["InstructorID"] = InstructorSelectList(groupClass?.InstructorID, OnlyActive);
            ViewData["FitnessCategoryID"] = FitnessCategoryList(groupClass?.FitnessCategoryID);
            ViewData["ClassTimeID"] = ClassTimeList(groupClass?.ClassTimeID);
        }

        private void PopulateAssignedEnrollmentData(GroupClass groupClass)
        {
            //For this to work, you must have Included the child collection in the parent object
            var allOptions = _context.Clients;
            var currentOptionsHS = new HashSet<int>(groupClass.Enrollments.Select(b => b.ClientID));
            //Instead of one list with a boolean, we will make two lists
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            foreach (var c in allOptions)
            {
                if (currentOptionsHS.Contains(c.ID))
                {
                    selected.Add(new ListOptionVM
                    {
                        ID = c.ID,
                        DisplayText = c.FormalName
                    });
                }
                else
                {
                    available.Add(new ListOptionVM
                    {
                        ID = c.ID,
                        DisplayText = c.FormalName
                    });
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }
        private void UpdateEnrollments(string[] selectedOptions, GroupClass groupClassToUpdate)
        {
            if (selectedOptions == null)
            {
                groupClassToUpdate.Enrollments = new List<Enrollment>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var currentOptionsHS = new HashSet<int>(groupClassToUpdate.Enrollments.Select(b => b.ClientID));
            foreach (var c in _context.Clients)
            {
                if (selectedOptionsHS.Contains(c.ID.ToString()))//it is selected
                {
                    if (!currentOptionsHS.Contains(c.ID))//but not currently in the GroupClass's collection - Add it!
                    {
                        groupClassToUpdate.Enrollments.Add(new Enrollment
                        {
                            ClientID = c.ID,
                            GroupClassID = groupClassToUpdate.ID
                        });
                    }
                }
                else //not selected
                {
                    if (currentOptionsHS.Contains(c.ID))//but is currently in the GroupClass's collection - Remove it!
                    {
                        Enrollment? enrollmentToRemove = groupClassToUpdate.Enrollments
                            .FirstOrDefault(d => d.ClientID == c.ID);
                        if (enrollmentToRemove != null)
                        {
                            _context.Remove(enrollmentToRemove);
                        }
                    }
                }
            }
        }

        private bool GroupClassExists(int id)
        {
            return _context.GroupClasses.Any(e => e.ID == id);
        }
    }
}
