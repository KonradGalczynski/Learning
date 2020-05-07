using Xunit;

namespace TestabilityComparison
{
	public class UnitTest1
	{
		[Fact]
		public void SampleTest()
		{

		}
	}


	internal abstract class MovieSelectorBase<T>
	{
		private readonly IValidator _validator;
		private readonly IMovieRepository _movieRepository;

		protected MovieSelectorBase(IValidator validator, 
			IMovieRepository movieRepository)
		{
			_validator = validator;
			_movieRepository = movieRepository;
		}

		protected Movie GetById(int id)
		{
			_validator.IsValid(id);
			return _movieRepository.GetById(id);
		}

		public abstract T GetFormattedMovieData(int id);
	}

	internal class JsonFormatMovieSelector : MovieSelectorBase<JsonMovie>
	{
		private readonly IMovieConverter _movieConverter;

		public JsonFormatMovieSelector(IValidator validator,
			IMovieRepository movieRepository,
			IMovieConverter movieConverter)
			:base(validator, movieRepository)
		{
			_movieConverter = movieConverter;
		}

		public override JsonMovie GetFormattedMovieData(int id)
		{
			var movie = GetById(id);
			return _movieConverter.ToJson(movie);
		}
	}

	internal class CsvFormatMovieSelector : MovieSelectorBase<CsvMovie>
	{
		private readonly IMovieConverter _movieConverter;

		public CsvFormatMovieSelector(IValidator validator,
			IMovieRepository movieRepository,
			IMovieConverter movieConverter)
			: base(validator, movieRepository)
		{
			_movieConverter = movieConverter;
		}

		public override CsvMovie GetFormattedMovieData(int id)
		{
			var movie = GetById(id);
			return _movieConverter.ToCsv(movie);
		}
	}

	internal interface IMovieConverter
	{
		CsvMovie ToCsv(Movie movie);
		JsonMovie ToJson(Movie movie);
	}

	internal interface IMovieRepository
	{
		Movie GetById(in int id);
	}

	internal interface IValidator
	{
		void IsValid(in int id);
	}

	public class Movie
	{
	}

	public class JsonMovie
	{
	}

	internal class CsvMovie
	{
	}
}
