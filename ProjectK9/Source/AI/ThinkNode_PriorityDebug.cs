using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using Verse.AI;

namespace ProjectK9.AI
{
    /*
     * Currently unused in the mod. I used this when debugging to get a sense of what jobs are getting assigned 
     * to which Pawns.
     */
    //class ThinkNode_PriorityDebug : ThinkNode
    //{
    //    public override ThinkResult TryIssueJobPackage(Pawn pawn)
    //    {
    //        try
    //        {
    //            using (List<ThinkNode>.Enumerator enumerator = this.subNodes.GetEnumerator())
    //            {
    //                while (enumerator.MoveNext())
    //                {
    //                    JobPackage jobPackage = enumerator.Current.TryIssueJobPackage(pawn);
    //                    if (jobPackage != null)
    //                    {
    //                        Log.Message(pawn + " got job: " + jobPackage.job.def.defName);
    //                        return jobPackage;
    //                    }
    //                }
    //            }
    //            Log.Warning(pawn + " got no job.");
    //            return (JobPackage)null;
    //        }
    //        catch (Exception e)
    //        {
    //            Log.Error("Exception trying to get job: " + e.Message + "\n" + e.StackTrace);
    //            throw e;
    //        }
    //    }
    //}
}
