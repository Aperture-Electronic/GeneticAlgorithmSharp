using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GeneticAlgorithm
{
    /// <summary>
    /// 染色体
    /// </summary>
    public class Chromosome
    {
        /// <summary>
        /// 返回或设置组成该染色体的DNA链数据（表达链数据）
        /// </summary>
        public byte[] dnaChain;

        /// <summary>
        /// 返回组成该染色体的DNA链的长度
        /// </summary>
        public int Length => dnaChain.Length;

        /// <summary>
        /// 以一组DNA数据初始化染色体
        /// </summary>
        /// <param name="initialize">组成该染色体的DNA数据</param>
        public Chromosome(byte[] initialize)
        {
            dnaChain = initialize;
        }
    }

    /// <summary>
    /// 个体
    /// </summary>
    internal class Selection
    {
        /// <summary>
        /// 返回或设置该个体所含有的染色体组
        /// </summary>
        public Chromosome[] chromosomes;

        /// <summary>
        /// 返回或设置该个体量化适应度
        /// </summary>
        public double Fitness { get; set; }

        /// <summary>
        /// 以一组染色体初始化个体
        /// </summary>
        /// <param name="initialize">组成该个体的染色体组</param>
        public Selection(Chromosome[] initialize)
        {
            chromosomes = initialize;
        }
    }

    /// <summary>
    /// 可被序列化为染色体的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ChromosomeSerializableAttribute : Attribute
    {
        /// <summary>
        /// 染色体数目
        /// </summary>
        public int ChromosomeCount;

        /// <summary>
        /// 每个染色体上DNA序列长度
        /// </summary>
        public int[] DNAChainLength;
    }

    /// <summary>
    /// 染色体序列化方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChromosomeSeriable<T>
    {
        /// <summary>
        /// 获得对象的染色体
        /// </summary>
        /// <returns></returns>
        public static List<byte[]> GetDNAChain(T Source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获得染色体的对象
        /// </summary>
        /// <param name="Chromosomes">染色体</param>
        /// <returns></returns>
        public static T GetMetaObject(Chromosome[] Chromosomes)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 遗传算法
    /// </summary>
    /// <typeparam name="T">种群基类</typeparam>
    public class GeneticAlgorithm<T> where T : ChromosomeSeriable<T>
    {
        /// <summary>
        /// 种群
        /// </summary>
        private List<Selection> Population = new List<Selection>();
        private readonly int defaultChromosomeCount;
        private readonly int[] dnaChainLength;

        /// <summary>
        /// 遗传算法
        /// </summary>
        public GeneticAlgorithm()
        {
            // 获得对应的染色体特性
            ChromosomeSerializableAttribute chromosomeAttribute = Attribute.GetCustomAttribute(typeof(T), typeof(ChromosomeSerializableAttribute)) as ChromosomeSerializableAttribute;

            if (chromosomeAttribute is null)
            {
                throw new ArgumentNullException("对应类不属于可转化成染色体的类");
            }

            defaultChromosomeCount = chromosomeAttribute.ChromosomeCount;
            dnaChainLength = chromosomeAttribute.DNAChainLength;
        }

        /// <summary>
        /// 初始化种群
        /// </summary>
        /// <param name="initializePopulation">初始种群数量</param>
        /// <param name="ramdomInitializer">随机初始化器</param>
        public void Initialize(int initializePopulation, Func<int, byte[]> ramdomInitializer)
        {
            Population = new List<Selection>();

            // 创建种群
            for (int i = 0; i < initializePopulation; i++)
            {
                // 创建染色体
                Chromosome[] chromosomes = new Chromosome[defaultChromosomeCount];

                // 创建DNA序列
                for (int j = 0; j < defaultChromosomeCount; j++)
                {
                    byte[] init = ramdomInitializer(j);
                    if (init.Length != dnaChainLength[j])
                    {
                        throw new Exception("随机种群初始化返回了错误的DNA序列");
                    }

                    chromosomes[j] = new Chromosome(init);
                }

                Selection selection = new Selection(chromosomes);

                Population.Add(selection);
            }
        }

        /// <summary>
        /// 评价种群
        /// </summary>
        /// <param name="evaluateFunction"></param>
        public void Evaluate(Func<List<T>, double[]> evaluateFunction)
        {
            // 利用反射获得染色体转换器静态方法
            MethodInfo GetMetaObject = typeof(T).GetMethod(nameof(ChromosomeSeriable<T>.GetMetaObject), BindingFlags.Static | BindingFlags.Public);

            List<T> targets = new List<T>();
            foreach (Selection selection in Population)
            {
                targets.Add(GetMetaObject.Invoke(null, new object[] { selection.chromosomes }) as T);
            }

            double[] eval = evaluateFunction(targets);

            for (int i = 0; i < targets.Count; i++)
            {
                Population[i].Fitness = eval[i];
            }
        }

        /// <summary>
        /// 选择种群
        /// </summary>
        /// <param name="reserve">幸存数</param>
        /// <param name="threshold">阈值</param>
        public void Select(int reserve, double? threshold = null)
        {
            // 选择评价最高的个体，其余淘汰
            // 若设置了阈值，则淘汰阈值以下的个体
            Population.Sort((Selection a, Selection b) => b.Fitness.CompareTo(a.Fitness));

            // 保留个体
            Population = Population.Take(reserve).ToList();

            // 阈值
            if (threshold != null)
            {
                Population = Population.Where((Selection s) => s.Fitness > threshold).ToList();
                int c = Population.Count; // 阈值处理完后的种群剩余数量

                // 随机复制个体补全种群防止数量过低
                Random random = new Random();

                while (Population.Count < reserve)
                {
                    int index = random.Next(c);
                    Population.Add(new Selection(Population[index].chromosomes));
                }
            }
        }

        /// <summary>
        /// 交叉互换（个体交配）
        /// </summary>
        /// <param name="crossCount">可能的子代个数</param>
        /// <param name="crossFunction">DNA交叉函数</param>
        public void Crossover(int crossCount, Func<byte[], byte[], byte[]> crossFunction)
        {
            Random randomSelector = new Random();
            List<Selection> SubPopulation = new List<Selection>(); // 子代种群

            for (int i = 0; i < crossCount; i++)
            {
                // 随机选取两个亲本
                Selection father = Population[randomSelector.Next(Population.Count)];
                Selection mother = Population[randomSelector.Next(Population.Count)];

                // 染色体交叉互换
                Chromosome[] babyChromosome = new Chromosome[defaultChromosomeCount];
                for (int c = 0; c < defaultChromosomeCount; c++)
                {
                    babyChromosome[c] = new Chromosome(crossFunction(father.chromosomes[c].dnaChain, mother.chromosomes[c].dnaChain));
                }

                // 子代个体
                SubPopulation.Add(new Selection(babyChromosome));
            }

            // 用子代替换亲本
            Population = SubPopulation;
        }

        /// <summary>
        /// 遗传变异
        /// </summary>
        /// <param name="probability">变异概率</param>
        /// <param name="x">变异倍率</param>
        public void Mutation(int[] probability, byte x)
        {
            int p = probability[0];
            int q = probability[1];

            Random random = new Random();
            // 每个个体的每个染色体上的每个DNA片段（碱基对）都有相同的突变概率
            foreach (Selection selection in Population)
            {
                foreach (Chromosome chromosome in selection.chromosomes)
                {
                    for (int i = 0; i < chromosome.Length; i++)
                    {
                        if (random.Next(q) < p)
                        {
                            chromosome.dnaChain[i] += (byte)(2.0 * x * (random.NextDouble() - 0.5));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获得能够表示当前种群的字符串
        /// </summary>
        public string GetPopulationString()
        {
            // 利用反射获得染色体转换器静态方法
            MethodInfo GetMetaObject = typeof(T).GetMethod(nameof(ChromosomeSeriable<T>.GetMetaObject), BindingFlags.Static | BindingFlags.Public);

            string str = "----遗传算法 种群表----\n";
            str += $"种群容量: {Population.Count}, 染色体条数: {defaultChromosomeCount} \nDNA长度: ";
            for (int i = 0; i < defaultChromosomeCount; i++)
            {
                str += dnaChainLength[i].ToString() + ", ";
            }

            str += "\n -----------------------------------------------------------------\n";

            foreach (Selection selection in Population)
            {
                str += $"个体={GetMetaObject.Invoke(null, new object[] { selection.chromosomes })}[";
                foreach (Chromosome chromosome in selection.chromosomes)
                {
                    str += "染色体[";
                    foreach (byte dna in chromosome.dnaChain)
                    {
                        str += dna.ToString() + ", ";
                    }
                    str = str.Trim(new char[] { ',', ' ' }) + "], ";
                }
                str = str.Trim(new char[] { ',', ' ' }) + $"], 量化评价 = {selection.Fitness}";
            }

            return str;
        }

        /// <summary>
        /// 获得能够表示当前最佳个体的字符串
        /// </summary>
        /// <returns></returns>
        public string GetBestSelectionString()
        {
            // 利用反射获得染色体转换器静态方法
            MethodInfo GetMetaObject = typeof(T).GetMethod(nameof(ChromosomeSeriable<T>.GetMetaObject), BindingFlags.Static | BindingFlags.Public);

            double maxFitness = Population.Max((Selection s) => s.Fitness);
            Selection best = Population.Find((Selection s) => s.Fitness == maxFitness);
            string str = $"个体={GetMetaObject.Invoke(null, new object[] { best.chromosomes })}[";
            foreach (Chromosome chromosome in best.chromosomes)
            {
                str += "染色体[";
                foreach (byte dna in chromosome.dnaChain)
                {
                    str += dna.ToString() + ", ";
                }
                str = str.Trim(new char[] { ',',' '}) + "], ";
            }
            str =  str.Trim(new char[] { ',', ' ' }) + $"], 量化评价 = {best.Fitness}";
            return str;
        }
    }
}
