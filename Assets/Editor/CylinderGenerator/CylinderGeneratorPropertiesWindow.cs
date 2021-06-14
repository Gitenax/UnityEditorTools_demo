using System;
using UnityEditor;
using UnityEngine;

namespace Project.Editors.CylinderGenerator
{
    public class CylinderGeneratorPropertiesWindow : EditorWindow
    {
        private static MeshCollider _meshCollider;
        private Cylinder _cylinder;
        
        [MenuItem("CONTEXT/MeshCollider/Generate cylinder", false, 1)]
        private static void ShowWindow(MenuCommand command)
        {
            _meshCollider = command.context as MeshCollider;
            
            var window = GetWindow<CylinderGeneratorPropertiesWindow>();
            window.titleContent = new GUIContent("Cylinder generator");
            window.Show();
        }

        private void OnEnable()
        {
            _cylinder = new Cylinder();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            _cylinder.Radius = EditorGUILayout.FloatField("Радиус", _cylinder.Radius);
            _cylinder.Height = EditorGUILayout.FloatField("Высота", _cylinder.Height);
            _cylinder.Edges = EditorGUILayout.IntField("Граней", _cylinder.Edges);

            
            var buttonRect = new Rect(GUILayoutUtility.GetLastRect());
            buttonRect.y += EditorGUIUtility.singleLineHeight * 2;
            buttonRect.height = EditorGUIUtility.singleLineHeight * 2;
            
            if (GUI.Button(buttonRect, "Сгенерировать"))
            {
                _meshCollider.sharedMesh = _cylinder.GenerateMesh();
            }
        }
    }
    
    public class Cylinder 
    {
        private float _radius = 1;
        private float _height = 5; 
        private int _edges = 8;
        private float _thetaStep;
        private MeshFilter _filter;
        private Mesh _mesh;

        public Cylinder()
        {
            _mesh = new Mesh();
        }
        
        public float Radius
        {
            get => _radius;
            set => _radius = value > 0 ? value : 0.01f;
        }

        public float Height
        {
            get => _height;
            set => _height = value > 0 ? value : 0.01f;
        }

        public int Edges
        {
            get => _edges;
            set => _edges = value > 2 ? value : 3;
        }

        public Mesh GenerateMesh()
        {
            // Получение величины угла для каждой грани
            _thetaStep = CalculateTheta(_edges);

            // Получение базовых значений вершин
            var vertices = CalculateCircleVertices();
            var triangles = GetCircleTriangles(vertices);
            var sideTriangles = GetSideTriangles(vertices);

            // Объединение массива треугольников окружностей и сторон
            Array.Resize(ref triangles, triangles.Length + sideTriangles.Length);
            sideTriangles.CopyTo(triangles, triangles.Length - sideTriangles.Length);
            
            // Установка высоты для каждой окружности
            SetCircleHeight(vertices);
         
            // Назначение вершин и треугольников мешу
            return UpdateMesh(vertices, triangles);
        }

        private float CalculateTheta(int edges)
        {
            return (2 * Mathf.PI) / edges;
        }

        private Vector3[] CalculateCircleVertices()
        {
            // +1 - средняя точка
            var vertices = new Vector3[(_edges + 1) * 2];
            int nextToIndex = _edges - 1;
            int index = 0;
            // Средняя точка для верхней окружности
            // Средняя точка для нижней идет последней и инициализируется в 0 по умолчанию
            vertices[index++] = Vector3.zero;  
            
            // Расчет точек окружности цилиндра
            for (int edge = 0; edge < _edges; edge++)
            {
                var x = Mathf.Cos(edge * _thetaStep) * _radius;
                var z = Mathf.Sin(edge * _thetaStep) * _radius; // z - т.к. в юнити ось Y смотри вверх
                vertices[index++] = new Vector3(x, 0, z);
                vertices[index + nextToIndex] = new Vector3(x, 0, z);
            }
           
            return vertices;
        }
        
        private int[] GetCircleTriangles(Vector3[] vertices)
        {
            int lastIndex = vertices.Length - 1;
            int availableVertices = vertices.Length - 2;
            int[] triangles = new int[availableVertices * 3];
            int index = 0;
            
            // Верхняя окружность
            for (int i = 1; i < availableVertices / 2; i++, index += 3)
            {
                triangles[index]     = i;
                triangles[index + 1] = 0;
                triangles[index + 2] = i + 1;
            }
            triangles[index]     = availableVertices / 2;
            triangles[index + 1] = 0;
            triangles[index + 2] = 1;
            index += 3;
            
            // нижняя окружность
            for (int i = availableVertices / 2 + 1; i < availableVertices; i++, index += 3)
            {
                triangles[index]     = i;
                triangles[index + 1] = i + 1;
                triangles[index + 2] = lastIndex;
            }
            triangles[index]     = availableVertices;
            triangles[index + 1] = availableVertices / 2 + 1;
            triangles[index + 2] = lastIndex;
            
            return triangles;
        }

        private int[] GetSideTriangles(Vector3[] vertices)
        {
            int availableVertices = vertices.Length - 2;        // Вычитаем средние точки окружностей
            int nextIndex = availableVertices / 2 + 1;          // Номер индекса с которого начинается счет на нижней окружности
            int[] triangles = new int[availableVertices * 3];
            int index = 0;
            
            // Верхние трегугольники
            for (int i = 1; i < availableVertices / 2; i++, index += 3)
            {
                triangles[index]     = i;
                triangles[index + 1] = i + 1;
                triangles[index + 2] = i + nextIndex;
            }
            triangles[index]     = nextIndex - 1;
            triangles[index + 1] = 1;
            triangles[index + 2] = nextIndex;
            index += 3;
            
            // нижние трегугольники
            for (int i = availableVertices / 2 + 1; i < availableVertices; i++, index += 3)
            {
                triangles[index]     = i + 1 - nextIndex;
                triangles[index + 1] = i + 1;
                triangles[index + 2] = i;
            }
            triangles[index]     = availableVertices / 2;
            triangles[index + 1] = availableVertices / 2 + 1;
            triangles[index + 2] = availableVertices;

            return triangles;
        }
        
        private void SetCircleHeight(Vector3[] vertices)
        {
            for (int i = 0; i < vertices.Length / 2; i++)
                vertices[i] += Vector3.up * _height / 2;
            
            for (int i = vertices.Length / 2; i < vertices.Length; i++)
                vertices[i] += Vector3.up * -_height / 2;
        }

        private Mesh UpdateMesh(Vector3[] vertices, int[] triangles)
        {
            _mesh.Clear();
            _mesh.name = "Generated Cylinder";
            _mesh.vertices = vertices;
            _mesh.triangles = triangles;
            _mesh.RecalculateNormals();

            return _mesh;
        }
    }
}