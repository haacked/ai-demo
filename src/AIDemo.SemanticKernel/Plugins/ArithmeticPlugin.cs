using Microsoft.SemanticKernel;

namespace AIDemo.SemanticKernel.Plugins;

public class ArithmeticPlugin
{
    [KernelFunction]
    public long Add(long a, long b) => a + b;

    [KernelFunction]
    public long Subtract(long a, long b) => a - b;

    [KernelFunction]
    public long Multiply(long a, long b) => a * b;

    [KernelFunction]
    public long Divide(int a, int b) => a / b;
}