using Contract;
using NSubstitute;
using Xunit;

namespace Testability
{
	public class Testability
	{
		[Fact]
		public void TestWithDependencyOnConcrete()
		{
			var imposeContract = new ContractImplementer.ContractImplementer();

			var sut = new ConcreteConsumer.ConcreteConsumer(imposeContract);

			sut.DoWork();
			// Assert that something ...

			// what about complicated logic inside ImposeContract?
			// consider adding code for testing?
		}

		[Fact]
		public void TestWithoutDependencyOnConcrete()
		{
			var imposeContract = Substitute.For<IContract>();

			var sut = new ContractConsumer.ContractConsumer(imposeContract);

			sut.DoWork();

			// Assert that something ...
			imposeContract.Received().DoContractualWork();
		}
	}
}