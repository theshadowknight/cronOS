using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;
public class BaseLibrary
{
    // private CodeTask _currentCodeTask;

    public static bool no => false;
    public static bool yes => true;
    public static bool sync => yes;
    //todo 9 remove
  /*  protected static CodeTask currentCodeTask
    {
        get
        {
            //  if (_currentCodeTask == null)
            {
                return (CodeTask)Thread.GetData(Thread.GetNamedDataSlot(CodeTask.ThreadCodeTaskID));
            }
         //   return _currentCodeTask;
        }
    }*/
}