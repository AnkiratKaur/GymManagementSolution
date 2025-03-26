using GymManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace GymManagement.Data
{
    public static class GymInitializer
    {
        /// <summary>
        /// Prepares the Database and seeds data as required
        /// </summary>
        /// <param name="serviceProvider">DI Container</param>
        /// <param name="DeleteDatabase">Delete the database and start from scratch</param>
        /// <param name="UseMigrations">Use Migrations or EnsureCreated</param>
        /// <param name="SeedSampleData">Add optional sample data</param>
        public static void Initialize(IServiceProvider serviceProvider,
            bool DeleteDatabase = false, bool UseMigrations = true, bool SeedSampleData = true)
        {
            using (var context = new GymContext(
                serviceProvider.GetRequiredService<DbContextOptions<GymContext>>()))
            {
                //Refresh the database as per the parameter options
                #region Prepare the Database
                try
                {
                    //Note: .CanConnect() will return false if the database is not there!
                    if (DeleteDatabase || !context.Database.CanConnect())
                    {
                        context.Database.EnsureDeleted(); //Delete the existing version 
                        if (UseMigrations)
                        {
                            context.Database.Migrate(); //Create the Database and apply all migrations
                        }
                        else
                        {
                            context.Database.EnsureCreated(); //Create and update the database as per the Model
                        }
                        //Now create any additional database objects such as Triggers or Views
                        //--------------------------------------------------------------------
                        //Create the Triggers for Client
                        string sqlCmd = @"
                            CREATE TRIGGER SetClientTimestampOnUpdate
                            AFTER UPDATE ON Clients
                            BEGIN
                                UPDATE Clients
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
                        context.Database.ExecuteSqlRaw(sqlCmd);

                        sqlCmd = @"
                            CREATE TRIGGER SetClientTimestampOnInsert
                            AFTER INSERT ON Clients
                            BEGIN
                                UPDATE Clients
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END
                        ";
                        context.Database.ExecuteSqlRaw(sqlCmd);

                        //Triggers for GroupClass
                        sqlCmd = @"
                            CREATE TRIGGER SetGroupClassTimestampOnUpdate
                            AFTER UPDATE ON GroupClasses
                            BEGIN
                                UPDATE GroupClasses
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END;
                        ";
                        context.Database.ExecuteSqlRaw(sqlCmd);

                        sqlCmd = @"
                            CREATE TRIGGER SetGroupClassTimestampOnInsert
                            AFTER INSERT ON GroupClasses
                            BEGIN
                                UPDATE GroupClasses
                                SET RowVersion = randomblob(8)
                                WHERE rowid = NEW.rowid;
                            END
                        ";
                        context.Database.ExecuteSqlRaw(sqlCmd);
                    }
                    else //The database is already created
                    {
                        if (UseMigrations)
                        {
                            context.Database.Migrate(); //Apply all migrations
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.GetBaseException().Message);
                }
                #endregion

                //Seed data needed for production and during development
                #region Seed Required Data
                try
                {
                    //Add some Class Start times
                    if (!context.ClassTimes.Any())
                    {
                        context.ClassTimes.AddRange(
                         new ClassTime
                         {
                             ID = 10,
                             StartTime = "10:00 AM"
                         }, new ClassTime
                         {
                             ID = 14,
                             StartTime = "2:00 PM"
                         }, new ClassTime
                         {
                             ID = 19,
                             StartTime = "7:00 PM"
                         });
                        context.SaveChanges();
                    }

                    //Add some Membership types
                    if (!context.MembershipTypes.Any())
                    {
                        context.MembershipTypes.AddRange(
                         new MembershipType
                         {
                             Type = "Basic",
                             StandardFee = 100d
                         }, new MembershipType
                         {
                             Type = "Premium",
                             StandardFee = 175d
                         }, new MembershipType
                         {
                             Type = "VIP",
                             StandardFee = 300d
                         });
                        context.SaveChanges();
                    }

                    //Add some Fitness Categories
                    if (!context.FitnessCategories.Any())
                    {
                        context.FitnessCategories.AddRange(
                         new FitnessCategory
                         {
                             Category = "Personal Training"
                         }, new FitnessCategory
                         {
                             Category = "Cardio"
                         }, new FitnessCategory
                         {
                             Category = "Yoga"
                         }, new FitnessCategory
                         {
                             Category = "Strength Training"
                         }, new FitnessCategory
                         {
                             Category = "High Intensity Interval Training"
                         }, new FitnessCategory
                         {
                             Category = "Swimming"
                         });
                        context.SaveChanges();
                    }

                    //Add some Exercises
                    if (!context.Exercises.Any())
                    {
                        //Personal Training
                        int fitnessCategoryID = context.FitnessCategories.FirstOrDefault(d => d.Category == "Personal Training").ID;
                        string[] PT = new string[] { "squats", "deadlifts", "lunges", "bench - press", "bicep curls", "push - ups", "pull - ups", "chest press", "calf raises", "leg raises", "shoulder press", "tricep dips" };
                        foreach (string exercise in PT)
                        {
                            Exercise e = new Exercise
                            {
                                Name = exercise,
                            };
                            context.Exercises.Add(e);
                            context.SaveChanges();
                            //Since it has been saved, the exercise ID will br the new Identity value generated by the database
                            ExerciseCategory c = new ExerciseCategory
                            {
                                ExerciseID = e.ID,
                                FitnessCategoryID = fitnessCategoryID

                            };
                            context.ExerciseCategories.Add(c);

                        }

                        //Cardio
                        fitnessCategoryID = context.FitnessCategories.FirstOrDefault(d => d.Category == "Cardio").ID;
                        string[] Cardio = new string[] { "running", "cycling", "elliptical training", "circuit training", "kickboxing", "aerobics", "dancing" };
                        foreach (string exercise in Cardio)
                        {
                            Exercise e = new Exercise
                            {
                                Name = exercise,
                            };
                            context.Exercises.Add(e);
                            context.SaveChanges();
                            ExerciseCategory c = new ExerciseCategory
                            {
                                ExerciseID = e.ID,
                                FitnessCategoryID = fitnessCategoryID

                            };
                            context.ExerciseCategories.Add(c);
                        }

                        //Yoga
                        fitnessCategoryID = context.FitnessCategories.FirstOrDefault(d => d.Category == "Yoga").ID;
                        string[] Yoga = new string[] { "warrior pose", "tree pose", "cobra pose", "cat - cow pose", "downward dog pose" };
                        foreach (string exercise in Yoga)
                        {
                            Exercise e = new Exercise
                            {
                                Name = exercise,
                            };
                            context.Exercises.Add(e);
                            context.SaveChanges();
                            ExerciseCategory c = new ExerciseCategory
                            {
                                ExerciseID = e.ID,
                                FitnessCategoryID = fitnessCategoryID

                            };
                            context.ExerciseCategories.Add(c);
                        }

                        //Swimming
                        string[] Swimming = new string[] { "water jogging", "pool sprints", "pool dancing" };
                        fitnessCategoryID = context.FitnessCategories.FirstOrDefault(d => d.Category == "Swimming").ID;
                        foreach (string exercise in Swimming)
                        {
                            Exercise e = new Exercise
                            {
                                Name = exercise,
                            };
                            context.Exercises.Add(e);
                            context.SaveChanges();
                            ExerciseCategory c = new ExerciseCategory
                            {
                                ExerciseID = e.ID,
                                FitnessCategoryID = fitnessCategoryID

                            };
                            context.ExerciseCategories.Add(c);
                        }

                        //High Intensity Interval Training
                        fitnessCategoryID = context.FitnessCategories.FirstOrDefault(d => d.Category == "High Intensity Interval Training").ID;
                        string[] HIT = new string[] { "sprints", "box jumps", "squat jumps", "high knees" };
                        foreach (string exercise in HIT)
                        {
                            Exercise e = new Exercise
                            {
                                Name = exercise,
                            };
                            context.Exercises.Add(e);
                            context.SaveChanges();
                            ExerciseCategory c = new ExerciseCategory
                            {
                                ExerciseID = e.ID,
                                FitnessCategoryID = fitnessCategoryID

                            };
                            context.ExerciseCategories.Add(c);
                        }
                        context.SaveChanges();
                        //We have one exercise (push - ups) that was added to Personal Training earlier that we also want
                        //associated with High Intensity Interval Training
                        ExerciseCategory ec = new ExerciseCategory
                        {
                            ExerciseID = context.Exercises.FirstOrDefault(d => d.Name == "push - ups").ID,
                            FitnessCategoryID = fitnessCategoryID

                        };
                        context.ExerciseCategories.Add(ec);

                        //Strength Training
                        //For this we need to take a different approach because ALL of the exercises are already seeded.
                        //We just need to associate them with the additional Fitness Category
                        fitnessCategoryID = context.FitnessCategories.FirstOrDefault(d => d.Category == "Strength Training").ID;
                        string[] StrengthTraining = new string[] { "squats", "deadlifts", "bench - press", "bicep curls", "pull - ups", "tricep dips" };
                        foreach (string exercise in StrengthTraining)
                        {
                            ExerciseCategory c = new ExerciseCategory
                            {
                                ExerciseID = context.Exercises.FirstOrDefault(d => d.Name == exercise).ID,
                                FitnessCategoryID = fitnessCategoryID

                            };
                            context.ExerciseCategories.Add(c);
                        }
                        //Save all data
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.GetBaseException().Message);
                }
                #endregion

                //Seed meaningless data as sample data during development
                #region Seed Sample Data 
                if (SeedSampleData)
                {
                    //To randomly generate data
                    Random random = new Random();

                    //Seed a few specific Instructors and GroupClasses. We will add more with random values later,
                    //but it can be useful to know we will have some specific records in the sample data.
                    try
                    {
                        // Seed Instructors if there aren't any.
                        if (!context.Instructors.Any())
                        {
                            context.Instructors.AddRange(
                            new Instructor
                            {
                                FirstName = "Fred",
                                MiddleName = "Reginald",
                                LastName = "Flintstone",
                                HireDate = DateTime.Parse("2018-09-01"),
                                Phone = "9055551212",
                                Email = "fflintstone@outlook.com",
                                IsActive = true
                            },
                            new Instructor
                            {
                                FirstName = "Wilma",
                                MiddleName = "Jane",
                                LastName = "Flintstone",
                                HireDate = DateTime.Parse("2020-04-23"),
                                Phone = "9055551212",
                                Email = "wflintstone@outlook.com",
                                IsActive = true
                            },
                            new Instructor
                            {
                                FirstName = "Barney",
                                LastName = "Rubble",
                                HireDate = DateTime.Parse("2021-02-22"),
                                Phone = "9055551213",
                                Email = "brubble@outlook.com",
                                IsActive = true
                            },
                            new Instructor //Note that I removed the assignment of an OHIP since we are setting Coverage.OutOfProvince
                            {
                                FirstName = "Jane",
                                MiddleName = "Samantha",
                                LastName = "Doe",
                                HireDate = DateTime.Parse("2023-01-21"),
                                Phone = "9055551234",
                                Email = "jdoe@outlook.com",
                                IsActive = true
                            });
                            context.SaveChanges();
                        }

                        // Now we can seed a few Group Classes because we have some records in all three
                        // of the 'parent' entities.  Be careful that you choose ones that exist!
                        if (!context.GroupClasses.Any())
                        {
                            context.GroupClasses.AddRange(
                            new GroupClass
                            {
                                Description = "Intense Cardio workout",
                                DOW = DOW.Monday,
                                ClassTimeID = 10,
                                FitnessCategoryID = context.FitnessCategories.FirstOrDefault(c => c.Category == "Cardio").ID,
                                InstructorID = context.Instructors.FirstOrDefault(d => d.FirstName == "Fred" && d.LastName == "Flintstone").ID
                            }, new GroupClass
                            {
                                Description = "Introductory Yoga",
                                DOW = DOW.Tuesday,
                                ClassTimeID = 14,
                                FitnessCategoryID = context.FitnessCategories.FirstOrDefault(c => c.Category == "Yoga").ID,
                                InstructorID = context.Instructors.FirstOrDefault(d => d.FirstName == "Wilma" && d.LastName == "Flintstone").ID
                            }, new GroupClass
                            {
                                Description = "Endurance Swimming",
                                DOW = DOW.Friday,
                                ClassTimeID = 14,
                                FitnessCategoryID = context.FitnessCategories.FirstOrDefault(c => c.Category == "Swimming").ID,
                                InstructorID = context.Instructors.FirstOrDefault(d => d.FirstName == "Barney" && d.LastName == "Rubble").ID
                            });
                            context.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.GetBaseException().Message);
                    }

                    //Now seed data with random values
                    //Get a bunch of names to choose from
                    string[] firstNames = new string[] { "Woodstock", "Violet", "Charlie", "Lucy", "Linus", "Franklin", "Marcie", "Schroeder", "Lyric", "Antoinette", "Kendal", "Vivian", "Ruth", "Jamison", "Emilia", "Natalee", "Yadiel", "Jakayla", "Lukas", "Moses", "Kyler", "Karla", "Chanel", "Tyler", "Camilla", "Quintin", "Braden", "Clarence" };
                    string[] lastNames = new string[] { "Hightower", "Broomspun", "Jones", "Bloggs" };
                    int firstNameCount = firstNames.Length;

                    // For dates we will subtract a random number of days from today
                    DateTime startDate = DateTime.Today;// More efficient

                    //Add more Instructors
                    if (context.Instructors.Count() < 5)
                    {
                        foreach (string lastName in lastNames)
                        {
                            //Choose a random HashSet of 5 first names
                            HashSet<string> selectedFirstNames = new HashSet<string>();
                            while (selectedFirstNames.Count() < 5)
                            {
                                selectedFirstNames.Add(firstNames[random.Next(firstNameCount)]);
                            }

                            foreach (string firstName in selectedFirstNames)
                            {
                                //Construct some Instructor details
                                Instructor instructor = new Instructor()
                                {
                                    FirstName = firstName,
                                    LastName = lastName,
                                    MiddleName = lastName[1].ToString().ToUpper(),
                                    Email = (firstName.Substring(0, 2) + lastName + random.Next(11, 111).ToString() + "@outlook.com").ToLower(),
                                    Phone = random.Next(2, 10).ToString() + random.Next(213214131, 989898989).ToString(),
                                    HireDate = startDate.AddDays(-random.Next(156, 2210)),
                                    IsActive = true
                                };
                                try
                                {
                                    //Could be a duplicate Email
                                    context.Instructors.Add(instructor);
                                    context.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    //so skip it and go on to the next
                                    context.Instructors.Remove(instructor);
                                }
                            }
                        }
                    }

					//Add Cleints 
					//Add one to match the security login
					Client securityClient = new Client()
					{
						MembershipNumber = 10000,
						FirstName = "Sam",
						LastName = "Client",
						Phone = random.Next(2, 10).ToString() + random.Next(10, 99).ToString()
		                    + random.Next(2132141, 9898989).ToString(),
						Email = "client@outlook.com",
						DOB = startDate.AddDays(-random.Next(5850, 22100)),
						PostalCode = "L3C " + random.Next(10).ToString() + "L" + random.Next(10).ToString(),
						HealthCondition = "Very healthy",
						Notes = "Seeded cleint to match the security login",
						MembershipStartDate = DateTime.Today,
						MembershipEndDate = DateTime.Today.AddDays(365),
						MembershipTypeID = 2,
						MembershipFee = 199d,
						FeePaid = true
					};
                    context.Clients.Add(securityClient);
                    context.SaveChanges();

					//Create 5 notes from Bacon ipsum
					string[] baconNotes = new string[] { "Bacon ipsum dolor amet meatball corned beef kevin, alcatra kielbasa biltong drumstick strip steak spare ribs swine. Pastrami shank swine leberkas bresaola, prosciutto frankfurter porchetta ham hock short ribs short loin andouille alcatra. Andouille shank meatball pig venison shankle ground round sausage kielbasa. Chicken pig meatloaf fatback leberkas venison tri-tip burgdoggen tail chuck sausage kevin shank biltong brisket.", "Sirloin shank t-bone capicola strip steak salami, hamburger kielbasa burgdoggen jerky swine andouille rump picanha. Sirloin porchetta ribeye fatback, meatball leberkas swine pancetta beef shoulder pastrami capicola salami chicken. Bacon cow corned beef pastrami venison biltong frankfurter short ribs chicken beef. Burgdoggen shank pig, ground round brisket tail beef ribs turkey spare ribs tenderloin shankle ham rump. Doner alcatra pork chop leberkas spare ribs hamburger t-bone. Boudin filet mignon bacon andouille, shankle pork t-bone landjaeger. Rump pork loin bresaola prosciutto pancetta venison, cow flank sirloin sausage.", "Porchetta pork belly swine filet mignon jowl turducken salami boudin pastrami jerky spare ribs short ribs sausage andouille. Turducken flank ribeye boudin corned beef burgdoggen. Prosciutto pancetta sirloin rump shankle ball tip filet mignon corned beef frankfurter biltong drumstick chicken swine bacon shank. Buffalo kevin andouille porchetta short ribs cow, ham hock pork belly drumstick pastrami capicola picanha venison.", "Picanha andouille salami, porchetta beef ribs t-bone drumstick. Frankfurter tail landjaeger, shank kevin pig drumstick beef bresaola cow. Corned beef pork belly tri-tip, ham drumstick hamburger swine spare ribs short loin cupim flank tongue beef filet mignon cow. Ham hock chicken turducken doner brisket. Strip steak cow beef, kielbasa leberkas swine tongue bacon burgdoggen beef ribs pork chop tenderloin.", "Kielbasa porchetta shoulder boudin, pork strip steak brisket prosciutto t-bone tail. Doner pork loin pork ribeye, drumstick brisket biltong boudin burgdoggen t-bone frankfurter. Flank burgdoggen doner, boudin porchetta andouille landjaeger ham hock capicola pork chop bacon. Landjaeger turducken ribeye leberkas pork loin corned beef. Corned beef turducken landjaeger pig bresaola t-bone bacon andouille meatball beef ribs doner. T-bone fatback cupim chuck beef ribs shank tail strip steak bacon." };

                    //Use a different collection of Last Names
                    lastNames = new string[] { "Brown", "Smith", "Daniel", "Watts", "Randall", "Arias", "Weber", "Stone", "Carlson", "Robles", "Frederick", "Parker", "Morris", "Soto", "Bruce", "Orozco", "Boyer", "Burns", "Cobb", "Blankenship", "Houston", "Estes", "Atkins", "Miranda", "Zuniga", "Ward", "Mayo", "Costa", "Reeves", "Anthony", "Cook", "Krueger", "Crane", "Watts", "Little", "Henderson", "Bishop" };
                    int lastNameCount = lastNames.Length;

                    //We will need a collection of the primary keys and Fees of the Membership Types
                    int[] memTypeIDs = context.MembershipTypes.Select(s => s.ID).ToArray();
                    double[] memTypeFee = context.MembershipTypes.Select(s => s.StandardFee).ToArray();
                    int memTypeIDCount = memTypeIDs.Length;

                    //Choose a random HashSet of 10 last names
                    HashSet<string> selectedLastNames = new HashSet<string>();
                    while (selectedLastNames.Count() < 11)
                    {
                        selectedLastNames.Add(lastNames[random.Next(lastNameCount)]);
                    }

                    //Add More Clients
                    if (context.Clients.Count() < 5)
                    {
                        //Counter for MembershipNumbers
                        int counter = 0;
                        foreach (string lastName in selectedLastNames)
                        {
                            //Choose a random HashSet of 10 first names
                            HashSet<string> selectedFirstNames = new HashSet<string>();
                            while (selectedFirstNames.Count() < 11)
                            {
                                selectedFirstNames.Add(firstNames[random.Next(firstNameCount)]);
                            }

                            foreach (string firstName in selectedFirstNames)
                            {
                                //We need to set a couple of choices for the client before we start building the object
                                DateTime memStartDate = startDate.AddDays(-random.Next(300));
                                int membershipTypeOrdinal = random.Next(memTypeIDCount);

                                //Construct some Client details
                                Client client = new Client()
                                {
                                    MembershipNumber = 10001 + counter,//create them is sequence
                                    FirstName = firstName,
                                    LastName = lastName,
                                    MiddleName = firstName[1].ToString().ToUpper(),
                                    Phone = random.Next(2, 10).ToString() + random.Next(10, 99).ToString()
                                        + random.Next(2132141, 9898989).ToString(),
                                    Email = (firstName.Substring(0, 2) + lastName + random.Next(11, 111).ToString() + "@outlook.com").ToLower(),
                                    DOB = startDate.AddDays(-random.Next(5850, 22100)),
                                    PostalCode = "L3C " + random.Next(10).ToString() + "L" + random.Next(10).ToString(),
                                    HealthCondition = "Very healthy",
                                    Notes = baconNotes[random.Next(5)],
                                    MembershipStartDate = memStartDate,
                                    MembershipEndDate = memStartDate.AddDays(365),
                                    MembershipTypeID = memTypeIDs[membershipTypeOrdinal],
                                    MembershipFee = memTypeFee[membershipTypeOrdinal],
                                    FeePaid = true
                                };
                                counter++;
                                try
                                {
                                    //Could be a duplicate MembershipNumber
                                    context.Clients.Add(client);
                                    context.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    //so skip it and go on to the next
                                    context.Clients.Remove(client);
                                }
                            }

                        }
                    }
                     
                    //Add more GroupClasses
                    //We will need an array of values from our Enum
                    Array valuesDOW = Enum.GetValues(typeof(DOW));
                    //You can now get a random DOW with  = (DOW)valuesDOW.GetValue(random.Next(valuesDOW.Length))

                    //We will need a collection of the primary keys of the Fitness Categories
                    int[] fitCatIDs = context.FitnessCategories.Select(s => s.ID).ToArray();
                    string[] fitCat = context.FitnessCategories.Select(s => s.Category).ToArray();
                    int fitCatIDCount = fitCatIDs.Length;

                    //Create 10 intensity modifiers for describing group classes
                    string[] intensities = new string[] { "Gentle", "Light", "Moderate", "Balanced", "Vigorous", "Intense", "Strenuous", "Extreme", "All-out", "Maximal" };

                    //Get the array of Instructor primary keys
                    int[] instructorIDs = context.Instructors.Select(a => a.ID).ToArray();
                    int instructorIDCount = instructorIDs.Length;

                    //Get the array of ClassTime primary keys
                    int[] classTimeIDs = context.ClassTimes.Select(a => a.ID).ToArray();
                    int classTimeIDCount = classTimeIDs.Length;

                    //Add More GroupClasses
                    if (context.GroupClasses.Count() < 4)
                    {
                        //Create up to 30 more group classes
                        for (int i = 0; i < 30; i++)
                        {
                            //Choose the fitness category
                            int fitnessCategoryOrdinal = random.Next(fitCatIDCount);
                            GroupClass groupClass = new GroupClass()
                            {
                                Description = intensities[random.Next(10)] + " "
                                    + fitCat[fitnessCategoryOrdinal] + " Workout",
                                DOW = (DOW)valuesDOW.GetValue(random.Next(valuesDOW.Length)),
                                FitnessCategoryID = fitCatIDs[fitnessCategoryOrdinal],
                                InstructorID = instructorIDs[random.Next(instructorIDCount)],
                                ClassTimeID = classTimeIDs[random.Next(classTimeIDCount)]
                            };
                            try
                            {
                                //Could be a duplicate 
                                context.GroupClasses.Add(groupClass);
                                context.SaveChanges();
                            }
                            catch (Exception)
                            {
                                //so skip it and go on to the next
                                context.GroupClasses.Remove(groupClass);
                            }
                        }
                    }
                     

                    //For extra fun, lets add some random enrollments in group classes
                    //Get the array of Client primary keys
                    int[] clientIDs = context.Clients.Select(a => a.ID).ToArray();
                    int clientIDCount = clientIDs.Length;
                    //Get the array of GroupClass primary keys
                    int[] groupclassIDs = context.GroupClasses.Select(a => a.ID).ToArray();
                    int groupclassIDCount = groupclassIDs.Length;

                    if(!context.Enrollments.Any())//Add Enrollments in each Group Class
                    {
                        //Lets add some clients into each group class
                        foreach (int i in groupclassIDs)
                        {
                            int howMany = random.Next(5, 20);
                            for (int j = 1; j < howMany; j++)
                            {
                                Enrollment enrollment = new Enrollment()
                                {
                                    GroupClassID = i,
                                    ClientID = clientIDs[random.Next(clientIDCount)]
                                };
                                try
                                {
                                    //Could be a duplicate 
                                    context.Enrollments.Add(enrollment);
                                    context.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    //so skip it and go on to the next
                                    context.Enrollments.Remove(enrollment);
                                }
                            }
                        }
                    }
                    

                    //We couldn't do it earlier so now we will create some
                    //random Workouts and then WorkoutExercises.
                    //We will need ID's for Clints, Exercises and Instructors, although
                    //we will only assign an instructor every few Workouts.

                    //Get the array of Exercise primary keys
                    int[] exerciseIDs = context.Exercises.Select(a => a.ID).ToArray();
                    int exerciseIDCount = exerciseIDs.Length;

                    if(!context.Workouts.Any()) //Add Workouts for each Client
                    {
                        int i = 0;//lets us step through all Instructors so we can make sure each gets used 
                        foreach (int clientID in clientIDs)
                        {                            
                            int howMany = random.Next(5, 20);
                            for (int j = 1; j <= howMany; j++)
                            {
                                Workout workout = new Workout
                                {
                                    ClientID = clientID,
                                    StartTime = DateTime.Today.AddDays((-1 * random.Next(1000))),
                                    Notes = baconNotes[random.Next(5)]
                                };
                                //StartTime will be at midnight so add hours to it so it is more reasonable
                                workout.StartTime = workout.StartTime.AddHours(random.Next(8, 17));
                                //Set a random EndTime between 10 and 80 minutes later
                                workout.EndTime = workout.StartTime + new TimeSpan(0, random.Next(1, 8) * 10, 0);
                                //Assign an Instructor to some
                                i = (i >= instructorIDCount) ? 0 : i;
                                if (j % 2 == 0)
                                {
                                    workout.InstructorID = instructorIDs[i];
                                    i++;
                                }
                                //Now add few WorkoutExercises to the workout
                                int howManyExercises = random.Next(1, 5);
                                for (int k = 1; k <= howManyExercises; k++)
                                {
                                    workout.WorkoutExercises.Add(new WorkoutExercise
                                    {
                                         WorkoutID=workout.ID,
                                         ExerciseID= exerciseIDs[random.Next(exerciseIDCount)]
                                    });
                                }
                                try
                                {
                                    context.Workouts.Add(workout);
                                    context.SaveChanges();
                                }
                                catch (Exception)//won't worry about retry exceptions
                                {
                                    context.Workouts.Remove(workout);
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }
    }
}
