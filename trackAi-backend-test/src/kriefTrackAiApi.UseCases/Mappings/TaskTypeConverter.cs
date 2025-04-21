using AutoMapper;
using System;
using System.Threading.Tasks;

public class TaskTypeConverter<TSource, TDestination> : ITypeConverter<Task<TSource>, Task<TDestination>>
{
    public Task<TDestination> Convert(Task<TSource> source, Task<TDestination> destination, ResolutionContext context)
    {
        return source.ContinueWith(t => context.Mapper.Map<TDestination>(t.Result), TaskScheduler.Default);
    }
}
