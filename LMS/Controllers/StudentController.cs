using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : CommonController
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid) //works
        {
            // uid --> classID,  grade
            // classID --> catalog ID, semester,( semester --> year, season)
            // catalogID --> subject, number, name 

            JsonResult json = null;
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var myClassesQuery = from cla1 in (from cl in (from e in db.Enrolled
                                                               where e.UId == uid
                                                               select new
                                                               { grade = e.Grade ?? "--", classID = e.ClassId, })
                                                   join cla in db.Classes
                                                   on cl.classID equals cla.ClassId
                                                   select new
                                                   {
                                                       catalogID = cla.CatalogId,
                                                       season = cla.Semester.ToString().Substring(4),
                                                       year = cla.Semester.ToString().Substring(0, 4),
                                                       cl.grade
                                                   })
                                     join cou in db.Courses
                                     on cla1.catalogID equals cou.CatalogId
                                     select new
                                     { subject = cou.Subject, number = cou.Num, name = cou.Name, cla1.season, cla1.year, cla1.grade };
                json = Json(myClassesQuery.ToArray());


            }
            return json;
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid) //works
        {
            //sub, num --> catalogID
            //CatalogID , season, year --> classID
            // classID --> acID
            // acID --> aIDs
            // aIDs , uid --> all score earned 

            JsonResult json = null;
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var asgICQuery = from asg in (from acID in (from ci in (from cid in (from c in db.Courses
                                                                                     where c.Subject == subject && c.Num == num
                                                                                     select new { c.CatalogId })
                                                                        join cl in db.Classes
                                                                        on cid.CatalogId equals cl.CatalogId
                                                                        where cl.Semester == year.ToString() + season
                                                                        select new { classID = cl.ClassId })
                                                            join ac in db.AssignmentCategories
                                                            on ci.classID equals ac.ClassId
                                                            select new { acID = ac.AcId, cname = ac.Name })
                                              join ass in db.Assignments
                                              on acID.acID equals ass.AcId
                                              select new { aname = ass.Name, acID.cname, due = ass.Due, aID = ass.AId })
                                 join s in db.Submission
                                 on new { asg.aID, uID = uid } equals new { aID = s.AId, uID = s.UId }
                                 into aJoinS
                                 from aS in aJoinS.DefaultIfEmpty()
                                 select new { asg.aname, asg.cname, asg.due, score = aS == null ? (uint?)null : aS.Score };
                json = Json(asgICQuery.ToArray());
            }
            return json;
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            //sub, num --> catalogID
            // catalogID, year, season --> ClassID
            // classID, category --> acID
            // acID, asgname -- > aID
            // aID, uID, Constents --> add to submission 
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var aIDQuery = from acID in (from ci in (from cid in (from c in db.Courses
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
                               select new { asg.AId, asg.Points };
                uint aID = aIDQuery.First().AId;

                var subQuery = from s in db.Submission
                               where s.AId == aID && s.UId == uid
                               select s;

                if (subQuery.Count() > 0)
                {
                    subQuery.First().Time = DateTime.Now;
                    subQuery.First().Contents = contents;
                    try
                    {
                        db.Update(subQuery.First());
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }
                }
                else
                {
                    Submission newSub = new Submission();
                    newSub.AId = aID;
                    newSub.UId = uid;
                    newSub.Time = DateTime.Now;
                    newSub.Contents = contents;
                    //newSub.Score = 0;
                    db.Submission.Add(newSub);
                    db.SaveChanges();
                }
            }
            return Json(new { success = true });
        }
        
        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid) //works
        {
            //sub , num --> catalogID
            // catalogID, season, year --> classid
            // classid, uied -- add to Enroll
            var flag = false;
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var classIDQuery = from cid in (from c in db.Courses
                                                where c.Subject == subject && c.Num == num
                                                select new { CatalogId = c.CatalogId })
                                   join cl in db.Classes
                                   on cid.CatalogId equals cl.CatalogId
                                   where cl.Semester == year.ToString() + season
                                   select new { classID = cl.ClassId };
                Enrolled newEn = new Enrolled();
                newEn.UId = uid;
                newEn.ClassId = (uint)Convert.ToInt32(classIDQuery.First().classID);

                db.Enrolled.Add(newEn);
                db.SaveChanges();

                flag = true;
            }
            return Json(new { success = flag });
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var finalGPA = 0.0;
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var gradeQuery = from e in db.Enrolled
                                 where e.UId == uid
                                 select new { e.Grade };
                List<string> grades = new List<string>();
                foreach (var g in gradeQuery)
                {
                    if (g.Grade != "--")
                        grades.Add(g.Grade);
                }

                finalGPA = GPA.GPACalculation(grades);
            }

            return Json(new { gpa = finalGPA });
        }

        /*******End code to modify********/

    }
}