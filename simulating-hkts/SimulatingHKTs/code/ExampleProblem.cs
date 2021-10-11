using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace SimulatingHKTs {
  /// <summary>ID of some resource.</summary>
  public record ResourceId(string id);
  
  /// <summary>Data of a resource loadable from <see cref="ResourceId"/>.</summary>
  public record ResourceData(byte[] data);
  
  public static class ExampleProblem {
    #region Asynchronous resource loading
    
    /// <summary>Given a <see cref="ResourceId"/> loads the <see cref="ResourceData"/> asynchronously.</summary>
    public delegate Task<ResourceData> AsyncResourceLoader(ResourceId id);
    
    /// <summary>
    /// Given a list of <see cref="resourceIds"/> and an asynchronous <see cref="loader"/> to load those resources,
    /// return a dictionary containing all loaded resources. 
    /// </summary>
    public static async Task<ImmutableDictionary<ResourceId, ResourceData>> asyncLoadResources(
      ImmutableList<ResourceId> resourceIds, AsyncResourceLoader loader
    ) {
      // Start loading all resources in parallel at the same time.
      var allInProgress = resourceIds.ToImmutableDictionary(
        keySelector: id => id,
        elementSelector: id => loader(id)
      );

      // Wait until all resources are loaded.
      return await aggregate1();

      async Task<ImmutableDictionary<ResourceId, ResourceData>> aggregate1() {
        var loaded = ImmutableDictionary.CreateBuilder<ResourceId, ResourceData>();
        foreach (var (id, loadTask) in allInProgress) {
          var data = await loadTask;
          loaded[id] = data;
        }

        return loaded.ToImmutable();
      }

      Task<ImmutableDictionary<ResourceId, ResourceData>> aggregate2() =>
        allInProgress.Aggregate(
          seed: Task.FromResult(ImmutableDictionary<ResourceId, ResourceData>.Empty),
          func: async (currentTask, keyValue) => {
            var (resourceId, resourceDataTask) = keyValue;
            var dictionary = await currentTask;
            var resourceData = await resourceDataTask;
            var result = dictionary.Add(resourceId, resourceData);
            return result;
          }
        );
    }

    #endregion

    #region Synchronous resource loading

    /// <summary>Given a <see cref="ResourceId"/> loads the <see cref="ResourceData"/> synchronously.</summary>
    public delegate Id<ResourceData> SyncResourceLoader(ResourceId id);
    
    /// <summary>
    /// Given a list of <see cref="resourceIds"/> and an synchronous <see cref="loader"/> to load those resources,
    /// return a dictionary containing all loaded resources. 
    /// </summary>
    public static ImmutableDictionary<ResourceId, ResourceData> syncLoadResources(
      ImmutableList<ResourceId> resourceIds, SyncResourceLoader loader
    ) {
      var loaded = asyncLoadResources(resourceIds, id => Task.FromResult(loader(id).value));
      // From https://docs.microsoft.com/en-us/dotnet/api/System.Threading.Tasks.Task-1.Result?view=net-5.0#remarks :
      //
      // Accessing the property's get accessor blocks the calling thread until the asynchronous operation is complete;
      // it is equivalent to calling the Wait method.
      return loaded.Result;
    }

    #endregion

    #region Optional resource loading

    /// <summary>
    /// Given a <see cref="ResourceId"/> loads the <see cref="ResourceData"/> synchronously, but it might fail and thus,
    /// return an empty <see cref="Option{A}"/>.
    /// </summary>
    public delegate Option<ResourceData> OptionalResourceLoader(ResourceId id);
    
    /// <summary>
    /// Given a list of <see cref="resourceIds"/> and an optional synchronous <see cref="loader"/> to load those
    /// resources, return a dictionary containing all loaded resources or None if any of the resource loads failed. 
    /// </summary>
    public static Option<ImmutableDictionary<ResourceId, ResourceData>> optionalSyncLoadResources(
      ImmutableList<ResourceId> resourceIds, OptionalResourceLoader loader
    ) {
      var result = asyncLoadResources(resourceIds, asAsync);
      return result.IsCompletedSuccessfully ? Some.a(result.Result) : None._;
      
      Task<ResourceData> asAsync(ResourceId id) =>
        loader(id).tryGet(out var resourceData)
          ? Task.FromResult(resourceData)
          : Task.FromException<ResourceData>(new Exception($"Could not load resource {id}"));
    }

    #endregion
  }

  public static class Solution {
    /// <summary>Given a <see cref="ResourceId"/> loads the <see cref="ResourceData"/> in some way.</summary>
    public delegate HigherKindedType<Witness, ResourceData> HigherKindedResourceLoader<Witness>(ResourceId id);
    
    /// <summary>
    /// Given a list of <see cref="resourceIds"/> and an asynchronous <see cref="loader"/> to load those resources,
    /// return a dictionary containing all loaded resources. 
    /// </summary>
    public static HigherKindedType<Witness, ImmutableDictionary<ResourceId, ResourceData>> loadResources<Witness>(
      ImmutableList<ResourceId> resourceIds, 
      HigherKindedResourceLoader<Witness> loader,
      Monad<Witness> monad
    ) {
      // Start loading all resources in parallel at the same time.
      ImmutableDictionary<ResourceId, HigherKindedType<Witness, ResourceData>> allInProgress = 
        resourceIds.ToImmutableDictionary(
          keySelector: id => id,
          elementSelector: id => loader(id)
        );

      // Wait until all resources are loaded.
      var loaded = aggregate1();
      return loaded;

      HigherKindedType<Witness, ImmutableDictionary<ResourceId, ResourceData>> aggregate1() =>
        allInProgress.Aggregate(
          seed: ImmutableDictionary<ResourceId, ResourceData>.Empty.wrap(monad),
          func: (currentHkt, keyValue) => {
            var (resourceId, hktResourceData) = keyValue;
            
            var result = 
              currentHkt.selectMany(monad, dictionary => 
                hktResourceData.select(monad, resourceData => 
                  dictionary.Add(resourceId, resourceData)
                )
              );
            return result;
          }
        );

      HigherKindedType<Witness, ImmutableDictionary<ResourceId, ResourceData>> aggregate2() {
        var allInProgressAsEnumerable = 
          allInProgress.Select(keyValue => {
            var (resourceId, hktResourceData) = keyValue;
            return hktResourceData.select(monad, resourceData => (resourceId, resourceData));
          });
        
        return 
          allInProgressAsEnumerable.aggregate(
            startWith: Enumerable.Empty<(ResourceId, ResourceData)>(), 
            monad: monad,
            aggregator: (current, tuple) => current.Concat(new [] { tuple })
          ).select(
            monad,
            enumerable => enumerable.ToImmutableDictionary(tuple => tuple.Item1, tuple => tuple.Item2) 
          );
      }
    }

    public static Task<ImmutableDictionary<ResourceId, ResourceData>> asyncLoadResources(
      ImmutableList<ResourceId> resourceIds, ExampleProblem.AsyncResourceLoader loader
    ) => loadResources(resourceIds, id => loader(id).toHKT(), HKTTask.monad).narrowKind();
    
    public static ImmutableDictionary<ResourceId, ResourceData> syncLoadResources(
      ImmutableList<ResourceId> resourceIds, ExampleProblem.SyncResourceLoader loader
    ) => loadResources(resourceIds, id => loader(id), Id.monad).narrowKind();
    
    public static Option<ImmutableDictionary<ResourceId, ResourceData>> optionalLoadResources(
      ImmutableList<ResourceId> resourceIds, ExampleProblem.OptionalResourceLoader loader
    ) => loadResources(resourceIds, id => loader(id), Option.monad).narrowKind();
  }
}