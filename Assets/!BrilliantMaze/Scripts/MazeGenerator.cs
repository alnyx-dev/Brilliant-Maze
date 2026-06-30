using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Размер лабиринта (в клетках)")]
    public int width = 10;
    public int height = 10;

    [Header("Геометрия")]
    [Tooltip("Размер одной клетки лабиринта (пол + проход)")]
    public float cellSize = 2f;
    [Tooltip("Толщина стены (меньше cellSize, иначе проходов не будет видно)")]
    public float wallThickness = 0.2f;
    public float wallHeight = 2f;

    [Header("Материалы")]
    public Material wallMaterial;
    public Material floorMaterial;
    public bool generateFloor = true;

    [Header("Вход / выход")]
    public bool openEntrance = true;
    public bool openExit = true;

    [Header("Комнаты")]
    [Tooltip("Сколько прямоугольных комнат попытаться разместить")]
    public int roomCount = 4;
    public Vector2Int roomMinSize = new Vector2Int(2, 2);
    public Vector2Int roomMaxSize = new Vector2Int(4, 4);

    [Header("Разнообразие маршрутов")]
    [Tooltip("Шанс пробить лишнюю стену (создаёт петли/развилки вместо единственного пути)")]
    [Range(0f, 1f)]
    public float extraLoopChance = 0.05f;

    [Header("Seed")]
    public bool useRandomSeed = true;
    public int seed = 0;

    private bool[,] grid;
    private int gridW, gridH;

    private float[] colWidth, colPos;
    private float[] rowDepth, rowPos;

    private List<RectInt> rooms = new List<RectInt>();

    private const string ROOT_NAME = "MazeRoot";

    [ContextMenu("Generate Maze")]
    public void Generate()
    {
        ClearMaze();

        if (useRandomSeed) seed = System.Environment.TickCount;
        Random.InitState(seed);

        gridW = width * 2 + 1;
        gridH = height * 2 + 1;

        grid = new bool[gridW, gridH];
        for (int x = 0; x < gridW; x++)
            for (int y = 0; y < gridH; y++)
                grid[x, y] = true;

        CarveMaze();

        if (openEntrance) grid[1, 0] = false;
        if (openExit) grid[gridW - 2, gridH - 1] = false;

        BuildSizeArrays();

        Transform root = new GameObject(ROOT_NAME).transform;
        root.SetParent(transform, false);

        BuildWalls(root);
        if (generateFloor) BuildFloor(root);
    }

    [ContextMenu("Clear Maze")]
    public void ClearMaze()
    {
        Transform existing = transform.Find(ROOT_NAME);
        if (existing == null) return;

        if (Application.isPlaying)
            Destroy(existing.gameObject);
        else
            DestroyImmediate(existing.gameObject);
    }


    private void GenerateRooms()
    {
        rooms.Clear();
        if (roomCount <= 0) return;

        int maxAttempts = roomCount * 25;
        int placed = 0;
        int attempt = 0;

        while (placed < roomCount && attempt < maxAttempts)
        {
            attempt++;

            int rw = Mathf.Min(Random.Range(roomMinSize.x, roomMaxSize.x + 1), width);
            int rh = Mathf.Min(Random.Range(roomMinSize.y, roomMaxSize.y + 1), height);
            if (rw <= 0 || rh <= 0) continue;

            int rx = Random.Range(0, width - rw + 1);
            int ry = Random.Range(0, height - rh + 1);

            RectInt candidate = new RectInt(rx, ry, rw, rh);

            bool overlaps = false;
            foreach (var r in rooms)
            {
                RectInt padded = new RectInt(r.x - 1, r.y - 1, r.width + 2, r.height + 2);
                if (padded.Overlaps(candidate)) { overlaps = true; break; }
            }

            if (!overlaps)
            {
                rooms.Add(candidate);
                placed++;
            }
        }
    }


    private class DisjointSet
    {
        private readonly int[] parent;
        private readonly int[] rank;

        public DisjointSet(int n)
        {
            parent = new int[n];
            rank = new int[n];
            for (int i = 0; i < n; i++) parent[i] = i;
        }

        public int Find(int x)
        {
            while (parent[x] != x)
            {
                parent[x] = parent[parent[x]];
                x = parent[x];
            }
            return x;
        }

        public bool Union(int a, int b)
        {
            int ra = Find(a), rb = Find(b);
            if (ra == rb) return false;

            if (rank[ra] < rank[rb]) (ra, rb) = (rb, ra);
            parent[rb] = ra;
            if (rank[ra] == rank[rb]) rank[ra]++;
            return true;
        }
    }


    private void CarveMaze()
    {
        int Index(int cx, int cy) => cy * width + cx;

        for (int cx = 0; cx < width; cx++)
            for (int cy = 0; cy < height; cy++)
                grid[cx * 2 + 1, cy * 2 + 1] = false;

        var dsu = new DisjointSet(width * height);

        GenerateRooms();
        foreach (var room in rooms)
        {
            for (int cy = room.y; cy < room.y + room.height; cy++)
            {
                for (int cx = room.x; cx < room.x + room.width; cx++)
                {
                    int gx = cx * 2 + 1, gy = cy * 2 + 1;

                    if (cx + 1 < room.x + room.width)
                    {
                        grid[gx + 1, gy] = false;
                        dsu.Union(Index(cx, cy), Index(cx + 1, cy));
                    }
                    if (cy + 1 < room.y + room.height)
                    {
                        grid[gx, gy + 1] = false;
                        dsu.Union(Index(cx, cy), Index(cx, cy + 1));
                    }
                }
            }
        }

        var edges = new List<(int ax, int ay, int bx, int by, int wallX, int wallY)>();

        for (int cx = 0; cx < width; cx++)
        {
            for (int cy = 0; cy < height; cy++)
            {
                if (cx + 1 < width)
                    edges.Add((cx, cy, cx + 1, cy, cx * 2 + 2, cy * 2 + 1));
                if (cy + 1 < height)
                    edges.Add((cx, cy, cx, cy + 1, cx * 2 + 1, cy * 2 + 2));
            }
        }

        for (int i = edges.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (edges[i], edges[j]) = (edges[j], edges[i]);
        }

        foreach (var e in edges)
        {
            int ia = Index(e.ax, e.ay), ib = Index(e.bx, e.by);

            if (dsu.Union(ia, ib))
            {
                grid[e.wallX, e.wallY] = false;
            }
            else if (extraLoopChance > 0f && Random.value < extraLoopChance)
            {
                grid[e.wallX, e.wallY] = false;
            }
        }
    }

    private void BuildSizeArrays()
    {
        colWidth = new float[gridW];
        colPos = new float[gridW];
        rowDepth = new float[gridH];
        rowPos = new float[gridH];

        float passage = Mathf.Max(0.01f, cellSize - wallThickness);

        float pos = 0f;
        for (int x = 0; x < gridW; x++)
        {
            colWidth[x] = (x % 2 == 1) ? passage : wallThickness;
            colPos[x] = pos;
            pos += colWidth[x];
        }

        pos = 0f;
        for (int y = 0; y < gridH; y++)
        {
            rowDepth[y] = (y % 2 == 1) ? passage : wallThickness;
            rowPos[y] = pos;
            pos += rowDepth[y];
        }
    }


    private void BuildWalls(Transform root)
    {
        bool[,] used = new bool[gridW, gridH];

        MeshBuilder wallBuilder = new MeshBuilder();

        for (int y = 0; y < gridH; y++)
        {
            for (int x = 0; x < gridW; x++)
            {
                if (!grid[x, y] || used[x, y]) continue;

                int rw = 1;
                while (x + rw < gridW && grid[x + rw, y] && !used[x + rw, y]) rw++;

                int rh = 1;
                bool canExpand = true;
                while (y + rh < gridH && canExpand)
                {
                    for (int xi = x; xi < x + rw; xi++)
                    {
                        if (!grid[xi, y + rh] || used[xi, y + rh]) { canExpand = false; break; }
                    }
                    if (canExpand) rh++;
                }

                for (int yi = y; yi < y + rh; yi++)
                    for (int xi = x; xi < x + rw; xi++)
                        used[xi, yi] = true;

                if (rw > 1 || rh > 1)
                    AddWallBlock(wallBuilder, x, y, rw, rh);
            }
        }

        GameObject wallObj = new GameObject("Walls", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        wallObj.transform.SetParent(root, false);
        
        Mesh wallMesh = wallBuilder.Build();
        wallMesh.name = "WallMesh";
        wallObj.GetComponent<MeshFilter>().sharedMesh = wallMesh;
        wallObj.GetComponent<MeshCollider>().sharedMesh = wallMesh;
        
        if (wallMaterial != null)
            wallObj.GetComponent<Renderer>().sharedMaterial = wallMaterial;
    }

    private void AddWallBlock(MeshBuilder builder, int x, int y, int rw, int rh)
    {
        float worldW = 0f, worldD = 0f;
        for (int xi = x; xi < x + rw; xi++) worldW += colWidth[xi];
        for (int yi = y; yi < y + rh; yi++) worldD += rowDepth[yi];

        float startX = colPos[x];
        float startZ = rowPos[y];

        float endX = startX + worldW;
        float endZ = startZ + worldD;
        float bottomY = 0f;
        float topY = wallHeight;

        builder.AddQuad(
            new Vector3(startX, bottomY, startZ), new Vector3(endX, bottomY, startZ),
            new Vector3(endX, bottomY, endZ), new Vector3(startX, bottomY, endZ),
            Vector3.down, worldW, worldD);

        builder.AddQuad(
            new Vector3(startX, topY, endZ), new Vector3(endX, topY, endZ),
            new Vector3(endX, topY, startZ), new Vector3(startX, topY, startZ),
            Vector3.up, worldW, worldD);

        builder.AddQuad(
            new Vector3(startX, bottomY, startZ), new Vector3(startX, topY, startZ),
            new Vector3(endX, topY, startZ), new Vector3(endX, bottomY, startZ),
            Vector3.forward, worldW, wallHeight);

        builder.AddQuad(
            new Vector3(endX, bottomY, endZ), new Vector3(endX, topY, endZ),
            new Vector3(startX, topY, endZ), new Vector3(startX, bottomY, endZ),
            Vector3.back, worldW, wallHeight);

        builder.AddQuad(
            new Vector3(startX, bottomY, endZ), new Vector3(startX, topY, endZ),
            new Vector3(startX, topY, startZ), new Vector3(startX, bottomY, startZ),
            Vector3.left, worldD, wallHeight);

        builder.AddQuad(
            new Vector3(endX, bottomY, startZ), new Vector3(endX, topY, startZ),
            new Vector3(endX, topY, endZ), new Vector3(endX, bottomY, endZ),
            Vector3.right, worldD, wallHeight);
    }

    private void BuildFloor(Transform root)
    {
        float totalW = colPos[gridW - 1] + colWidth[gridW - 1];
        float totalD = rowPos[gridH - 1] + rowDepth[gridH - 1];

        MeshBuilder floorBuilder = new MeshBuilder();

        floorBuilder.AddQuad(
            new Vector3(0, -0.05f, totalD), new Vector3(totalW, -0.05f, totalD),
            new Vector3(totalW, -0.05f, 0), new Vector3(0, -0.05f, 0),
            Vector3.up, totalW, totalD);

        GameObject floorObj = new GameObject("Floor", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        floorObj.transform.SetParent(root, false);

        Mesh floorMesh = floorBuilder.Build();
        floorMesh.name = "FloorMesh";
        floorObj.GetComponent<MeshFilter>().sharedMesh = floorMesh;
        floorObj.GetComponent<MeshCollider>().sharedMesh = floorMesh;

        if (floorMaterial != null)
            floorObj.GetComponent<Renderer>().sharedMaterial = floorMaterial;
    }


    private class MeshBuilder
    {
        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<int> triangles = new List<int>();

        public void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal, float uvScaleX, float uvScaleY)
        {
            int startIndex = vertices.Count;

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);

            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, uvScaleY));
            uvs.Add(new Vector2(uvScaleX, uvScaleY));
            uvs.Add(new Vector2(uvScaleX, 0));

            triangles.Add(startIndex);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 2);
            
            triangles.Add(startIndex);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 3);
        }

        public Mesh Build()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();
            return mesh;
        }
    }
} 