// ReSharper disable RedundantLambdaParameterType
// ReSharper disable ConvertToLocalFunction
// ReSharper disable ReturnTypeCanBeEnumerable.Local
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace Variance {
  static class Program {
    // C# supports specifying variance in `delegate` and `interface`, but not `class` definitions.
    
    static void Main(string[] args) {
      brokenInvarianceDemo();
    }

    #region Data model
    
    public abstract class Animal {}

    public abstract class Dog : Animal {
      public class Bulldog : Dog {}
      public class BerneseMountainDog : Dog {}
      public class GermanShepherd : Dog {}
      public class Pug : Dog {}
      public class Poodle : Dog {}
    }
    
    public class Cat : Animal {}

    #endregion

    #region Covariance demo
    
    /// <summary>Covariant.</summary>
    public delegate A Create<out A>(uint index);
    
    static void covarianceDemo() {
      Create<Dog.Poodle> poodleCreator = (uint index) => new Dog.Poodle();
      Create<Dog>        dogCreator    = (uint index) => index < 3 ? new Dog.Pug() : new Dog.Bulldog();
      Create<Animal>     animalCreator = (uint index) => index < 9 ? new Cat() : new Dog.BerneseMountainDog();
      
      Dog[] poodles = puppyGenerator(5, poodleCreator); // Most specific
      Dog[] dogs    = puppyGenerator(5, dogCreator);    // Mid-specific
      // Dog[] animals = puppyGenerator(5, animalCreator); // Least specific

      static Dog[] puppyGenerator(uint count, Create<Dog> create) {
        var dogs = new Dog[count];
        for (var index = 0u; index < count; index++) {
          dogs[index] = create(index);
        }

        return dogs;
      }
    }

    #endregion

    #region Contravariance demo
    
    /// <summary>Contravariant.</summary>
    public delegate void Pet<in A>(A a);
    
    static void contravarianceDemo() {
      Pet<Dog.Poodle> petPoodle = (Dog.Poodle poodle) => { /* make poodles very happy */ };
      Pet<Dog>        petDog    = (Dog        dog   ) => { /* make any doggo happy */ };
      Pet<Animal>     petAnimal = (Animal     animal) => { /* make any animal happy, may it be a dog or a cat */ };
      
      // pettingZoo(petPoodle); // Most specific
      pettingZoo(petDog);    // Mid-specific
      pettingZoo(petAnimal); // Least specific

      static void pettingZoo(Pet<Dog> pet) {
        pet(new Dog.GermanShepherd());
        pet(new Dog.Bulldog());
        pet(new Dog.BerneseMountainDog());
      }
    }
    
    #endregion
    
    #region Invariance demo
    
    /// <summary>Invariant.</summary>
    public delegate A Clone<A>(A a);
    
    static void invarianceDemo() {
      Clone<Dog.Poodle> clonePoodle = (Dog.Poodle poodle) => poodle;
      Clone<Dog>        cloneDog    = (Dog        dog)    => dog;
      Clone<Animal>     cloneAnimal = (Animal     animal) => new Cat();
      
      // Dog poodle = cloneMyDoggo(clonePoodle); // Most specific
      Dog dog    = cloneMyDoggo(cloneDog);    // Mid-specific
      // Dog animal = cloneMyDoggo(cloneAnimal); // Least specific

      static Dog cloneMyDoggo(Clone<Dog> cloner) {
        return cloner(new Dog.GermanShepherd());
      }
    }
    
    #endregion

    static void brokenInvarianceDemo() {
      // Arrays in C# are always covariant.
      Dog.Bulldog[] bulldogs = { new Dog.Bulldog() };
      replaceFirstDog(bulldogs);
      Dog.Bulldog dog = bulldogs[0];

      static void replaceFirstDog(Dog[] dogs) {
        dogs[0] = new Dog.Pug();
      }
    }
  }
}