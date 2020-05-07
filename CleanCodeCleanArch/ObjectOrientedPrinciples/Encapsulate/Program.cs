using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Encapsulate
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
		}
	}

	internal class MoviesRepository
	{
		public async Task<Movie> GetById(int id)
		{
			HttpMessageHandler handler = new HttpClientHandler();
			using (var client = new HttpClient(handler))
			{
				var response = await client.GetAsync($"{Configuration.MoviesRepositoryAddress}/{id}");
				return JsonConvert.DeserializeObject<Movie>(await response.Content.ReadAsStringAsync());
			}
		}

		public async Task<Movie> GetByName(string name)
		{
			HttpMessageHandler handler = new HttpClientHandler();
			using (var client = new HttpClient(handler))
			{
				var response = await client.GetAsync($"{Configuration.MoviesRepositoryAddress}/{name}");
				return JsonConvert.DeserializeObject<Movie>(await response.Content.ReadAsStringAsync());
			}
		}
	}

	internal class RepositoryClient
	{
		private readonly MoviesRepository _moviesRepository;

		public RepositoryClient(MoviesRepository moviesRepository)
		{
			_moviesRepository = moviesRepository;
		}
	}

	internal class Movie
	{
	}

	internal class Configuration
	{
		public static string MoviesRepositoryAddress { get; set; }
	}
}
