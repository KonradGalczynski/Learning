namespace ConcreteConsumer
{
	using ContractImplementer;

	public class ConcreteConsumer
	{
		private readonly ContractImplementer _contractImplementer;

		public ConcreteConsumer(ContractImplementer contractImplementer)
		{
			_contractImplementer = contractImplementer;
		}

		public void DoWork()
		{
			_contractImplementer.DoContractualWork();
		}
	}
}
