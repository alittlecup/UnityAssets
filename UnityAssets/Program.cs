using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetStudioGUI;
using Editor;
using AssetsManager = AssetsTools.NET.Extra.AssetsManager;

namespace UnityAssets
{
    class Program
    {
        private static AssetsManager _assetsManager = new AssetsManager();

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            _assetsManager.LoadClassPackage(classDataTpkPath);
            //
            string prefix = "../../../source/Data/";
            string prefixpreFix = "../../../source/increase/";


            var needAddResourceAssetItem = GetNeedAddResourceAssetItem(prefix + "globalgamemanagers", new List<string>()
            {
                "image/other/imgdailybonustitlenew"
            });
            AddResourcesAssetsItemsToGlobalGameManagers(prefix + "globalgamemanagers-old",prefix+"globalgamemanagers-old-new" ,needAddResourceAssetItem);

            // var assetsManager = new AssetStudio.AssetsManager();
            // assetsManager.LoadFolder("../../../source/increase");
            // var assetsFileList = assetsManager.assetsFileList;
            // var oldAssets = assetsFileList[0];
            // var newAssets = assetsFileList[1];
            //
            // for (var i = 0; i < oldAssets.Objects.Count; i++)
            // {
            //     var oldAssetsObject = oldAssets.Objects[i];
            //     var newAssetsObject = newAssets.Objects[i];
            //     if (oldAssetsObject.byteSize != newAssetsObject.byteSize)
            //     {
            //         Console.WriteLine("diff: "+oldAssetsObject.type.ToString());
            //     }
            // }
            var loadAllAssetTypeValueField = LoadAllAssetTypeValueField(prefix+"globalgamemanagers-old-new");
            Print(loadAllAssetTypeValueField);
            using (var writer = File.CreateText(prefix + "logs.txt"))
            {
                foreach (var log in logs)
                {
                    writer.WriteLine(log);
                }
            }
        }

        private static void LoadGlobalGameManagers(string sourcePath)
        {
            var ggm = _assetsManager.LoadAssetsFile(sourcePath, false);
            _assetsManager.LoadClassDatabaseFromPackage(ggm.file.typeTree.unityVersion);

            //Add assetTypeValueFields
            var resourcesManagerInfoEx = ggm.table.GetAssetsOfType((int) AssetClassID.ResourceManager)[0];
            var resourcesManagerBaseField = _assetsManager.GetTypeInstance(ggm, resourcesManagerInfoEx).GetBaseField();
        }

        private static List<string> logs = new List<string>();

        private static void Print(AssetTypeValueField[] assetTypeValueFields, string preFix = "")
        {
            var names = new List<string>();
            foreach (var data in assetTypeValueFields)
            {
                // PrintAssetField(data, preFix);
                var name = data[0].GetValue().AsString();
                var pathId = data[1].Get("m_PathID").GetValue().AsInt64();
                var fileID = data[1].Get("m_FileID").GetValue().AsInt();
                names.Add(fileID + "-" + pathId + "-" + name);
            }

            names.Sort((x, y) => int.Parse(x.Split("-")[0]).CompareTo(int.Parse(y.Split("-")[0])));
            foreach (var name in names)
            {
                Console.WriteLine("name: " + name);
                logs.Add("name: " + name);
            }
        }

        private static void PrintAssetField(AssetTypeValueField data, string preFix = "")
        {
            var name = data[0].GetValue().AsString();
            var pathId = data[1].Get("m_PathID").GetValue().AsInt64();
            var fileID = data[1].Get("m_FileID").GetValue().AsInt();
            Console.WriteLine($"{preFix} in resources.assets, fileid {fileID} pathid {pathId} = {name}");
        }

        private static string classDataTpkPath = "../../../assets/classdata.tpk";

        private static AssetTypeValueField[] LoadAllAssetTypeValueField(string sourcePath)
        {
            var am = new AssetsManager();
            am.LoadClassPackage(classDataTpkPath);
            var ggm = am.LoadAssetsFile(sourcePath, false);
            am.LoadClassDatabaseFromPackage(ggm.file.typeTree.unityVersion);

            var resourcesManagerInfoEx = ggm.table.GetAssetsOfType((int) AssetClassID.ResourceManager)[0];
            var resourcesManagerBaseField = am.GetTypeInstance(ggm, resourcesManagerInfoEx).GetBaseField();
            var resourcesContainerArray = resourcesManagerBaseField.Get("m_Container").Get("Array");
            var assetsFileDependencies = ggm.file.dependencies.dependencies;
            for (var i = 0; i < assetsFileDependencies.Count; i++)
            {
                var assetsFileDependency = assetsFileDependencies[i];
                Console.WriteLine("Dependencies: " + assetsFileDependency.assetPath + " - " + i);
                logs.Add("Dependencies: " + assetsFileDependency.assetPath + " - " + i);
            }

            return resourcesContainerArray.children;
        }

        private static void AddAssetsFileDependencyList(string sourcePath, List<AssetsFileDependency> dependencies)
        {
            var am = new AssetsManager();
            am.LoadClassPackage(classDataTpkPath);
            var ggm = am.LoadAssetsFile(sourcePath, false);
            am.LoadClassDatabaseFromPackage(ggm.file.typeTree.unityVersion);
            var assetsReplacers = new List<AssetsReplacer>();
            var assetsFileDependencyList = new AssetsFileDependencyList();
            var oldDependencies = ggm.file.dependencies;
            var fileDependencies = new List<AssetsFileDependency>(oldDependencies.dependencies);
            fileDependencies.AddRange(dependencies);
            assetsFileDependencyList.dependencies = fileDependencies;
            assetsFileDependencyList.dependencyCount = fileDependencies.Count;
            ggm.file.dependencies = assetsFileDependencyList;

            using (AssetsFileWriter writer = new AssetsFileWriter(File.OpenWrite(sourcePath + "-new")))
            {
                ggm.file.Write(writer, 0, assetsReplacers, 0);
            }
        }

        private static void AddResourceValueFieldsToAssets(string sourcePath, AssetTypeValueField[] fields)
        {
            var am = new AssetsManager();
            am.LoadClassPackage(classDataTpkPath);
            var ggm = am.LoadAssetsFile(sourcePath, false);
            am.LoadClassDatabaseFromPackage(ggm.file.typeTree.unityVersion);

            var resourcesManagerInfoEx = ggm.table.GetAssetsOfType((int) AssetClassID.ResourceManager)[0];
            var resourcesManagerBaseField = am.GetTypeInstance(ggm, resourcesManagerInfoEx).GetBaseField();
            var resourcesContainerArray = resourcesManagerBaseField.Get("m_Container").Get("Array");
            var assetTypeValueFields = resourcesContainerArray.children;
            var list = new List<AssetTypeValueField>();
            bool hasAdd = false;

            resourcesContainerArray.SetChildrenList(list.ToArray());

            var assetsReplacers = new List<AssetsReplacer>();
            assetsReplacers.Add(new AssetsReplacerFromMemory(0, resourcesManagerInfoEx.index,
                (int) resourcesManagerInfoEx.curFileType, 0xffff, resourcesManagerBaseField.WriteToByteArray()));

            using (AssetsFileWriter writer = new AssetsFileWriter(File.OpenWrite(sourcePath + "-new")))
            {
                ggm.file.Write(writer, 0, assetsReplacers, 0);
            }
        }


        private static List<ResourceAssetItem> GetNeedAddResourceAssetItem(string addSourcePath, List<string> fileNames)
        {
            if (fileNames == null || fileNames.Count == 0)
            {
                return new List<ResourceAssetItem>();
            }

            var ggm = _assetsManager.LoadAssetsFile(addSourcePath, false);
            _assetsManager.LoadClassDatabaseFromPackage(ggm.file.typeTree.unityVersion);

            var resourcesManagerInfoEx = ggm.table.GetAssetsOfType((int) AssetClassID.ResourceManager)[0];
            var resourcesManagerBaseField = _assetsManager.GetTypeInstance(ggm, resourcesManagerInfoEx).GetBaseField();

            var resourcesContainerArray = resourcesManagerBaseField.Get("m_Container").Get("Array").children;
            var assetsFileDependencies = ggm.file.dependencies.dependencies;
            var resourcesItems = new List<ResourceAssetItem>();
            var listTypeValueField = new List<AssetTypeValueField>();
            var needAddAssetTypeValueFile = new List<AssetTypeValueField>();
            for (var i = 0; i < resourcesContainerArray.Length; i++)
            {
                var assetTypeValueField = resourcesContainerArray[i];
                var name = assetTypeValueField.GetFileName();
                if (fileNames.Contains(name))
                {
                    needAddAssetTypeValueFile.Add(assetTypeValueField);
                }
            }

            for (var i = 0; i < needAddAssetTypeValueFile.Count; i++)
            {
                var assetTypeValueField = needAddAssetTypeValueFile[i];
                var fileID = assetTypeValueField.GetFileID();
                if (i == 0)
                {
                    listTypeValueField.Add(assetTypeValueField);
                }
                else
                {
                    var preAssetTypeValueField = needAddAssetTypeValueFile[i - 1];
                    if (fileID == preAssetTypeValueField.GetFileID())
                    {
                        listTypeValueField.Add(assetTypeValueField);
                    }
                    else
                    {
                        var dependenciesIndex = preAssetTypeValueField.GetFileID() - 1;
                        var assetsFileDependency = assetsFileDependencies[dependenciesIndex];
                        var resourceAssetItem = new ResourceAssetItem(assetsFileDependency,
                            new List<AssetTypeValueField>(listTypeValueField));
                        resourcesItems.Add(resourceAssetItem);
                        listTypeValueField.Clear();
                        listTypeValueField.Add(assetTypeValueField);
                    }
                }

                if (i == needAddAssetTypeValueFile.Count - 1)
                {
                    var dependenciesIndex = assetTypeValueField.GetFileID() - 1;
                    var assetsFileDependency = assetsFileDependencies[dependenciesIndex];
                    var resourceAssetItem = new ResourceAssetItem(assetsFileDependency,
                        new List<AssetTypeValueField>(listTypeValueField));
                    resourcesItems.Add(resourceAssetItem);
                    listTypeValueField.Clear();
                }
            }

            _assetsManager.UnloadAssetsFile(addSourcePath);
            return resourcesItems;
        }

        private static void AddResourcesAssetsItemsToGlobalGameManagers(string oldSourcePath, string dstSourcePath,
            List<ResourceAssetItem> items)
        {
            var ggm = _assetsManager.LoadAssetsFile(oldSourcePath, false);
            _assetsManager.LoadClassDatabaseFromPackage(ggm.file.typeTree.unityVersion);

            //Add assetTypeValueFields
            var resourcesManagerInfoEx = ggm.table.GetAssetsOfType((int) AssetClassID.ResourceManager)[0];
            var resourcesManagerBaseField = _assetsManager.GetTypeInstance(ggm, resourcesManagerInfoEx).GetBaseField();

            var resourcesContainerArray = resourcesManagerBaseField.Get("m_Container").Get("Array");
            var assetTypeValueFields = resourcesContainerArray.children;
            var list = new List<AssetTypeValueField>();
            var lastFileID = assetTypeValueFields.Last().GetFileID();
            var lastFileDependenciesIndex = lastFileID - 1;

            foreach (var assetTypeValueField in assetTypeValueFields)
            {
                list.Add(assetTypeValueField);
            }

            foreach (var resourceAssetItem in items)
            {
                lastFileID += 1;
                lastFileDependenciesIndex += 1;
                foreach (var assetTypeValueField in resourceAssetItem.Fields)
                {
                    assetTypeValueField.SetFileID(lastFileID);
                    list.Add(assetTypeValueField);
                }

                ggm.file.dependencies.dependencies.Insert(lastFileDependenciesIndex, resourceAssetItem.Dependency);
                ggm.file.dependencies.dependencyCount += 1;
            }

            resourcesContainerArray.SetChildrenList(list.ToArray());

            var assetsReplacers = new List<AssetsReplacer>();
            assetsReplacers.Add(new AssetsReplacerFromMemory(0, resourcesManagerInfoEx.index,
                (int) resourcesManagerInfoEx.curFileType, 0xffff, resourcesManagerBaseField.WriteToByteArray()));

            using (AssetsFileWriter writer = new AssetsFileWriter(
                File.OpenWrite(dstSourcePath)))
            {
                ggm.file.Write(writer, 0, assetsReplacers, 0);
            }

            _assetsManager.UnloadAssetsFile(oldSourcePath);
        }


        public static bool ExportRawFile(AssetItem item, string exportPath)
        {
            if (!TryExportFile(exportPath, item, ".dat", out var exportFullPath))
                return false;
            File.WriteAllBytes(exportFullPath, item.Asset.GetRawData());
            return true;
        }

        private static bool TryExportFile(string dir, AssetItem item, string extension, out string fullPath)
        {
            var fileName = "";
            fullPath = Path.Combine(dir, fileName + extension);
            if (!File.Exists(fullPath))
            {
                Directory.CreateDirectory(dir);
                return true;
            }

            fullPath = Path.Combine(dir, fileName + item.UniqueID + extension);
            if (!File.Exists(fullPath))
            {
                Directory.CreateDirectory(dir);
                return true;
            }

            return false;
        }
    }
}