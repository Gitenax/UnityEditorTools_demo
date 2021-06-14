using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using Object = UnityEngine.Object;

namespace Editor.MeshStats.Controls.DataGrid
{
    public enum DataGridItemType
    {
        IntField,
        Label,
        ObjectField,
        Toogle,
        EditableToogle,
        PropertyField
    }
    
    public class DataGrid<T>
    {
        private static readonly Color s_lighterColor = Color.white * 0.3f;
        private static readonly Color s_darkerColor = Color.white * 0.1f;
        
        private MultiColumnHeader _multiColumnHeader;
        private MultiColumnHeaderState _multiColumnHeaderState;
        private DataGridColumn<T>[] _columns;
        private T[] _dataSource;
        
        private Rect _rect;
        private float _width;
        private float _height;
        private Vector2 _scrollPosition;
        private Rect _scrollViewArea;
        private string _searchString;
        
        public DataGrid(DataGridColumn<T>[] columns, T[] dataSource)
        {
            _columns = columns;
            _multiColumnHeaderState = new MultiColumnHeaderState(_columns);
            _multiColumnHeader = new MultiColumnHeader(_multiColumnHeaderState);
            _dataSource = dataSource;
            
            Initialize();
        }

        public event Action CellValueChanged;
        
        public float HeaderHeight { get; set; } = EditorGUIUtility.singleLineHeight;
        public float ItemHeight { get; set; } = EditorGUIUtility.singleLineHeight;
        
        public T[] DataSource
        {
            get => _dataSource;
            set
            {
                _dataSource = value;
                _multiColumnHeader.Repaint();
            }
        }
        
        public string SearchString
        {
            get => _searchString;
            set => _searchString = value;
        }
        
        private bool HasSearch => !string.IsNullOrEmpty(_searchString);
        
        public void OnGUI(Rect position)
        {
            _rect = position;
            _scrollViewArea = GUILayoutUtility.GetRect(0, float.MaxValue, _multiColumnHeader.height * 2, float.MaxValue);
            
            DrawHeader();
            
            var viewRect = new Rect(_rect)
            {
                xMax = _columns.Sum(column => column.width),
                height = _dataSource.Length * ItemHeight
            };
            
            var columnRectPrototype = _rect;
            columnRectPrototype.height = ItemHeight;
            
            BeginScroll(viewRect);
            {
                DrawData(columnRectPrototype);
            }
            EndScroll();
        }

        private void DrawData(Rect columnRect)
        {
            var rowRect = new Rect(columnRect);
            
            if (!HasSearch)
            {
                for (int row = 0; row < _dataSource.Length; row++)
                {
                    rowRect.y = ItemHeight * (row + 1) + _rect.y;
                    DrawLineBackground(rowRect, row);
                        
                    for (int column = 0; column < _columns.Length; column++)
                        DrawColumnItem(rowRect, column, row);
                }
            }
            else
            {
                var indices = GetFilteredRowsIndices();

                for (int row = 0; row < indices.Count; row++)
                {
                    rowRect.y = ItemHeight * (row + 1) + _rect.y;
                    DrawLineBackground(rowRect, row);
                        
                    for (int column = 0; column < _columns.Length; column++)
                        DrawColumnItem(rowRect, column, indices[row]);
                }
            }
        }
        
        private void Initialize()
        {
            for (int column = 0; column < _columns?.Length; column++)
            {
                _columns[column].autoResize = true;
                _columns[column].canSort = true;
            }
            
            _multiColumnHeader.visibleColumnsChanged += header => header.ResizeToFit();
            _multiColumnHeader.height = HeaderHeight;
            _multiColumnHeader.ResizeToFit();
        }
        
        private void DrawHeader()
        {
            var headerRect = _rect;
            headerRect.height = HeaderHeight;
            _multiColumnHeader.OnGUI(headerRect, 0f);

            _rect.y += HeaderHeight;
        }
        
        private void BeginScroll(Rect viewRect)
        {
            _scrollPosition = GUI.BeginScrollView( _scrollViewArea, _scrollPosition, viewRect, false, false );
        }

        private void EndScroll()
        {
            GUI.EndScrollView(true);
        }
        
        private void DrawLineBackground(Rect rowRect, int rowIndex)
        {
            // Draw a texture before drawing each of the fields for the whole row.
            if (rowIndex % 2 == 0)
                EditorGUI.DrawRect(rowRect, s_darkerColor);
            else
                EditorGUI.DrawRect(rowRect, s_lighterColor);
        }

        private void DrawColumnItem(Rect rowRect, int columnIndex, int dataIndex)
        {
            if (_multiColumnHeader.IsColumnVisible(columnIndex))
            {
                var visibleColumnIndex = _multiColumnHeader.GetVisibleColumnIndex(columnIndex);
                var columnRect = _multiColumnHeader.GetColumnRect(visibleColumnIndex);
                columnRect.y = rowRect.y;

                DrawSpecificField(_columns[visibleColumnIndex], visibleColumnIndex, rowRect, _dataSource[dataIndex]);
            }
        }

        private void DrawSpecificField(DataGridColumn<T> currentColumn, int columnIndex, Rect columnRect, T dataValue)
        {
            switch (currentColumn.ColumnType)
            {
                case DataGridItemType.Label:
                {
                    EditorGUI.LabelField(
                        _multiColumnHeader.GetCellRect(columnIndex, columnRect),
                        new GUIContent(currentColumn.GetValue(dataValue).ToString()),
                        currentColumn.CellStyle ?? GUI.skin.label); 
                }
                break;
                
                case DataGridItemType.IntField:
                {
                    EditorGUI.IntField(
                        _multiColumnHeader.GetCellRect(columnIndex, columnRect),
                        (int)currentColumn.GetValue(dataValue),
                        currentColumn.CellStyle ?? GUI.skin.textField);
                }
                break;
                
                case DataGridItemType.ObjectField:
                {
                    var fieldStyle = currentColumn.CellStyle;
                    var rectWithOffset = _multiColumnHeader.GetCellRect(columnIndex, columnRect);
                    
                    if(fieldStyle != null)
                    {
                        rectWithOffset.x += fieldStyle.padding.left;
                        rectWithOffset.y += fieldStyle.padding.top;
                        rectWithOffset.width -= fieldStyle.padding.left +  fieldStyle.padding.right;
                        rectWithOffset.height -= fieldStyle.padding.top +  fieldStyle.padding.bottom;
                    }
                    
                    EditorGUI.ObjectField(
                        rectWithOffset,
                        (Object)currentColumn.GetValue(dataValue),
                        currentColumn.ColumnValueType,
                        true);
                }
                break;

                case DataGridItemType.Toogle:
                case DataGridItemType.EditableToogle:
                {
                    bool cellValue = (bool) currentColumn.GetValue(dataValue);

                    var toggle = EditorGUI.Toggle(
                        _multiColumnHeader.GetCellRect(columnIndex, columnRect),
                        cellValue,
                        currentColumn.CellStyle ?? GUI.skin.toggle);

                    if (currentColumn.ColumnType == DataGridItemType.EditableToogle)
                    {
                        if(cellValue != toggle)
                            CellValueChanged?.Invoke();
                        
                        currentColumn.SetValue(dataValue, toggle);
                    }
                }
                break;

                default:
                {
                    EditorGUI.LabelField(
                        _multiColumnHeader.GetCellRect(columnIndex, columnRect),
                        new GUIContent(currentColumn.GetValue(dataValue).ToString())
                        /*GetStyleForColumn(columnIndex)*/); 
                }
                break;
            }
        }
        
        private List<int> GetFilteredRowsIndices()
        {
            var parsedData = ParseSearchString();
            var indices = new List<int>();

            for (int row = 0; row < _dataSource.Length; row++)
            {
                for (int column = 0; column < _columns.Length; column++)
                {
                    if (parsedData.Keys.Contains("int"))
                    {
                        if(_columns[column].GetValue(_dataSource[row]).ToString() == parsedData["int"].ToString())
                        {
                            indices.Add(row);
                            break;
                        }
                    }
                    else if (parsedData.Keys.Contains("intRange"))
                    {
                        int[] values = (int[])parsedData["intRange"];
                        var cellString = _columns[column].GetValue(_dataSource[row]).ToString();

                        if (int.TryParse(cellString, out int cellValue))
                        {
                            if (cellValue >= values[0] && cellValue <= values[1])
                            {
                                indices.Add(row);
                                break;
                            }
                        }
                    }
                    else if (parsedData.Keys.Contains("bool"))
                    {
                        if(_columns[column].GetValue(_dataSource[row]).ToString() == parsedData["bool"].ToString())
                        {
                            indices.Add(row);
                            break;
                        }
                    }
                    else if (parsedData.Keys.Contains("string"))
                    {
                        if(_columns[column].GetValue(_dataSource[row]).ToString().Contains(parsedData["string"].ToString()))
                        {
                            indices.Add(row);
                            break;
                        }
                    }
                }
            }
            return indices;
        }
        
        private Dictionary<string, object> ParseSearchString()
        {
            var results = new Dictionary<string, object>();
            
            // Парсинг простого числа
            if (int.TryParse(_searchString, out int intResult))
            {
                results.Add("int", intResult);
                return results;
            }
            
            // Парсинг логического значения
            if(bool.TryParse(_searchString, out bool boolResult))
            {
                results.Add("bool", boolResult);
                return results;
            }
            
            // Парсинг диапазона
            if (_searchString.Contains("-"))
            {
                int separators = new Regex("-").Matches(_searchString).Count;
                if (separators == 1)
                {
                    var strings = _searchString.Split('-');
                    
                    if(int.TryParse(strings[0], out int fromValue) && int.TryParse(strings[1], out int toValue))
                    {
                        results.Add("intRange", new [] { fromValue, toValue});
                        return results;
                    }
                }
            }
            
            // Обычная строка(поиск по имени)
            results.Add("string", _searchString);
            return results;
        }
    }
    
    [Serializable]
    public class DataGridColumn<T> : MultiColumnHeaderState.Column 
    {
        private string _propertyName;
        private DataGridItemType _columnType;
        private Type _propertyType;
        private GUIStyle _cellStyle;

        public DataGridColumn(string propertyName, DataGridItemType type)
        {
            _propertyName = propertyName;
            _columnType = type;
            
            var t = typeof(T);
            var propertyInfo = t.GetProperty(_propertyName);
            _propertyType = propertyInfo?.GetType();
        }

        public DataGridColumn(string propertyName, DataGridItemType type, GUIStyle cellStyle) 
            : this(propertyName, type)
        {
            _cellStyle = cellStyle;
        }

        public DataGridItemType ColumnType => _columnType;

        public Type ColumnValueType => _propertyType;

        public GUIStyle CellStyle => _cellStyle;

        public object GetValue(T sourceObject)
        {
            var t = sourceObject.GetType();
            var propertyInfo = t.GetProperty(_propertyName);
            return propertyInfo?.GetValue(sourceObject);
        }

        public void SetValue<U>(T sourceObject, U value)
        {
            var t = sourceObject.GetType();
            var property = t.GetProperty(_propertyName);
            property?.SetValue(sourceObject, value);
        }
    }
}