using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace AssetScope
{
	[CreateAssetMenu(fileName = "TextureReferenceInfo", menuName = "AssetScope/Texture Reference Info", order = 2)]
	public class TextureReferenceInfo : ScriptableObject
	{
		public TextureReference[] m_TextureReferences;
    }

	[Serializable]
	public class TextureReference
	{
		public Texture2D m_Texture;
		public Mesh[] m_Meshes;

		public override int GetHashCode()
		{
			return m_Texture != null ? m_Texture.GetHashCode() : 0;
		}
	}
}