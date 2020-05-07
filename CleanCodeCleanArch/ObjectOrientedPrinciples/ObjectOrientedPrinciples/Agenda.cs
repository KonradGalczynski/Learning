namespace ObjectOrientedPrinciples
{
	public class Agenda
	{
		// 1. Program with interfaces
		//		- interfaces impose contract
		//		- depend on abstraction not concrete
		//		- huge impact on testability
		//		- dependency inversion - compilation/runtime dependency

		// 2. Favor aggregation over inheritance
		//		- coupling: A: weak - I: strong
		//		- behavior reusability: the same
		//		- encapsulation: A: strong - I: weak
		//		- OCP: A: complex - I: simple
		//		- liskov substitution principle
		//		- bad usage of inheritance - code inheritance instead of behavior inheritance
		//		- testability of inheritance - where to test code in base class?
		//		- testability of composition

		// 3. Encapsulate what is changing
		//		- expose what is needed by client - nothing more(exception is preparing library)
		//		- carve out subcomponents with well defined api
		//		- if you have some problems with testing private methods -> extract
	}
}
