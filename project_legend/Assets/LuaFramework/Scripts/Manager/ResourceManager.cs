using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace LuaFramework {
    public class ResourceManager : Manager {
        private AssetBundle shared;
        //manifest：用于加载assetbundle的相关资源
        private AssetBundleManifest manifest = null;
        //bundles:用于记录该assetbundle是否已经被加载过
        private Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();
        //ResDic:用于记录该UI prefab是否已经被加载过
        private Dictionary<string, GameObject> ResDic = new Dictionary<string, GameObject>();


        void Awake()
        {
            LoadAssetBundleManifest();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void initialize(Action func) {
            if (AppConst.ExampleMode) {
                //------------------------------------Shared--------------------------------------
                string uri = Util.DataPath + "shared" + AppConst.ExtName;
                Debug.LogWarning("LoadFile::>> " + uri);

                shared = AssetBundle.LoadFromFile(uri);
#if UNITY_5
                shared.LoadAsset("Dialog", typeof(GameObject));
#else
                shared.Load("Dialog", typeof(GameObject));
#endif
            }
            if (func != null) func();    //资源初始化完成，回调游戏管理器，执行后续操作 
        }

        /// <summary>
        /// 载入相关UI预制体
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public UnityEngine.GameObject LoadRes(Transform parent, string path)
        {
            if (CheckResExist(path))
            {
                if (GetResInDic(path) != null)
                {
                    return GetResInDic(path);
                }
                else
                {
                    ResDic.Remove(path);
                }
            }

            GameObject objLoad = null;
            objLoad = GameObject.Instantiate(LoadDirectly(path)) as GameObject;
            objLoad.transform.parent = parent;
            objLoad.name = objLoad.name.Substring(0, objLoad.name.Length - 7); 
            objLoad.transform.localPosition = Vector3.zero;
            objLoad.transform.localScale = Vector3.one;
            ResDic.Add(path, objLoad);
            return objLoad;
        }

        /// <summary>
        /// 把相关UI预制体删除掉,释放内存
        /// </summary>
        /// <param name="obj"></param>
        public void DestroyRes(GameObject obj)
        {
            if (ResDic == null || ResDic.Count == 0)
            {
                return;
            }
            if (obj == null)
            {
                return;
            }
            foreach (string key in ResDic.Keys)
            {
                GameObject objLoad;
                if (ResDic.TryGetValue(key, out objLoad) && objLoad == obj)
                {
                    GameObject.DestroyImmediate(obj);
                    ResDic.Remove(key);
                    break;
                }
            }
        }


        private UnityEngine.Object LoadDirectly(string path)
        {
#if IsDevelop
            return null;
#else
            path = path.ToLower();
            int lastIndex = path.LastIndexOf("/");
            string abName = path.Substring(0, lastIndex);
            string name = path.Substring(lastIndex + 1, path.Length - 1 - lastIndex);
            string pathname = "mobilelegend/" + abName;
            return LoadBundle(pathname, name);
#endif
        }

        private bool CheckResExist(string path)
        {
            if (ResDic == null || ResDic.Count == 0)
            {
                return false;
            }
            return ResDic.ContainsKey(path);
        }

        private GameObject GetResInDic(string path)
        {
            if (ResDic == null || ResDic.Count == 0)
            {
                return null;
            }
            GameObject obj = null;
            if (ResDic.TryGetValue(path, out obj))
            {
                return obj;
            }
            return null;
        }

        /// <summary>
        /// 载入相关素材
        /// </summary>
        public UnityEngine.Object LoadBundle(string pathname, string name)
        {
#if IsDevelop
            return null;
#else
            AssetBundle bundle = null;
            bundles.TryGetValue(pathname, out bundle);
            if (bundle == null || !bundles.ContainsKey(pathname)) //如果之前没有加载过的,才去加载AssetBundle
            {
                LoadDependence(pathname);
                string uri = Util.DataPath + pathname.ToLower() + AppConst.ExtName;
                bundle = AssetBundle.LoadFromFile(uri);
                bundles.Remove(pathname);
                bundles.Add(pathname, bundle);
            }
            return bundle.LoadAsset(name);
#endif
        }

        /// <summary>
        /// 加载新场景
        /// </summary>
        /// <param name="pathname"></param>
        /// <param name="name"></param>
        public void LoadScene(string pathname, string name)
        {
#if IsDevelop
            return null;
#else
            AssetBundle bundle = null;
            bundles.TryGetValue(pathname, out bundle);
            if (bundle == null || !bundles.ContainsKey(pathname)) //如果之前没有加载过的,才去加载AssetBundle
            {
                LoadDependence(pathname);
                string uri = Util.DataPath + pathname.ToLower() + AppConst.ExtName;
                bundle = AssetBundle.LoadFromFile(uri);
                bundles.Remove(pathname);
                bundles.Add(pathname, bundle);
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene(name, UnityEngine.SceneManagement.LoadSceneMode.Single);
#endif
        }

        /// <summary>
        /// 加载相关连的资源
        /// </summary>
        /// <param name="pathname"></param>
        private void LoadDependence(string pathname)
        {
            //GetAllDependencies会返回直接和间接关联的AssetBundle
            string bundleName = pathname.ToLower() + AppConst.ExtName;
            string[] dependences = manifest.GetAllDependencies(bundleName);
            for (int i = 0; i < dependences.Length; i++)
            {
                AssetBundle.LoadFromFile(Util.DataPath + dependences[i].ToLower());
                //注意:  这里不需要手动LoadAsset
                //       只需要加载AssetBundle即可
                //       Asset会在加载其他关联Asset时自动加载
            }
        }

        /// <summary>
        /// 加载Manifest文件
        /// </summary>
        private void LoadAssetBundleManifest()
        {
            if (manifest == null)
            {
                string uri = Util.DataPath + AppConst.AssetDirname;
                var bundle = AssetBundle.LoadFromFile(uri);
                manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                //压缩释放掉
                bundle.Unload(false);
                bundle = null;
            }   
        }

        /// <summary>
        /// 销毁资源
        /// </summary>
        void OnDestroy() {
            if (shared != null) shared.Unload(true);
            Debug.Log("~ResourceManager was destroy!");
        }
    }
}