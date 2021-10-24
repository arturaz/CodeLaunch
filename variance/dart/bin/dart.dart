/// Data model
abstract class Animal {}
abstract class Dog extends Animal {}
class Cat extends Animal {}

class Bulldog extends Dog {}
class BerneseMountainDog extends Dog {}
class GermanShepherd extends Dog {}
class Pug extends Dog {}
class Poodle extends Dog {}

void main(List<String> arguments) {
  print("Covariance");
  covarianceDemo();

  print("Contravariance");
  contravarianceDemo();

  print("Invariance");
  invarianceDemo();

  print("Broken arrays");
  brokenInvarianceDemo();
}

//region Covariance demo

/// Covariant
abstract class Create<A> {
  A create(int index);
}

class CreatePoodle extends Create<Poodle> {
  @override Poodle create(int index) => Poodle();
}
class CreateDog extends Create<Dog> {
  @override Dog create(int index) => index < 3 ? Poodle() : Bulldog();
}
class CreateAnimal extends Create<Animal> {
  @override Animal create(int index) => Cat();
}
void covarianceDemo() {
  Create<Poodle> poodleCreator = CreatePoodle();
  Create<Dog>    dogCreator    = CreateDog();
  Create<Animal> animalCreator = CreateAnimal();

  List<Dog> poodles = puppyGenerator(5, poodleCreator); // Most specific
  List<Dog> dogs    = puppyGenerator(5, dogCreator);    // Mid-specific
  List<Dog> animals = puppyGenerator(5, animalCreator); // Least specific
}
List<Dog> puppyGenerator(int count, Create<Dog> create) {
  return List.generate(count, (index) => create.create(index));
}

//endregion

//region Contravariance demo

/// Contravariant
abstract class Pet<A> {
  void pet(A a);
}

class PetPoodle extends Pet<Poodle> {
  @override void pet(Poodle poodle) { /* make poodles very happy */ }
}
class PetDog extends Pet<Dog> {
  @override void pet(Dog dog) { /* make any doggo happy */ }
}
class PetAnimal extends Pet<Animal> {
  @override void pet(Animal a) { /* make any animal happy, may it be a dog or a cat */ }
}
// class PetAnimalAsDog extends Pet<Dog> {
//   PetAnimal doPet;
//
//   @override void pet(Dog a) { doPet.pet(a); }
// }
void contravarianceDemo() {
  Pet<Poodle> petPoodle = PetPoodle();
  Pet<Dog>    petDog    = PetDog();
  Pet<Animal> petAnimal = PetAnimal();

  pettingZoo(petPoodle); // Most specific
  pettingZoo(petDog);    // Mid-specific
  pettingZoo(petAnimal); // Least specific
}
void pettingZoo(Pet<Dog> pet) {
  pet.pet(GermanShepherd());
}

//endregion

//region Invariance demo

/// Invariant
abstract class Clone<A> {
  A clone(A a);
}

class ClonePoodle extends Clone<Poodle> {
  @override Poodle clone(Poodle a) => a;
}
class CloneDog extends Clone<Dog> {
  @override Dog clone(Dog a) => a;
}
class CloneAnimal extends Clone<Animal> {
  @override Animal clone(Animal a) => a;
}
void invarianceDemo() {
  Clone<Poodle> clonePoodle = ClonePoodle();
  Clone<Dog>    cloneDog    = CloneDog();
  Clone<Animal> cloneAnimal = CloneAnimal();

  Dog poodle = cloneMyDoggo(clonePoodle); // Most specific
  Dog dog    = cloneMyDoggo(cloneDog);    // Mid-specific
  Dog animal = cloneMyDoggo(cloneAnimal); // Least specific
}
Dog cloneMyDoggo(Clone<Dog> cloner) {
  return cloner.clone(GermanShepherd());
}

//endregion

void brokenInvarianceDemo() {
  // Arrays in Dart are always covariant.
  List<Bulldog> bulldogs = [Bulldog()];
  replaceFirstDog(bulldogs);
  Bulldog dog = bulldogs[0];
}
void replaceFirstDog(List<Dog> dogs) {
  dogs[0] = Pug();
}