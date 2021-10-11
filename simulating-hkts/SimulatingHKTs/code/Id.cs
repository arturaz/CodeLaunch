using System;

namespace SimulatingHKTs {
  /// <summary>
  /// Lifts a value of type <see cref="A"/> into the higher-kinded type form.
  /// </summary>
  public readonly struct Id<A> : HigherKindedType<Id.W, A> {
    public readonly A value;

    public Id(A value) => this.value = value;

    public override string ToString() => value.ToString();
    
    public static implicit operator A(Id<A> id) => id.value;
    public static implicit operator Id<A>(A value) => new Id<A>(value);
  }

  public static class Id {
    /// <summary>Witness type used in higher-kinded types simulation.</summary>
    public readonly struct W {}

    public static Id<A> a<A>(A a) => new(a);

    /// <summary>
    /// Safely turns <see cref="HigherKindedType{Witness,Element}"/> back into <see cref="Id{A}"/>.
    /// </summary>
    public static Id<A> narrowKind<A>(this HigherKindedType<W, A> hkt) => (Id<A>) hkt;

    /// <summary>The <see cref="Monad{W}"/> instance for <see cref="Id{A}"/>.</summary>
    public static readonly Monad<W> monad = new Monad();
    
    class Monad : Monad<W> {
      public HigherKindedType<W, Element> wrap<Element>(Element element) => 
        new Id<Element>(element);

      public HigherKindedType<W, To> select<From, To>(
        HigherKindedType<W, From> hktOfFrom, 
        Func<From, To> mapper
      ) {
        Id<From> from = hktOfFrom.narrowKind();
        To to         = mapper(from);
        return wrap(to);
      }

      public HigherKindedType<W, To> selectMany<From, To>(
        HigherKindedType<W, From> hktOfFrom, 
        Func<From, HigherKindedType<W, To>> mapper
      ) {
        Id<From> from = hktOfFrom.narrowKind();
        return mapper(from);
      }
    }
  }
}