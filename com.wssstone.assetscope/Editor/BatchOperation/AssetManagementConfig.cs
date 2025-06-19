using System;
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

		public RenameRule MeshRenameRule = new RenameRule()
		{
			Prefix = "GR_MCC_QYH01_",
			SuffixRule = ESUFFIXRULE.Number,
			SuffixPad = 4,
		};

		public RenameRule MaterialRenameRule = new RenameRule()
		{
			Prefix = "MI_",
			SuffixRule = ESUFFIXRULE.Number,
			SuffixPad = 4,
		};

		public RenameRule TextureRenameRule = new RenameRule()
		{
			Prefix = "T_",
			SuffixRule = ESUFFIXRULE.Number,
			SuffixPad = 4,
		};

		public string ExportDir = string.Empty;
	}

	public enum ESUFFIXRULE
	{
		None,
		Number,
	}

	[Serializable]
	public class RenameRule
	{
		public string Prefix;
		public ESUFFIXRULE SuffixRule;
		public int SuffixPad;
	}
}