using System;
using System.Threading.Tasks;

namespace SimulatingHKTs {
  /// <summary>
  /// Uses the adapter pattern to turn <see cref="Task{A}"/> into a <see cref="HigherKindedType{Witness,Element}"/>.
  /// </summary>
  public record HKTTask<A>(Task<A> task) : HigherKindedType<HKTTask.W, A> {
    public static implicit operator HKTTask<A>(Task<A> task) => task.toHKT();
    public static implicit operator Task<A>(HKTTask<A> hkt) => hkt.task;
  }

  public static class HKTTask {
    /// <summary>Witness type for <see cref="HigherKindedType{Witness,Element}"/>.</summary>
    public readonly struct W {}
    
    public static HKTTask<A> toHKT<A>(this Task<A> task) => new HKTTask<A>(task);

    /// <summary>
    /// Safely turns <see cref="HigherKindedType{Witness,Element}"/> back into <see cref="HKTTask{A}"/>.
    /// </summary>
    public static HKTTask<A> narrowKind<A>(this HigherKindedType<W, A> hkt) => (HKTTask<A>) hkt;

    /// <summary>The <see cref="Monad{W}"/> instance for <see cref="HKTTask{A}"/>.</summary>
    public static readonly Monad<W> monad = new Monad();
    
    class Monad : Monad<W> {
      public HigherKindedType<W, Element> wrap<Element>(Element element) => 
        new HKTTask<Element>(Task.FromResult(element));

      public HigherKindedType<W, To> select<From, To>(
        HigherKindedType<W, From> hktOfFrom, 
        Func<From, To> mapper
      ) {
        return new HKTTask<To>(mapTask());

        async Task<To> mapTask() {
          HKTTask<From> hktTask = hktOfFrom.narrowKind();
          From from             = await hktTask.task;
          To to                 = mapper(from);
          return to;
        }
      }

      public HigherKindedType<W, To> selectMany<From, To>(
        HigherKindedType<W, From> hktOfFrom, 
        Func<From, HigherKindedType<W, To>> mapper
      ) {
        return new HKTTask<To>(mapTask());

        async Task<To> mapTask() {
          HKTTask<From> hktFromTask = hktOfFrom.narrowKind();
          From from                 = await hktFromTask.task;
          HKTTask<To> hktToTask     = mapper(from).narrowKind();
          To to                     = await hktToTask.task;
          return to;
        }
      }
    }
  }
}