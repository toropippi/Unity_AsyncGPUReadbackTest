using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

//����2:Async�̂��Ƃɂ���GPU�̃����������������Ă����v�������x�͗����ĂȂ���
public class test1 : MonoBehaviour
{
    [SerializeField] ComputeShader computeShader;
    int kernel1, kernel2, kernel3;
    ComputeBuffer buffer1;
    ComputeBuffer buffer2;
    int cnt, n;
    int lastAsyncendcnt, lastAsyncfirstcnt;
    int lasttm = 0;
    void Start()
    {
        n = 65536 * 256;
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
        for (int i = 0; i < 64; i++)
        {
            computeShader.SetInt("xx", cnt);
            computeShader.Dispatch(kernel1, 128, n / 64 / 128, 1);
            ItemGPUtoCPU();
            cnt++;
        }
        /*
        uint[] host = new uint[1];
        buffer2.GetData(host, 0, n - 1, 1);
        Debug.Log(host[0]);
        */
        if (cnt / 64 % 30 == 0)
        {
            var gtn = Gettime();
            Debug.Log(gtn - lasttm);
            lasttm = gtn;
        }
    }


    void ItemGPUtoCPU()
    {
        int tmpv = cnt;
        AsyncGPUReadback.Request(buffer1, 4 * 256, n * 4 - 256 * 4, request =>
        {
            if (request.hasError)
            {
                // �G���[
                Debug.LogError("Error1.");
            }
            else
            {
                // gpu�ɋL������Ă���particle���̃f�[�^���擾����Texture2D�ɔ��f����
                var data = request.GetData<uint>().ToArray();
                if ((int)(data[256 - 1]) != tmpv + 1)
                    Debug.Log("" + data[n - 1] + "/" + tmpv + "/" + cnt + "");
            }
        });
    }









    private void OnDestroy()
    {
        buffer1.Release();
        buffer2.Release();
    }




    //���݂̎�����ms�P�ʂŎ擾
    int Gettime()
    {
        return DateTime.Now.Millisecond + DateTime.Now.Second * 1000
            + DateTime.Now.Minute * 60 * 1000 + DateTime.Now.Hour * 60 * 60 * 1000;
    }


}
