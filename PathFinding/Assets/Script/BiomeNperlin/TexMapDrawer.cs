using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using System.Linq;
using System.IO;

public class TexMapDrawer
{
    public static Sprite DrawSprite(Vector2Int size, Color[] colorDatas)
    {
        Texture2D texture = new Texture2D(size.x, size.y);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorDatas);
        texture.Apply();

        Rect rect = new Rect(0, 0, size.x, size.y);
        Sprite map = Sprite.Create(texture, rect, Vector2.one * 0.5f);

        return map;
    }

    public static Sprite DrawVoronoiToSprite(Voronoi vo, Color[] areaColor)
    {
        Rect rect = vo.PlotBounds;
        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);
        Color[] pixelColors = Enumerable.Repeat(Color.white, width * height).ToArray();
        List<Vector2> siteCoords = vo.SiteCoords();
        List<Color> posColor = new List<Color>();
        Dictionary<int, Color> posOre = new Dictionary<int, Color>();
    
        // �����(�����߽� �׸���)
        foreach (Vector2 coord in siteCoords)
        {
            int x = Mathf.RoundToInt(coord.x);
            int y = Mathf.RoundToInt(coord.y);

            //2������ 1��������
            int index = x + width * y;
            Color area = areaColor[Random.Range(0, areaColor.Length)];
            pixelColors[index] = area;
            posColor.Add(pixelColors[index]);
            posOre.Add(index, area);
        }
        Vector2Int size = new Vector2Int(width, height);
        
        //���׸��� (�𼭸� �׸���)
        foreach (Site site in vo.Sites)
        {
            List<Site> neighbors = site.NeighborSites();

            foreach (Site neighbor in neighbors)
            {
                // �̿��� ������鿡�Լ� ��ġ�� �����ڸ��� �����س���.=> �ﰢ����
                Edge edge = vo.FindEdgeFromAdjacentPolygons(site, neighbor);

                if (edge.ClippedVertices is null)
                    continue;

                // �����ڸ��� �̷�� �𼭸� ����(������) 2���� ���´�.
                Vector2 corner1 = edge.ClippedVertices[LR.LEFT];
                Vector2 corner2 = edge.ClippedVertices[LR.RIGHT];
                // 1�� �Լ� �׷����� �׸��� �����ڸ� ������ �׸���.
                Vector2 targetPoint = corner1;
                float delta = 1 / (corner2 - corner1).magnitude;
                float lerpRatio = 0f;
                while((int)targetPoint.x != (int)corner2.x || (int)targetPoint.y != (int)corner2.y)
                {
                    //corner1�� corner2 ������ ���� ���������� ���� lerpRatio��ŭ ������ ���� �޾ƿ´�.
                    targetPoint = Vector2.Lerp(corner1, corner2, lerpRatio);
                    lerpRatio += delta;
                    //�ؽ����� ��ǥ ������ ( 0 ~ size.x - 1) ������ ������ ���γ��� ���̾�׷��� ��ǥ�� (~ (float)size.x)�̴�.
                    int x = Mathf.Clamp((int)targetPoint.x, 0, size.x - 1);
                    int y = Mathf.Clamp((int)targetPoint.y, 0, size.y - 1);

                    int index = x + size.x * y;
                    pixelColors[index] = Color.black;
                }

            }
        }

        foreach(int index in posOre.Keys)
        {
            Color area = posOre[index];

            int cx = index % width;
            int cy = index / width;
            pixelColors[cx + width * cy] = Color.cyan;
            ColorringFace(pixelColors, cx, cy, Color.black, area, size);
        }

        //Draw(width, height, pixelColors, posColor, 0, 0);

        // �ؽ���ȭ ��Ű�� ��������Ʈ�� �����
        return DrawSprite(size, pixelColors);

    }

    public static float[] GetRadialGrandientMask(Vector2Int size, int maskRadius)
    {
        float[] colorData = new float[size.x * size.y];

        Vector2Int center = size / 2;   // �� �߽�
        float radius = center.x;        // �� ������
        int index = 0;
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Vector2Int position = new Vector2Int(x, y);
                //���� �������κ��� �Ÿ��� ���� ���� ����(����ŷ ������ ���).
                float distFromCenter = Vector2Int.Distance(center, position) + (radius-maskRadius);
                float colorFactor = distFromCenter / radius;
                //�Ÿ��� �ּ��� ���� 1�� ����������� �������ϼ��� �����븦 �����ؾ� �ؼ� ���� ������.
                colorData[index++] = 1 - colorFactor;

            }
        }
        return colorData;
    }

    static void ColorringFace(Color[] pixelColors, int x, int y, Color checkColor, Color targetColor, Vector2Int size)
    {
        if (x >= size.x || x < 0 || y >= size.y || y < 0)
            return;
        if (pixelColors[x + size.x * y] == checkColor)
            return;
        if (pixelColors[x + size.x * y] == targetColor)
            return;
        pixelColors[x + size.x * y] = targetColor;

        ColorringFace(pixelColors, x - 1, y, checkColor, targetColor, size);
        ColorringFace(pixelColors, x + 1, y, checkColor, targetColor, size);
        ColorringFace(pixelColors, x, y - 1, checkColor, targetColor, size);
        ColorringFace(pixelColors, x, y + 1, checkColor, targetColor, size);
    }

    //����Ʈ�� => ī������ο� ����
    //�޸� �˰���

    public static void CreateFileFromTexture2D(Texture2D texture, string saveFileName)
    {
        byte[] by = texture.EncodeToPNG();
        string path = Application.dataPath + "/Images/" + saveFileName;
        FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(by);

        bw.Close();
        fs.Close();
    }

}
