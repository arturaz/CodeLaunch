using System;

namespace SimulatingHKTs {
  /// <summary>
  /// Data structure that replaces nulls and <see cref="Nullable{T}"/>. Works for both value types (structs) and
  /// reference types (classes).
  ///
  /// Has two cases: `None` (no value) and `Some(value)` .
  /// </summary>
  public readonly struct Option<A> : HigherKindedType<Option.W, A> {
    /// <summary>Does this <see cref="Option{A}"/> represents the `Some` case?</summary>
    public readonly bool isSome;
    
    /// <summary>
    /// If this <see cref="Option{A}"/> represents the `Some` case the value will be stored here.
    /// In the `None` case this will equal to `default(A)`.
    /// </summary>
    public readonly A __unsafeGet;

    /// <summary>Create an <see cref="Option{A}"/> representing the `Some` case.</summary>
    public Option(A a) {
      isSome = true;
      __unsafeGet = a;
    }

    public override string ToString() => isSome ? $"Some({__unsafeGet})" : "None";

    /// <example><code><![CDATA[
    /// if (option.tryGet(out var value)) {
    ///   // do something with value
    /// }
    /// ]]></code></example>
    public bool tryGet(out A a) {
      a = __unsafeGet;
      return isSome;
    }

    public Option<B> map<B>(Func<A, B> mapper) =>
      isSome ? Some.a(mapper(__unsafeGet)) : None._;

    public Option<B> flatMap<B>(Func<A, Option<B>> mapper) =>
      isSome ? mapper(__unsafeGet) : None._;

    /// <summary>
    /// Converts from <see cref="None"/> to a `None` case of <see cref="Option{A}"/>.
    ///
    /// See <see cref="None._"/> for more information.
    /// </summary>
    public static implicit operator Option<A>(None _) => new ();
  }

  /// <summary>Helpers for creating `Some` case of <see cref="Option{A}"/> conveniently.</summary>
  public static class Some {
    /// <example><code><![CDATA[
    /// var someOption = Some.a(42);
    /// ]]></code></example>
    public static Option<A> a<A>(A a) => a.some();
    
    /// <example><code><![CDATA[
    /// using static SimulatingHKTs.Some;
    /// 
    /// var someOption = some(42);
    /// ]]></code></example>
    public static Option<A> some<A>(this A a) => new Option<A>(a);
  }

  /// <summary>Helpers for creating `None` case of <see cref="Option{A}"/> conveniently.</summary>
  public readonly struct None {
    /// <example><code><![CDATA[
    /// Option<int> someOption => None._;
    /// ]]></code></example>
    public static None _ => new None();
    
    /// <example><code><![CDATA[
    /// using static SimulatingHKTs.None;
    /// 
    /// var someOption = none;
    /// ]]></code></example>
    public static None none => new None();
  }

  public static class Option {
    /// <summary>Witness type used in higher-kinded types simulation.</summary>
    public readonly struct W {}

    /// <summary>
    /// Safely turns <see cref="HigherKindedType{Witness,Element}"/> back into <see cref="Option{A}"/>.
    /// </summary>
    public static Option<A> narrowKind<A>(this HigherKindedType<W, A> hkt) => (Option<A>) hkt;

    /// <summary>The <see cref="Monad{W}"/> instance for <see cref="Option{A}"/>.</summary>
    public static readonly Monad<W> monad = new Monad();
    
    class Monad : Monad<W> {
      public HigherKindedType<W, Element> wrap<Element>(Element element) => 
        Some.a(element);

      public HigherKindedType<W, To> select<From, To>(
        HigherKindedType<W, From> hktOfFrom, 
        Func<From, To> mapper
      ) {
        Option<From> option = hktOfFrom.narrowKind();
        return option.map(mapper);
      }

      public HigherKindedType<W, To> selectMany<From, To>(
        HigherKindedType<W, From> hktOfFrom, 
        Func<From, HigherKindedType<W, To>> mapper
      ) {
        Option<From> option = hktOfFrom.narrowKind();
        return option.flatMap(from => mapper(from).narrowKind());
      }
    }
  }
}