using Libraries.system.file_system;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Concurrent;

[CreateAssetMenu(fileName = "Drive", menuName = "ScriptableObjects/Drive")]

[Serializable]
public class Drive : ScriptableObject
{
    // [SerializeField]
    // public File root;
    //  public Dictionary<string, File> pathLinks = new Dictionary<string, File>();
    [NonSerialized]
    public bool cached = false;
    [SerializeField]
    public ThreadSafeList<File> files = new ThreadSafeList<File>();
    [SerializeField]
    public ConcurrentQueue<int> freeSpaces = new ConcurrentQueue<int>();
    [NaughtyAttributes.Button]
    public void OpenEditor()
    {
        GenerateCacheData();

        SerializedObject so = new SerializedObject(this, this);
        //   FileEditor.ShowWindow(root, so.FindProperty("root"), so);
    }
    /*  [Button]
      public void GenerateParentLinks()
      {
          GenerateCacheData();
      }*/
    public File GetRoot()
    {
        File root = GetFileByID(0);
        root ??= GetFileByPath("");
        return root;
    }
    [Button]
    public void GenerateCacheData()
    {
        cached = true;
        for (int i = 0; i < files.Count; i++)
        {
            if (files[i].children != null)
            {


                files[i].children.Clear();
            }
        }
        for (int i = 0; i < files.Count; i++)
        {
            File file = files[i];
            file.SetDrive(this);
            File parent = GetFileByID(file.ParentID);
            file.Parent = parent;
            parent?.AddChild(file);
        }
    }
    public File GetFileByID(int id)
    {
        try
        {
            return files[id];
        }
        catch (Exception e)
        {
            return null;
        }
    }
    /*  [Button]

      public void Test()
      {
          GenerateCacheData();
          for (int i = 0; i < files.Count; i++)
          {
              File file = files[i];
              File file2 = files[i];

              List<File> path = new List<File>();
              path.Add(file);
              while (file.parent != null)
              {
                  file = file.parent;
                  path.Add(file);
              }
              path.Reverse();
              Debug.Log($"{file2} path: {string.Join("/", path)}");
          }
      }*/

    public File GetFileByPath(string rawPath, File parent = null)
    {
        return GetPath(rawPath, parent).GetFile();
    }

    public Path GetPath(string rawPath, File parent = null)
    {
        return new Path(rawPath, parent);
    }

    public bool RemoveFile(string path)
    {
        //todo-future add errors
        File file = GetFileByPath(path);
        file.Parent.RemoveChild(file);
        freeSpaces.Enqueue(file.FileID);
        return true;
    }
    /**
     <summary> 
    This doesn't add <see cref="File"/> of type folder to <see cref="DriveSO"/>
    </summary>
       <returns>New sample folder</returns>
     **/
    public File MakeFolder(string name)
    {
        File newFile = new File();
        newFile.name = name;
        newFile.permissions = (FilePermission)0b1111;

        return newFile;
    }
    /**
     <summary> 
    This doesn't add <see cref="File"/> to <see cref="DriveSO"/>
    </summary>
       <returns>New sample file</returns>
     **/
    public File MakeFile(string name, byte[] data = null)
    {
        File newFile = new File();
        newFile.name = name;
        newFile.permissions = (FilePermission)0b0111;

        newFile.data = data;

        return newFile;
    }
    public int GetFreeID()
    {
        if (freeSpaces.Count > 1)
        {
            if (freeSpaces.TryDequeue(out int result))
            {
                return result;
            }
        }

        return files.Count;

    }

}