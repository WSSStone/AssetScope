using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetScope
{
	[CreateAssetMenu(fileName = "AssetManagementSettings", menuName = "AssetScope/AssetManagementSettings", order = 5)]
	public class AssetManagementConfig : ScriptableObject
	{
		public string[] PrefabDirs = null;
		public string PrefabAssetInfoPath = "Packages/com.migu.assetscope/Editor/Assets/PrefabAssetInfo.asset";
		public string MeshTextureMappingPath = "Packages/com.migu.assetscope/Editor/Assets/MeshTextureMapping.asset";
		public string AlbedoName = "_BaseMap";
		public string NormalName = "_BumpMap";
		public string MaskName = "_MetallicGlossMap";
		public string[] ModelRanges = { "Assets/MetaMeeting/Models/Scenes/PortalScene/7/MCC_QYH" };
		public string[] OldPrefix = null;
		public string[] OldSuffix = null;
		public string MeshPrefix = "GR_MCC_QYH01_";
		public string MeshSuffix = "_0001";
		public string ExportDir = string.Empty;
	}
}