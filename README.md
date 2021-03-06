# Genetic Algorithm Sharp
A genetic algorithm library with C#.

## Introduction
Genetic algorithm(GA) is a optimal value finding algroithm which imitate biological evolution in nature. 

All variables has been converted to the "Chromosome", which like the real chromosome in the nature. 
It can storage the information. In GA, a chromosome means a solve value of the function.

"Selection", which contain a couple of chromosomes, can express a group of solve of the function.Evolution

GA will find the best selection in the population, which is the set of selection.
Population has been initialized on start with random chromosomes. It means, at first, the population is chaotic.
Then the GA will go into interation.
In each interation, GA will do these steps on population:
1. Evolution (To evolute each selection with a **loss function**. Loss function return the fitnrss of the solve group.
For example, if you want to find a minimum value of function f(x) = x^2, you can set the loss function l(x) = -x^2. 
When the solve value more close to the minimum value, the loss function l(x) will more higher, it means the fitness is more higher.)
2. Select (To select the selections which satisfactory or close to the best solve. Selection can fix the population to avoid it too large,
and only reserve the best selections. There can be a reserve count and a threshold,
all of selections dissatisfy requirement will be remove from the population.)
3. Crossover (To cross and swap the chromosomes of two selection by some algorithm. GA will select two selections randomly, 
and select there's chromosome fragments randomly, to generate a filial generation selections by mix the chromosome fragments selected. 
After all crossover, population will be replace by filial generation.)
4. Mutation (Change some chromosome fragments randomly with a probability. This operation can import uncertainty, 
that can let GA find a better solve without value locking.)
5. Return to step 1 and redo the steps, until the solve under the precision has been found.

## C# Functions Description
Genetic Algorithm Sharp library provide some functions that you can use it easily in your applictation.

### Classes
* (initial)Chromosome: a chromosome contains some DNA fragments
* (initial)Selection: a selection contains some chromosom
* (Attribute)ChromosomeSerializableAttribute: A class attribute, add it to your value class, 
to set the number of chromosome and DNA chain length for encoding.
* ChromosomeSeriable<T>: Contains encoding and decoding function to convert the value class to selection or inverse.
* GeneticAlgorithm<T>: Main algorithm class.
  
### Attribute
**ChromosomeSerializableAttribute** 
#### Grammar
```csharp
     [ChromosomeSerializable(ChromosomeCount = ..., DNAChainLength = new int[] { ..., ..., ... })]
     class MyClass : ChromosomeSeriable<MyClass>
     {
        //...Class codes
     }
```
#### Parameters
ChromosomeCount: Number of chromosome in the selsction serialized by class. In general, how many variables is the class have then how many chrosomes it need.

DNAChainLength: A array of integer to set DNA chain length of each chromosome. This array's length must be equal of ChromosomeCount. A length of DNA is a byte, can be store data from 0~255(or 8 bits binary). For example, if you want to storage a 32-bits float number, you need at least 4 bytes to storage it, so DNA length can be set greater than 4.


  
  
