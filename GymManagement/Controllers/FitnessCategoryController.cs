using GymManagement.CustomControllers;
using GymManagement.Data;
using GymManagement.Models;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace GymManagement.Controllers
{
    [Authorize]
    public class FitnessCategoryController : CognizantController
    {
        private readonly GymContext _context;

        public FitnessCategoryController(GymContext context)
        {
            _context = context;
        }

        // GET: FitnessCategory
        public async Task<IActionResult> Index()
        {
            var fitnessCategories = await _context.FitnessCategories
                .Include(c=>c.ExerciseCategories)
                .AsNoTracking()
                .ToListAsync();
            return View(fitnessCategories);
        }

        // GET: FitnessCategory/Details/5
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fitnessCategory = await _context.FitnessCategories
                .Include(fc => fc.ExerciseCategories).ThenInclude(e => e.Exercise)
                .AsNoTracking()
                .FirstOrDefaultAsync(fc => fc.ID == id);

            if (fitnessCategory == null)
            {
                return NotFound();
            }

            return View(fitnessCategory);
        }

        // GET: FitnessCategory/Create
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: FitnessCategory/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Create([Bind("Category")] FitnessCategory fitnessCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(fitnessCategory);
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
                ExceptionMessageVM msg = new();
                string baseMessage = dex.GetBaseException().Message;

                if (baseMessage.Contains("UNIQUE") && baseMessage.Contains("Category"))
                {
                    msg.ErrProperty = "Category";
                    msg.ErrMessage = "Unable to save changes. Remember, " +
                        "you cannot have duplicate Fitness Category Name.";
                }
                else
                {
                    msg.ErrProperty = string.Empty;
                }

                ModelState.AddModelError(msg.ErrProperty, msg.ErrMessage);
            }
            //Decide if we need to send the Validaiton Errors directly to the client
            if (!ModelState.IsValid && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                //Was an AJAX request so build a message with all validation errors
                string errorMessage = "";
                foreach (var modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errorMessage += error.ErrorMessage + "|";
                    }
                }
                //Note: returning a BadRequest results in HTTP Status code 400
                return BadRequest(errorMessage);
            }

            return View(fitnessCategory);
        }

        // GET: FitnessCategory/Edit/5
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fitnessCategory = await _context.FitnessCategories.FindAsync(id);
            if (fitnessCategory == null)
            {
                return NotFound();
            }
            return View(fitnessCategory);
        }

        // POST: FitnessCategory/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            //Go get the Doctor to update
            var fitnessCategoryToUpdate = await _context.FitnessCategories
                .FirstOrDefaultAsync(p => p.ID == id);

            //Check that you got it or exit with a not found error
            if (fitnessCategoryToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<FitnessCategory>(fitnessCategoryToUpdate, "",
                d => d.Category))
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
                catch (DbUpdateConcurrencyException)
                {
                    if (!FitnessCategoryExists(fitnessCategoryToUpdate.ID))
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
                    ExceptionMessageVM msg = new();
                    string baseMessage = dex.GetBaseException().Message;

                    if (baseMessage.Contains("UNIQUE") && baseMessage.Contains("Category"))
                    {
                        msg.ErrProperty = "Category";
                        msg.ErrMessage = "Unable to save changes. Remember, " +
                            "you cannot have duplicate Fitness Category Name.";
                    }
                    else
                    {
                        msg.ErrProperty = string.Empty;
                    }

                    ModelState.AddModelError(msg.ErrProperty, msg.ErrMessage);
                }
                //Decide if we need to send the Validaiton Errors directly to the client
                if (!ModelState.IsValid && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    //Was an AJAX request so build a message with all validation errors
                    string errorMessage = "";
                    foreach (var modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelState.Errors)
                        {
                            errorMessage += error.ErrorMessage + "|";
                        }
                    }
                    //Note: returning a BadRequest results in HTTP Status code 400
                    return BadRequest(errorMessage);
                }
            }
            return View(fitnessCategoryToUpdate);
        }

        // GET: FitnessCategory/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fitnessCategory = await _context.FitnessCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (fitnessCategory == null)
            {
                return NotFound();
            }

            return View(fitnessCategory);
        }

        // POST: FitnessCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fitnessCategory = await _context.FitnessCategories.FindAsync(id);
            try
            {
                if (fitnessCategory != null)
                {
                    _context.FitnessCategories.Remove(fitnessCategory);
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
                    ModelState.AddModelError("", "Unable to Delete Fitness Category. " +
                        "Remember, you cannot delete a Category if there are Group Classes in it.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(fitnessCategory);

        }

        [Authorize(Roles = "Staff, Supervisor, Admin")]
        [HttpPost]
        public async Task<IActionResult> InsertFromExcel(IFormFile theExcel)
        {
            string feedBack = string.Empty;
            string errorMessages = "";

            if (theExcel != null)
            {
                string mimeType = theExcel.ContentType;
                long fileLength = theExcel.Length;
                if (!(mimeType == "" || fileLength == 0)) // Looks like we have a file!
                {
                    if (mimeType.Contains("excel") || mimeType.Contains("spreadsheet"))
                    {
                        ExcelPackage excel;
                        using (var memoryStream = new MemoryStream())
                        {
                            await theExcel.CopyToAsync(memoryStream);
                            excel = new ExcelPackage(memoryStream);
                        }
                        var workSheet = excel.Workbook.Worksheets[0];
                        var start = workSheet.Dimension.Start;
                        var end = workSheet.Dimension.End;
                        int successCount = 0;
                        int errorCount = 0;
                        int exerciseCount = 0;
                        int fitnessCategoryCount = 0;
                        int exerciseCategoryCount = 0;

                        if (workSheet.Cells[1, 1].Text == "Exercise" && workSheet.Cells[1, 2].Text == "FitnessCategory")
                        {
                            for (int row = start.Row + 1; row <= end.Row; row++)
                            {
                                string exerciseName = workSheet.Cells[row, 1].Text.Trim();
                                string fitnessCategoryName = workSheet.Cells[row, 2].Text.Trim();

                                FitnessCategory fitnessCategory = new FitnessCategory();
                                Exercise exercise = new Exercise();

                                try
                                {

                                    fitnessCategory = _context.FitnessCategories.FirstOrDefault(fc => fc.Category == fitnessCategoryName);
                                    exercise = _context.Exercises.FirstOrDefault(e => e.Name == exerciseName);
                                   
                                    //Adding Fitness Category if it does not exist
                                    if (fitnessCategory == null)
                                    {
                                        fitnessCategory = new FitnessCategory { Category = fitnessCategoryName };
                                        _context.FitnessCategories.Add(fitnessCategory);
                                        _context.SaveChanges();
                                        fitnessCategoryCount++;
                                        successCount++;
                                    }

                                    //Adding Exercise if it does not exist
                                    if (exercise == null)
                                    {
                                        exercise = new Exercise { Name = exerciseName };
                                        _context.Exercises.Add(exercise);
                                        _context.SaveChanges();
                                        exerciseCount++;
                                        successCount++;
                                    }

                                    // Adding ExerciseCategory relationship if it does not exist
                                    if (!_context.ExerciseCategories.Any(ec => ec.ExerciseID == exercise.ID && ec.FitnessCategoryID == fitnessCategory.ID))
                                    {
                                        var exerciseCategory = new ExerciseCategory
                                        {
                                            FitnessCategoryID = fitnessCategory.ID,
                                            ExerciseID = exercise.ID
                                        };

                                        _context.ExerciseCategories.Add(exerciseCategory);
                                        _context.SaveChanges();
                                        exerciseCategoryCount++;
                                        successCount++;
                                    }
                                    else
                                    {
                                        errorMessages += "Error: Exercise '" + exerciseName + "' is already linked to Fitness Category '" + fitnessCategoryName + "'." + "<br />";
                                        errorCount++;
                                    }
                                }
                                catch (DbUpdateException dex)
                                {
                                    errorCount++;
                                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                                    {
                                        feedBack += $"'{fitnessCategoryName}' was rejected as a duplicate.<br />";
                                        feedBack += $"'{exerciseName}' was rejected as a duplicate.<br />";
                                    }
                                    else
                                    {
                                        feedBack += $"'{fitnessCategoryName}' caused a database error.<br />";
                                        feedBack += $"'{exerciseName}' caused a database error.<br />";
                                    }
                                }

                                catch (Exception ex)
                                {
                                    errorCount++;
                                    if (ex.GetBaseException().Message.Contains("correct format"))
                                    {
                                        feedBack += $"'{fitnessCategoryName}' was rejected because it was not in the correct format.<br />";
                                        feedBack += $"'{exerciseName}' was rejected because it was not in the correct format.<br />";
                                    }
                                    else
                                    {
                                        feedBack += $"'{fitnessCategoryName}' caused a general error: {ex.Message}<br />";
                                        feedBack += $"'{exerciseName}' caused a general error: {ex.Message}<br />";
                                    }
                                }

                            }
                            feedBack += "Finished Importing Records:<br />";
                            feedBack += fitnessCategoryCount + " Fitness Category record(s) added successfully.<br />";
                            feedBack += exerciseCount + " Exercise record(s) added successfully.<br />";
                            feedBack += exerciseCategoryCount + " Exercise Category record(s) added successfully.<br />";
                            feedBack += successCount + " record(s) total added to the databased.<br />";
                            feedBack += errorCount + " records(s) rejected.<br />";
                            feedBack += errorMessages;
                        }
                        else
                        {
                            feedBack = "Error: You may have selected the wrong file to upload.<br /> " +
                                "Remember, you must have the heading 'Exercise' in the " +
                                "first cell of the first row and 'FitnessCategory' heading in the second cell of the first row.";
                        }
                    }
                    else
                    {
                        feedBack = "Error: That file is not an Excel spreadsheet.";
                    }
                }
                else
                {
                    feedBack = "Error:  file appears to be empty";
                }
            }
            else
            {
                feedBack = "Error: No file uploaded";
            }

            TempData["Feedback"] = feedBack + "<br /><br />";

            return RedirectToAction(nameof(Index));
        }

        private bool FitnessCategoryExists(int id)
        {
            return _context.FitnessCategories.Any(e => e.ID == id);
        }
    }
}
