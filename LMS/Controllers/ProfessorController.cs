using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : CommonController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year) //works
        {
            //subject , num -- > catalogID
            //catalogID , season, year --> classID
            // classID -- all uid
            // all information

            JsonResult json = null;
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var sicQuery = from uid in (from ci in (from cid in (from c in db.Courses
                                                                     where c.Subject == subject && c.Num == num
                                                                     select new { c.CatalogId })
                                                        join cl in db.Classes
                                                        on cid.CatalogId equals cl.CatalogId
                                                        where cl.Semester == year.ToString() + season
                                                        select new { classID = cl.ClassId })
                                            join e in db.Enrolled
                                            on ci.classID equals e.ClassId
                                            select new { uid = e.UId, grade = e.Grade })
                               join u in db.Users
                               on uid.uid equals u.UId
                               select new
                               {
                                   fname = u.FirstName,
                                   lname = u.LastName,
                                   dob = u.Dob,
                                   uid = u.UId,
                                   uid.grade
                               };
                json = Json(sicQuery.ToArray());
            }
            return json;
        }

        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        
        {
            // sub, num -- catalog ID
            // season year, catalogID -- classID
            // classID, category -- acID
            // all class with acID

            if (category == null)
            {
                using (Team9LMSContext db = new Team9LMSContext())
                {
                    var asQuery = from acID in (from ci in (from cid in (from c in db.Courses
                                                                         where c.Subject == subject && c.Num == num
                                                                         select new { CatalogId = c.CatalogId })
                                                            join cl in db.Classes
                                                            on cid.CatalogId equals cl.CatalogId
                                                            where cl.Semester == year.ToString() + season
                                                            select new { classID = cl.ClassId })
                                                join ac in db.AssignmentCategories
                                                on ci.classID equals ac.ClassId
                                                select new { acID = ac.AcId, Name = ac.Name })
                                  join ass in db.Assignments
                                  on acID.acID equals ass.AcId
                                  select new
                                  {
                                      aname = ass.Name,
                                      cname = acID.Name,
                                      due = ass.Due,
                                      submissions = NumSub(ass.AId)
                                  };
                    return Json(asQuery.ToArray());
                }
            }
                

            JsonResult json = null;
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var asQuery = from acID in (from ci in (from cid in (from c in db.Courses
                                                                     where c.Subject == subject && c.Num == num
                                                                     select new { CatalogId = c.CatalogId })
                                                        join cl in db.Classes
                                                        on cid.CatalogId equals cl.CatalogId
                                                        where cl.Semester == year.ToString() + season
                                                        select new { classID = cl.ClassId })
                                            join ac in db.AssignmentCategories
                                            on ci.classID equals ac.ClassId
                                            where ac.Name == category
                                            select new { acID = ac.AcId, Name = ac.Name })
                              join ass in db.Assignments
                              on acID.acID equals ass.AcId
                              select new
                              {
                                  aname = ass.Name,
                                  cname = acID.Name,
                                  due = ass.Due,
                                  submissions = NumSub(ass.AId)
                              };
                json = Json(asQuery.ToArray());
            }
            return json;
        }

        public uint NumSub(uint aID)
        {

            using (Team9LMSContext db = new Team9LMSContext())
            {
                var subQuery = from s in db.Submission
                               where s.AId == aID
                               select s;
                return (uint)subQuery.Count();
            }

        }

        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year) //works
        {
            // subject , num -- CourseID
            // Course Id, seson , year -- Class ID
            // ClassID --> cate name ,weigt
            JsonResult json = null;
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var classIDQuery = from ci in (from cid in (from c in db.Courses
                                                            where c.Subject == subject && c.Num == num
                                                            select new { CatalogId = c.CatalogId })
                                               join cl in db.Classes
                                               on cid.CatalogId equals cl.CatalogId
                                               where cl.Semester == year.ToString() + season
                                               select new { classID = cl.ClassId })
                                   join ac in db.AssignmentCategories
                                   on ci.classID equals ac.ClassId
                                   select new { name = ac.Name, weight = ac.Weight };
                json = Json(classIDQuery.ToArray());
            }
            return json;
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)  //works
        {
            if (catweight.GetType() != typeof(int) || catweight < 0)
                return Json(new { success = false });

            using (Team9LMSContext db = new Team9LMSContext())
            {
                var classIDQuery = from cid in (from c in db.Courses
                                                where c.Subject == subject && c.Num == num
                                                select new { CatalogId = c.CatalogId })
                                   join cl in db.Classes
                                   on cid.CatalogId equals cl.CatalogId
                                   where cl.Semester == year.ToString() + season
                                   select new { classID = cl.ClassId };
                if (classIDQuery.Count() == 0)
                    return Json(new { success = false });
                foreach (var cq in classIDQuery)
                {
                    var ClassID = Convert.ToInt32(cq.classID);
                    AssignmentCategories newAC = new AssignmentCategories();
                    newAC.ClassId = (uint)ClassID;
                    newAC.Name = category;
                    newAC.Weight = (uint)catweight;

                    db.AssignmentCategories.Add(newAC);
                }
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("CreateAssignmentCategoryCreateAssignmentCategory");
                }
            }
            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)  // works
        {
            //subject, num -- catalogId
            //catalogID, semester -- > classID
            //class ID  , Name - ACID
            // asgname, asgcontenst, asgpoint, asgdue, acid  --- add new assignment
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var acIDQuery = from ci in (from cid in (from c in db.Courses
                                                         where c.Subject == subject && c.Num == num
                                                         select new { CatalogId = c.CatalogId })
                                            join cl in db.Classes
                                            on cid.CatalogId equals cl.CatalogId
                                            where cl.Semester == year.ToString() + season
                                            select new { classID = cl.ClassId })
                                join ac in db.AssignmentCategories
                                on ci.classID equals ac.ClassId
                                where ac.Name == category
                                select new { acID = ac.AcId, ci.classID };
                uint acID = (uint)Convert.ToInt32(acIDQuery.First().acID);



                //check to void duiplicate (name , acID )
                var duplicateCheckQuery = from asg in db.Assignments
                                          where asg.Name == asgname && asg.AcId == acID
                                          select asg;
                if (duplicateCheckQuery.Count() > 0)
                    return Json(new { success = false });

                if (asgpoints < 0)
                    return Json(new { success = false });

                //add new entry to db
                Assignments newAS = new Assignments();
                newAS.AcId = acID;
                newAS.Name = asgname;
                newAS.Contents = asgcontents;
                newAS.Points = (uint)asgpoints;
                newAS.Due = asgdue;
                db.Assignments.Add(newAS);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return Json(new { success = false });
                }
                //updata all students's grades.
                uint classID = (uint)Convert.ToInt32(acIDQuery.First().classID);
                GPA.ClassGradeUpdate(classID);


            }
            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname) //works
        {
            JsonResult json = null;
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var subQuery = from s in (from aid in (from acID in (from ci in (from cid in (from c in db.Courses
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
                                          on aid.AId equals s.AId
                                          select s)
                               join u in db.Users
                               on s.UId equals u.UId
                               into sJoinU
                               from sU in sJoinU
                               select new { fname = sU.FirstName, lname = sU.LastName, uid = s.UId, time = s.Time, score = s.Score };
                json = Json(subQuery.ToArray());
            }
            return json;
        }

        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        //works
        {
            uint classID = 0;
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var subQuery = from aid in (from acID in (from ci in (from cid in (from c in db.Courses
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
                                            select new { asg.AId, asg.Points })
                               join s in db.Submission
                               on aid.AId equals s.AId
                               where s.UId == uid
                               select new { s, aid.Points };

                uint points = (uint)subQuery.First().Points;
                if (score < 0 || score > points)
                    return Json(new { success = false });

                subQuery.First().s.Score = (uint)score;
                db.Update(subQuery.First().s);
                db.SaveChanges();

                //update student's letter grade.
                var classIDQuery = from cid in (from c in db.Courses
                                                where c.Subject == subject && c.Num == num
                                                select new { CatalogId = c.CatalogId })
                                   join cl in db.Classes
                                   on cid.CatalogId equals cl.CatalogId
                                   where cl.Semester == year.ToString() + season
                                   select new { classID = cl.ClassId };
                classID = (uint)Convert.ToInt32(classIDQuery.First().classID);
                GPA.GradeUpdate(uid, classID);

            }

            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)  //works
        {
            JsonResult json = null;
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var myClassesQuery = from cl in (from c in db.Classes
                                                 where c.UId == uid
                                                 select new
                                                 {
                                                     season = c.Semester.Substring(4),
                                                     year = c.Semester.Substring(0, 4),
                                                     catelogID = c.CatalogId
                                                 })
                                     join co in db.Courses
                                     on cl.catelogID equals co.CatalogId
                                     select new
                                     {
                                         subject = co.Subject,
                                         number = co.Num,
                                         name = co.Name,
                                         cl.season,
                                         cl.year
                                     };
                json = Json(myClassesQuery.ToArray());

            }
            return json;
        }


        /*******End code to modify********/

    }
}