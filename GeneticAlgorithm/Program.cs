using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithm
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            GeneticAlgorithm<doubleXYGA> geneticAlgorithm = new GeneticAlgorithm<doubleXYGA>();
            geneticAlgorithm.Initialize(100, (i) => doubleXYGA.RandomInitialize());

            for (int i = 0; i < 1000; i++)
            {
                geneticAlgorithm.Evaluate(delegate (List<doubleXYGA> list)
                {
                    double[] fitness = new double[list.Count];

                    // Function
                    IEnumerable<double> func_list = list.Select((doubleXYGA a) =>
                      a.x * a.x - 4.0 * a.x + a.y * a.y + 9.0 * a.y - a.x * a.y - 10.0
                    );

                    int i = 0;
                    foreach (double d in func_list)
                    {
                        fitness[i] = -d;
                        i++;
                    }

                    return fitness;
                });
                geneticAlgorithm.Select(20);

                Console.WriteLine(geneticAlgorithm.GetBestSelectionString());

                geneticAlgorithm.Crossover(100, delegate (byte[] father, byte[] mother)
                {
                    Random random = new Random();
                    byte[] baby = new byte[father.Length];

                    // 单点交叉
                    int index = random.Next(father.Length + 1);
                    baby = father.Take(index).Concat(mother.Skip(index)).ToArray();
                    if (baby.Length == 3)
                    {
                        throw new Exception("");
                    }

                    return baby;
                });
                geneticAlgorithm.Mutation(new int[] { 1, 100 }, 16);

                Console.WriteLine($"迭代[{i}]执行完毕");
            }

            Console.ReadKey();
        }
    }

    [ChromosomeSerializable(ChromosomeCount = 2, DNAChainLength = new int[] { 8, 8 })]
    internal class doubleXYGA : ChromosomeSeriable<doubleXYGA>
    {
        public double x, y;

        public doubleXYGA(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static byte[] RandomInitialize()
        {
            Random random = new Random();
            byte[] init = BitConverter.GetBytes((-random.NextDouble() + 0.5) * 10.0);
            return init;
        }

        public static new List<byte[]> GetDNAChain(doubleXYGA Source)
        {
            List<byte[]> rtn = new List<byte[]>
            {
                BitConverter.GetBytes(Source.x),
                BitConverter.GetBytes(Source.y),
            };

            return rtn;
        }

        public static new doubleXYGA GetMetaObject(Chromosome[] Chromosomes)
        {
            return new doubleXYGA(BitConverter.ToDouble(Chromosomes[0].dnaChain), BitConverter.ToDouble(Chromosomes[1].dnaChain));
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }

    [ChromosomeSerializable(ChromosomeCount = 1, DNAChainLength = new int[] { 8 })]
    internal class doubleGA : ChromosomeSeriable<doubleGA>
    {
        public double data;

        public doubleGA(double data)
        {
            this.data = data;
        }

        public static byte[] RandomInitialize()
        {
            Random random = new Random();
            byte[] init = BitConverter.GetBytes(random.NextDouble() * 10f);
            return init;
        }

        public static new List<byte[]> GetDNAChain(doubleGA Source)
        {
            List<byte[]> rtn = new List<byte[]>
            {
                BitConverter.GetBytes(Source.data)
            };

            return rtn;
        }

        public static new doubleGA GetMetaObject(Chromosome[] Chromosomes)
        {
            return new doubleGA(BitConverter.ToDouble(Chromosomes[0].dnaChain));
        }

        public override string ToString()
        {
            return data.ToString();
        }
    }
}
