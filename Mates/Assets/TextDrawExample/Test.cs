using NUnit.Framework.Constraints;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct Operation1 : IJob
{
    public int n1;
    public int n2;
    public NativeReference<int> Result; // salida

    public void Execute()
    {
        Result.Value = n1 + n2;
    }
}

[BurstCompile]
public struct Operation2 : IJob
{
    [ReadOnly] public NativeReference<int> Input;  // entrada (solo lectura)
    public NativeReference<int> Output;            // salida

    public void Execute()
    {
        Output.Value = Input.Value * 10; // ejemplo: usa el resultado de op1
    }
}

public struct PrimeJob : IJob
{
    public int Max;
    public int Result;


    public void Execute()
    {
        Result = CountPrimesUpTo(Max);
    }

    private static int CountPrimesUpTo(int max)
    {
        Debug.Log($"Begin CountPrimesUpTo({max})");
        if (max < 2) return 0;
        int count = 1; // 2 es primo
        for (int n = 3; n <= max; n += 2)
        {
            bool prime = true;
            int r = (int)Mathf.Sqrt(n);
            for (int d = 3; d <= r; d += 2)
            {
                if (n % d == 0) { prime = false; break; }
            }
            if (prime) count++;
        }

        Debug.Log($"End CountPrimesUpTo({max})");

        return count;
    }
}

public class Test : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;

    // Create a JobHandle for the job
    JobHandle handle;
    PrimeJob jobData;

    // Update is called once per frame
    void Update()
    {
        label.SetText(Time.deltaTime.ToString());
    }

    // Llama con un límite suficientemente alto para >1s. 
    // Empieza por ~80_000_000 y ajusta según tu CPU.
    public void RunPrimeLong()
    {
        var t1 = Time.deltaTime;

        int result = CountPrimesUpTo(80_000_000);

        var t2 = Time.deltaTime;

        Debug.Log($"Primes up to 80_000_000: {result} in {t2 - t1} seconds.");
    }

    public void Schedule2Jobs()
    {
        // Contenedores compartidos entre jobs
        var r1 = new NativeReference<int>(Allocator.TempJob);
        var r2 = new NativeReference<int>(Allocator.TempJob);

        var op1 = new Operation1 { n1 = 1, n2 = 2, Result = r1 };
        JobHandle h1 = op1.Schedule();

        var op2 = new Operation2 { Input = r1, Output = r2 };
        JobHandle h2 = op2.Schedule(h1); // depende de op1

        h2.Complete(); // espera al pipeline

        Debug.Log($"op1={r1.Value}  op2={r2.Value}"); // op1=3, op2=30

        r1.Dispose();
        r2.Dispose();
    }

    public void SchedulePrimeLong()
    {
        jobData = new PrimeJob { Max = 80_000_000 };

        handle = jobData.Schedule();

    }


    /*
    private void LateUpdate()
    {
        if (handle.IsCompleted)
        {
            Debug.Log($"Primes up to 80_000_000: {jobData.Result} in {Time.deltaTime} seconds.");
        }
    }
    */

    private int CountPrimesUpTo(int max)
    {
        if (max < 2) return 0;
        int count = 1; // 2 es primo
        for (int n = 3; n <= max; n += 2)
        {
            bool prime = true;
            int r = (int)Mathf.Sqrt(n);
            for (int d = 3; d <= r; d += 2)
            {
                if (n % d == 0) { prime = false; break; }
            }
            if (prime) count++;
        }
        return count;
    }
}
