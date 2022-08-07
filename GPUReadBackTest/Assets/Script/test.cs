using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//async内のDispatchは順番通りか
public class test : MonoBehaviour
{
    [SerializeField] ComputeShader computeShader;
    int kernel1, kernel2, kernel3;
    ComputeBuffer buffer1;
    ComputeBuffer buffer2;
    int cnt, n;
    int lastAsyncendcnt, lastAsyncfirstcnt;
    void Start()
    {
        n = 256 * 256;
        kernel1 = computeShader.FindKernel("k1");
        kernel2 = computeShader.FindKernel("k2");
        kernel3 = computeShader.FindKernel("k3");
        buffer1 = new ComputeBuffer(n, 4);
        buffer2 = new ComputeBuffer(n, 4);
        computeShader.SetBuffer(kernel1, "buffer1", buffer1);
        computeShader.SetBuffer(kernel1, "buffer2", buffer2);
        computeShader.SetBuffer(kernel2, "buffer1", buffer1);
        computeShader.SetBuffer(kernel2, "buffer2", buffer2);
        computeShader.SetBuffer(kernel3, "buffer1", buffer1);
        computeShader.SetBuffer(kernel3, "buffer2", buffer2);
        cnt = 0;
        lastAsyncendcnt = 0;
        lastAsyncfirstcnt = -1;
    }

    // Update is called once per frame
    void Update()
    {
        computeShader.SetInt("xx", cnt);
        computeShader.Dispatch(kernel1, n / 256, 1, 1);
        /*
        uint[] host = new uint[1];
        buffer2.GetData(host, 0, n - 1, 1);
        Debug.Log(host[0]);
        */
        ItemGPUtoCPU();
        cnt++;
    }


    void ItemGPUtoCPU()
    {
        if (lastAsyncendcnt > lastAsyncfirstcnt)
        {
            //Debug.Log("" + lastAsyncendcnt + "/" + lastAsyncfirstcnt + "/" + cnt + "");
            computeShader.Dispatch(kernel2, n / 256, 1, 1);
            int tmpv = cnt;
            lastAsyncfirstcnt = cnt;
            AsyncGPUReadback.Request(buffer2, 4 * n, 0, request =>
            {
                if (request.hasError)
                {
                    // エラー
                    Debug.LogError("Error1.");
                }
                else
                {
                    // gpuに記憶されているparticle数のデータを取得してTexture2Dに反映する
                    var data = request.GetData<uint>().ToArray();
                    if (data[n - 1] > 200)
                        Debug.Log("" + data[n - 1] + "/" + tmpv + "/" + cnt + "");
                    
                    computeShader.Dispatch(kernel3, n / 256, 1, 1);
                    lastAsyncendcnt = cnt;
                }

            });
        }
    }









    private void OnDestroy()
    {
        buffer1.Release();
        buffer2.Release();
    }
}
