using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdministratorController : CommonController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subject">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject) //works
        {
            JsonResult json = null;
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var query = from c in db.Courses
                            where c.Subject == subject
                            select new { name = c.Name, number = c.Num };
                json = Json(query.ToArray());
            }
            return json;
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>

        // jrm didnot be verified.

        public IActionResult GetProfessors(string subject) //works
        {
            JsonResult json = null;
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var query = from p in db.Professors
                            join u in db.Users
                            on new { uID = p.UId, sub = p.Subject } equals new { uID = u.UId, sub = subject }
                            select new { uid = p.UId, fname = u.FirstName, lname = u.LastName };
                json = Json(query.ToArray());
            }
            return json;
        }

        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name) //works
        {
            if (number > 9999 || number < 0)
                return Json(new { success = false });
            if (name.Length > 100)
                return Json(new { success = false });

            using (Team9LMSContext db = new Team9LMSContext())
            {
                //check to void duplicate (sub, num)
                var query = from c in db.Courses
                            where c.Subject == subject && c.Num == (uint)number
                            select c;
                if (query.Count() > 0)
                    return Json(new { success = false });

                //add new entry to db
                Courses newCourse = new Courses
                {
                    Name = name,
                    Num = (uint)number,
                    Subject = subject
                };

                db.Courses.Add(newCourse);
                db.SaveChanges();
            }
            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor) //works
        {

            using (Team9LMSContext db = new Team9LMSContext())
            {
                var catalogIDQuery = from c in db.Courses
                                     where c.Subject == subject && c.Num == number
                                     select new { c.CatalogId };
                uint catalogID = 0;
                foreach (var cq in catalogIDQuery)
                {
                    catalogID = cq.CatalogId;
                }
                // To void Location and time overlay or duplicate class in same semester
                TimeSpan nStart = DT2TimeSpan(start);
                TimeSpan nEnd = DT2TimeSpan(end);

                var locationTimeQuery = from c in db.Classes
                                        where (c.CatalogId == catalogID && c.Semester == year.ToString() + season) || ((c.Location == location && ((c.StartTime <= nStart && c.EndTime >= nEnd) || (c.StartTime >= nStart && c.StartTime <= nEnd) || (c.EndTime >= nStart && c.EndTime <= nEnd))) && (c.Semester == year.ToString() + season))
                                        select c;
                if (locationTimeQuery.Count() > 0)
                    return Json(new { success = false });

                Classes newClass = new Classes
                {
                    Semester = Convert.ToString(year) + season,
                    CatalogId = catalogID,
                    Location = location,
                    StartTime = DT2TimeSpan(start),
                    EndTime = DT2TimeSpan(end),
                    UId = instructor
                };

                db.Classes.Add(newClass);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    return Json(new { success = false });
                }
            }

            return Json(new { success = true });
        }

        public TimeSpan DT2TimeSpan(DateTime dt)
        {
            string dtDate = dt.ToString("yyyy-MM-dd");
            DateTime midnightDT = Convert.ToDateTime(dtDate);
            TimeSpan dtTime = dt - midnightDT;

            return dtTime;
        }

        /*******End code to modify********/

    }
}