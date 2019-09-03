using LMS.Models.LMSModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Controllers
{
    public class GPA
    {
        public static void GradeUpdate(string uid, uint classID)
        {
            
            Dictionary<uint, uint> acIDAndWeight = new Dictionary<uint, uint>();
            Dictionary<uint, uint> acIDAndTotalPoints = new Dictionary<uint, uint>();
            Dictionary<uint, uint> acIDAndTotalScores = new Dictionary<uint, uint>();
            Dictionary<uint, uint> acIDAndTotalAsgs = new Dictionary<uint, uint>();
            List<uint> acIDs = new List<uint>();
            uint totalWeight = 0;

            using (Team9LMSContext db = new Team9LMSContext())
            {
                //Get all categories of this class 
                var awQuery = from ac in db.AssignmentCategories
                              where ac.ClassId == classID
                              select new { ac.AcId, ac.Weight };
                foreach (var aw in awQuery)
                {
                    acIDAndWeight.Add(aw.AcId, (uint)aw.Weight);
                    acIDAndTotalPoints.Add(aw.AcId, 0);
                    acIDAndTotalScores.Add(aw.AcId, 0);
                    acIDs.Add(aw.AcId);
                }

                //Get all score of each assignment in each assignment category
                var asgsQuery = from asg1 in (from acID in (from ac in db.AssignmentCategories
                                                            where ac.ClassId == classID
                                                            select new { ac.AcId })
                                              join asg in db.Assignments
                                              on acID.AcId equals asg.AcId
                                              select new { asg.AId, asg.Points, asg.AcId })
                                join s in db.Submission
                                on asg1.AId equals s.AId
                                where s.UId == uid
                                select new { s, points = asg1.Points, acID = asg1.AcId };

                foreach (var a1 in asgsQuery)
                {
                    uint acID = (uint)a1.acID;
                    uint score = (uint?)a1.s.Score ?? 0;
                    acIDAndTotalScores[acID] += score;
                }

                //Get all point of each assignment
                var pointsQuery = from acID in (from ac in db.AssignmentCategories
                                                where ac.ClassId == classID
                                                select new { ac.AcId })
                                  join asg in db.Assignments
                                  on acID.AcId equals asg.AcId
                                  select new { asg.AId, points = asg.Points, acID = asg.AcId };

                foreach (var pq in pointsQuery)
                {
                    uint acID = (uint)pq.acID;
                    uint points = (uint)pq.points;
                    acIDAndTotalPoints[acID] += points;
                }

                //Get number of assignments for each assignment category
                var asgNumQuery = from ac in db.AssignmentCategories
                                  where ac.ClassId == classID
                                  select new { acID = ac.AcId, numAsg = numAssignments(ac.AcId) };

                foreach (var aNQ in asgNumQuery)
                    acIDAndTotalAsgs.Add(aNQ.acID, aNQ.numAsg);

                //calculation of total weight and scalefactor
                for (int i = 0; i < acIDs.Count(); i++)
                {
                    var tempAcID = acIDs[i];
                    if (acIDAndTotalAsgs[tempAcID] != 0)
                        totalWeight += acIDAndWeight[tempAcID];
                }
                
                double scaleFactor = 100.00 / totalWeight;

                //Calculation for final score for this class
                double finalScore = 0.0;
                for (int i = 0; i < acIDs.Count(); i++)
                {
                    var tempAcID = acIDs[i];
                    if (acIDAndTotalPoints[tempAcID] != 0)  // denominator should not be zero  --- mean no assigments in this category or this assignment will not be counted.
                    {
                        double tempScore = (double)acIDAndTotalScores[tempAcID] / acIDAndTotalPoints[tempAcID] * acIDAndWeight[tempAcID] * scaleFactor;
                        finalScore += tempScore;
                    }
                }
                
                //convert score to letter grade
                List<int> percentage = new List<int> { 93, 90, 87, 83, 80, 77, 73, 70, 67, 63, 60, 0 };
                List<string> letters = new List<string> { "A", "A-", "B+", "B", "B-", "C+", "C", "C-", "D+", "D", "D-", "E" };
                int index = indexOfLetters(percentage, finalScore);
                string letterGrade = letters[index];

                //update letter grade to DB
                var gradeQuery = from e in db.Enrolled
                                 where e.ClassId == classID && e.UId == uid
                                 select e;
                gradeQuery.First().Grade = letterGrade;
                db.Update(gradeQuery.First());
                db.SaveChanges();
            }
        }

        public static uint numAssignments(uint acID)
        {
            using (Team9LMSContext db = new Team9LMSContext())
            {
                var asgQuery = from asg in db.Assignments
                               where asg.AcId == acID
                               select asg;
                return (uint)asgQuery.Count();
            }
        }


        public static void ClassGradeUpdate(uint classID)
        {
            List<string> uids = new List<string>();
            using (Team9LMSContext db = new Team9LMSContext())
            {
                //Get all student's uid of this class 
                var uidQuery = from e in db.Enrolled
                               where e.ClassId == classID
                               select new { uid = e.UId };
                foreach (var uq in uidQuery)
                    uids.Add(uq.uid);
            }
            //update each student's grade
            foreach (string uid in uids)
                GradeUpdate(uid, classID);
        }

        public static int indexOfLetters(List<int> list, double value)
        {
            for (int i = 0; i < list.Count(); i++)
                if (value >= list[i])
                    return i;

            return -1;
        }
        public static double GPACalculation(List<string> grades)
        {
            Dictionary<string, double> grade2points = new Dictionary<string, double> { { "A", 4 }, { "A-", 3.7 }, { "B+", 3.3 }, { "B", 3.0 }, { "B-", 2.7 }, { "C+", 2.3 }, { "C", 2.0 }, { "C-", 1.7 }, { "D+", 1.3 }, { "D", 1.0 }, { "D-", 0.7 }, { "E", 0 } };
            double sum = 0.0;
            foreach (var g in grades)
                sum += grade2points[g];

            return sum / grades.Count();
        }
    }
}
