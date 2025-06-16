using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using UnityEditor;
using UnityEngine;

namespace AssetScope
{
	public class PrefabHelper
	{
		public PrefabAssetInfo m_PrefabInfo;

		public HashSet<string> m_PrefabSet = new HashSet<string>();

		public virtual bool TryAdd(GameObject inst)
		{
			var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(inst);
			if (string.IsNullOrEmpty(prefabPath))
			{
				Debug.LogWarning($"GameObject {inst.name} is not a prefab instance.");
				return false;
			}
			prefabPath = ToAbsPath(prefabPath);

			PrefabAsset asset = ConvertInstToPrefabAsset(inst);
			if (asset == null)
			{
				Debug.LogWarning($"Failed to extract info from {inst.name}.");
				return false;
			}

			var list = m_PrefabInfo.m_PrefabAssets.ToList();

			if (m_PrefabSet.Contains(prefabPath))
			{
				var info = m_PrefabInfo.m_PrefabAssets.First(o => o.m_PrefabPath == prefabPath);

				if (info != null) list.Remove(info);
			}

			list.Add(asset);

			m_PrefabInfo.m_PrefabAssets = list.ToArray();
			m_PrefabSet.Add(prefabPath);
			return true;
		}

		public void Parse(PrefabAssetInfo assetInfo)
		{
			m_PrefabSet.Clear();

			foreach (var unit in assetInfo.m_PrefabAssets)
			{
				m_PrefabSet.Add(unit.m_PrefabPath);
			}
		}

		public PrefabAsset ConvertInstToPrefabAsset(GameObject inst)
		{
			PrefabAsset ret = new PrefabAsset();

			ret.m_PrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(inst);
			ret.m_Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ret.m_PrefabPath);
			ret.m_PrefabPath = ToAbsPath(ret.m_PrefabPath);
			ret.m_Name = ret.m_PrefabPath.Split('/').Last();

			MeshFilter[] meshFilters = inst.GetComponentsInChildren<MeshFilter>(true);
			if (meshFilters.Length == 0)
			{
				Debug.LogWarning($"{ret.m_Prefab} has invalid mesh filters");
				return null;
			}

			var meshList = new List<MeshInfo>();
			for (int i = 0; i < meshFilters.Length; ++i)
			{
				MeshInfo meshInfo = new MeshInfo();
				meshInfo.m_Mesh = meshFilters[i].sharedMesh;
				if (meshInfo.m_Mesh == null) continue;

				meshInfo.m_AssetPath = AssetDatabase.GetAssetPath(meshInfo.m_Mesh);
				meshInfo.m_MeshPath = ToAbsPath(meshInfo.m_AssetPath) + '.' + meshInfo.m_Mesh.name;
				if (string.IsNullOrEmpty(meshInfo.m_MeshPath)) continue;

				meshInfo.m_Name = meshInfo.m_Mesh.name;

				Renderer renderer = meshFilters[i].GetComponent<Renderer>();
				if (renderer == null) continue;
				
				var matInfoList = new List<MaterialInfo>();
				for (int j = 0; j < renderer.sharedMaterials.Length; ++j)
				{
					MaterialInfo materialInfo = new MaterialInfo();
					materialInfo.m_Material = renderer.sharedMaterials[j];
					if (materialInfo.m_Material == null) continue;

					materialInfo.m_BaseColor = (Texture2D)materialInfo.m_Material.GetTexture("_BaseMap");
					if (materialInfo.m_BaseColor != null)
						materialInfo.m_BaseColorPath = ToAbsPath(AssetDatabase.GetAssetPath(materialInfo.m_BaseColor));

					materialInfo.m_Normal = (Texture2D)materialInfo.m_Material.GetTexture("_BumpMap");
					if (materialInfo.m_Normal != null)
						materialInfo.m_NormalPath = ToAbsPath(AssetDatabase.GetAssetPath(materialInfo.m_Normal));

					materialInfo.m_Mask = (Texture2D)materialInfo.m_Material.GetTexture("_MetallicGlossMap");
					if (materialInfo.m_Mask != null)
						materialInfo.m_MaskPath = ToAbsPath(AssetDatabase.GetAssetPath(materialInfo.m_Mask));

					materialInfo.m_Name = materialInfo.m_Material.name;

					matInfoList.Add(materialInfo);
				}

				if (matInfoList.Count == 0) continue;

				meshInfo.m_Materials = matInfoList.ToArray();
				meshList.Add(meshInfo);
			}

			if (meshList.Count == 0)
			{
				Debug.LogWarning($"{ret.m_Prefab} has no valid meshes");
				return null;
			}

			ret.m_Meshes = meshList.ToArray();

			// unload ret.m_Prefab
			ret.m_Prefab = null;

			return ret;
		}

		public string ToAbsPath(string relPath)
		{
			return Path.Combine(Application.dataPath, relPath.Substring("Assets/".Length)).Replace("\\", "/");
		}
	}
}