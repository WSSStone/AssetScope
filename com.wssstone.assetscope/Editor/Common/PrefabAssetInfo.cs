using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

using UnityEditor;
using UnityEngine;

namespace AssetScope
{
	[CreateAssetMenu(fileName = "PrefabAssetInfo", menuName = "AssetScope/Prefab Info", order = 3)]
	public class PrefabAssetInfo : ScriptableObject
    {
        public PrefabAsset[] m_PrefabAssets;
    }

    [Serializable]
    public class PrefabAsset
    {
        [JsonIgnore] public string m_Name;
        public string m_PrefabPath;
		[JsonIgnore] public GameObject m_Prefab;

		public MeshInfo[] m_Meshes;
	}

    [Serializable]
    public class MeshInfo
    {
        [JsonIgnore] public string m_Name;
		[JsonIgnore] public string m_AssetPath;
        public string m_MeshPath;
		[JsonIgnore] public Mesh m_Mesh;
        public MaterialInfo[] m_Materials;
    }

    [Serializable]
    public class MaterialInfo
    {
		[JsonIgnore] public string m_Name;

		[JsonIgnore] public Material m_Material;

        public string m_BaseColorPath;
        [JsonIgnore] public string m_BaseColorPropertyName = "_BaseMap";
		[JsonIgnore] public Texture2D m_BaseColor;

		public string m_NormalPath;
		[JsonIgnore] public string m_NormalPropertyName = "_BumpMap";
		[JsonIgnore] public Texture2D m_Normal;

        public string m_MaskPath;
		[JsonIgnore] public string m_MaskPropertyName = "_MaskMap";
		[JsonIgnore] public Texture2D m_Mask;
    }
}