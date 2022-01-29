using System;
using AssetsTools.NET;

namespace Editor
{
    public static class AssetTypeValueFieldEx
    {
        public static int GetFileID(this AssetTypeValueField field)
        {
            return field[1].Get("m_FileID").GetValue().AsInt();
        }

        public static Int64 GetPathID(this AssetTypeValueField field)
        {
            return field[1].Get("m_PathID").GetValue().AsInt64();
        }

        public static string GetFileName(this AssetTypeValueField field)
        {
            return field[0].GetValue().AsString();
        }

        public static void SetFileID(this AssetTypeValueField field, int fileID)
        {
            field[1].Get("m_FileID").GetValue().Set(fileID);
        }
    }
}