using UnityEngine;

public class MobileLut : MonoBehaviour
{
    [Range(2, 3)]
    public int lutDimension = 2;
    [Range(0, 1)]
    public float lutAmount = 1.0f;
    public Texture2D source;

    static readonly int lutTexture2dString = Shader.PropertyToID("_LutTex2D");
    static readonly int lutTexture3dString = Shader.PropertyToID("_LutTex3D");
    static readonly int lutAmountString = Shader.PropertyToID("_LutAmount");

    public Material material;

    private int previousLutDimension;
    private int isLinear;
    private Texture2D previous;
    private Texture2D converted2D = null;
    private Texture3D converted3D = null;

    private void Start()
    {
        isLinear = QualitySettings.activeColorSpace == ColorSpace.Linear ? 1 : 0;
        previousLutDimension = lutDimension;
    }
    public void Update()
    {
        if (previousLutDimension != lutDimension)
        {
            previousLutDimension = lutDimension;
            Convert(source);
            return;
        }

        if (source != previous)
        {
            previous = source;
            Convert(source);
        }
    }

    private void OnDestroy()
    {
        if (converted2D != null)
        {
            DestroyImmediate(converted2D);
        }
        converted2D = null;
    }

    private void Convert2D(Texture2D temp2DTex)
    {
        Color[] color = temp2DTex.GetPixels();
        Color[] newCol = new Color[65536];

        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                for (int x = 0; x < 16; x++)
                    for (int y = 0; y < 16; y++)
                    {
                        float bChannel = (i + j * 16.0f) / 16;
                        int bchIndex0 = Mathf.FloorToInt(bChannel);
                        int bchIndex1 = Mathf.Min(bchIndex0 + 1, 15);
                        float lerpFactor = bChannel - bchIndex0;
                        int index = x + (15 - y) * 256;
                        Color col1 = color[index + bchIndex0 * 16];
                        Color col2 = color[index + bchIndex1 * 16];

                        newCol[x + i * 16 + y * 256 + j * 4096] =
                            Color.Lerp(col1, col2, lerpFactor);
                    }

        if (converted2D)
            DestroyImmediate(converted2D);
        converted2D = new Texture2D(256, 256, TextureFormat.ARGB32, false);
        converted2D.SetPixels(newCol);
        converted2D.Apply();
        converted2D.wrapMode = TextureWrapMode.Clamp;
    }


    private void Convert3D(Texture2D temp3DTex)
    {
        var color = temp3DTex.GetPixels();
        var newCol = new Color[color.Length];

        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                for (int k = 0; k < 16; k++)
                {
                    int val = 16 - j - 1;
                    newCol[i + (j * 16) + (k * 256)] = color[k * 16 + i + val * 256];
                }
            }
        }
        if (converted3D)
            DestroyImmediate(converted3D);
        converted3D = new Texture3D(16, 16, 16, TextureFormat.ARGB32, false);
        converted3D.SetPixels(newCol);
        converted3D.Apply();
        converted3D.wrapMode = TextureWrapMode.Clamp;
    }

    private void Convert(Texture2D source)
    {
        if (lutDimension == 2)
        {
            Convert2D(source);
        }
        else
        {
            Convert3D(source);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (lutDimension == 2 && converted2D == null || lutDimension==3 && converted3D==null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetFloat(lutAmountString, lutAmount);

        if (lutDimension == 2)
        {
            material.SetTexture(lutTexture2dString, converted2D);
        }
        else
        {
            material.SetTexture(lutTexture3dString, converted3D);
        }
        Graphics.Blit(source, destination, material, 2 * (lutDimension - 2) + (QualitySettings.activeColorSpace == ColorSpace.Linear ? 1 : 0));

    }
}

