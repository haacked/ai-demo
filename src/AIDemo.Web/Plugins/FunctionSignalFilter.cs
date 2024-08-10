using System.Diagnostics.CodeAnalysis;
using Haack.AIDemoWeb.Library;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using AIDemo.Hubs;

namespace Haack.AIDemoWeb.Plugins;

[Experimental("SKEXP0001")]
public class FunctionSignalFilter(IHubContext<BotHub> hubContext)
    : IFunctionInvocationFilter, IAutoFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        await hubContext.BroadcastFunctionCall(
            context.Function.Name,
            context.Arguments);

        await next(context);
    }

    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context,
        Func<AutoFunctionInvocationContext, Task> next)
    {
        await hubContext.BroadcastFunctionCall(
            context.Function.Name,
            context.Arguments ?? new KernelArguments());

        await next(context);
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public async Task OnFunctionInvokedAsync(FunctionInvokedEventArgs args)
#pragma warning restore CS0618 // Type or member is obsolete
    {
        await hubContext.BroadcastFunctionResult(
            args.Function.Name,
            args.Result);
    }
}