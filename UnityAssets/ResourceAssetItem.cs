using System.Collections.Generic;
using AssetsTools.NET;
using AssetStudio;

namespace UnityAssets
{
    public class ResourceAssetItem
    {
        public AssetsFileDependency Dependency { get; }
        public List<AssetTypeValueField> Fields { get; }

        public ResourceAssetItem(AssetsFileDependency dependency, List<AssetTypeValueField> fields)
        {
            Dependency = dependency;
            Fields = fields;
        }
    }
}