using sys = System;

using UnityEngine;
using Libraries.system.file_system;
using System.Text;
using System;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
using helper;
using System.Linq;
using System.Text.RegularExpressions;


[sys.Serializable]
public class Hardware
{
    [SerializeField]
    internal Drive mainDrive;
    [SerializeField]
    internal InputManager inputManager;
    [SerializeField]
    internal ScreenManager screenManager;
    [SerializeField]
    internal StackExecutor stackExecutor;
    //todo 0 add shell
    //  public static SimpleShell
    //todo rename to 
    public static dynamic shell;




    #region Processor
    [NaughtyAttributes.OnValueChanged("UpdateWRR")]
    internal float FPSCap = 100;
    [NaughtyAttributes.OnValueChanged("UpdateFPSC")]
    internal int WaitRefreshRate = 100;

    internal int TasksPerCPULoop = -1;
    internal bool ignoreSomeErrors = true;//todo -9 remove
    internal static readonly Encoding mainEncoding = Encoding.GetEncoding("437");

    void UpdateWRR()
    {
        WaitRefreshRate = (int)((1f / FPSCap) * 1000f);
    }
    void UpdateFPSC()
    {
        FPSCap = ((1f / WaitRefreshRate) * 1000f);
    }
    #endregion
    #region Script running management
    #region static data
    private const string LINE_NUMBER_PREPROCESSOR_CODE = "__LINE__";

    private const string FILE_NUMBER_PREPROCESSOR_CODE = "__FILE__";

    private const bool ONLY_UPPERCASE_REDEFINE = false;
    public static ScriptOptions scriptOptionsBuffer = ScriptOptions.Default.AddReferences(
             typeof(UnityEngine.MonoBehaviour).GetTypeInfo().Assembly/*,
        typeof(System.Text.RegularExpressions.Regex).GetTypeInfo().Assembly,
             typeof(helper.GlobalHelper).GetTypeInfo().Assembly,
             typeof(System.Collections.Generic.Dictionary<int,int>).GetTypeInfo().Assembly
            //,typeof(System.Exception).GetTypeInfo().Assembly
*/
            ).AddImports(
          // "UnityEngine", "System"
          ).WithEmitDebugInformation(true)
            .WithFileEncoding(Hardware.mainEncoding);

    public static readonly List<Type> allLibraries = new List<Type>() {
            typeof(Libraries.system.output.Console),
            typeof(Libraries.system.Runtime),
            typeof(Libraries.system.output.graphics.Screen),
            typeof(Libraries.system.output.graphics.texture32.Texture32),
            typeof(Libraries.system.output.graphics.system_texture.SystemTexture),
            typeof(Libraries.system.output.graphics.color32.Color32),
            typeof(Libraries.system.output.graphics.system_colorspace.ColorConstants),
            typeof(Libraries.system.output.graphics.screen_buffer32.ScreenBuffer32),
            typeof(Libraries.system.output.graphics.system_screen_buffer.SystemScreenBuffer),
            typeof(Libraries.system.file_system.File),
            typeof(Libraries.system.shell.IShellProgram),
            typeof(Libraries.system.mathematics.Vector2),
            typeof(Libraries.system.input.KeyHandler),
            typeof(Libraries.system.input.MouseHander),
            typeof(Libraries.system.output.graphics.mask_texture.MaskTexture),


            typeof(sys.Text.RegularExpressions.Regex),
            typeof(helper.GlobalHelper),
            typeof(sys.Collections.Generic.Dictionary<int,int>),


            typeof(sys.Linq.Enumerable),
            typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)

    };
    public static readonly List<LibraryData> allLibraryDatas = allLibraries.ConvertAll(x => x.ToLibraryData());
    #endregion

    [SerializeField]
    private ConcurrentQueue<ITryToRun> actionQueue = new ConcurrentQueue<ITryToRun>();
    [SerializeField]

    internal List<CodeTask> scriptsRunning = new List<CodeTask>();
    internal T AddDelegateToStack<T>(MainThreadDelegate<T>.MTDFunction action, bool sync = true)
    {

        return AddDelegateToStack(new MainThreadDelegate<T>(action, WaitRefreshRate), sync);

    }
    internal T AddDelegateToStack<T>(MainThreadDelegate<T> mtf, bool sync = true)
    {
        actionQueue.Enqueue(mtf);
        if (sync)
        {
            return mtf.WaitForReturn();
        }
        return default(T);
    }
    internal void AddDelegateToStack(Action action, bool sync = true)
    {
        AddDelegateToStack((ref bool done, ref object returnValue) =>
        {
            action.Invoke();
        }, sync);
    }
    #region code Parsing
    static Regex includeRegex = new Regex("^\\s*#\\s*include\\s*\".*\"\\s*");
    static Regex includeRegex2 = new Regex("^\\s*#\\s*include\\s*\".*\"\\s*;*");

    static Regex redefineRegex = new Regex("^\\s*#\\s*redefine\\s*.*\\s*.*");

    static Regex undefineRegex = new Regex("^\\s*#\\s*undefine\\s*.*\\s*");
    static Regex undefineRegexOld = new Regex("^\\s*#\\s*undefine\\s*.*\\s*.*;*");
    internal void RunCode(CodeObject codeObject)
    {
        Debug.Log("Making new codeTask");
        CodeTask codeTask = new CodeTask(this);
        scriptsRunning.Add(codeTask);
        CodeParser(ref codeObject);
        codeTask.RunCode(codeObject);
        Debug.Log("After running code");


    }
    internal void RemoveCodeTask(CodeTask codeTask)
    {
        scriptsRunning.Remove(codeTask);
    }

    internal void CheackThreads()
    {

        foreach (CodeTask ct in scriptsRunning)
        {
            Debug.Log("thread null?" + (ct.thread == null ? "yes" : "no") + ct.thread);
        }
    }


    internal void KillAll()
    {
        for (int i = 0; i < scriptsRunning.Count; i++)
        {
            CodeTask ct = scriptsRunning[i];
            if (ct != null)
            {
                ct.Destroy();
            }
            else
            {
                scriptsRunning.Remove(ct);
            }
            i--;
        }

        //  scriptOptionsBuffer = null;

        GC.Collect();


    }
    internal void CodeParser(ref CodeObject codeObject)
    {
        List<string> lines = new List<string>(codeObject.code.Split('\n'));

        List<string> importedLibraries = new List<string>();
        Dictionary<string, string> currentRedefines = new Dictionary<string, string>();

        int positionToExpectNextInlcude = 0;
        bool checkIncludes = true;
        bool checkRedefines = true;
        bool checkUndefines = true;

        for (int index = 0; index < lines.Count; index++)
        {
            string buffer = lines[index];
            if (currentRedefines.Count > 0)
            {

                List<string> parts = buffer.SplitSpaceQArgs();
                foreach (var item in currentRedefines)
                {
                    // Debug.Log($"line:{buffer}. replace {item.Key} for {item.Value} so now it looks: { buffer.Replace(item.Key, item.Value.replacor)}");

                    //todo-z no replace args with values. Impossible #redefine "lol($x,$y)" "Console.Debug($x+"--"+$y)";
                    for (int i = 0; i < parts.Count; i++)
                    {



                        string part = parts[i];

                        if (part == item.Key)
                        {

                            string replacor = item.Value;

                            parts[i] = part.Replace(item.Key, replacor).Replace(LINE_NUMBER_PREPROCESSOR_CODE, (index + 1).ToString()).Replace(FILE_NUMBER_PREPROCESSOR_CODE, "unknown");

                        }

                    }

                }
                buffer = string.Join("", parts);

            }


            if ((positionToExpectNextInlcude < index && !includeRegex.IsMatch(buffer)))
            {
                if (string.IsNullOrEmpty(buffer) || string.IsNullOrWhiteSpace(buffer) || buffer.StartsWith("//") ||
                    buffer.StartsWith("/*"))
                {
                    positionToExpectNextInlcude++;
                }
                else
                {
                    //  Debug.Log($"next:{positionToExpectNextInlcude} index:{index}");
                    checkIncludes = false;
                }
            }

            if (checkRedefines)
            {
                if (redefineRegex.IsMatch(buffer))
                {

                    positionToExpectNextInlcude++;
                    List<string> lineParts = buffer.SplitSpaceQArgsCustom("#"); // buffer.Split(' ', '(', ')', ',');
                    int definitionIndex = lineParts.FindIndex(x => x != "#" && !string.IsNullOrWhiteSpace(x) && x != "redefine");
                    int definitionEndIndex = lineParts.FindIndex(definitionIndex, x => string.IsNullOrWhiteSpace(x));

                    string definition = string.Join("", lineParts.GetRange(definitionIndex, definitionEndIndex - definitionIndex));

                    if (ONLY_UPPERCASE_REDEFINE)
                    {
                        if (definition != definition.ToUpper())
                        {
                            //todo-future throw error
                            continue;
                        }
                    }
                    string definitionReplacor = string.Join(" ", lineParts.Skip(definitionEndIndex).Take(lineParts.Count - definitionEndIndex - 1).ToArray()).TrimStart();

                    if (definition == null || definitionReplacor == null)
                    {
                        continue;
                    }
                    buffer = "//" + buffer.Substring(1);
                    currentRedefines.Add(definition, definitionReplacor);

                }
            }
            if (checkUndefines)
            {
                if (undefineRegex.IsMatch(buffer))
                {
                    positionToExpectNextInlcude++;

                    List<string> lineParts = buffer.SplitSpaceQArgsCustom("#"); // buffer.Split(' ', '(', ')', ',');
                    int definitionIndex = lineParts.FindIndex(x => x != "#" || string.IsNullOrEmpty(x) || x != "undefine");
                    string definition = lineParts[definitionIndex];
                    currentRedefines.Remove(definition);
                }
            }
            if (checkIncludes)
            {
                if (includeRegex.IsMatch(buffer))
                {

                    string between = buffer.GetRangeBetweenFirstLast("\"");
                    if (string.IsNullOrEmpty(between))
                    {
                        between = buffer.GetRangeBetweenFirstLast("\'");
                    }
                    if (!importedLibraries.Contains(between))
                    {
                        importedLibraries.Add(between);
                        File f = mainDrive.GetFileByPath(between);
                        if (f == null)
                        {
                            /*if (null.Contains( between))
                            {
                           //todo 9 think about #include-ing core libraries like system or system.file_system
                            // this would be the best with system variables
                            }*/
                            lines[index] = $"//There would be imported \"{between}\" but it couldn't be found!";
                            continue;
                        }
                        List<string> importedLines = new List<string>(f.data.ToEncodedString().Split('\n'));
                        buffer = "//imported: " + between;

                        lines[index] = buffer;
                        for (int iL = 0; iL < importedLines.Count; iL++)
                        {
                            string importedLine = importedLines[iL];
                            lines.Insert(index + iL + 1, importedLine);


                        }
                        positionToExpectNextInlcude += importedLines.Count + 1;

                        index--;
                        continue;
                    }
                    else
                    {
                        lines[index] = $"//There would be imported \"{between}\" but it was already imported earlier!";

                        //todo-future add compilation error
                    }


                }
            }
            lines[index] = buffer;


        }
        Debug.Log(lines.ToFormatedString("\n"));
        codeObject.code = string.Join("\n", lines);
    }
    #endregion
    #endregion

}