using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GymManagement.Data;
using GymManagement.Models;
using GymManagement.Utilities;
using System.Drawing.Printing;
using GymManagement.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using GymManagement.CustomControllers;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.EntityFrameworkCore.Storage;
using NuGet.Packaging;
using Microsoft.AspNetCore.Authorization;

namespace GymManagement.Controllers
{
    [Authorize(Roles = "Client, Staff, Supervisor, Admin")]
    public class ClientWorkoutController : ElephantController
    {
        private readonly GymContext _context;
        private static readonly string[] DurationItems = ["10", "20", "30", "40", "50", "60", "70", "80", "90", "120"];

        public ClientWorkoutController(GymContext context)
        {
            _context = context;
        }

        // GET: Workout
        public async Task<IActionResult> Index(int? ClientID, int? page, int? pageSizeID, int? InstructorID, 
            string actionButton, string SearchString, string sortDirection = "desc", string sortField = "Workout")
        {
            //Get the URL with the last filter, sort and page parameters from THE CLIENTS Index View
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Client");

            if (User.IsInRole("Client"))
            {
                string clientUser = User?.Identity?.Name;
                ClientID = _context.Clients.Where(c => c.Email == clientUser)
                            .Select(c => c.ID).FirstOrDefault();
            }

            if (!ClientID.HasValue)
            {
                //Go back to the proper return URL for the Clients controller
                return Redirect(ViewData["returnURL"].ToString());
            }

            PopulateDropDownLists();

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Workout", "Instructor" };

            var workouts = _context.Workouts
                .Include(w => w.Client)
                .Include(w => w.Instructor)
                .Include(w => w.WorkoutExercises).ThenInclude(e => e.Exercise)
                .Where(w => w.ClientID == ClientID.GetValueOrDefault())
                .AsNoTracking();

            if (InstructorID.HasValue)
            {
                workouts = workouts.Where(p => p.InstructorID == InstructorID);
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                workouts = workouts.Where(p => p.Notes.ToUpper().Contains(SearchString.ToUpper()));
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
            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted so lets sort!
            {
                page = 1;//Reset back to first page when sorting or filtering

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            //Now we know which field and direction to sort by.
            if (sortField == "Instructor")
            {
                if (sortDirection == "asc")
                {
                    workouts = workouts
                        .OrderBy(w => w.Instructor.LastName)
                        .ThenBy(w => w.Instructor.FirstName);
                }
                else
                {
                    workouts = workouts
                        .OrderByDescending(w => w.Instructor.LastName)
                        .ThenByDescending(w => w.Instructor.FirstName);
                }
            }
            else //Workout Date
            {
                if (sortDirection == "asc")
                {
                    workouts = workouts
                        .OrderBy(p => p.StartTime);
                }
                else
                {
                    workouts = workouts
                        .OrderByDescending(p => p.StartTime);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Now get the MASTER record, the cleint, so it can be displayed at the top of the screen
            Client? client = await _context.Clients
                    .Include(c => c.MembershipType)
                    .Include(c => c.ClientThumbnail)
                    .Include(c => c.Enrollments).ThenInclude(e => e.GroupClass).ThenInclude(g => g.FitnessCategory)
                    .Include(c => c.Enrollments).ThenInclude(e => e.GroupClass).ThenInclude(g => g.ClassTime)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.ID == ClientID.GetValueOrDefault());

            ViewBag.Client = client;



            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Workout>.CreateAsync(workouts.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);

        }

        // GET: Workout/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workout = await _context.Workouts
                .Include(w => w.Client)
                .Include(w => w.Instructor)
                .Include(w => w.WorkoutExercises).ThenInclude(e => e.Exercise)
                .ThenInclude(e => e.ExerciseCategories).ThenInclude(f => f.FitnessCategory)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id); 

            if (workout == null)
            {
                return NotFound();
            }

            return View(workout);
        }

        // GET: Workout/Add
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public IActionResult Add(int? ClientID, string ClientName)
        {
            if (!ClientID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }

            Workout workout = new Workout()
            {
                ClientID = ClientID.GetValueOrDefault(),
                StartTime = DateUtilities.GetNextWeekday(DateTime.Today, 3)
            };
            PopulateDropDownLists();
            PopulateAssignedExerciseData(workout);
            ViewData["ClientName"] = ClientName;
            ViewData["Duration"] = new SelectList(DurationItems, "20");

            return View(workout);
        }

        // POST: Workout/Add
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff, Supervisor, Admin")]
        public async Task<IActionResult> Add([Bind("ID,StartTime,EndTime,Notes,ClientID,InstructorID")] 
            Workout workout, string[] selectedOptions, string ClientName, int? Duration)
        {
            try
            {
                int duration = (Duration == null) ? 30 : Duration.GetValueOrDefault();
                workout.EndTime = workout.StartTime.AddMinutes(duration);

                UpdateWorkoutExercise(selectedOptions, workout);

                if (ModelState.IsValid)
                {
                    WorkoutConflictVM workoutConflict = Overlapping(workout);
                    if (workoutConflict.isConflict)
                    {
                        ModelState.AddModelError("", workoutConflict.Comment);
                    }
                    else
                    {
                        _context.Add(workout);
                        await _context.SaveChangesAsync();
                        return Redirect(ViewData["returnURL"].ToString());
                    }
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            PopulateDropDownLists(workout);
            PopulateAssignedExerciseData(workout);
            ViewData["ClientName"] = ClientName;
            ViewData["Duration"] = new SelectList(DurationItems, Duration);
            return View(workout);
        }

		// GET: Workout/Update/5
		[Authorize(Roles = "Staff, Supervisor, Admin")]
		public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workout = await _context.Workouts
                .Include(w => w.Instructor)
                .Include(w => w.Client)
                .Include(w => w.WorkoutExercises).ThenInclude(e => e.Exercise)
                .FirstOrDefaultAsync(w => w.ID == id);

            if (workout == null)
            {
                return NotFound();
            }

            PopulateDropDownLists(workout);
            PopulateAssignedExerciseData(workout);
            return View(workout);
        }

        // POST: Workout/Update/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		[Authorize(Roles = "Staff, Supervisor, Admin")]
		public async Task<IActionResult> Update(int id, string[] selectedOptions)
        {
            var workoutToUpdate = await _context.Workouts
                .Include(w => w.Instructor)
                .Include(w => w.Client)
                .Include(w => w.WorkoutExercises).ThenInclude(e => e.Exercise)
                .FirstOrDefaultAsync(w => w.ID == id);

            //Check that you got it or exit with a not found error
            if (workoutToUpdate == null)
            {
                return NotFound();
            }

            //Update the Exercises done in the Workout
            UpdateWorkoutExercise(selectedOptions, workoutToUpdate);

            if (await TryUpdateModelAsync<Workout>(workoutToUpdate, "",
                a => a.StartTime, a => a.EndTime, a => a.Notes, 
                a => a.InstructorID))
            {
                try
                {
                    WorkoutConflictVM workoutConflict = Overlapping(workoutToUpdate);
                    if (workoutConflict.isConflict)
                    {
                        ModelState.AddModelError("", workoutConflict.Comment);
                    }
                    else
                    {
                        _context.Update(workoutToUpdate);
                        await _context.SaveChangesAsync();
                        return Redirect(ViewData["returnURL"].ToString());
                    } 
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkoutExists(workoutToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                        "persists see your system administrator.");
                }

            }
            
            PopulateDropDownLists(workoutToUpdate);
            PopulateAssignedExerciseData(workoutToUpdate);
            return View(workoutToUpdate);
        }

        // GET: Workout/Remove/5
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> Remove(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workout = await _context.Workouts
                .Include(w => w.Client)
                .Include(w => w.Instructor)
                .Include(w => w.WorkoutExercises).ThenInclude(e => e.Exercise)
                .ThenInclude(e => e.ExerciseCategories).ThenInclude(f => f.FitnessCategory)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (workout == null)
            {
                return NotFound();
            }

            return View(workout);
        }

        // POST: Workout/Remove/5
        [HttpPost, ActionName("Remove")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Admin")]
        public async Task<IActionResult> RemoveConfirmed(int id)
        {
            var workout = await _context.Workouts
                .Include(w => w.Client)
                .Include(w => w.Instructor)
                .Include(w => w.WorkoutExercises).ThenInclude(e => e.Exercise)
                .ThenInclude(e => e.ExerciseCategories).ThenInclude(f => f.FitnessCategory)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (workout != null)
            {
                try
                {
                    _context.Workouts.Remove(workout);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                        "persists see your system administrator.");
                }
            }
            return View(workout);
        }

        public WorkoutConflictVM Overlapping(Workout newWorkout)
        {
            
            WorkoutConflictVM workoutConflict = new WorkoutConflictVM();

            //Returns on the first conflicting workout one found
            //Just check the same day.  The Gym is closed at night so midnight is not a problem.
            var sameDayWorkouts = _context.Workouts
                .Where(r => r.StartTime.Date == newWorkout.StartTime.Date);
            if(sameDayWorkouts.Count()==0)
            {
                return workoutConflict; // IsConflict is false by default
            }
            
            foreach (var workout in sameDayWorkouts)
            {
                if(workout.ID!=newWorkout.ID)//Don't compare it to itself!
                {
                    //Check if it Overlaps
                    if (newWorkout.StartTime < workout.EndTime && newWorkout.EndTime > workout.StartTime)
                    {
                        if (workout.ClientID == newWorkout.ClientID)//Conflict for Client!
                        {
                            workoutConflict.isConflict = true;
                            workoutConflict.Comment = " for the client.";
                        }
                        if (newWorkout.InstructorID != null)//No conflict if no instructor assigned.
                        {
                            if (workout.InstructorID == newWorkout.InstructorID)//Conflict for the Instructor
                            {
                                if (workoutConflict.isConflict == true)//also a conflict for client
                                {
                                    workoutConflict.Comment = " for both the client and instructor.";
                                }
                                else
                                {
                                    workoutConflict.Comment = " for the instructor.";
                                }
                                workoutConflict.isConflict = true;//In case it was not a conflict fo the client.
                            }
                        }
                        if (workoutConflict.isConflict)
                        {
                            workoutConflict.Comment = "Unable to save changes. This workout " +
                                "overlaps with an existing " + workout.DurationSummary +
                                " workout scheduled to start at " + workout.StartDateSummary
                                + " " + workout.StartTimeSummary + " - This is a conflict" + workoutConflict.Comment;
                            //Return the conflict information
                            return workoutConflict;
                        }
                    }
                }
            }
            //Return the conflict information
            return workoutConflict;
        }

        private SelectList InstructorSelectList(int? id)
        {
            var dQuery = from d in _context.Instructors
                         orderby d.LastName, d.FirstName
                         select d;
            return new SelectList(dQuery, "ID", "FormalName", id);
        }
        private void PopulateDropDownLists(Workout? workout = null)
        {
            ViewData["InstructorID"] = InstructorSelectList(workout?.InstructorID);
        }

        private void PopulateAssignedExerciseData(Workout workout)
        {
            //For this to work, you must have Included the child collection in the parent object
            var allOptions = _context.Exercises
                .Include(e => e.ExerciseCategories)
                .ThenInclude(e => e.FitnessCategory);
            var currentOptionsHS = new HashSet<int>(workout.WorkoutExercises.Select(b => b.ExerciseID));
            //Instead of one list with a boolean, we will make two lists
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            foreach (var s in allOptions)
            {
                if (currentOptionsHS.Contains(s.ID))
                {
                    selected.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.Summary
                    });
                }
                else
                {
                    available.Add(new ListOptionVM
                    {
                        ID = s.ID,
                        DisplayText = s.Summary
                    });
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }
        private void UpdateWorkoutExercise(string[] selectedOptions, Workout workoutToUpdate)
        {
            if (selectedOptions == null)
            {
                workoutToUpdate.WorkoutExercises = new List<WorkoutExercise>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var currentOptionsHS = new HashSet<int>(workoutToUpdate.WorkoutExercises.Select(b => b.ExerciseID));
            foreach (var s in _context.Exercises)
            {
                if (selectedOptionsHS.Contains(s.ID.ToString()))//it is selected
                {
                    if (!currentOptionsHS.Contains(s.ID))//but not currently in the Workout's collection - Add it!
                    {
                        workoutToUpdate.WorkoutExercises.Add(new WorkoutExercise
                        {
                            ExerciseID = s.ID,
                            WorkoutID = workoutToUpdate.ID
                        });
                    }
                }
                else //not selected
                {
                    if (currentOptionsHS.Contains(s.ID))//but is currently in the Workout's collection - Remove it!
                    {
                        WorkoutExercise? specToRemove = workoutToUpdate.WorkoutExercises
                            .FirstOrDefault(d => d.ExerciseID == s.ID);
                        if (specToRemove != null)
                        {
                            _context.Remove(specToRemove);
                        }
                    }
                }
            }
        }
        private bool WorkoutExists(int id)
        {
            return _context.Workouts.Any(e => e.ID == id);
        }
    }
}
