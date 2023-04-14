using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using DefineEnumHelper;

public class MapManager : MonoBehaviour
{
    [Header("¸Ê Á¤º¸")]
    [SerializeField] Transform _mapRoot;
    [SerializeField] int _mapMaxHeight = 128;
    [SerializeField] int _mapGroundHeightOffset = 30;
    [SerializeField] int _SnowOnHeight = 55;
    [SerializeField] int _SoilOnHeight = 40;
    [SerializeField] int _GrassOnHeight = 0;

    [Header("¸Ê ÀÚ¿ø")]
    [SerializeField] GameObject[] _prefabOre;
    [SerializeField] Texture2D _perlinNoiseMap;
    [SerializeField] Texture2D _BiomMap;
    [SerializeField] int _floorLevelCount = 20;

    [SerializeField] int _stoneRate = 50;
    [SerializeField] int _IronRate = 50;
    [SerializeField] int _GoldRate = 50;
    [SerializeField] int _DiamondRate = 50;

    Block[,,] _worldBlock;

    private void Start()
    {
        Vector2Int size = new Vector2Int(_perlinNoiseMap.width, _perlinNoiseMap.height);
        _worldBlock = new Block[size.x, _mapMaxHeight, size.y];
        GenerateBlockMap(_perlinNoiseMap, _BiomMap, size);
    }

    private void Update()
    {
        if(Input.GetButtonDown("Fire3"))
        {
            RaycastHit rHit;
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(r, out rHit))
            {
                GameObject block = rHit.transform.gameObject;
                Vector3 position = block.transform.position;
                FindingBlockCheckSurrounding(position);
               
               /* DigupBlock(position);
                flyupBlock(position);*/
            }
        }
    }

    void FindingBlockCheckSurrounding(Vector3 blockpos)
    {
        if (blockpos.y <= 0)
            return;
        Block hitBlock = _worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z];
        _worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z] = null;
        Destroy(hitBlock._oreBlock);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && y == 0 && z == 0)
                        continue;
                    if (blockpos.x + x < 0 || blockpos.x + x > _perlinNoiseMap.width)
                        continue;
                    if (blockpos.y + y < 0 || blockpos.y + y > _mapMaxHeight)
                        continue;
                    if (blockpos.z + z < 0 || blockpos.z + z > _perlinNoiseMap.height)
                        continue;

                    Vector3 neighbour = new Vector3(blockpos.x + x, blockpos.y + y, blockpos.z + z);

                    if (_worldBlock[(int)neighbour.x, (int)neighbour.y, (int)neighbour.z] != null)
                        DrawBlock(neighbour);
                }
            }
        }
    }
    

    void GenerateBlockMap(Texture2D mapTex, Texture2D mapOre, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.x; z++)
            {
                int y = Mathf.RoundToInt(mapTex.GetPixel(x, z).grayscale * _floorLevelCount);
                y += _mapGroundHeightOffset;

                Vector3 pos = new Vector3(x, y - 0.5f, z);

                CreateBlock(y, pos, true, 50);
                while (y-- > 0)
                {
                    pos = new Vector3(x, y - 0.5f, z);
                    CreateBlock(y, pos, false, 50);
                }
            }
        }

      /*for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.x; z++)
            {
                int y = Mathf.RoundToInt(mapTex.GetPixel(x, z).grayscale * _floorLevelCount);
                y += _mapGroundHeightOffset;

                    Vector3 pos = new Vector3(x, y - 1f, z);
                    bool NullCheck = FindNullBlock(new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z));
                    if (NullCheck)
                        DrawBlock(new Vector3(x, y, z));
            }
        }*/

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.x; z++)
            {
                int y = Mathf.RoundToInt(mapTex.GetPixel(x, z).grayscale * _floorLevelCount);
                y += _mapGroundHeightOffset;

                int minY = EmptyBlockCreate(mapTex, _mapGroundHeightOffset, x, z, size.x);

                while (y-- > 0)
                {
                    Vector3 pos = new Vector3(x, y - 0.5f, z);

                    DrawBlock(pos);

                    if (y <= minY)
                        break;

                }
            }
        }

    }

    bool FindNullBlock(Vector3Int blockPos)
    {
        if (blockPos.y <= 0)
            return false;
        for(int x = 0; x <= 1; x++)
        {
            if (blockPos.x - x < 0 || blockPos.x + x > _perlinNoiseMap.width - 1)
                continue;
            if (_worldBlock[blockPos.x + x, blockPos.y, blockPos.z] == null || _worldBlock[blockPos.x - x, blockPos.y, blockPos.z] == null)
                return true;
        }
        for(int z = 0; z <= 1; z++)
        {
            if (blockPos.z - z < 0 || blockPos.z + z > _perlinNoiseMap.height - 1)
                continue;
            if (_worldBlock[blockPos.x, blockPos.y, blockPos.z + z] == null || _worldBlock[blockPos.x, blockPos.y, blockPos.z - z] == null)
                return true;
        }

        return false;
    }

    void DrawBlock(Vector3 pos)
    {
        Block block = _worldBlock[(int)pos.x, (int)pos.y, (int)pos.z];

        if(block != null)
        if(!block._isView)
        {
            GameObject newBlock = null;
            block._isView = true;
            newBlock = Instantiate(_prefabOre[(int)block._type], pos, Quaternion.identity);

            if (newBlock != null)
                _worldBlock[(int)pos.x, (int)pos.y, (int)pos.z]._oreBlock = newBlock;
        }
    }


    void CreateBlock(int y, Vector3 pos, bool isView, int ratio, eOreType res = eOreType.Grass)
    {
        GameObject go = null;
        eOreType type = res;

        if (y == 0)
        {
            type = eOreType.UnDestoryed;
            isView = true;
        }
        else
        {
            if (y > _SnowOnHeight)
            {

                type = eOreType.Snow;
            }
            else if (y > _GrassOnHeight)
            {
                type = eOreType.Grass;
            }
            else
            {
                type = eOreType.Soil;
            }
            type = BiomSearch(pos, _BiomMap, type);
        }

        if (isView)
        {
            go = Instantiate(_prefabOre[((int)type)], pos, Quaternion.identity, _mapRoot);
        }
        _worldBlock[(int)pos.x, (int)pos.y, (int)pos.z] = new Block(type, isView, go);
    }

    eOreType BiomSearch(Vector3 pos, Texture2D mapTex, eOreType type)
    {
        Color GoldMountain;
        Color IronMountain;
        Color DiaMountain;

        Color yellow = Color.yellow;

        Color color = mapTex.GetPixel((int)pos.x, (int)pos.z);
        int RandomB = Random.Range(0, 1000);

        int red = (int)(color.r * 10);
        int green = (int)(color.g * 10);
        int blue = (int)(color.b * 10);

        if (red == 10 && green == 7 && blue == 0)
        {
            if (_GoldRate > RandomB)
                type = eOreType.Gold;
        }
        else if (red == blue && green  == blue && red == green )
        {
            if (_IronRate > RandomB)
                type = eOreType.Iron;
        }
        else if (red == 5 && green == 9 && blue == 10)
        {
            if (_DiamondRate > RandomB)
                type = eOreType.Diamond;
        }
        else
            type = type;

        return type;
    }

    #region MyCoding
    int EmptyBlockCreate(Texture2D mapTex, int _mapGroundHeightOffset, int x, int z, int max)
    {
        int Mx = 0;
        int Mz = 0;
        int Px = 0;
        int Pz = 0;
        int y = 0;
        if (x != 0)
        {
            Mx = Mathf.RoundToInt(mapTex.GetPixel(x - 1, z).grayscale * _floorLevelCount) + _mapGroundHeightOffset;
            y = Mx;
        }

        if (z != 0)
        {
            Mz = Mathf.RoundToInt(mapTex.GetPixel(x - 1, z).grayscale * _floorLevelCount) + _mapGroundHeightOffset;
            if (y > Mz)
                y = Mz;
        }

        if (x < max)
            Px = Mathf.RoundToInt(mapTex.GetPixel(x + 1, z).grayscale * _floorLevelCount) + _mapGroundHeightOffset;

        if (y > Px || y == 0)
            y = Px;

        if (z < max)
            Mz = Mathf.RoundToInt(mapTex.GetPixel(x, z + 1).grayscale * _floorLevelCount) + _mapGroundHeightOffset;

        if (y > Px)
            y = Mz;

        return y;
    }

    void DigupBlock(Vector3 position)
    {
        Vector3Int blockpos = new Vector3Int((int)position.x, (int)position.y - 1, (int)position.z);
        if (blockpos.y != 0)
        {
            if (blockpos.x - 1 != 0)
            {
                if (_worldBlock[blockpos.x - 1, blockpos.y, blockpos.z]._isView != true)
                    DropBlock(blockpos.x - 1, blockpos.y, blockpos.z);
            }

            if (blockpos.x + 1 < _perlinNoiseMap.width - 1)
            {
                if (_worldBlock[blockpos.x + 1, blockpos.y, blockpos.z]._isView != true)
                    DropBlock(blockpos.x + 1, blockpos.y, blockpos.z);
            }
            if (blockpos.z - 1 != 0)
            {
                if (_worldBlock[blockpos.x, blockpos.y, blockpos.z - 1]._isView != true)
                    DropBlock(blockpos.x, blockpos.y, blockpos.z - 1);
            }

            if (blockpos.z + 1 < _perlinNoiseMap.width - 1)
            {
                if (_worldBlock[blockpos.x, blockpos.y, blockpos.z + 1]._isView != true)
                    DropBlock(blockpos.x, blockpos.y, blockpos.z + 1);
            }
            if (_worldBlock[blockpos.x, blockpos.y, blockpos.z]._isView != true)
                DropBlock(blockpos.x, blockpos.y, blockpos.z);
        }


    }

    void flyupBlock(Vector3 position)
    {
        Vector3Int blockpos = new Vector3Int((int)position.x, (int)position.y + 1, (int)position.z);
        if (blockpos.x - 1 != 0)
        {
            int y = Mathf.RoundToInt(_perlinNoiseMap.GetPixel(blockpos.x - 1, blockpos.z).grayscale * _floorLevelCount) + _mapGroundHeightOffset;
            if (blockpos.y < y)
                if (_worldBlock[blockpos.x - 1, blockpos.y, blockpos.z]._isView != true)
                    DropBlock(blockpos.x - 1, blockpos.y, blockpos.z);
        }
        if (blockpos.x + 1 < _perlinNoiseMap.width - 1)
        {
            int y = Mathf.RoundToInt(_perlinNoiseMap.GetPixel(blockpos.x + 1, blockpos.z).grayscale * _floorLevelCount) + _mapGroundHeightOffset;
            if (blockpos.y < y)
                if (_worldBlock[blockpos.x + 1, blockpos.y, blockpos.z]._isView != true)
                    DropBlock(blockpos.x + 1, blockpos.y, blockpos.z);
        }
        if (blockpos.z - 1 != 0)
        {
            int y = Mathf.RoundToInt(_perlinNoiseMap.GetPixel(blockpos.x, blockpos.z - 1).grayscale * _floorLevelCount) + _mapGroundHeightOffset;
            if (blockpos.y < y)
                if (_worldBlock[blockpos.x, blockpos.y, blockpos.z - 1]._isView != true)
                    DropBlock(blockpos.x, blockpos.y, blockpos.z - 1);
        }

        if (blockpos.z + 1 < _perlinNoiseMap.width - 1)
        {
            int y = Mathf.RoundToInt(_perlinNoiseMap.GetPixel(blockpos.x, blockpos.z + 1).grayscale * _floorLevelCount) + _mapGroundHeightOffset;
            if (blockpos.y < y)
                if (_worldBlock[blockpos.x, blockpos.y, blockpos.z + 1]._isView != true)
                    DropBlock(blockpos.x, blockpos.y, blockpos.z + 1);
        }

        int k = Mathf.RoundToInt(_perlinNoiseMap.GetPixel(blockpos.x, blockpos.z).grayscale * _floorLevelCount) + _mapGroundHeightOffset;
        if (blockpos.y < k)
            if (_worldBlock[blockpos.x, blockpos.y, blockpos.z]._isView != true)
            {

                if (blockpos.y < k)
                    DropBlock(blockpos.x, blockpos.y, blockpos.z);
            }

    }

    void DropBlock(int x, int y, int z)
    {
        _worldBlock[x, y - 1, z]._isView = true;

        Vector3 pos = new Vector3(x, y - 0.5f, z);

        GameObject go = Instantiate(_prefabOre[(int)_worldBlock[x, y, z]._type], pos, Quaternion.identity, _mapRoot);
    }

    #endregion
}
