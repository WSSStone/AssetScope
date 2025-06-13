using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace AssetScope
{
    public class GUIUtils
	{
		
	}

	public class QYHGUIStyle
	{
		private static GUIStyle _headerStyle;
		public static GUIStyle HeaderStyle
		{
			get
			{
				if (_headerStyle == null)
				{
					_headerStyle = new GUIStyle();
					_headerStyle.fontSize = 14;
					_headerStyle.fontStyle = UnityEngine.FontStyle.Bold;
					_headerStyle.normal.textColor = UnityEngine.Color.white;
				}
				return _headerStyle;
			}
			private set => _headerStyle = value;
		}
	}

	public abstract class ReorderableListWrapper<T>
	{
		protected Func<T[]> _getter;
		protected Action<T[]> _setter;
		public T[] m_Items
		{
			get => _getter();
			set => _setter(value);
		}

		public ReorderableList m_List { get; protected set; }
		public string m_Label { get; set; } = string.Empty;
		public Vector2 m_ScrollPos { get; set; } = Vector2.zero;
		public int m_LabelWidth = 90;
		public int m_CountWidth = 40;

		public ReorderableListWrapper(Func<T[]> getter, Action<T[]> setter)
		{
			_getter = getter;
			_setter = setter;
		}

		public void InitList()
		{
			if (m_Items is null)
			{
				m_Items = new T[0];
			}

			m_List = new ReorderableList(m_Items, typeof(T), true, true, true, true);

			m_List.drawHeaderCallback = DrawHeaderCallBack;

			m_List.drawElementCallback = DrawElementCallBack;

			m_List.onAddCallback = OnAddCallback;

			m_List.onRemoveCallback = OnRemoveCallback;
		}

		protected virtual void DrawHeaderCallBack(Rect rect)
		{
			EditorGUI.LabelField(new Rect
			{
				xMin = rect.xMin,
				xMax = rect.xMax + m_LabelWidth,
				yMin = rect.yMin,
				yMax = rect.yMax
			}, m_Label);

			EditorGUI.IntField(new Rect
			{
				xMin = rect.xMin + m_LabelWidth,
				xMax = rect.xMin + m_LabelWidth + m_CountWidth,
				yMin = rect.yMin,
				yMax = rect.yMax
			}, m_Items.Length);
		}

		protected abstract void DrawElementCallBack(Rect rect, int index, bool isActive, bool isFocused);

		protected virtual void OnAddCallback(ReorderableList list)
		{
			var items = m_Items.ToList();
			items.Add(default);
			m_Items = items.ToArray();
			list.list = m_Items;
		}

		protected virtual void OnRemoveCallback(ReorderableList list)
		{
			if (m_Items.Length == 0) return;

			var newList = new T[m_Items.Length - 1];
			for (int i = 0, j = 0; i < m_Items.Length; i++)
			{
				if (i != list.index)
				{
					newList[j++] = m_Items[i];
				}
			}

			m_Items = newList;
			list.list = m_Items;
		}

		public void OnGUI(int Height = 300)
		{
			m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, GUILayout.Height(Height));
			if (m_List != null)
			{
				m_List.DoLayoutList();
			}
			EditorGUILayout.EndScrollView();
		}
	}

	public class StringListWrapper : ReorderableListWrapper<string>
	{
		public StringListWrapper(Func<string[]> getter, Action<string[]> setter)
		: base(getter, setter)
		{
		}

		protected override void DrawElementCallBack(Rect rect, int index, bool isActive, bool isFocused)
		{
			m_Items[index] = EditorGUI.TextField(rect, m_Items[index]);
		}
	}

	public class ObjectListWrapper<T> : ReorderableListWrapper<T> where T : UnityEngine.Object
	{
		public ObjectListWrapper(Func<T[]> getter, Action<T[]> setter)
		: base(getter, setter)
		{
		}

		protected override void DrawElementCallBack(Rect rect, int index, bool isActive, bool isFocused)
		{
			m_Items[index] = EditorGUI.ObjectField(
				rect,
				m_Items[index],
				typeof(T),
				false) as T;
		}
	}
}