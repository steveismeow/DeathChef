using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;

public class RoomManager : MonoBehaviour
{
    public Vector2Int totalRoomSizeInTiles;
    public Vector2Int roomInteriorSizeInTiles;

    public Vector2Int playerSpawn;

    [SerializeField]
    private float 
        minDoorCount,
        minCountersCount,
        maxCounterLength;

    public Vector3Int cellPositionWorkspace;

    public Animator fadeCanvasAnimator;
    public CompositeShadowCaster2D compositeShadow; 

    private Tilemap roomInterior;
    private Tilemap roomPerimeter;

    public static List<Vector2Int> fourDirections = new()
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    #region Wall References
    [Header("Walls")]
    [SerializeField]
    private List<Sprite> NWallDoorSprites = new List<Sprite>();

    [SerializeField]
    private GameObject 
        doorTileN,
        doorTileW,
        doorTileE,
        doorTileS;

    [SerializeField]
    private List<TileBase> wallTileN = new List<TileBase>();
    [SerializeField]
    private List<TileBase> wallTileW = new List<TileBase>();
    [SerializeField]
    private List<TileBase> wallTileE = new List<TileBase>();
    [SerializeField]
    private List<TileBase> wallTileS = new List<TileBase>();
    #endregion

    #region Counter References
    [Header("Counters")]
    [SerializeField]
    private int procGenLoopThreshold;

    [SerializeField]
    private List<Sprite> V_MiddleCounterTopSprites = new List<Sprite>();
    [SerializeField]
    private List<Sprite> H_MiddleCounterTopSprites = new List<Sprite>();

    [SerializeField]
    private Sprite
        stoveTopSpriteH,
        stoveTopSpriteV,
        S_endCounterSprite,
        W_endCounterSprite,
        E_endCounterSprite,
        N_endCounterSprite,
        SW_CornerSprite,
        NW_CornerSprite,
        NE_CornerSprite,
        SE_CornerSprite;

    [SerializeField]
    private List<Sprite> CounterBottomSprites = new List<Sprite>();

    [SerializeField]
    private List<GameObject> counterTops = new List<GameObject>();
    [SerializeField]
    private List<GameObject> counterBottoms = new List<GameObject>();

    private List<Vector2Int> endCounters = new List<Vector2Int>();

    private Vector2Int previousCounterPosition;
    #endregion

    #region Props
    [Header("Counters & Props")]
    [SerializeField]
    private List<GameObject> props = new List<GameObject>();
    [SerializeField]
    private GameObject fire;

    #endregion

    // Start is called before the first frame update
    void Awake()
    {

        fadeCanvasAnimator = GameObject.FindGameObjectWithTag("FadeCanvas").GetComponent<Animator>();
        roomInterior = GameObject.FindGameObjectWithTag("Ground").GetComponent<Tilemap>();
        roomPerimeter = GameObject.FindGameObjectWithTag("Walls").GetComponent<Tilemap>();

        //fadeCanvasAnimator.gameObject.GetComponent<CanvasGroup>().alpha = 1;

        if (compositeShadow != null)
        {
            compositeShadow.enabled = false;
        }

        configuringRoom = false;
    }


    public void ConfigureRoom()
    {
        GatherRoomData();
        roomConfiguration = StartCoroutine(ConfiguringRoom());
    }
    bool configuringRoom;
    Coroutine roomConfiguration = null;
    IEnumerator ConfiguringRoom()
    {
        configuringRoom = true;

        while (configuringRoom)
        {
            //fadeCanvasAnimator.Play("FadeOut");
            //yield return new WaitForSeconds(fadeCanvasAnimator.GetCurrentAnimatorStateInfo(0).length);
            PlaceRoomPerimeter();
            PlaceCounters();
            PlacePickUps();
            fadeCanvasAnimator.Play("FadeIn");

            yield return new WaitForSeconds(fadeCanvasAnimator.GetCurrentAnimatorStateInfo(0).length);

            if (compositeShadow != null)
            {
                compositeShadow.enabled = true;
            }

            configuringRoom = false;
        }

        //WaveManager.instance.StartWave();

    }

    private void GatherRoomData()
    {
        ClearRoomData();

        //get floor tile positions
        foreach (Vector2Int tilePosition in GetTileLocations(roomInterior))
        {
            FloorTiles.Add(tilePosition);
            MapTiles.Add(tilePosition);

            FloorTiles.Remove(playerSpawn);
        }
        //get wall tile positions
        foreach (Vector2Int tilePosition in GetTileLocations(roomPerimeter))
        {
            if (tilePosition.y == 5)
            {
                continue;
            }
            WallTiles.Add(tilePosition);
            MapTiles.Add(tilePosition);
        }

        foreach (Vector2Int tilePosition in FloorTiles)
        {
            int neighboursCount = 4;

            if (FloorTiles.Contains(tilePosition + Vector2Int.up) == false)
            {
                NearWallTilesUp.Add(tilePosition);
                neighboursCount--;
            }
            if (FloorTiles.Contains(tilePosition + Vector2Int.down) == false)
            {
                NearWallTilesDown.Add(tilePosition);
                neighboursCount--;
            }
            if (FloorTiles.Contains(tilePosition + Vector2Int.right) == false)
            {
                NearWallTilesRight.Add(tilePosition);
                neighboursCount--;
            }
            if (FloorTiles.Contains(tilePosition + Vector2Int.left) == false)
            {
                NearWallTilesLeft.Add(tilePosition);
                neighboursCount--;
            }

            if (neighboursCount == 4)
                InnerTiles.Add(tilePosition);
        }

        foreach (Vector2Int tilePosition in WallTiles)
        {

            if (WallTiles.Contains(tilePosition + Vector2Int.up) == false && WallTiles.Contains(tilePosition + Vector2Int.left) == false)
            {
                CornerTiles.Add(tilePosition);
            }
            if (WallTiles.Contains(tilePosition + Vector2Int.down) == false && WallTiles.Contains(tilePosition + Vector2Int.left) == false)
            {
                CornerTiles.Add(tilePosition);
            }
            if (WallTiles.Contains(tilePosition + Vector2Int.up) == false && WallTiles.Contains(tilePosition + Vector2Int.right) == false)
            {
                CornerTiles.Add(tilePosition);
            }
            if (WallTiles.Contains(tilePosition + Vector2Int.down) == false && WallTiles.Contains(tilePosition + Vector2Int.right) == false)
            {
                CornerTiles.Add(tilePosition);
            }
        }

        WallTiles.ExceptWith(CornerTiles);
    }

    public void ClearRoomData()
    {
        MapTiles.Clear();
        FloorTiles.Clear();
        WallTiles.Clear();

        NearWallTilesUp.Clear();
        NearWallTilesDown.Clear();
        NearWallTilesLeft.Clear();
        NearWallTilesRight.Clear();
        CornerTiles.Clear();
        InnerTiles.Clear();

        CounterPositions.Clear();
        PropPositions.Clear();
        DoorPositions.Clear();
    }

    #region Perimeter Config
    /// <summary>
    /// Place the doors around the room first
    /// </summary>
    private void PlaceRoomPerimeter()
    {
        //HashSet<Vector2Int> availableWallSpaces = new HashSet<Vector2Int>(WallTiles);

        int tempDoorCount = Mathf.RoundToInt(Random.Range(minDoorCount, 1.5f * minDoorCount));

        List<Vector2Int> NorthWallTiles = new List<Vector2Int>();
        List<Vector2Int> WestWallTiles = new List<Vector2Int>();
        List<Vector2Int> EastWallTiles = new List<Vector2Int>();
        List<Vector2Int> SouthWallTiles = new List<Vector2Int>();

        foreach (Vector2Int tilePosition in WallTiles)
        {
            if (tilePosition.y == 4)
            {
                NorthWallTiles.Add(tilePosition);
            }
            if (tilePosition.x == -10)
            {
                WestWallTiles.Add(tilePosition);
            }
            if (tilePosition.x == 9)
            {
                EastWallTiles.Add(tilePosition);
            }
            if (tilePosition.y == -6)
            {
                SouthWallTiles.Add(tilePosition);
            }
        }

        HashSet<Vector2Int> availableWallSpaces = new HashSet<Vector2Int>(WallTiles);

        RemoveWallTiles(availableWallSpaces);

        //North wall
        for (int i=0;i<tempDoorCount / 4;i++)
        {
            int tileIndex = Random.Range(1, NorthWallTiles.Count - 1);

            Vector2Int doorLocation = NorthWallTiles[tileIndex];
            NorthWallTiles.Remove(NorthWallTiles[tileIndex]);

            GameObject door = Instantiate(doorTileN, TileFromWorldPosition(doorLocation), Quaternion.identity, GameObject.FindGameObjectWithTag("Walls").transform);
            door.GetComponent<SpriteRenderer>().sprite = NWallDoorSprites[Random.Range(0, NWallDoorSprites.Count)];
            WaveManager.instance.EnemySpawnPoints.Add(door.transform);

            foreach(Vector2Int position in NorthWallTiles)
            {
                roomPerimeter.SetTile((Vector3Int)position, wallTileN[Random.Range(0,wallTileN.Count - 1)]);
            }
        }

        //West wall
        for (int i = 0; i < tempDoorCount / 4; i++)
        {
            int tileIndex = Random.Range(1, WestWallTiles.Count - 1);

            Vector2Int doorLocation = WestWallTiles[tileIndex];
            WestWallTiles.Remove(WestWallTiles[tileIndex]);

            GameObject door = Instantiate(doorTileW, TileFromWorldPosition(doorLocation), Quaternion.identity, GameObject.FindGameObjectWithTag("Walls").transform);
            WaveManager.instance.EnemySpawnPoints.Add(door.transform);

            foreach (Vector2Int position in WestWallTiles)
            {
                roomPerimeter.SetTile((Vector3Int)position, wallTileW[Random.Range(0, wallTileW.Count)]);
            }

        }

        //East wall
        for (int i = 0; i < tempDoorCount / 4; i++)
        {
            int tileIndex = Random.Range(1, EastWallTiles.Count - 1);

            Vector2Int doorLocation = EastWallTiles[tileIndex];
            EastWallTiles.Remove(EastWallTiles[tileIndex]);

            GameObject door = Instantiate(doorTileE, TileFromWorldPosition(doorLocation), Quaternion.identity, GameObject.FindGameObjectWithTag("Walls").transform);
            WaveManager.instance.EnemySpawnPoints.Add(door.transform);

            foreach (Vector2Int position in EastWallTiles)
            {
                roomPerimeter.SetTile((Vector3Int)position, wallTileE[Random.Range(0, wallTileE.Count)]);
            }


        }

        //South wall
        for (int i = 0; i < tempDoorCount / 4; i++)
        {
            int tileIndex = Random.Range(1, SouthWallTiles.Count - 1);

            Vector2Int doorLocation = SouthWallTiles[tileIndex];
            SouthWallTiles.Remove(SouthWallTiles[tileIndex]);

            GameObject door = Instantiate(doorTileS, TileFromWorldPosition(doorLocation), Quaternion.identity, GameObject.FindGameObjectWithTag("Walls").transform);
            WaveManager.instance.EnemySpawnPoints.Add(door.transform);

            foreach (Vector2Int position in SouthWallTiles)
            {
                roomPerimeter.SetTile((Vector3Int)position, wallTileS[Random.Range(0, wallTileS.Count)]);
            }


        }



    }

    private void RemoveWallTiles(HashSet<Vector2Int> availableWallSpaces)
    {
        foreach (Vector2Int tilePosition in availableWallSpaces)
        {
            roomPerimeter.SetTile((Vector3Int)tilePosition, null);
        }
    }

    //Place Tile
    //public void PlaceTile(Vector3Int gridPosition, string tileName)
    //{
    //    tileMap.SetTile(gridPosition, tileNameCodex[tileName].tile);

    //    Vector3 position = GetVector3FromGridPosition(gridPosition);

    //    print(position);

    //}
    #endregion

    #region Prop/Environment Config
    /// <summary>
    /// Generate counter tops first using variation of a randomwalk proc gen, add counter sides, add props
    /// </summary>
    void PlaceCounters()
    {
        HashSet<Vector2Int> availableFloorSpaces = new HashSet<Vector2Int>(InnerTiles);

        List<Vector2Int> CounterTopPositions = new List<Vector2Int>();

        for (int i = 0; i < minCountersCount; i++)
        {
            int index = Random.Range(0, availableFloorSpaces.Count - 1);
            Vector2Int counterStartPosition = availableFloorSpaces.ElementAt(index);
            if (PositionProblematic(counterStartPosition))
            {
                availableFloorSpaces.Remove(counterStartPosition);
                continue;
            }
            CounterTopPositions.Add(counterStartPosition);
            availableFloorSpaces.Remove(counterStartPosition);

            endCounters.Clear();

            List<Vector2Int> counterPath = GenerateCounterTopPath(counterStartPosition);

            foreach (Vector2Int pathPosition in counterPath)
            {
                availableFloorSpaces.Remove(pathPosition);
                if (!CounterPositions.Contains(pathPosition))
                {
                    CounterPositions.Add(pathPosition);
                }

                GameObject counterTop = null;

                if (endCounters.Contains(pathPosition))
                {
                    counterTop = Instantiate(counterTops[0], TileFromWorldPosition(pathPosition), Quaternion.identity, GameObject.FindGameObjectWithTag("Ground").transform);

                    var direction = GetEndCounterType(pathPosition);
                    switch (direction)
                    {
                        case "S":
                            counterTop.GetComponent<SpriteRenderer>().sprite = S_endCounterSprite;
                            PlaceBottomCounter(pathPosition, counterTop);
                            break;
                        case "W":
                            counterTop.GetComponent<SpriteRenderer>().sprite = W_endCounterSprite;
                            PlaceBottomCounter(pathPosition, counterTop);
                            break;
                        case "E":
                            counterTop.GetComponent<SpriteRenderer>().sprite = E_endCounterSprite;
                            PlaceBottomCounter(pathPosition, counterTop);
                            break;
                        case "N":
                            counterTop.GetComponent<SpriteRenderer>().sprite = N_endCounterSprite;
                            break;
                    }
                }
                else
                {
                    var counterType = GetMiddleCounterType(pathPosition);

                    switch (counterType)
                    {
                        case "SW_Corner":
                            counterTop = Instantiate(counterTops[0], TileFromWorldPosition(pathPosition), Quaternion.identity, GameObject.FindGameObjectWithTag("Ground").transform);
                            counterTop.GetComponent<SpriteRenderer>().sprite = SW_CornerSprite;
                            PlaceBottomCounter(pathPosition, counterTop);
                            break;
                        case "NW_Corner":
                            counterTop = Instantiate(counterTops[0], TileFromWorldPosition(pathPosition), Quaternion.identity, GameObject.FindGameObjectWithTag("Ground").transform);
                            counterTop.GetComponent<SpriteRenderer>().sprite = NW_CornerSprite;
                            break;
                        case "NE_Corner":
                            counterTop = Instantiate(counterTops[0], TileFromWorldPosition(pathPosition), Quaternion.identity, GameObject.FindGameObjectWithTag("Ground").transform);
                            counterTop.GetComponent<SpriteRenderer>().sprite = NE_CornerSprite;
                            break;
                        case "SE_Corner":
                            counterTop = Instantiate(counterTops[0], TileFromWorldPosition(pathPosition), Quaternion.identity, GameObject.FindGameObjectWithTag("Ground").transform);
                            counterTop.GetComponent<SpriteRenderer>().sprite = SE_CornerSprite;
                            PlaceBottomCounter(pathPosition, counterTop);
                            break;
                        case "V":

                            float vindex = Random.value;

                            if (vindex >= 0.20)
                            {
                                counterTop = Instantiate(counterTops[0], TileFromWorldPosition(pathPosition), Quaternion.identity, GameObject.FindGameObjectWithTag("Ground").transform);
                                counterTop.GetComponent<SpriteRenderer>().sprite = V_MiddleCounterTopSprites[Random.Range(0, V_MiddleCounterTopSprites.Count - 1)];
                            }
                            else
                            {
                                counterTop = Instantiate(counterTops[1], TileFromWorldPosition(pathPosition), Quaternion.identity, GameObject.FindGameObjectWithTag("Ground").transform);
                                counterTop.GetComponent<SpriteRenderer>().sprite = stoveTopSpriteV;
                            }
                            break;
                        case "H":

                            float hindex = Random.value;

                            if (hindex >= 0.20)
                            {
                                counterTop = Instantiate(counterTops[0], TileFromWorldPosition(pathPosition), Quaternion.identity, GameObject.FindGameObjectWithTag("Ground").transform);
                                counterTop.GetComponent<SpriteRenderer>().sprite = H_MiddleCounterTopSprites[Random.Range(0, H_MiddleCounterTopSprites.Count - 1)];
                            }
                            else
                            {
                                counterTop = Instantiate(counterTops[1], TileFromWorldPosition(pathPosition), Quaternion.identity, GameObject.FindGameObjectWithTag("Ground").transform);
                                counterTop.GetComponent<SpriteRenderer>().sprite = stoveTopSpriteH;
                            }
                            PlaceBottomCounter(pathPosition, counterTop);
                            break;

                    }
                }
            }
        }
    }

    private List<Vector2Int> GenerateCounterTopPath(Vector2Int counterStartPosition)
    {
        previousCounterPosition = Vector2Int.zero;

        List<Vector2Int> counterPath = new List<Vector2Int>();

        int counterLength = Random.Range(2, (int)maxCounterLength);
        int loopTracker = 0;

        Vector2Int currentPosition = counterStartPosition;
        Vector2Int direction = new Vector2Int();
        
        counterPath.Add(currentPosition);
        if (!CounterPositions.Contains(currentPosition))
        {
            CounterPositions.Add(currentPosition);
        }




        for (int i = 0; i < counterLength; i++)
        {
            if (loopTracker >= procGenLoopThreshold)
            {
                break;
            }

            int directionIndex = Random.Range(1, 5);

            switch (directionIndex)
            {
                case 1:
                    direction = Vector2Int.down;
                    break;
                case 2:
                    direction = Vector2Int.left;
                    break;
                case 3:
                    direction = Vector2Int.up;
                    break;
                case 4:
                    direction = Vector2Int.right;
                    break;
            }
            previousCounterPosition = currentPosition;
            currentPosition += direction;

            if (PositionProblematic(currentPosition))
            {
                currentPosition -= direction;
                i--;
                loopTracker++;
                continue;
            }
            else
            {
                counterPath.Add(currentPosition);
                if (!CounterPositions.Contains(currentPosition))
                {
                    CounterPositions.Add(currentPosition);
                }
            }
        }

        endCounters.Add(counterStartPosition);
        endCounters.Add(currentPosition);

        return counterPath;

    }

    private void PlaceBottomCounter(Vector2Int  pathPosition, GameObject counterTop)
    {
        GameObject counterBottom = Instantiate(counterBottoms[Random.Range(0, counterBottoms.Count - 1)], TileFromWorldPosition(pathPosition + Vector2Int.down), Quaternion.identity, counterTop.transform);
        counterBottom.GetComponent<SpriteRenderer>().sprite = CounterBottomSprites[Random.Range(0, CounterBottomSprites.Count - 1)];
    }

    private string GetEndCounterType(Vector2Int counterPosition)
    {
        if (CounterPositions.Contains(counterPosition + Vector2Int.up))
        {
            return "S";
        }
        else if (CounterPositions.Contains(counterPosition + Vector2Int.right))
        {
            return "W";
        }
        else if (CounterPositions.Contains(counterPosition + Vector2Int.left))
        {
            return "E";
        }
        else
        {
            return "N";
        }
    }
    private string GetMiddleCounterType(Vector2Int counterPosition)
    {
        if(CounterPositions.Contains(counterPosition + Vector2Int.up))
        {
            if (CounterPositions.Contains(counterPosition + Vector2Int.left))
            {
                return "SE_Corner";
            }
            if(CounterPositions.Contains(counterPosition + Vector2Int.right))
            {
                return "SW_Corner";
            }
            else
            {
                return "V";
            }
        }
        else if(CounterPositions.Contains(counterPosition + Vector2Int.down))
        {
            if (CounterPositions.Contains(counterPosition + Vector2Int.left))
            {
                return "NE_Corner";
            }
            if (CounterPositions.Contains(counterPosition + Vector2Int.right))
            {
                return "NW_Corner";
            }
            else
            {
                return "V";
            }
        }
        else
        {
            return "H";
        }
    }


    private bool PositionProblematic(Vector2Int position)
    {
        if (WallTiles.Contains(position))
        {
            return true;
        }
        else if (CounterPositions.Contains(position))
        {
            return true;
        }
        else if (NearWallTilesDown.Contains(position))
        {
            return true;
        }
        else if (NearWallTilesLeft.Contains(position))
        {
            return true;
        }
        else if (NearWallTilesRight.Contains(position))
        {
            return true;
        }
        else if (NearWallTilesUp.Contains(position))
        {
            return true;
        }
        else if (GetAdjacentCountersToPosition(position) > 1)
        {
            return true;
        }
        //else if (Get tiles adjacent to NearWallTiles))
        //{
        //    return true;
        //}
        else
        {
            return false;
        }
    }
    private int GetAdjacentCountersToPosition(Vector2Int position)
    {
        int adjacentCountersToPosition = 0;

        if(CounterPositions.Contains(position + Vector2Int.up) || CounterPositions.Contains(position + Vector2Int.up*2))
        {
            adjacentCountersToPosition++;
        }
        if (CounterPositions.Contains(position + Vector2Int.left) || CounterPositions.Contains(position + Vector2Int.left * 2))
        {
            adjacentCountersToPosition++;
        }
        if (CounterPositions.Contains(position + Vector2Int.right) || CounterPositions.Contains(position + Vector2Int.right * 2))
        {
            adjacentCountersToPosition++;
        }
        if (CounterPositions.Contains(position + Vector2Int.down) || CounterPositions.Contains(position + Vector2Int.down * 2))
        {
            adjacentCountersToPosition++;
        }

        return adjacentCountersToPosition;

    }

    #endregion

    #region PickUps 
    //This may be incorporated into the Room Object Placement system
    private void PlacePickUps()
    {

    }

    public List<Vector2Int> GetBuffSpawnPositions()
    {

        List<Vector2Int> buffPositions = new List<Vector2Int>();

        foreach (Vector2Int tilePosition in GetTileLocations(roomInterior))
        {
            FloorTiles.Add(tilePosition);

            FloorTiles.Remove(playerSpawn);
        }

        foreach (Vector2Int tilePosition in FloorTiles)
        {
            int neighboursCount = 4;

            if (FloorTiles.Contains(tilePosition + Vector2Int.up) == false)
            {
                neighboursCount--;
            }
            if (FloorTiles.Contains(tilePosition + Vector2Int.down) == false)
            {
                neighboursCount--;
            }
            if (FloorTiles.Contains(tilePosition + Vector2Int.right) == false)
            {
                neighboursCount--;
            }
            if (FloorTiles.Contains(tilePosition + Vector2Int.left) == false)
            {
                neighboursCount--;
            }

            if (neighboursCount == 4)
            {
                InnerTiles.Add(tilePosition);
            }
        }


        for (int i = 0; i < InterWaveManager.instance.numOfBuffsToSpawn; i++)
        {
            int tileIndex = Random.Range(0, InnerTiles.Count);

            Vector2Int position = InnerTiles.ElementAt(tileIndex);
            buffPositions.Add(position);
        }
       

        return buffPositions;

    }
    #endregion

    #region Extras
    public void SpawnFire(Vector2 position, float splashRange)
    {
        Vector3Int explosionPosition = new Vector3Int((int)position.x, (int)position.y, 0);
        if (roomInterior.HasTile(explosionPosition))
        {
            Vector2Int tilePosition = new Vector2Int(explosionPosition.x, explosionPosition.y);

            List<Vector2Int> tilePositions = GetAdjacentTiles(tilePosition);

            foreach(Vector2Int location in tilePositions)
            {
                //Instantiate Fire object
                GameObject newFire = Instantiate(fire, TileFromWorldPosition(location), Quaternion.identity);
            }
        }
    }
    #endregion

    #region Get Functions and Conversions
    private List<Vector2Int> GetTileLocations(Tilemap tilemap)
    {
        List<Vector2Int> tileWorldLocations = new List<Vector2Int>();

        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            Vector3Int localPos = new Vector3Int(pos.x, pos.y, pos.z);
            //Vector3 position = GetVector3FromGridPosition(localPos);

            if (tilemap.HasTile(localPos))
            {
                Vector2Int tilePosition = new Vector2Int(localPos.x, localPos.y);
                tileWorldLocations.Add(tilePosition);
            }
        }

        return tileWorldLocations;

    }

    private List<Vector2Int> GetAdjacentTiles(Vector2Int tilePosition)
    {
        List<Vector2Int> tilePositions = new List<Vector2Int>();

        if (FloorTiles.Contains(tilePosition + Vector2Int.up))
        {
            tilePositions.Add(tilePosition + Vector2Int.up);
        }
        if (FloorTiles.Contains(tilePosition + Vector2Int.down))
        {
            tilePositions.Add(tilePosition + Vector2Int.down);
        }
        if (FloorTiles.Contains(tilePosition + Vector2Int.left))
        {
            tilePositions.Add(tilePosition + Vector2Int.left);
        }
        if (FloorTiles.Contains(tilePosition + Vector2Int.right))
        {
            tilePositions.Add(tilePosition + Vector2Int.right);
        }


        return tilePositions;
    }

    //Get a Vector2 from a given grid/cell location
    public Vector2 TileFromWorldPosition(Vector2Int worldPosition)
    {
        Vector2 adjustedPosition = worldPosition + Vector2.one * 0.5f;

        return adjustedPosition;
    }

    #endregion

    #region Room Data
    public HashSet<Vector2Int> MapTiles { get; private set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> FloorTiles { get; private set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> WallTiles { get; private set; } = new HashSet<Vector2Int>();

    public HashSet<Vector2Int> NearWallTilesUp { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTilesDown { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTilesLeft { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTilesRight { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> CornerTiles { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> InnerTiles { get; set; } = new HashSet<Vector2Int>();

    public HashSet<Vector2Int> CounterPositions { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> PropPositions { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> DoorPositions { get; set; } = new HashSet<Vector2Int>();
    #endregion

}
