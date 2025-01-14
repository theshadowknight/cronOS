using System.Collections.Generic;
using System.Linq;

namespace Libraries.system
{
    namespace file_system
    {
        public class Path
        {
            public List<string> parts = new List<string>();
            public List<File> fileparts = new List<File>();

            public Path(string rawPath, File root = null, File parent = null)
            {
                rawPath ??= "";

                parent ??= root;

                rawPath = rawPath.StartsWith("./") ? (parent.GetFullPath() + rawPath.Substring(1)) : rawPath;
                parts = rawPath.Split(FileSystem.catalogSymbol).ToList();

                File currentFile = root;

                fileparts.Add(currentFile);
                for (int i = 1; i < parts.Count; i++)
                {
                    if (currentFile == null)
                    {
                        //todo-future throw error

                        parts = null;
                        fileparts = null;
                        break;
                    }

                    string name = parts[i];
                    if (string.IsNullOrEmpty(name))
                    {
                        //todo-future throw error
                        break;
                    }

                    if (name == "..")
                    {
                        if (fileparts.Count > 1)
                        {
                            fileparts.RemoveAt(fileparts.Count - 1);
                            fileparts.RemoveAt(fileparts.Count - 1);

                            parts.RemoveAt(i);
                        }
                        else
                        {
                            parts = null;
                            fileparts = null;
                            break;
                        }

                        currentFile = currentFile?.Parent;
                        //  continue;
                    }
                    else
                    {
                        currentFile = currentFile.GetChildByName(name);
                    }

                    if (currentFile == null)
                    {
                        //todo-future throw error

                        parts = null;
                        fileparts = null;
                        break;
                    }

                    fileparts.Add(currentFile);
                }

                if (parts != null && parts.Count == 1 && currentFile.GetFullPath() != rawPath)
                {
                    //Debug.Log("uh!");
                    parts = null;
                    fileparts = null;
                }
            }

            public override string ToString()
            {
                string path = parts == null
                    ? ""
                    : string.Join( /*FileSystemInternal.catalogSymbol.ToString()*/"-", parts) + "\n";
                fileparts?.ForEach(x => path += x.name + "-");
                return path;
            }

            public File GetFile()
            {
                return fileparts?[fileparts.Count - 1];
            }
        }
    }
}