/****************************************************
*	File：AssetsManagementWindow.cs
*	Auth：Wang Shi
*	Mail：wangshi@migu.chinamobile.com
*	Date：2025/06/03 10:44
*	Desc：Batch Rename Texture Assets.
*****************************************************/

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;

namespace AssetScope
{
	public class AssetsManagementWindow : EditorWindow
	{
		private static readonly string s_title = "资产整理";

		private int _listMinHeight = 150;

		private ReorderableListWrapper<string> _folderList;
		private ReorderableListWrapper<string> _modelRangeList;

		private AssetManageHelper m_AssetHelper = null;
		private ReorderableListWrapper<string> _prefixList;
		private ReorderableListWrapper<string> _suffixList;

		private string _fbxDir = "Assets/MetaMeeting/Models/Scenes/PortalScene/7/MCC_QYH";

		[MenuItem("TA_Tools/全运会/资产整理")]
		static void Init()
		{
			AssetsManagementWindow window = EditorWindow.GetWindow<AssetsManagementWindow>(s_title);
			window.Show();
			window.Focus();
		}

		private void OnEnable()
		{
			m_AssetHelper = new AssetManageHelper();

			_folderList = new StringListWrapper(
				() => m_AssetHelper.m_PrefabDirs,
				v => m_AssetHelper.m_PrefabDirs = v);
			_folderList.m_Label = "文件夹";
			_folderList.InitList();

			_modelRangeList = new StringListWrapper(
				() => m_AssetHelper.m_ModelRanges,
				v => m_AssetHelper.m_ModelRanges = v);
			_modelRangeList.m_Label = "模型范围";
			_modelRangeList.InitList();

			_prefixList = new StringListWrapper(
				() => m_AssetHelper.m_OldPrefix,
				v => m_AssetHelper.m_OldPrefix = v);
			_prefixList.m_Label = "旧前缀";
			_prefixList.InitList();

			_suffixList = new StringListWrapper(
				() => m_AssetHelper.m_OldSuffix,
				v => m_AssetHelper.m_OldSuffix = v);
			_suffixList.m_Label = "旧后缀";
			_suffixList.InitList();
		}

		void OnGUI()
		{
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Folder", QYHGUIStyle.HeaderStyle);
			EditorGUILayout.HelpBox("添加在Project窗口中选中的目标文件夹，提取需要重命名资源的预制体到列表中。", MessageType.Info);

			_folderList.OnGUI(GetListHeight(_folderList.m_Items, 1));

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("添加文件夹"))
			{
				AddFolders();
			}
			if (GUILayout.Button("清空列表"))
			{
				ClearFolders();
			}
			if (GUILayout.Button("提取预制体"))
			{
				ExtractPrefabFromFolders();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Assets", QYHGUIStyle.HeaderStyle);
			EditorGUILayout.HelpBox("从场景，或资产窗口添加目标预制体到列表中。", MessageType.Info);

			m_AssetHelper.m_PrefabInfo = EditorGUILayout.ObjectField("Prefab资产", m_AssetHelper.m_PrefabInfo, typeof(PrefabAssetInfo), false) as PrefabAssetInfo;

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("添加物体"))
			{
				AddPrefabObject();
			}
			if (GUILayout.Button("清空列表"))
			{
				ClearPrefabLists();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			m_AssetHelper.m_MeshTextureMap = EditorGUILayout.ObjectField("待输出资产", m_AssetHelper.m_MeshTextureMap, typeof(MeshTextureMapping), false) as MeshTextureMapping;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			GUILayout.Label("Diffuse名称");
			m_AssetHelper.m_AlbedoName = 
				EditorGUILayout.TextField(m_AssetHelper.m_AlbedoName);
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical();
			GUILayout.Label("Normal名称");
			m_AssetHelper.m_NormalName =
				EditorGUILayout.TextField(m_AssetHelper.m_NormalName);
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical();
			GUILayout.Label("Mask名称");
			m_AssetHelper.m_MaskName =
				EditorGUILayout.TextField(m_AssetHelper.m_MaskName);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("查找依赖纹理"))
			{
				m_AssetHelper.GenerateMapping();
			}
			if (GUILayout.Button("清空依赖"))
			{
				m_AssetHelper.ClearMeshTexMap();
			}
			EditorGUILayout.EndHorizontal();

			_modelRangeList.OnGUI(GetListHeight(_modelRangeList.m_Items, 1));
			if (GUILayout.Button("移除多余模型"))
			{
				m_AssetHelper.Prune(_modelRangeList.m_Items);
			}

			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal();
			_prefixList.OnGUI(GetListHeight(_prefixList.m_Items, 1));
			_suffixList.OnGUI(GetListHeight(_suffixList.m_Items, 1));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			m_AssetHelper.m_MeshNameRule.Prefix = EditorGUILayout.TextField("Mesh前缀", m_AssetHelper.m_MeshNameRule.Prefix);
			m_AssetHelper.m_MeshNameRule.SuffixRule = (ESUFFIXRULE)EditorGUILayout.EnumPopup("后缀规则", m_AssetHelper.m_MeshNameRule.SuffixRule);
			using (new EditorGUI.DisabledScope(m_AssetHelper.m_MeshNameRule.SuffixRule == ESUFFIXRULE.None))
			{
				m_AssetHelper.m_MeshNameRule.SuffixPad = EditorGUILayout.IntField("后缀填位", m_AssetHelper.m_MeshNameRule.SuffixPad);
			}
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("重命名Mesh"))
			{
				m_AssetHelper.RenameMeshes();
			}

			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("输出路径", m_AssetHelper.m_ExportDir);
			if (GUILayout.Button("选择路径"))
			{
				string path = EditorUtility.OpenFolderPanel("选择输出路径", m_AssetHelper.m_ExportDir, "");
				if (!string.IsNullOrEmpty(path))
				{
					m_AssetHelper.m_ExportDir = path;
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			m_AssetHelper.m_MaterialNameRule.Prefix = EditorGUILayout.TextField("材质前缀", m_AssetHelper.m_MaterialNameRule.Prefix);
			m_AssetHelper.m_MaterialNameRule.SuffixRule = (ESUFFIXRULE)EditorGUILayout.EnumPopup("后缀规则", m_AssetHelper.m_MaterialNameRule.SuffixRule);
			using (new EditorGUI.DisabledScope(m_AssetHelper.m_MaterialNameRule.SuffixRule == ESUFFIXRULE.None))
			{
				m_AssetHelper.m_MaterialNameRule.SuffixPad = EditorGUILayout.IntField("后缀填位", m_AssetHelper.m_MaterialNameRule.SuffixPad);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			m_AssetHelper.m_TextureNameRule.Prefix = EditorGUILayout.TextField("贴图前缀", m_AssetHelper.m_TextureNameRule.Prefix);
			m_AssetHelper.m_TextureNameRule.SuffixRule = (ESUFFIXRULE)EditorGUILayout.EnumPopup("后缀规则", m_AssetHelper.m_TextureNameRule.SuffixRule);
			using (new EditorGUI.DisabledScope(m_AssetHelper.m_TextureNameRule.SuffixRule == ESUFFIXRULE.None))
			{
				m_AssetHelper.m_TextureNameRule.SuffixPad = EditorGUILayout.IntField("后缀填位", m_AssetHelper.m_TextureNameRule.SuffixPad);
			}
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("输出资产"))
			{
				m_AssetHelper.Export();
			}
		}

		void AddFolders()
		{
			var selectedFolders = Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets);

			HashSet<string> folderSet = new HashSet<string>(_folderList.m_Items);
			foreach (var folder in selectedFolders)
			{
				string path = AssetDatabase.GetAssetPath(folder);
				if (AssetDatabase.IsValidFolder(path))
				{
					folderSet.Add(path);
				}
			}
			_folderList.m_Items = folderSet.ToArray();
			_folderList.m_List.list = _folderList.m_Items;
		}

		void ClearFolders()
		{
			_folderList.m_Items = new string[0];
			_folderList.m_List.list = _folderList.m_Items;
		}

		void ExtractAssetFromFolders<T>(string Filter, ref ReorderableListWrapper<T> listWrapper) where T : UnityEngine.Object
		{
			var list = _folderList.m_Items;
			var set = new HashSet<T>(listWrapper.m_Items);
			foreach (var folder in list)
			{
				var assetGUIDs = AssetDatabase.FindAssets($"t:{Filter}", new[] { folder });
				var assets = assetGUIDs.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
					.Select(path => AssetDatabase.LoadAssetAtPath<T>(path))
					.Where(asset => asset != null).ToArray();

				foreach (var asset in assets)
				{
					Debug.Log($"Extract {asset}");
					set.Add(asset);
				}
			}
			listWrapper.m_Items = set.ToArray();
			listWrapper.m_List.list = listWrapper.m_Items;
		}

		void ExtractPrefabFromFolders()
		{
			var list = _folderList.m_Items;
			foreach (var dir in list)
			{
				var prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { dir });
				var prefabs = prefabGUIDs.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
					.Select(path => AssetDatabase.LoadAssetAtPath<GameObject>(path))
					.Where(prefab => prefab != null && (PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Regular ||
						PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Model ||
						PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Variant)).ToArray();

				foreach (var prefab in prefabs)
				{
					Debug.Log($"Extract {prefab}");
					m_AssetHelper.TryAdd(prefab);
				}
			}
		}

		void AddPrefabObject()
		{
			if (m_AssetHelper.m_PrefabInfo == null) return;

			foreach (var obj in Selection.gameObjects)
			{
				m_AssetHelper.TryAdd(obj);
			}
		}

		void ClearPrefabLists()
		{
			m_AssetHelper.ClearPrefab();
		}

		private int GetListHeight(System.Array array, int times = 1)
		{
			return Mathf.Max(70,
				Mathf.Min(_listMinHeight, array == null ? 0 : array.Length * 50 * times));
		}
	}

	public class AssetManageHelper : PrefabHelper
	{
		private static string s_ConfigPath
		{
			get
			{
				var guids = AssetDatabase.FindAssets("AssetManagementSettings t:AssetManagementConfig");
				if (guids.Length > 0)
					return AssetDatabase.GUIDToAssetPath(guids[0]);
				// fallback to default
				return "Packages/com.migu.assetscope/Editor/Assets/AssetManagementSettings.asset";
			}
		}
		public AssetManagementConfig m_Config;
		public MeshTextureMapping m_MeshTextureMap;

		public string[] m_PrefabDirs
		{
			get => m_Config.PrefabDirs;
			set
			{
				m_Config.PrefabDirs = value;
				EditorUtility.SetDirty(m_Config);
			}
		}

		public string m_AlbedoName
		{
			get => m_Config.AlbedoName;
			set
			{
				m_Config.AlbedoName = value;
				EditorUtility.SetDirty(m_Config);
			}
		}

		public string m_NormalName
		{
			get => m_Config.NormalName;
			set
			{
				m_Config.NormalName = value;
				EditorUtility.SetDirty(m_Config);
			}
		}

		public string m_MaskName
		{
			get => m_Config.MaskName;
			set
			{
				m_Config.MaskName = value;
				EditorUtility.SetDirty(m_Config);
			}
		}

		public string[] m_ModelRanges
		{
			get => m_Config.ModelRanges;
			set
			{
				m_Config.ModelRanges = value;
				EditorUtility.SetDirty(m_Config);
			}
		}

		public RenameRule m_MeshNameRule
		{
			get => m_Config.MeshRenameRule;
			set
			{
				m_Config.MeshRenameRule = value;
				EditorUtility.SetDirty(m_Config);
			}
		}

		public RenameRule m_MaterialNameRule
		{
			get => m_Config.MaterialRenameRule;
			set
			{
				m_Config.MaterialRenameRule = value;
				EditorUtility.SetDirty(m_Config);
			}
		}

		public RenameRule m_TextureNameRule
		{
			get => m_Config.TextureRenameRule;
			set
			{
				m_Config.TextureRenameRule = value;
				EditorUtility.SetDirty(m_Config);
			}
		}


		public string m_ExportDir
		{
			get => m_Config.ExportDir;
			set
			{
				m_Config.ExportDir = value;
				EditorUtility.SetDirty(m_Config);
			}
		}

		public string[] m_OldPrefix
		{
			get => m_Config.OldPrefix;
			set
			{
				m_Config.OldPrefix = value;
				EditorUtility.SetDirty(m_Config);
			}
		}

		public string[] m_OldSuffix
		{
			get => m_Config.OldSuffix;
			set
			{
				m_Config.OldSuffix = value;
				EditorUtility.SetDirty(m_Config);
			}
		}

		public AssetManageHelper()
		{
			m_Config = AssetDatabase.LoadAssetAtPath<AssetManagementConfig>(s_ConfigPath);
			if (m_Config == null)
			{
				m_Config = ScriptableObject.CreateInstance<AssetManagementConfig>();
				AssetDatabase.CreateAsset(m_Config, s_ConfigPath);
				AssetDatabase.Refresh();
			}

			m_PrefabInfo = AssetDatabase.LoadAssetAtPath<PrefabAssetInfo>(m_Config.PrefabAssetInfoPath);
			m_MeshTextureMap = AssetDatabase.LoadAssetAtPath<MeshTextureMapping>(m_Config.MeshTextureMappingPath);
		}

		public void GenerateMapping()
		{
			ClearMeshTexMap();

			var dict = new Dictionary<string, HashSet<Material>>();
			var mi_dict = new Dictionary<string, MeshInfo>();

			foreach (var prefabAsset in m_PrefabInfo.m_PrefabAssets)
			{
				foreach (var meshInfo in prefabAsset.m_Meshes)
				{
					HashSet<Material> matSet;
					
					if (!dict.TryGetValue(meshInfo.m_AssetPath, out matSet))
					{
						matSet = new HashSet<Material>();
					}

					foreach (var materialInfo in meshInfo.m_Materials)
					{
						if (materialInfo.m_Material == null ||
							!materialInfo.m_Material.HasProperty(m_AlbedoName) ||
							!materialInfo.m_Material.HasProperty(m_NormalName) ||
							!materialInfo.m_Material.HasProperty(m_MaskName))
							continue;

						matSet.Add(materialInfo.m_Material);
					}

					dict[meshInfo.m_AssetPath] = matSet;
					mi_dict[meshInfo.m_AssetPath] = meshInfo;
				}
			}

			var mapTupleList = new List<MeshTextureTuple>();
			foreach (var kvp in dict)
			{
				var meshInfo = mi_dict[kvp.Key];
				var materials = kvp.Value.ToList();

				if (meshInfo == null || materials.Count == 0) continue;

				MeshTextureTuple tuple = new MeshTextureTuple();
				tuple.m_Path = ToAbsPath(meshInfo.m_AssetPath);
				tuple.m_Asset = AssetDatabase.LoadAssetAtPath<GameObject>(meshInfo.m_AssetPath);
				if (tuple.m_Asset == null)
				{
					Debug.LogWarning($"Mesh asset at path {tuple.m_Path} is null.");
					continue;
				}
				AssetDatabase.TryGetGUIDAndLocalFileIdentifier(tuple.m_Asset, out tuple.m_Guid, out tuple.m_FileID);

				var textureSetList = new List<PBRTextureSet>();
				for (int i = 0; i < materials.Count; ++i)
				{
					var material = materials[i];

					var textureSet = new PBRTextureSet();
					textureSet.Albedo = material.GetTexture(m_AlbedoName) as Texture2D;
					textureSet.Normal = material.GetTexture(m_NormalName) as Texture2D;
					textureSet.Mask = material.GetTexture(m_MaskName) as Texture2D;

					textureSetList.Add(textureSet);
				}

				tuple.TextureSets = textureSetList.ToArray();
				mapTupleList.Add(tuple);
			}
			m_MeshTextureMap.m_Mapping = mapTupleList.ToArray();
			SortMap();
			EditorUtility.SetDirty(m_MeshTextureMap);
		}

		public void ClearPrefab()
		{
			m_PrefabSet.Clear();
			m_PrefabInfo.m_PrefabAssets = new PrefabAsset[0];

			EditorUtility.SetDirty(m_PrefabInfo);
		}

		public void Prune(string[] Dirs)
		{
			m_MeshTextureMap.m_Mapping = m_MeshTextureMap.m_Mapping
				.Where(
				o =>
				{
					foreach (var dir in Dirs)
					{
						if (o.m_Path.Contains(dir)) return true;
					}

					return false;
				}).ToArray();

			EditorUtility.SetDirty(m_MeshTextureMap);
		}

		public void ClearMeshTexMap()
		{
			m_MeshTextureMap.m_Mapping = new MeshTextureTuple[0];

			EditorUtility.SetDirty(m_MeshTextureMap);
		}

		public void RenameMeshes()
		{
			m_OldPrefix = m_OldPrefix.Where(s => !string.IsNullOrEmpty(s)).OrderByDescending(s => s.Length).ToArray();
			m_OldSuffix = m_OldSuffix.Where(s => !string.IsNullOrEmpty(s)).OrderByDescending(s => s.Length).ToArray();

			foreach (var meshTuple in m_MeshTextureMap.m_Mapping)
			{
				var oldname = meshTuple.m_Asset.name;

				var newname = NamingUtils.RemovePrefix(m_OldPrefix, oldname);
				newname = NamingUtils.RemoveSuffix(m_OldSuffix, newname);

				(string main, string num) = NamingUtils.ExtractMainNameAndNumber(newname);

				string suf = "";
				string number = "1";

				Action createSuffix = () => {
					if (m_MeshNameRule.SuffixRule != ESUFFIXRULE.None)
					{
						number = string.IsNullOrEmpty(num) ? number : int.Parse(num).ToString();
						number = number.PadLeft(m_MeshNameRule.SuffixPad, '0');
						suf = $"_{number}";
					}
				};

				createSuffix();

				newname = $"{m_MeshNameRule.Prefix}{main}{suf}";
				string res = AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(meshTuple.m_Asset), newname);

				number = "0";
				while (!string.IsNullOrEmpty(res))
				{
					Debug.Log($"Retry remame <b>{meshTuple.m_Asset}</b> because <b>{res}</b>");

					suf = "";
					createSuffix();
					newname = $"{m_MeshNameRule.Prefix}{main}{suf}";
					res = AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(meshTuple.m_Asset), newname);
				}
				
				meshTuple.m_Path = AssetDatabase.GetAssetPath(meshTuple.m_Asset);
				meshTuple.m_Asset.name = newname;
				EditorUtility.SetDirty(meshTuple.m_Asset);

				Debug.Log($"Renamed mesh {oldname} to {meshTuple.m_Asset.name}");
			}
		}

		public void Export()
		{
			if (string.IsNullOrEmpty(m_ExportDir))
			{
				Debug.LogWarning($"Invalid export directory [{m_ExportDir}]!");
				return;
			}

			if (!Directory.Exists(m_ExportDir))
			{
				var result = Directory.CreateDirectory(m_ExportDir);
				if (result == null || !result.Exists)
				{
					Debug.LogError($"Failed to create export directory {m_ExportDir}!");
					return;
				}
			}

			foreach (var meshTuple in m_MeshTextureMap.m_Mapping)
			{
				ExportMeshTextureTuple(meshTuple);
			}
			
			CleanUpTemps();
		}

		private void CleanUpTemps()
		{
			string tmpDir = Path.Combine("Assets", "Tmp");
			if (Directory.Exists(tmpDir))
			{
				Directory.Delete(tmpDir, true);
			}

			string tmpMeta = Path.Combine("Assets", "Tmp.meta");
			if (File.Exists(tmpMeta))
			{
				File.Delete(tmpMeta);
			}

			AssetDatabase.Refresh();
		}

		private void ExportMeshTextureTuple(MeshTextureTuple MeshTuple)
		{
			var meshPath = MeshTuple.m_Path;
			var path = Path.GetFileName(meshPath);
			var textureSets = MeshTuple.TextureSets;

			if (path == null)
			{
				Debug.LogWarning($"Invalid asset for {path}");
				return;
			}

			var meshDir = Path.Combine(m_ExportDir, path.Split('.').First());

			Debug.Log($"{meshPath}, {path}, {meshDir}, {m_ExportDir}");

			if (!Directory.Exists(meshDir))
			{
				Directory.CreateDirectory(meshDir);
			}

			var meshFileName = Regex.Replace(path, @"\.(fbx|FBX)$", ".fbx", RegexOptions.IgnoreCase);
			var destMeshPath = Path.Combine(meshDir, meshFileName);
			ExportMesh(meshPath, destMeshPath);
			Debug.Log($"Exported mesh {path} to {destMeshPath}");

			int len = textureSets.Length;
			for (int i = 0; i < len; ++i)
			{
				var textureSet = textureSets[i];
				ExportTextures(textureSet, meshDir, path.Split('.').First(), i);
			}

			AssetDatabase.Refresh();
		}

		private void ExportMesh(string MeshPath, string DstMeshPath)
		{
			var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(MeshPath);
			Debug.Log($"{fbxAsset}");
			if (fbxAsset == null) return;

			var inst = PrefabUtility.InstantiatePrefab(fbxAsset) as GameObject;

			var renderers = inst.GetComponentsInChildren<Renderer>();

			m_OldPrefix = m_OldPrefix.Where(s => !string.IsNullOrEmpty(s)).OrderByDescending(s => s.Length).ToArray();
			m_OldSuffix = m_OldSuffix.Where(s => !string.IsNullOrEmpty(s)).OrderByDescending(s => s.Length).ToArray();

			int order = 1;
			foreach (var renderer in renderers)
			{
				var materials = renderer.sharedMaterials;
				for (int i = 0; i < materials.Length; i++)
				{
					if (materials[i] == null) continue;

					// Create a new material with renamed name
					var oldMat = materials[i];
					var newMat = new Material(oldMat);

					var matname = inst.name;
					matname = NamingUtils.RemovePrefix(new string[] { m_MeshNameRule.Prefix }, matname);
					matname = NamingUtils.RemoveSuffix(new string[] { "_0*\\d+$" }, matname);

					var matsuffix = "";
					if (m_MaterialNameRule.SuffixRule != ESUFFIXRULE.None)
					{
						matsuffix = $"_{(order++).ToString().PadLeft(m_MaterialNameRule.SuffixPad, '0')}";
					}
					newMat.name = $"{m_MaterialNameRule.Prefix}_{matname}{matsuffix}";

					// Assign the new material
					materials[i] = newMat;

					// Save new material to Assets
					var matPath = $"Assets/ASSETSCOPE_TMP/MAT/{newMat.name}.mat";
					Directory.CreateDirectory("Assets/ASSETSCOPE_TMP");
					Directory.CreateDirectory("Assets/ASSETSCOPE_TMP/MAT");
					Directory.CreateDirectory("Assets/ASSETSCOPE_TMP/FBX");
					AssetDatabase.CreateAsset(newMat, matPath);
				}
				renderer.sharedMaterials = materials;
			}

			ModelExporter.ExportObject(DstMeshPath, inst);

			GameObject.DestroyImmediate(inst);
			AssetDatabase.Refresh();
			Debug.Log("Export complete: " + DstMeshPath);
		}

		private void ExportTextures(PBRTextureSet TextureSet, string MeshDir, string MeshName, int Index = -1)
		{
			(string meshName, string number) = NamingUtils.ExtractMainNameAndNumber(MeshName);

			string suf = "";
			if (m_TextureNameRule.SuffixRule != ESUFFIXRULE.None)
			{
				suf = $"_{(Index + 1).ToString().PadLeft(m_TextureNameRule.SuffixPad, '0')}";
			}

			string albedoFilename = $"{m_TextureNameRule.Prefix}_{meshName}{suf}_D";
			ExportTexture(TextureSet.Albedo, MeshDir, albedoFilename);

			string normalFilename = $"{m_TextureNameRule.Prefix}_{meshName}{suf}_N";
			ExportTexture(TextureSet.Normal, MeshDir, normalFilename);

			string maskFilename = $"{m_TextureNameRule.Prefix}_{meshName}{suf}_M";
			ExportTexture(TextureSet.Mask, MeshDir, maskFilename);
		}

		private void ExportTexture(Texture2D Texture, string Dir, string Filename)
		{
			if (Texture == null) return;

			string extension = Path.GetExtension(AssetDatabase.GetAssetPath(Texture));
			string destPath = Path.Combine(Dir, Filename + extension.ToLower());
			if (!File.Exists(destPath))
			{
				File.Copy(AssetDatabase.GetAssetPath(Texture), destPath, true);
				Debug.Log($"Exported texture {Texture.name} to {destPath}");
			}
			else
			{
				Debug.LogWarning($"Texture {Texture.name} already exists at {destPath}, skipping export.");
			}
		}

		private void SortMap()
		{
			m_MeshTextureMap.m_Mapping.OrderBy(
				tuple => tuple.m_Asset.name, StringComparer.OrdinalIgnoreCase).ToArray();
		}
	}
}