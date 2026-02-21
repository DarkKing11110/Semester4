using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RiddlePuzzle : MonoBehaviour
{
    [Header("UI Grid")]
    [Tooltip("Panel/Container für das 8x8 Puzzle (RectTransform).")]
    public RectTransform gridPanel;
    [Tooltip("Alle 63 Puzzle-Teile (ein Slot bleibt leer). Reihenfolge = Zielreihenfolge von 0..62.")]
    public List<RectTransform> tiles = new List<RectTransform>();
    [Tooltip("Abstand zwischen Zellen in Pixeln.")]
    public Vector2 cellSpacing = new Vector2(4, 4);
    [Tooltip("Padding im Panel in Pixeln.")]
    public Vector2 gridPadding = new Vector2(8, 8);

    [Header("Einstellungen")]
    public int gridSize = 8; // 8x8
    [Tooltip("Beim Start mischen.")]
    public bool shuffleOnStart = true;
    [Tooltip("Mischschritte (mehr = stärker gemischt, aber immer lösbar).")]
    public int shuffleMoves = 200;

    [Header("Quest Stuff")]
    public string GeneratorSolved;
    [Tooltip("StoryManager-Referenz (optional, wird sonst automatisch gesucht).")]
    public StoryManager storyManager;
    [Tooltip("Quest-ID oder Name, bei der dieses Puzzle zählen soll.")]
    public string requiredQuestIdOrName;

    [Header("Generator-Material")]
    [Tooltip("Renderer des Generators (Material mit Exposed Property).")]
    public Renderer generatorRenderer;
    [Tooltip("Optional explizites Material. Wenn leer, wird das Material des Renderers verwendet.")]
    public Material generatorMaterialOverride;
    [Tooltip("Name der Exposed Property im Shader (z. B. 'MainTile').")]
    public string generatorPropertyName = "MainTile";
    [Tooltip("Dauer des Hochfahrens von 0 auf 1 in Sekunden.")]
    public float generatorRampDuration = 2.0f;

    [Header("FMOD Events")]
    [SerializeField] private EventReference tileMoveEvent;
    [Tooltip("FMOD Studio Event für Puzzle gelöst.")]
    [SerializeField] private EventReference puzzleSolveEvent;

    // interner Zustand
    private RectTransform[,] gridSlots; // aktuelle tile-Belegung (null = leer)
    private Vector2Int empty;           // Koordinate des leeren Feldes
    private Vector2 cellSize;           // berechnete Zellgröße
    private bool initialized;

    void Start()
    {
        InitGrid();
        PlaceTilesInGoalOrder();
        if (shuffleOnStart) ShuffleSolvable(shuffleMoves);
    }

    void Update()
    {
        // Tastatursteuerung: schiebe das Feld, das zum leeren Feld benachbart ist (Pfeile „ziehen“ das leere Feld)
        if (Input.GetKeyDown(KeyCode.Keypad2)) TryMoveTile(empty + Vector2Int.down);     // oben = ziehe unten ins leere
        if (Input.GetKeyDown(KeyCode.Keypad8)) TryMoveTile(empty + Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.Keypad6)) TryMoveTile(empty + Vector2Int.right);
        if (Input.GetKeyDown(KeyCode.Keypad4)) TryMoveTile(empty + Vector2Int.left);
    }

    // Initialisiert Grid-Slots und Zellgrößen
    private void InitGrid()
    {
        if (gridPanel == null)
        {
            Debug.LogError("[RiddlePuzzle] gridPanel ist nicht zugewiesen.");
            return;
        }
        if (tiles.Count != gridSize * gridSize - 1)
        {
            Debug.LogError($"[RiddlePuzzle] Erwartet {gridSize*gridSize-1} Tiles, erhalten: {tiles.Count}.");
            return;
        }
        gridSlots = new RectTransform[gridSize, gridSize];

        // Zellgröße aus Panel berechnen (unter Berücksichtigung von Padding/Spacing)
        var panelSize = gridPanel.rect.size;
        float totalSpacingX = cellSpacing.x * (gridSize - 1);
        float totalSpacingY = cellSpacing.y * (gridSize - 1);
        float usableX = panelSize.x - 2 * gridPadding.x - totalSpacingX;
        float usableY = panelSize.y - 2 * gridPadding.y - totalSpacingY;
        cellSize = new Vector2(usableX / gridSize, usableY / gridSize);

        // Alle Tiles auf korrekte Größe setzen (optional)
        foreach (var t in tiles)
        {
            if (t == null) continue;
            var sizeFitter = t.GetComponent<LayoutElement>();
            if (sizeFitter != null)
            {
                sizeFitter.preferredWidth = cellSize.x;
                sizeFitter.preferredHeight = cellSize.y;
            }
            t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellSize.x);
            t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize.y);

            // Klick-Handler hinzufügen
            var btn = t.GetComponent<Button>();
            if (btn != null)
            {
                var captured = t;
                btn.onClick.AddListener(() => OnTileClicked(captured));
            }
        }

        // leeres Feld initial am Ende rechts unten
        empty = new Vector2Int(gridSize - 1, gridSize - 1);
        initialized = true;
    }

    // Platziert Tiles in Zielreihenfolge (0..62) zeilenweise; rechts unten bleibt leer
    private void PlaceTilesInGoalOrder()
    {
        if (!initialized) return;

        int idx = 0;
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if (x == gridSize - 1 && y == gridSize - 1)
                {
                    gridSlots[x, y] = null; // leer
                    continue;
                }
                var tile = tiles[idx++];
                gridSlots[x, y] = tile;
                SetTilePosition(tile, new Vector2Int(x, y));
            }
        }
    }

    // Positioniert ein Tile visuell in Zelle (x,y)
    private void SetTilePosition(RectTransform tile, Vector2Int cell)
    {
        if (tile == null) return;
        // Panel ist typischerweise mit Pivot .5/.5; wir nutzen lokale Position mit Ankern auf Mittelpivot
        Vector2 origin = new Vector2(-gridPanel.rect.width * 0.5f, -gridPanel.rect.height * 0.5f);
        float px = origin.x + gridPadding.x + cell.x * (cellSize.x + cellSpacing.x) + cellSize.x * 0.5f;
        float py = origin.y + gridPadding.y + cell.y * (cellSize.y + cellSpacing.y) + cellSize.y * 0.5f;
        tile.localPosition = new Vector3(px, py, 0f);
    }

    // Zufälliges, stets lösbares Shuffle: führe legale Moves des leeren Feldes aus
    private void ShuffleSolvable(int moves)
    {
        var rng = new System.Random();
        for (int i = 0; i < moves; i++)
        {
            var neighbors = GetNeighborsOfEmpty();
            var choice = neighbors[rng.Next(neighbors.Count)];
            TryMoveTile(choice);
        }
    }

    // Klicke-Interaktion: verschiebe Tile in leeres Feld, falls benachbart
    private void OnTileClicked(RectTransform tile)
    {
        if (tile == null) return;
        var pos = FindTile(tile);
        if (!pos.HasValue) return;
        TryMoveTile(pos.Value);
    }

    // Versuche, Tile an Position „from“ ins leere Feld zu schieben; nur erlaubt, wenn orthogonal benachbart
    private bool TryMoveTile(Vector2Int from)
    {
        if (!initialized) return false;
        if (!InBounds(from)) return false;
        if (gridSlots[from.x, from.y] == null) return false; // kein Tile

        // orthogonal benachbart?
        int manhattan = Mathf.Abs(from.x - empty.x) + Mathf.Abs(from.y - empty.y);
        if (manhattan != 1) return false;

        // verschiebe in leeres Feld
        var tile = gridSlots[from.x, from.y];
        gridSlots[empty.x, empty.y] = tile;
        gridSlots[from.x, from.y] = null;

        SetTilePosition(tile, empty);
        empty = from;

        // FMOD Event abspielen
        FMODUnity.RuntimeManager.PlayOneShot(tileMoveEvent);

        // Optional: Pr�fe Sieg
        if (IsSolved())
            OnSolved();

        return true;
    }

    // Finde Nachbarzellen (orthogonal) des leeren Feldes, die ein Tile enthalten
    private List<Vector2Int> GetNeighborsOfEmpty()
    {
        var res = new List<Vector2Int>();
        var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            var p = empty + d;
            if (InBounds(p) && gridSlots[p.x, p.y] != null)
                res.Add(p);
        }
        return res;
    }

    private Vector2Int? FindTile(RectTransform tile)
    {
        for (int y = 0; y < gridSize; y++)
            for (int x = 0; x < gridSize; x++)
                if (gridSlots[x, y] == tile)
                    return new Vector2Int(x, y);
        return null;
    }

    private bool InBounds(Vector2Int p) => p.x >= 0 && p.x < gridSize && p.y >= 0 && p.y < gridSize;

    // Zielpr�fung: Tiles in Reihenfolge 0..62 an (x,y) = y*gridSize + x
    private bool IsSolved()
    {
        int idx = 0;
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if (x == gridSize - 1 && y == gridSize - 1)
                {
                    if (gridSlots[x, y] != null) return false; // letztes muss leer sein
                }
                else
                {
                    if (gridSlots[x, y] != tiles[idx++]) return false;
                }
            }
        }
        return true;
    }

    private void OnSolved()
    {
        FMODUnity.RuntimeManager.PlayOneShot(puzzleSolveEvent);
        Debug.Log("[RiddlePuzzle] Puzzle gelöst!");

        // Material-Property von 0 nach 1 hochfahren
        //StartCoroutine(RampGeneratorProperty());

        if (IsCorrectQuestActive())
        {
            GameEvents.OnMinigameCompleted?.Invoke(GeneratorSolved);
            StartCoroutine(RampGeneratorProperty());
        }
    }

    private bool IsCorrectQuestActive()
    {
        if (storyManager == null)
            storyManager = FindObjectOfType<StoryManager>();

        if (storyManager == null) return false;
        if (string.IsNullOrWhiteSpace(requiredQuestIdOrName)) return false;

        string currentId = storyManager.GetCurrentMissionIdOrName();
        if (string.IsNullOrEmpty(currentId)) return false;

        return string.Equals(currentId, requiredQuestIdOrName, StringComparison.OrdinalIgnoreCase);
    }

    private IEnumerator RampGeneratorProperty()
    {
        Material mat = generatorMaterialOverride;
        if (mat == null && generatorRenderer != null)
            mat = generatorRenderer.material;

        if (mat == null)
        {
            Debug.LogWarning("[RiddlePuzzle] Kein Generator-Material zugewiesen.");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(generatorPropertyName))
        {
            Debug.LogWarning("[RiddlePuzzle] generatorPropertyName ist leer.");
            yield break;
        }

        // 1. Versuche den angegebenen Namen
        string prop = generatorPropertyName;

        // 2. Falls nicht vorhanden und kein Unterstrich, versuche _Name (ShaderGraph-Standard)
        if (!mat.HasProperty(prop) && !prop.StartsWith("_"))
        {
            string alt = "_" + prop;
            if (mat.HasProperty(alt))
            {
                Debug.Log($"[RiddlePuzzle] Property '{prop}' nicht gefunden, verwende '{alt}' stattdessen.");
                prop = alt;
            }
        }

        // 3. Wenn immer noch nicht vorhanden -> abbrechen
        if (!mat.HasProperty(prop))
        {
            Debug.LogWarning($"[RiddlePuzzle] Material '{mat.name}' hat keine Property '{generatorPropertyName}' (oder '_{generatorPropertyName}').");
            yield break;
        }

        float duration = Mathf.Max(0.01f, generatorRampDuration);
        float t = 0f;
        float start = 0f;
        float end = 1f;

        mat.SetFloat(prop, start);

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float value = Mathf.Lerp(start, end, Mathf.SmoothStep(0f, 1f, k));
            mat.SetFloat(prop, value);
            yield return null;
        }

        mat.SetFloat(prop, end);
    }
}
