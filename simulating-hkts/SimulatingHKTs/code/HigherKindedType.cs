using System;
using System.Collections.Generic;
using System.Linq;

namespace SimulatingHKTs {
  // Expresses the idea of a higher-kinded generic type.
  //
  // The `Witness` specifies what kind of wrapper around `Element` it is.
  // The `Element` specifies what kind of element is contained inside the wrapper.
  //
  // For example, a `HigherKindedType<Option.W, int>` represents an Option<int> type.
  //
  // ### Notes about types ###
  //
  // #### A first-order type: Option<A> ####
  //
  // This has a "signature" of `type -> concrete type` (* -> *).
  //
  // Think of it as you put a type (like `int`) into the `A` slot and you get a proper type like `Option<int>`.
  //
  //
  // #### A higher-kinded type: F<A> ####
  //
  // This has a "signature" of `(type -> first-order type) -> concrete type` ((* -> *) -> *).
  //
  // Think of it as you put a type (like `Option<A>`) into the `F` slot and you get a first-order type like `Option<A>`.
  //
  // Func<A>
  // Action<A>
  //
  // Func<A, B>
  // 
  public interface HigherKindedType<Witness, Element> {}

  public static class HigherKindedType {
    /// <summary>
    /// Given an enumerable of <![CDATA[ F<Element> ]]>, a starting <see cref="Result"/> and an <see cref="aggregator"/>
    /// function, return a <![CDATA[ F<Result> ]]>.
    ///
    /// Basically the <see cref="Enumerable.Aggregate{TSource}"/>
    /// </summary>
    public static HigherKindedType<W, Result> aggregate<W, Element, Result>(
      this IEnumerable<HigherKindedType<W, Element>> enumerable, 
      Result startWith, Monad<W> monad, Func<Result, Element, Result> aggregator
    ) =>
      enumerable.Aggregate(
        startWith.wrap(monad),
        (hktCurrent, hktElement) => 
          hktCurrent.selectMany(monad, current =>
            hktElement.select(monad, element => aggregator(current, element))
          )
      );
    
    /// <summary>Better syntax for <see cref="Monad{Witness}.wrap{Element}"/>.</summary>
    public static HigherKindedType<W, Element> wrap<W, Element>(
      this Element element, Monad<W> monad
    ) => monad.wrap(element);

    /// <summary>Better syntax for <see cref="Monad{Witness}.select{From,To}"/>.</summary>
    public static HigherKindedType<W, To> select<W, From, To>(
      this HigherKindedType<W, From> hkt, Functor<W> functor, Func<From, To> mapper
    ) => functor.select(hkt, mapper);

    /// <summary>Better syntax for <see cref="Monad{Witness}.selectMany{From,To}"/>.</summary>
    public static HigherKindedType<W, To> selectMany<W, From, To>(
      this HigherKindedType<W, From> hkt, Monad<W> monad, Func<From, HigherKindedType<W, To>> mapper
    ) => monad.selectMany(hkt, mapper);
  }
}