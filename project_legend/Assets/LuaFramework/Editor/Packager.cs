using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using LuaFramework;

public class Packager {
    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();

    ///-----------------------------------------------------------
    static string[] exts = { ".txt", ".xml", ".lua", ".assetbundle", ".json" };
    static bool CanCopy(string ext) {   //能不能复制
        foreach (string e in exts) {
            if (ext.Equals(e)) return true;
        }
        return false;
    }

    /// <summary>
    /// 获取各个平台的 相对应的游戏开发目录
    /// </summary>
    static string AppDataPath
    {
        get { return Application.dataPath.ToLower(); }
    }

    private static readonly string assetBundlePath = Path.GetDirectoryName(AppDataPath) + "/extract/AssetBundle" + "/StreamingAssets/";
    private static readonly string assetPath = AppDataPath + "/StreamingAssets/"; //AssetBundle 打包后的 配置文件名称

    /// <summary>
    /// 载入(Builds下的指定)素材
    /// </summary>
    static UnityEngine.Object LoadAsset(string file) {
        if (file.EndsWith(".lua")) file += ".txt";
        return AssetDatabase.LoadMainAssetAtPath("Assets/LuaFramework/Examples/Builds/" + file);
    }

//    [MenuItem("LuaFramework/Build iPhone Resource", false, 100)]
//    public static void BuildiPhoneResource() {
//        BuildTarget target;
//#if UNITY_5
//        target = BuildTarget.iOS;
//#else
//        target = BuildTarget.iPhone;
//#endif
//        BuildAssetResource(target);
//    }

    [MenuItem("LuaFramework/Build Lua Resource", false, 100)]
    public static void BuildAndroidResource()
    {
        BuildLuaResource(BuildTarget.Android);
    }

    //[MenuItem("LuaFramework/Build Windows Resource", false, 102)]
    //public static void BuildWindowsResource() {
    //    BuildAssetResource(BuildTarget.StandaloneWindows);
    //}

    /// <summary>
    /// 生成绑定素材,打AssetBundle包,以及对Lua文件的处理
    /// </summary>
    public static void BuildLuaResource(BuildTarget target)
    {
        //Delete_OldFile();
        SetAllAssetBundleName();
        //Build_AssetBundle(assetBundlePath, target);
        HandleLuaFile();
        //BuildFileIndex();
        //extractAssetBundle(LoadFileList("files.txt"));
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 删除旧资源
    /// </summary>
    static void Delete_OldFile()
    {
        //删除Extract资源
        DirectoryInfo oldPath = new DirectoryInfo(assetBundlePath);
        if (oldPath.Exists)
        {
            oldPath.Delete(true);
        }

        //删除AssetBundle资源
        FileUtil.DeleteFileOrDirectory(assetPath);
        AssetDatabase.Refresh();

        //强制清空一下缓存
        Caching.CleanCache();
    }

    /// <summary>
    /// 对所有需要打包的资源设置AssetBundle标签
    /// </summary>
    static void SetAllAssetBundleName()
    {
        string name = string.Empty;
        foreach (var item in AssetDatabase.GetAllAssetPaths()) //获取unity Assets文件下的所有资源的路径
        {
            string path_lower = item.ToLower();
            if (path_lower.EndsWith(".cs") || path_lower.EndsWith(".js")) continue;
            int buildIndex = item.IndexOf("/Build/");

            if (buildIndex >= 0)
            {
                string path = item;
                string gameModuleName = string.Empty;
                string assetBundleName = string.Empty;
                gameModuleName = path.Substring(0, buildIndex);
                gameModuleName = Path.GetFileName(gameModuleName); //拿到需要打包项目的字符串
                gameModuleName = gameModuleName.Trim('_');         //以下划线开头的目录不会打包到Android的StreamingAssets目录下
                gameModuleName += "/";
                if (gameModuleName == "Assets/") gameModuleName = string.Empty;//如果Build 文件夹直接在Assets/ 下, 那么就没有模块

                assetBundleName = path.Substring(buildIndex + "/Build/".Length);
                if (assetBundleName.Contains("/"))//该资源所需要打的AssetBundle包含多个资源,因此该资源在Build文件夹下的xxx 文件夹(Build/xxx/ ... asset.asset)下
                {
                    assetBundleName = assetBundleName.Substring(0, assetBundleName.IndexOf("/"));
                }
                else
                {   //该资源所需要打的AssetBundle 就只有该文件一个资源
                    assetBundleName = Path.GetFileNameWithoutExtension(path);
                }

                name = gameModuleName + assetBundleName + ".assetbundle";      //AssetBundle标签组成完成
                AssetImporter assetImporter = AssetImporter.GetAtPath(path);   
                assetImporter.assetBundleName = name;
            }
        }
    }

    /// <summary>
    /// 游戏资源打到游戏目录的 Extract下
    /// </summary>
    /// <param name="assetPath">Extract目录</param>
    /// <param name="target">打包平台</param>
    static void Build_AssetBundle(string assetPath, BuildTarget target)
    {
        if (!Directory.Exists(assetPath)) Directory.CreateDirectory(assetPath);

        BuildPipeline.BuildAssetBundles(assetPath, BuildAssetBundleOptions.None, target);
    }

    /// <summary>
    /// 处理Lua文件
    /// </summary>
    static void HandleLuaFile()
    {
        string luaPath = AppDataPath + "/StreamingAssets/lua/";

        //----------复制Lua文件----------------
        if (!Directory.Exists(luaPath))
        {
            Directory.CreateDirectory(luaPath);
        }
        string[] luaPaths = { AppDataPath + "/LuaFramework/lua/",
                              AppDataPath + "/LuaFramework/Tolua/Lua/" };

        for (int i = 0; i < luaPaths.Length; i++)
        {
            paths.Clear(); files.Clear();
            string luaDataPath = luaPaths[i].ToLower();
            Recursive(luaDataPath);
            int n = 0;
            foreach (string f in files)
            {
                if (f.EndsWith(".meta")) continue;
                string newfile = f.Replace(luaDataPath, "");
                string newpath = luaPath + newfile;
                string path = Path.GetDirectoryName(newpath);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                if (File.Exists(newpath))
                {
                    File.Delete(newpath);
                }
                if (AppConst.LuaByteMode)
                {
                    EncodeLuaFile(f, newpath);
                }
                else
                {
                    File.Copy(f, newpath, true);
                }
                UpdateProgress(n++, files.Count, newpath);
            }
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    static void BuildFileIndex()
    {
        string resPath = assetBundlePath;
        ///----------------------创建文件列表-----------------------
        string newFilePath = resPath + "/files.txt";
        if (File.Exists(newFilePath)) File.Delete(newFilePath);

        paths.Clear(); files.Clear();
        Recursive(resPath);

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            //string ext = Path.GetExtension(file);
            if (file.EndsWith(".meta") || file.Contains(".DS_Store") || file.Contains(".manifest")) continue;

            string md5 = Util.md5file(file);
            string value = file.Replace(resPath, string.Empty);
            sw.WriteLine(value + "|" + md5);
        }
        sw.Close(); fs.Close();
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }

    /// <summary>
    /// 把Extract AssetBundle文件移动到 Assets/StreamingAsset下 
    /// </summary>
    /// <param name="extractPath">需要移动的assetBundle</param>
    static void extractAssetBundle(List<string> extractPath)
    {
        if (!Directory.Exists(assetPath)) Directory.CreateDirectory(assetPath);

        string targetPath = assetPath + "mobilelegend/";
        for (int i = 1; i < extractPath.Count; i++)
        {
            int start_index = extractPath[i].IndexOf("/") + 1;
            int end_Length = extractPath[i].IndexOf("|") - start_index;

            //string folderName = extractPath[i].Substring(start_index, end_Length - 12);
            string bundleName = extractPath[i].Substring(start_index, end_Length);
            string sourceBundle = assetBundlePath + "mobilelegend/" + bundleName;

            /// 创建目录
            if (Directory.Exists(targetPath) == false)
            {
                Directory.CreateDirectory(targetPath);
            }

            /// 拷贝资源
            File.Copy(sourceBundle, targetPath + bundleName, true);       
        }

        //拷贝StreamingAsset 和 files.txt
        File.Copy(assetBundlePath + "StreamingAssets", assetPath + "StreamingAssets", true);
        File.Copy(assetBundlePath + "files.txt", assetPath + "files.txt", true);
    }

    static List<string> LoadFileList(string fileName)
    {
        string filePath = assetBundlePath + fileName;

        if (!File.Exists(filePath))
        {
            UnityEngine.Debug.Log("File.txt does not exist");
            return null;
        }
            
        List <string> fileList = new List<string>();
        StreamReader sr = File.OpenText(filePath);
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            fileList.Add(line);
        }
        return fileList;
    }

    /// <summary>
    /// 生成Lua文件
    /// </summary>
    static void HandleBundle()
    {
        BuildLuaBundles();
        string luaPath = AppDataPath + "/StreamingAssets/lua/";
        string[] luaPaths = { AppDataPath + "/LuaFramework/lua/",
                              AppDataPath + "/LuaFramework/Tolua/Lua/" };

        for (int i = 0; i < luaPaths.Length; i++)
        {
            paths.Clear(); files.Clear();
            string luaDataPath = luaPaths[i].ToLower();
            Recursive(luaDataPath);
            foreach (string f in files)
            {
                if (f.EndsWith(".meta") || f.EndsWith(".lua")) continue;
                string newfile = f.Replace(luaDataPath, "");
                string path = Path.GetDirectoryName(luaPath + newfile);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                string destfile = path + "/" + Path.GetFileName(f);
                File.Copy(f, destfile, true);
            }
        }
    }

    static void ClearAllLuaFiles() {
        string osPath = Application.streamingAssetsPath + "/" + LuaConst.osDir;

        if (Directory.Exists(osPath)) {
            string[] files = Directory.GetFiles(osPath, "Lua*.unity3d");

            for (int i = 0; i < files.Length; i++) {
                File.Delete(files[i]);
            }
        }

        string path = osPath + "/Lua";

        if (Directory.Exists(path)) {
            Directory.Delete(path, true);
        }

        path = Application.dataPath + "/Resources/Lua";

        if (Directory.Exists(path)) {
            Directory.Delete(path, true);
        }

        path = Application.persistentDataPath + "/" + LuaConst.osDir + "/Lua";

        if (Directory.Exists(path)) {
            Directory.Delete(path, true);
        }
    }

    static void CreateStreamDir(string dir) {
        dir = Application.streamingAssetsPath + "/" + dir;

        if (!File.Exists(dir)) {
            Directory.CreateDirectory(dir);
        }
    }

    static void CopyLuaBytesFiles(string sourceDir, string destDir, bool appendext = true) {
        if (!Directory.Exists(sourceDir)) {
            return;
        }

        string[] files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
        int len = sourceDir.Length;

        if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\') {
            --len;
        }

        for (int i = 0; i < files.Length; i++) {
            string str = files[i].Remove(0, len);
            string dest = destDir + str;
            if (appendext) dest += ".bytes";
            string dir = Path.GetDirectoryName(dest);
            Directory.CreateDirectory(dir);

            if (AppConst.LuaByteMode) {
                Packager.EncodeLuaFile(files[i], dest);
            } else {
                File.Copy(files[i], dest, true);
            }
        }
    }

    static void BuildLuaBundles() {
        ClearAllLuaFiles();
        CreateStreamDir("lua/");
        CreateStreamDir(AppConst.LuaTempDir);

        string dir = Application.persistentDataPath;
        if (!File.Exists(dir)) {
            Directory.CreateDirectory(dir);
        }

        string streamDir = Application.dataPath + "/" + AppConst.LuaTempDir;
        CopyLuaBytesFiles(CustomSettings.luaDir, streamDir);
        CopyLuaBytesFiles(CustomSettings.FrameworkPath + "/ToLua/Lua", streamDir);

        AssetDatabase.Refresh();
        string[] dirs = Directory.GetDirectories(streamDir, "*", SearchOption.AllDirectories);

        for (int i = 0; i < dirs.Length; i++) {
            string str = dirs[i].Remove(0, streamDir.Length);
            BuildLuaBundle(str);
        }

        BuildLuaBundle(null);
        Directory.Delete(streamDir, true);
        AssetDatabase.Refresh();
    }

    static void BuildLuaBundle(string dir) {
        BuildAssetBundleOptions options = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets |
                                          BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle;
        string path = "Assets/" + AppConst.LuaTempDir + dir;
        string[] files = Directory.GetFiles(path, "*.lua.bytes");
        List<Object> list = new List<Object>();
        string bundleName = "lua.unity3d";
        if (dir != null) {
            dir = dir.Replace('\\', '_').Replace('/', '_');
            bundleName = "lua_" + dir.ToLower() + AppConst.ExtName;
        }
        for (int i = 0; i < files.Length; i++) {
            Object obj = AssetDatabase.LoadMainAssetAtPath(files[i]);
            list.Add(obj);
        }

        if (files.Length > 0) {
            string output = Application.streamingAssetsPath + "/lua/" + bundleName;
            if (File.Exists(output)) {
                File.Delete(output);
            }
            BuildPipeline.BuildAssetBundle(null, list.ToArray(), output, options, EditorUserBuildSettings.activeBuildTarget);
            AssetDatabase.Refresh();
        }
    }

    static void UpdateProgress(int progress, int progressMax, string desc) {
        string title = "Processing...[" + progress + " - " + progressMax + "]";
        float value = (float)progress / (float)progressMax;
        EditorUtility.DisplayProgressBar(title, desc, value);
    }

    public static void EncodeLuaFile(string srcFile, string outFile) {
        if (!srcFile.ToLower().EndsWith(".lua")) {
            File.Copy(srcFile, outFile, true);
            return;
        }
        bool isWin = true; 
        string luaexe = string.Empty;
        string args = string.Empty;
        string exedir = string.Empty;
        string currDir = Directory.GetCurrentDirectory();
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            isWin = true;
            luaexe = "luajit.exe";
            args = "-b -g " + srcFile + " " + outFile;
            exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luajit/";
        } else if (Application.platform == RuntimePlatform.OSXEditor) {
            isWin = false;
            luaexe = "./luajit";
            args = "-b -g " + srcFile + " " + outFile;
            exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luajit_mac/";
        }
        Directory.SetCurrentDirectory(exedir);
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = luaexe;
        info.Arguments = args;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.UseShellExecute = isWin;
        info.ErrorDialog = true;
        Util.Log(info.FileName + " " + info.Arguments);

        Process pro = Process.Start(info);
        pro.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }

    [MenuItem("LuaFramework/Build Protobuf-lua-gen File")]
    public static void BuildProtobufFile() {
        //if (!AppConst.ExampleMode) {
        //    UnityEngine.Debug.LogError("若使用编码Protobuf-lua-gen功能，需要自己配置外部环境！！");
        //    return;
        //}
        string dir = AppDataPath + "/LuaFramework/Lua/3rd/pblua";
        paths.Clear(); files.Clear(); Recursive(dir);

        string protoc = "d:/protobuf-2.5.0/src/protoc.exe"; 
        string protoc_gen_dir = "\"d:/protoc-gen-lua/plugin/protoc-gen-lua.bat\""; 

        foreach (string f in files) {
            string name = Path.GetFileName(f);
            string ext = Path.GetExtension(f);
            if (!ext.Equals(".proto")) continue;

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = protoc;
            info.Arguments = " --lua_out=./ --plugin=protoc-gen-lua=" + protoc_gen_dir + " " + name;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = true;
            info.WorkingDirectory = dir;
            info.ErrorDialog = true;
            Util.Log(info.FileName + " " + info.Arguments);

            Process pro = Process.Start(info);
            pro.WaitForExit();
        }
        AssetDatabase.Refresh();
    }
}