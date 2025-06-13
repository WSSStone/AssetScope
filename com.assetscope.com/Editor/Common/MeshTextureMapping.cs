using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace AssetScope
{
	[CreateAssetMenu(fileName = "MeshTextureMapping", menuName = "AssetScope/Mesh-Textures Mapping", order = 3)]
	public class MeshTextureMapping : ScriptableObject
	{
		public MeshTextureTuple[] m_Mapping;
	}

	[Serializable]
	public class MeshTextureTuple
	{
		public string m_Path;
		public string m_Guid;
		public long m_FileID;
		public GameObject m_Asset;
		public PBRTextureSet[] TextureSets;

		public override int GetHashCode()
		{
			return m_Path == null ? 0 : m_Path.GetHashCode();
		}
	}

	[Serializable]
	public class PBRTextureSet
	{
		public Texture2D Albedo;
		public Texture2D Normal;
		public Texture2D Mask;
	}
}