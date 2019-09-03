using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    public class CommonController : Controller
    {

        /*******Begin code to modify********/

        // TODO: Uncomment and change 'X' after you have scaffoled


        protected Team9LMSContext db;

        public CommonController()
        {
            db = new Team9LMSContext();
        }


        /*
         * WARNING: This is the quick and easy way to make the controller
         *          use a different LibraryContext - good enough for our purposes.
         *          The "right" way is through Dependency Injection via the constructor 
         *          (look this up if interested).
        */

        // TODO: Uncomment and change 'X' after you have scaffoled

        public void UseLMSContext(Team9LMSContext ctx)
        {
            db = ctx;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }




        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments() //works
        {
            // TODO: Do not return this hard-coded array.
            JsonResult json = null;
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var query = from d in db.Departments
                            select new { name = d.DepartmentName, subject = d.Subject };
                json = Json(query.ToArray());
            }
            return json;
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>        
        public IActionResult GetCatalog() //works
        {
            JsonResult json = null;
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var departments_query = from d in db.Departments
                                        select new
                                        {
                                            subject = d.Subject,
                                            dname = d.DepartmentName,
                                            courses = from c in db.Courses
                                                      where c.Subject == d.Subject
                                                      select new { number = c.Num, cname = c.Name }
                                        };
                json = Json(departments_query.ToArray());
            }
            return json;
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>

        public IActionResult GetClassOfferings(string subject, int number) //works
        {
            JsonResult json = null;
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

                var classQuery = from c in db.Classes
                                 join u in db.Users
                                 on c.UId equals u.UId
                                 where c.CatalogId == catalogID
                                 select new
                                 {
                                     season = c.Semester.Substring(4),
                                     year = c.Semester.Substring(0, 4),
                                     location = c.Location,
                                     start = c.StartTime,
                                     end = c.EndTime,
                                     fname = u.FirstName,
                                     lname = u.LastName
                                 };
                json = Json(classQuery.ToArray());
            }
            return json;
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        // works
        {
            //sub, num --> catalogID
            // catalogID, year, season --> ClassID
            // classID, category --> acID
            // acID, name -- > contents

            using (Team9LMSContext db = new Team9LMSContext())
            {
                var conQuery = from acID in (from ci in (from cid in (from c in db.Courses
                                                                      where c.Subject == subject && c.Num == num
                                                                      select new { CatalogId = c.CatalogId })
                                                         join cl in db.Classes
                                                         on cid.CatalogId equals cl.CatalogId
                                                         where cl.Semester == year.ToString() + season
                                                         select new { classID = cl.ClassId })
                                             join ac in db.AssignmentCategories
                                             on ci.classID equals ac.ClassId
                                             where ac.Name == category
                                             select new { acID = ac.AcId })
                               join asg in db.Assignments
                               on acID.acID equals asg.AcId
                               where asg.Name == asgname
                               select new { asg.Contents };

                var content = conQuery.First().Contents.ToString();
                return Content(content);
            }
        }
        
        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        //works
        {
            //sub, num --> catalogID
            // catalogID, year, season --> ClassID
            // classID, category --> acID
            // acID, name -- > aID
            // aID,uID -- GetSubmissonText
            
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var conQuery = from aid in (from acID in (from ci in (from cid in (from c in db.Courses
                                                                                   where c.Subject == subject && c.Num == num
                                                                                   select new { CatalogId = c.CatalogId })
                                                                      join cl in db.Classes
                                                                      on cid.CatalogId equals cl.CatalogId
                                                                      where cl.Semester == year.ToString() + season
                                                                      select new { classID = cl.ClassId })
                                                          join ac in db.AssignmentCategories
                                                          on ci.classID equals ac.ClassId
                                                          where ac.Name == category
                                                          select new { acID = ac.AcId })
                                            join asg in db.Assignments
                                            on acID.acID equals asg.AcId
                                            where asg.Name == asgname
                                            select new { asg.AId })
                               join s in db.Submission
                               on new { aID = aid.AId, uid } equals new { aID = s.AId, uid = s.UId }
                               into aJoinS
                               from aS in aJoinS.DefaultIfEmpty()
                               select new { content = aS == null ? "" : aS.Contents };
                var content = conQuery.First().content.ToString();
                return Content(content);
            }
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>


        public IActionResult GetUser(string uid) // works
        {
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var aQuery = from a in db.Administrators
                             join u in db.Users
                             on a.UId equals u.UId
                             where a.UId == uid
                             select new
                             { fname = u.FirstName, lname = u.LastName, uid = a.UId, };
                if (aQuery.Count() > 0)
                    return Json(aQuery.First());
                
                var pQuery = from p in (from p in db.Professors
                                        join d in db.Departments
                                        on p.Subject equals d.Subject
                                        where p.UId == uid
                                        select new { UId = uid, dName = d.DepartmentName })
                             join u in db.Users
                             on p.UId equals u.UId
                             select new
                             { fname = u.FirstName, lname = u.LastName, uid = p.UId, department = p.dName };
                if (pQuery.Count() > 0)
                    return Json(pQuery.First());
                
                var sQuery = from p in (from s in db.Students
                                        join d in db.Departments
                                        on s.Subject equals d.Subject
                                        where s.UId == uid
                                        select new { UId = uid, dName = d.DepartmentName })
                             join u in db.Users
                             on p.UId equals u.UId
                             select new
                             { fname = u.FirstName, lname = u.LastName, uid = p.UId, department = p.dName };

                if (sQuery.Count() > 0)
                    return Json(sQuery.First());
            }



            return Json(new { success = false });
        }


        /*******End code to modify********/

    }
}