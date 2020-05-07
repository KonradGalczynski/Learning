using System;
using System.Linq;

namespace Liskov
{
	class Program
	{
		static void Main(string[] args)
		{
			SumCalculator1 sumCalculator1 = new SumCalculator1(new[] { 1, 2, 3 });
			SumCalculator1 evenNumbersSumCalculator1 = new EvenNumbersSumCalculator1(new []{ 1, 2, 3});
			Console.WriteLine($"{sumCalculator1.Calculate()}");
			Console.WriteLine($"{evenNumbersSumCalculator1.Calculate()}");

			SumCalculatorBase sumCalculator2 = new SumCalculator2(new[] { 1, 2, 3 });
			SumCalculatorBase evenNumbersSumCalculator2 = new EvenNumbersSumCalculator2(new[] { 1, 2, 3 });
			Console.WriteLine($"{sumCalculator2.Calculate()}");
			Console.WriteLine($"{evenNumbersSumCalculator2.Calculate()}");

			Console.ReadKey();
		}
	}

	public class SumCalculator1
	{
		protected readonly int[] Numbers;

		public SumCalculator1(int[] numbers)
		{
			Numbers = numbers;
		}

		public virtual int Calculate() => Numbers.Sum();
	}

	public class EvenNumbersSumCalculator1 : SumCalculator1
	{
		public EvenNumbersSumCalculator1(int[] numbers)
			: base(numbers)
		{
		}

		public override int Calculate() => Numbers.Where(x => x % 2 == 0).Sum();
	}
























	public abstract class SumCalculatorBase
	{
		protected readonly int[] Numbers;

		protected SumCalculatorBase(int[] numbers)
		{
			Numbers = numbers;
		}

		public abstract int Calculate();
	}

	public class SumCalculator2 : SumCalculatorBase
	{
		public SumCalculator2(int[] numbers)
		: base(numbers)
		{
		}

		public override int Calculate() => Numbers.Sum();
	}

	public class EvenNumbersSumCalculator2 : SumCalculatorBase
	{
		public EvenNumbersSumCalculator2(int[] numbers)
			: base(numbers)
		{
		}

		public override int Calculate() => Numbers.Where(x => x % 2 == 0).Sum();
	}
}
