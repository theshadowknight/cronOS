﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cinemachine;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using NaughtyAttributes;
using UnityEngine;
using System.Linq;
using System.Collections;

[SaveDuringPlay]
public class CodeRunner : MonoBehaviour
{
    public ScriptOptions scriptOptions = null;
    //todo remove task
    //  public CancellationTokenSource tokenSource = new CancellationTokenSource();
    //   public CancellationToken ctoken;


    public static readonly Type[] allLibraries = new Type[] { typeof(libraries.system.console), typeof(libraries.system.graphics.color) };
    public Type[] enabledLibraries = allLibraries;

    public static CodeRunner instance;
    //todo make special customdrawer to get and add code blocks
    [AllowNesting]
    public CodeBasement codeBasement;

    public string tag;


    [ResizableTextArea] public string code;
    public List<CodeTask> codeTasks = new List<CodeTask>();
    [SerializeField]
    public Queue<MainThreadFunction> actionStack = new Queue<MainThreadFunction>();


    [Button("Add to basement")]
    void AddToBasement()
    {
        codeBasement.codeBlocks.Add(new CodeBlock(tag, code));
    }


    public void TryToInitScriptOptions()
    {
        if (scriptOptions != null)
        {
            return;
        }
        // flag: task
        // ctoken = tokenSource.Token;
        scriptOptions = ScriptOptions.Default;


        Type[] enabledLibraries = allLibraries;

        scriptOptions = scriptOptions.AddReferences(Array.ConvertAll(enabledLibraries, x => x.GetTypeInfo().Assembly));

        scriptOptions = scriptOptions.AddImports(
           Array.ConvertAll(enabledLibraries, x => x.GetTypeInfo().Namespace)



        );
        scriptOptions = scriptOptions.AddReferences(
             typeof(UnityEngine.MonoBehaviour).GetTypeInfo().Assembly
            );

        scriptOptions = scriptOptions.AddImports(
            "UnityEngine"
       );
        /*    int a = 1;
            int b = 2;
            int c = 3;

            float Δ = b ^ 2 - 4 * a * c;
            Debug.Log(Δ);*/
        Debug.Log("Debug done");
    }


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        TryToInitScriptOptions();
        //  StartCoroutine(e());
    }
    /*  IEnumerator e()
      {
          while (true)
          {
              ExecuteFromStack();
              //  yield return new WaitForSecondsRealtime(0.01f);
              yield return;
          }
      }*/



    [Button("Run code")]
    void RunCode()
    {
        TryToInitScriptOptions();
        CodeTask codeTask = new CodeTask();
        codeTasks.Add(codeTask);
        codeTask.RunCode(CodeParser(code));
    }

    [Button("Display stack")]
    void DisplayStack()
    {
        foreach (MainThreadFunction mtf in actionStack)
        {
            Debug.Log(mtf.function.ToString());

        }
    }

    [Button("Collect")]
    void TRYFIX()
    {

        GC.Collect();

    }
    public string CodeParser(string code)
    {
        return
            //  "console console = new console();console.init(currentCodeTask);\n" +
            code;

    }

    public void Update()
    {


        ExecuteFromStack();
        // flag: task
        //  Debug.Log("token source" + tokenSource.IsCancellationRequested);
        //  Debug.Log("token" + ctoken.IsCancellationRequested);
    }
    public void LateUpdate()
    {
        //     ExecuteFromStack();
    }
    private void ExecuteFromStack(int count = -1)
    {
        count = count == -1 ? actionStack.Count : count;
        for (int i = 0; i < count; i++)
        {
            if (actionStack.Count > 0)
            {

                MainThreadFunction mtf = actionStack.Dequeue();
                mtf?.Run();
                mtf.Dispose();
                mtf = null;
            }
            else
            {
                return;
            }
        }
    }

    private static void AddToStack(MainThreadFunction function)
    {
        instance.actionStack.Enqueue(function);
    }
    [Button("Wipe")]

    public void Wipe()
    {
        // flag: task
        //  tokenSource.Cancel();



        Debug.Log("destryoying");
        for (int i = 0; i < codeTasks.Count; i++)
        {
            CodeTask ct = codeTasks[i];
            if (ct != null)
            {
                ct.Destroy();
            }
            else
            {
                codeTasks.Remove(ct);
            }
            i--;
        }

        scriptOptions = null;

        GC.Collect();


    }
    public void OnDestroy()
    {
        Wipe();

    }
    public void OnApplicationQuit()
    {
        Wipe();
    }
    public static object AddFunctionToStack(Func<object> action, bool wait = true)
    {
        MainThreadFunction mtf = new MainThreadFunction(action);

        AddToStack(mtf);
        if (wait)
        {
            return mtf.WaitForAction();
        }
        return null;

    }

    public static object AddFunctionToStack(Action action, bool wait = true)
    {
        return AddFunctionToStack(() =>
        {
            action.Invoke();
            return null;
        }, wait);
    }
}
