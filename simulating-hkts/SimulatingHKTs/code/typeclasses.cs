using System;
using System.Collections.Generic;
using System.Linq;

namespace SimulatingHKTs {
  /// <summary>
  /// Allows you to transform <![CDATA[  F<A> -> F<B>  ]]>. given a mapper function `A -> B`, via the
  /// <see cref="select{From,To}"/> function.
  ///
  /// In Scala this would look like:
  /// <code><![CDATA[
  /// 
  /// trait Functor[F[_]] {
  ///   def select[From, To](hktOfFrom: F[From], mapper: From => To): F[To] 
  /// }
  /// 
  /// ]]></code>
  /// </summary>
  public interface Functor<Witness> {
    /// <summary>
    /// <para>
    /// Given a higher-kinded type of <see cref="From"/> pass the all of the value in it through <see cref="mapper"/>.
    /// </para>
    ///
    /// <para>
    /// Basically what <see cref="Enumerable.Select{From,To}(IEnumerable{From},System.Func{From,To})"/> does but for
    /// higher-kinded types.
    /// </para>
    /// 
    /// <para>
    /// This method is also known as `map` in other programming languages and libraries.
    /// </para>
    /// 
    /// In Scala this would look like:
    /// <![CDATA[
    ///     def select[From, To](hktOfFrom: F[From], mapper: From => To): F[To]
    /// ]]>.
    /// </summary>
    HigherKindedType<Witness, To> select<From, To>(
      HigherKindedType<Witness, From> hktOfFrom, 
      Func<From, To> mapper
    );
  }
  
  /// <summary>
  /// Allows you to transform <![CDATA[  F<A> -> F<B>  ]]>, given a mapper function <![CDATA[  `A -> F<B>` ]]>, via the
  /// <see cref="selectMany{From,To}"/> function.
  ///
  /// Also allows to lift a value of type `A` into the context `F`, producing a value of type <![CDATA[  `F<A>` ]]>, via
  /// the <see cref="wrap{Element}"/> function.
  ///
  /// Every <see cref="Monad{Witness}"/> is also a <see cref="Functor{Witness}"/>, as
  /// <see cref="Functor{Witness}.select{From,To}"/> can be expressed via
  /// <see cref="selectMany{From,To}"/> plus <see cref="wrap{Element}"/>.
  /// 
  /// In Scala this would look like:
  /// <code><![CDATA[
  /// 
  /// trait Monad[F[_]] extends Functor[F] {
  ///   def wrap[Element](element: Element): F[Element]
  ///   def selectMany[From, To](hktOfFrom: F[From], mapper: A => F[To]): F[To] 
  /// }
  /// 
  /// ]]></code>
  /// </summary>
  public interface Monad<Witness> : Functor<Witness> {
    /// <summary>
    /// <para>
    /// Given a value of type <see cref="Element"/> wraps that value in a <see cref="HigherKindedType{Witness,Element}"/>.
    /// </para>
    ///
    /// <para>
    /// For example, <![CDATA[ Option.monad.wrap(42) == some(42) ]]>.
    /// For example, <![CDATA[ List.monad.wrap(42) == new List<int>(42) ]]>.
    /// </para>
    ///
    /// <para>
    /// This method is also known as `lift`, `point` or `pure` in other programming languages and libraries.
    /// </para>
    ///
    /// In Scala this would look like:
    /// <![CDATA[
    ///     def wrap[Element](element: Element): F[Element]
    /// ]]>.
    /// </summary>
    HigherKindedType<Witness, Element> wrap<Element>(Element element);

    /// <summary>
    /// <para>
    /// Given a higher-kinded type of <see cref="From"/> pass the all of the value in it through <see cref="mapper"/> and
    /// flatten the results.
    /// </para>
    ///
    /// <para>
    /// Basically what <see cref="Enumerable.SelectMany{From,To}(IEnumerable{From},System.Func{From,IEnumerable{To}})"/>
    /// does but for higher-kinded types.
    /// </para>
    ///
    /// <para>
    /// This method is also known as `flatMap` or `bind` in other programming languages and libraries.
    /// </para>
    /// 
    /// In Scala this would look like:
    /// <![CDATA[
    ///     def selectMany[From, To](hktOfFrom: F[From], mapper: A => F[To]): F[To]
    /// ]]>.
    /// </summary>
    HigherKindedType<Witness, To> selectMany<From, To>(
      HigherKindedType<Witness, From> hktOfFrom, 
      Func<From, HigherKindedType<Witness, To>> mapper
    );
  }
}