using System;
using System.Collections.Generic;
using System.Linq;
using Editor.MeshStats.Controls.DataGrid;
using Project.Editors.MeshStats.Controls;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Editors.MeshStats
{
    public class MeshStatsWindow : EditorWindow
    {
        private SearchField _searchField;
        private DataGridColumn<MeshInfo>[] _columns;
        private DataGrid<MeshInfo> _dataGrid;
        
        [MenuItem("Tools/Open MeshStats window")]
        private static void ShowWindow()
        {
            var window = GetWindow<MeshStatsWindow>("MeshStats");
            window.Show();
        }
        
        private void OnEnable()
        {
            if(_columns == null || _dataGrid == null)
                InitializeGrid();
            
            _searchField = new SearchField();
            
            EditorApplication.hierarchyChanged += () => _dataGrid.DataSource = GetMeshesFromScene();
            EditorSceneManager.sceneClosed += scene => _dataGrid.DataSource = Array.Empty<MeshInfo>();
        }
        
        
        private void OnGUI()
        {
            var rectOffset = DoToolbar();
            rectOffset.y += rectOffset.height;
            rectOffset.height += position.height;

            _dataGrid?.OnGUI(rectOffset);
        }

        private void InitializeGrid()
        {
            var objectColumnStyle = new GUIStyle()
            {
                padding = new RectOffset(3, 3, 3, 3),
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState()
                {
                    textColor = Color.white
                }
            };

            _columns = new []
            {
                new DataGridColumn<MeshInfo>("Mesh", DataGridItemType.ObjectField, objectColumnStyle) { headerContent = new GUIContent("Меш"), },
                new DataGridColumn<MeshInfo>("Vertices", DataGridItemType.Label) { headerContent = new GUIContent("Вертексы"), },
                new DataGridColumn<MeshInfo>("Polygons", DataGridItemType.Label) { headerContent = new GUIContent("Полигоны"), },
                new DataGridColumn<MeshInfo>("Count", DataGridItemType.Label) { headerContent = new GUIContent("Использовано"), },
                new DataGridColumn<MeshInfo>("TotalVertices", DataGridItemType.Label) { headerContent = new GUIContent("Сумма вертексов"), },
                new DataGridColumn<MeshInfo>("IsReadable", DataGridItemType.EditableToogle) { headerContent = new GUIContent("Редактируемо"), },
                new DataGridColumn<MeshInfo>("GenerateLightmap", DataGridItemType.EditableToogle) { headerContent = new GUIContent("UV Lightmap"), }
            };

            for (int i = 0; i < _columns.Length; i++)
            {
                _columns[i].autoResize = true;
                _columns[i].headerTextAlignment = TextAlignment.Center;
                _columns[i].allowToggleVisibility = false;
            }
            
            _dataGrid = new DataGrid<MeshInfo>(_columns, GetMeshesFromScene());
            _dataGrid.CellValueChanged += DataGridOnCellValueChanged;
            _dataGrid.ItemHeight = EditorGUIUtility.singleLineHeight + 5;
        }

        private void DataGridOnCellValueChanged()
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            foreach (var meshInfo in _dataGrid.DataSource)
            {
                meshInfo.Importer.SaveAndReimport();
            }
        }

        private Rect DoToolbar()
        {
            GUILayout.BeginHorizontal (EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            _dataGrid.SearchString = _searchField.OnToolbarGUI(_dataGrid.SearchString);
            GUILayout.EndHorizontal();

            return GUILayoutUtility.GetLastRect();
        }
        
        private MeshInfo[] GetMeshesFromScene()
        {
            var filters = FindObjectsOfType<MeshFilter>(true).ToList();
            return RemoveRepeating(filters);
        }

        private MeshInfo[] RemoveRepeating(List<MeshFilter> filters)
        {
            var info = new List<MeshInfo>();
            int repetitiveCount = 0;
            
            for (int i = 0; i < filters.Count; i++, repetitiveCount = 0)
            {
                var current = filters[i];
                
                for (int j = i + 1; j < filters.Count; j++)
                {
                    if (current.sharedMesh.CompareMesh(filters[j].sharedMesh))
                    {
                        filters.RemoveAt(j);
                        repetitiveCount++;
                        j--;
                    }
                }
                var importerForMesh = ModelImporterUtility.LoadImporterForMeshFilter(current);
                
                if(repetitiveCount > 0)
                    info.Add(new MeshInfo(current, importerForMesh, repetitiveCount));
                else
                    info.Add(new MeshInfo(current, importerForMesh));
            }

            return info.ToArray();
        }
    }
}