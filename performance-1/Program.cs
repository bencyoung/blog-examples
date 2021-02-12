using System;
using System.Linq;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

Console.WriteLine("Running Benchmark");

BenchmarkRunner.Run<SimpleBinaryBenchmarks>();

[DisassemblyDiagnoser]
public class SimpleBinaryBenchmarks
{
    private readonly double[] a;
    private readonly double[] b;
    private readonly double[] result;

    public SimpleBinaryBenchmarks()
    {
        this.a = Enumerable.Range(0, 500).Select(i => (double)(i / 10.0)).ToArray();
        this.b = Enumerable.Range(0, 500).Select(i => 10.0 - (double)(i / 5.0)).ToArray();
        this.result = new double[500];
    }

    [Params(5, 500)]
    public int Loops { get; set; }

    [Benchmark(Baseline = true)]
    public void BaselineLoop()
    {
        int loops = this.Loops;
        var a = this.a;
        var b = this.b;
        var result = this.result;
        for(int i =0 ; i < loops; ++i)
        {
            result[i] = a[i] + b[i];
        }
    }

    [Benchmark]
    public void FunctionLoop()
    {
        PerformFunc((a, b) => a + b);
    }

    [Benchmark]
    public void ClassImplLoop()
    {
        PerformClass(new AddOpClass());
    }

    [Benchmark]
    public void StructImplLoop()
    {
        PerformStruct<AddOpStruct>();
    }

    public void PerformFunc(Func<double, double, double> function)
    {
        int loops = this.Loops;
        var a = this.a;
        var b = this.b;
        var result = this.result;
        for(int i =0 ; i < loops; ++i)
        {
            result[i] = function(a[i], b[i]);
        }
    }

    public void PerformClass(IBinaryOp op)
    {
        int loops = this.Loops;
        var a = this.a;
        var b = this.b;
        var result = this.result;
        for(int i =0 ; i < loops; ++i)
        {
            result[i] = op.Perform(a[i], b[i]);
        }
    }

    public void PerformStruct<T>() where T : struct, IBinaryOp
    {
        int loops = this.Loops;
        var a = this.a;
        var b = this.b;
        var result = this.result;
        var op = default(T);
        for(int i =0 ; i < loops; ++i)
        {
            result[i] = op.Perform(a[i], b[i]);
        }
    }

    public interface IBinaryOp
    {
        double Perform(double lhs, double rhs);
    }

    public class AddOpClass : IBinaryOp
    {
         public double Perform(double lhs, double rhs) => lhs + rhs;
    }

    public struct AddOpStruct : IBinaryOp
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Perform(double lhs, double rhs) => lhs + rhs;
    }
}