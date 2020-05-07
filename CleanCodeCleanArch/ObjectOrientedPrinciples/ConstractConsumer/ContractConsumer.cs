namespace ContractConsumer
{
	using Contract;

	public class ContractConsumer
	{
		private readonly IContract _contract;

		public ContractConsumer(IContract contract)
		{
			_contract = contract;
		}

		public void DoWork()
		{
		}
	}
}
