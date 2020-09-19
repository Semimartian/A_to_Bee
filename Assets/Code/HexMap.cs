using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour
{
    [SerializeField] private Collider mapZone;

    public static void PrepareHexExplosion(Hex origin)
    {
        instance.StartCoroutine(instance.PrepareHexExplosionCoroutine(origin));   
    }

    private IEnumerator PrepareHexExplosionCoroutine(Hex origin)
    {
        int range = 2;
        float waitTime = 0.15f;
        List<Hex> hexesInRange = new List<Hex>(6);
        hexesInRange.Add(origin);
        if(origin.State == HexStates.Empty)
        {
            origin.ChangeState(HexStates.PotentiallyFull);

        }
        for (int i = 0; i < range; i++)
        {
            yield return new WaitForSeconds(waitTime);
            int count = hexesInRange.Count;
            for (int j = 0; j < count; j++)
            {

                Hex[] neighbours = hexesInRange[j].GetNeighbours();
                for (int n = 0; n < neighbours.Length; n++)
                {
                    Hex neighbour = neighbours[n];
                    if (neighbour.State == HexStates.Empty)// || neighbour.State == HexStates.PotentiallyFull )
                    {
                        neighbour.ChangeState(HexStates.PotentiallyFull);
                    }
                    hexesInRange.Add(neighbour);

                }
            }
        }
        //awaitingFill = true;
    }

    // [SerializeField] private Material[] mildClimateMaterials;
    // [SerializeField] private Material resourcesMaterial;
    private CombineableMesh waterHexMesh;
    private CombineableMesh[] mildClimateHexMeshes;

    [SerializeField] private CombineableMesh combineableMeshPreFab;
    [SerializeField] private Hex hexPreFab;
    [SerializeField] private GameObject hexHighLightPreFab;
    [SerializeField] private HexBomb hexBombPreFab;
    private static Hex[,] hexes;
    private static Hex[] realHexes;
    public readonly static int numberOfRows = 77;
    public readonly static int numberOfColumns = 77;

    public static HexMap instance;
    private static bool mapWasGenerated;

    public Material emptyHexMat;
    public Material highLightedHexMat;
    public Material awaitingFillHexMat;

    public Material fullHexMat;
    public Material hardHexMat;

    public static Hex GetHex(int q, int r)
    {
        if (hexes == null)
        {
            Debug.LogError("hexes == null");          
        }
        else if (q >= 0 && q < numberOfColumns && r >= 0 && r < numberOfRows)
        {
            return hexes[q, r];
        }
        Debug.LogError("hex is null");
        return null;
    }

    public static Hex GetHex(HexCoordinates hexCoordinates)
    {
        int q = hexCoordinates.q;
        int r = hexCoordinates.r;
        if (hexes == null)
        {
            Debug.LogError("hexes == null");
        }
        else if (q >= 0 && q < numberOfColumns && r >= 0 && r < numberOfRows)
        {
            return hexes[q, r];
        }
        Debug.LogError("hex is null");
        return null;
    }

    private int GetRandomQ()
    {
       return Random.Range(0,numberOfColumns);
    }

    private int GetRandomR()
    {
        return Random.Range(0, numberOfRows);
    }

    private Hex GetRandomHex()
    {
        return GetHex(GetRandomQ(), GetRandomR());
    }


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Tried to instantiate more than one map!");
            return;
        }

        GenerateMap();
    }

    [SerializeField] private int bombGenerationChance = 32;
    [SerializeField] private int hardHexGenerationChance = 14;

    public void GenerateMap()
    {
        if (mapWasGenerated)
        {
            Debug.LogError("Someone's trying to regenerate the map!");
        }

        hexes = new Hex[numberOfColumns, numberOfRows];

        Bounds zoneBounds = mapZone.bounds;
        List<Hex> realHexesList = new List<Hex>(32);
        Wall[] walls = FindObjectsOfType<Wall>();
        
        for (int column = 0; column < numberOfColumns; column++)
        {
            for (int row = 0; row < numberOfRows; row++)
            {
                Vector3 hexPosition = Hex.PositionInWorld(column, row);
                bool hexIsInBounds = zoneBounds.Contains(hexPosition);
                /*    (hexPosition.x >= bounds.min.x &&
                     hexPosition.x <= bounds.max.x &&
                     hexPosition.z >= bounds.min.z &&
                     hexPosition.z <= bounds.max.z);*/
                if (hexIsInBounds)
                {
                    bool isAWall = false;
                    for (int i = 0; i < walls.Length; i++)
                    {
                        Bounds wallBounds = walls[i].collider.bounds;
                        if (wallBounds.Contains(hexPosition))
                        {
                            isAWall = true;
                            break;
                        }
                    }
                    Hex hex = Instantiate(hexPreFab);
                    hex.Construct(column, row, hex.GetComponent<HexComponent>());
                    hex.transform.position = hexPosition;
                    hexes[column, row] = hex;
                    realHexesList.Add(hex);

                    if (isAWall)
                    {
                        hex.ChangeState(HexStates.Full);
                    }
                    else
                    {
                        if (Random.Range(0, bombGenerationChance) == 0)
                        {
                            HexBomb hexBomb = Instantiate
                                (hexBombPreFab, hexPosition, Quaternion.identity);
                            hexBomb.hex = hex;

                        }
                        else if (Random.Range(0, hardHexGenerationChance) == 0)
                        {

                            hex.ChangeState(HexStates.Hard);

                        }
                    }

                }

                //Material hexMaterial = GetMaterialForHex(hex);
                //hexComponent.GetComponentInChildren<MeshRenderer>().material = hexMaterial;
                //hexComponent.SetHex(hex);
               /* if (hexComponent.GetComponentInChildren<TMPro.TextMeshPro>())
                {
                    hexComponent.GetComponentInChildren<TMPro.TextMeshPro>().text = column + "," + row;
                }*/
               // if(column == 0 && row == 0)
               /* {
                    GameObject hexModel = Instantiate(hexModelPreFab, hex.PositionInWorld(), Quaternion.identity);
                    hexModel.transform.parent = transform;
                }*/

                //  hexComponent.GetComponentInChildren<TMPro.TextMeshPro>().enabled=false;

            }
        }

        realHexes = realHexesList.ToArray();

        for (int i = 0; i < walls.Length; i++)
        {
            Destroy(walls[i].gameObject);////0;
        }
        Destroy(mapZone.gameObject);
        #region irrelevant
        //StaticBatchingUtility.Combine(gameObject);

        //waterHexMesh = Instantiate(combineableMeshPreFab, transform.position, Quaternion.identity,transform);
        /*mildClimateHexMeshes = new CombineableMesh[mildClimateMaterials.Length];
        for (int i = 0; i < mildClimateHexMeshes.Length; i++)
        {
            mildClimateHexMeshes[i] = Instantiate(combineableMeshPreFab, transform.position, Quaternion.identity, transform);
        }

        for (int i = 0; i < mildClimateHexMeshes.Length; i++)
        {
            mildClimateHexMeshes[i].CombineMeshes(mildClimateMaterials[i]);
        }*/
        #endregion
        mapWasGenerated = true;
    }

    /*public static void BuildTerritoryMesh()
    {
        CombineableMesh combineableMesh = instance.territoryHighLights[player.Index];

        foreach (Hex hex in player.Territory)
        {
            Instantiate(instance.hexHighLightPreFab, hex.PositionInWorld(), Quaternion.identity, combineableMesh.transform);
        }

        combineableMesh.CombineMeshes(instance.playersHighLightsMaterials[player.Index]);
    }*/

    public List<Hex> GetHexesInRangeOf(Hex centreHex, int range)
    {
        List<Hex> hexesInRange = new List<Hex>();
        for (int dx = -range; dx < range-1; dx++)
        {
            for (int dy = Mathf.Max(-range+1,-dx-range); dy <  Mathf.Min(range,-dx+range-1); dy++)
            {
                Hex hex = GetHex(centreHex.Q + dx, centreHex.R + dy);
                if (hex != null)
                {
                    hexesInRange.Add(hex);

                }
            }
        }
        return hexesInRange;
    }


    private void FixedUpdate()
    {
        /*if (Input.GetKeyDown(KeyCode.F))
        {
            CalculateFill();
        }*/

        if (awaitingFill &&  !HexPainter.isOnAPotentialWall)
        {
            Debug.Log("CompleteFill" + HexPainter.isOnAPotentialWall);
            CompleteFill();
            awaitingFill = false;
        }
    }
    #region Flood Fill:
    private const sbyte PAINTED = -1;
    private const sbyte UNPAINTED = -2;
    private static List<Hex> fills = new List<Hex>();
    private static List<Hex> fillsNext = new List<Hex>();

    public struct FillUnit
    {
        public byte q;
        public byte r;

        public FillUnit(byte q, byte r)
        {
            this.q = q;
            this.r = r;
        }
    }

    private static void ClearFloodFillMap()
    {
        for (int i = 0; i < realHexes.Length; i++)
        {
            Hex hex = realHexes[i];
            hex.fillMark = hex.State==HexStates.Empty ? UNPAINTED: PAINTED ;
        }  
    }

    private static List<int> floodFillSlicesSizes = new List<int>();

    private static bool awaitingFill = false;
    public static void CalculateFill()
    {
        Debug.Log("CalculateFill");
        ClearFloodFillMap();
        int width = HexMap.numberOfColumns;
        int height = HexMap.numberOfRows;
        sbyte currentID = -1;

        fills.Clear();
        fillsNext.Clear();
        floodFillSlicesSizes.Clear();

        for (int i = 0; i < realHexes.Length; i++)
        {
            Hex hex = realHexes[i];
            if (hex.fillMark == UNPAINTED)
            {
                currentID += 1;
                floodFillSlicesSizes.Add(0);

                hex.fillMark = currentID;
                fills.Add(hex);
            }

            while (fills.Count > 0)
            {
                // yield return new WaitForSeconds(0.004f);
                //loopCount += 1;
                foreach (Hex fill in fills)
                {
                    // yield return new WaitForSeconds(0.002f);
                    Hex[] neighbours = fill.GetNeighbours();
                    Hex neighbour;
                    for (int j = 0; j < neighbours.Length; j++)
                    {
                        neighbour = neighbours[j];
                        if (neighbour.fillMark == UNPAINTED)
                        {
                            neighbour.fillMark = currentID;
                            fillsNext.Add(neighbour);
                        }
                    }
                    // Debug.Log("currentID:" + currentID);
                    floodFillSlicesSizes[currentID] += 1;//TODO: bad writing

                    /* if (fy <= 0 || fx <= 0 || fx >= width - 1 || fy >= height - 1)//TODO: find out what this is all about
                     {
                         floodFillMap[fx, fy] = TRANSPARENT;
                         continue;
                     }*/
                }

                List<Hex> swap = fills;
                swap.Clear();
                fills = fillsNext;
                fillsNext = swap;
            }
        }

        if (floodFillSlicesSizes.Count < 2)
        {
            Debug.Log("Number of slices has to be greater than 1 in order to fill the area");
        }
        else
        {
            

            sbyte largestSliceID = 0;
            int largestSizeSoFar = 0;
            for (sbyte i = 0; i < floodFillSlicesSizes.Count; i++)
            {
                if (floodFillSlicesSizes[i] > largestSizeSoFar)
                {
                    largestSliceID = i;
                    largestSizeSoFar = floodFillSlicesSizes[i];
                }
            }

            for (int i = 0; i < realHexes.Length; i++)
            {
                Hex hex = realHexes[i];
                if (hex.State == HexStates.PotentiallyFull||
                    (hex.fillMark >= 0 && hex.fillMark != largestSliceID))
                {
                    //hex.Fill(true);
                    hex.ChangeState(HexStates.AwaitingFill);
                }
            }


            instance.StartCoroutine(instance.AwaitFillIn(0));
        }       
    }

    IEnumerator AwaitFillIn(float time)
    {
        yield return new WaitForSeconds(time);
        awaitingFill = true;
        HexPainter.instance.FloorCheck();
    }
    private static void CompleteFill()
    {
        for (int i = 0; i < realHexes.Length; i++)
        {
            Hex hex = realHexes[i];
            if (hex.State == HexStates.AwaitingFill)
            {
                hex.ChangeState(HexStates.Full);
            }
        }
    }
    #endregion
}
