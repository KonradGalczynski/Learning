## Missing aspect

Probably every single one of us writes at least one extension method. They are extremely useful as they enable us to add functionality to existing types without creating derived types, recompiling existing types, and even modifying them. Even though they are static methods they can be called in the same way as if they were instance methods. Typical scenarios when we would like to use them are:

- adding functionality to collections
- adding functionalities to Domain Entities or Data Transfer Objects
- adding functionalities to predefined system types

When we read guidelines of extension methods we usually see much information:

- how to use them
- in which namespace they should be put
- how to be prepared for changing contracts of an extended type

However one aspect of extension methods is often omitted - testability. One thing is the testability extension method on the unit level. The second thing is the testability of the code which is using an extension method. In the following paragraphs I will explain to you what impact can extension method has on the testability of your code on a unit level. I have divided extension methods into three buckets: good citizens, neutral guys and mighty villains. Lets meet these guys.

## Good citizen
Extension methods which are good citizens are easy to unit test. What is more the code which is using them is also easy to test on the unit level. So let's take a look at the example of good citizens.
The first extension method is checking whether the HTTP status code indicates success or not.

```
internal static bool IsSuccess(this HttpStatusCode httpStatusCode)
{
    return ((int)httpStatusCode >= 200) && ((int)httpStatusCode <= 299);
}
```

In order to test this method on unit level we can write following test:

```
[TestCase(HttpStatusCode.SwitchingProtocols, false)]
[TestCase(HttpStatusCode.OK, true)]
[TestCase(HttpStatusCode.Created, true)]
[TestCase(HttpStatusCode.Ambiguous, false)]
public void ShouldCorrectlyDecideWhetherHttpStatusCodeIsSuccessful(HttpStatusCode httpStatusCode, bool expectedResult)
{
    var result = httpStatusCode.IsSuccess();

    result.Should().Be(expectedResult);
}
```
This was indeed a piece of cake. Let's have a look a the testability of the code which is using the `IsSuccess` extension method. Below there is a very simple class with one public method which uses the `IsSuccess` extension method. This method is used to retrieve some data over the network. In the case of unsuccessfully response error is logged and an empty object with data is returned. Please bear in mind that this implementation is oversimplified to show the testability aspect.

```
public class SomeDataRetriever
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public SomeDataRetriever(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SomeData> RetrieveSomeData(Uri requestUri)
    {
        var response = await _httpClient.GetAsync(requestUri);
        if (!response.StatusCode.IsSuccess())
        {
            _logger.Error("Error message");
            return new SomeData { IsEmpty = true };
        }

        return JsonConvert.DeserializeObject<SomeData>(await response.Content.ReadAsStringAsync());
    }
}
```

To test this class on the unit level we need to mock both `HttpClient` and `ILogger`. `IsSuccess` method cannot be mocked so despite the fact we are testing `SomeDataRetriever` on a unit level we will be also testing the behavior of the `IsSuccess` method. We need to know this trait of extension methods - when classes which are using extension methods are unit tested not only the logic of these classes are tested. Extension methods logic is also tested. As a result unit tests for class consuming extension methods can fail and the reason for this could be an error in extension method implementation. This is one of the negative impacts of extension methods on the testability of the classes which are using them. Having that said let's write a unit test for unsuccessfully retrieving data with `SomeDataRetriever`.

```
[Test]
public void ShouldLogErrorAndReturnEmptyDataWhenRetrievingWasUnsuccessful()
{
    var handlerMock = new MockHttpMessageHandler();
    var httpClient = new HttpClient(handlerMock);
    var logger = Substitute.For<ILogger>();
    var uut = new SomeDataRetriever(httpClient, logger);

    var result = uut.RetrieveSomeData(new Uri("http://test.com"));

    result.Result.IsEmpty.Should().BeTrue();
    logger.Received(1).Error("Error message");
}

public class MockHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
        });
    }
}
```

Unit testing of `SomeDataRetriever` is relatively simple. We need to create a mock for `HttpMessageHandler` as there is no possibility to mock `HttpClient`. Our `MockHttpMessageHandler` is returning `HttpResponseMessage` with status code set to 500 (`BadRequest`) so that we can test path for unsuccessful data retrieving. We must do so because we cannot mock the extension method and the actual implementation of `IsSuccess` will be run in this scenario.

## Neutral guy
Neutral guy are just neutral in terms of testability. You can test both extension method and code which is using it on unit level. You can ask what is the difference then between normal guy and good citizen? The difference is that usually it takes more effort to test code which is using normal guys extension methods.
Example of extension method which behaves like a neutral guy in terms of testability is presented below:

```
public static async Task<JobSummary> RunJobAsync(this IJobRestClient client, JobRequest job, CancellationToken token)
{
    var jobId = await client.PostJobAsync(job, token);
    var summary = await client.GetJobAsync(jobId, token);

    while (!summary.IsRunning)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(100), token);
        summary = await client.GetJobAsync(jobId, token);
    }

    return summary;
}
```

It was written to extend `IJobRestClient` which is used to start some jobs in remote location. When job start is requested, job is queued. When resources are available job is transferred to running state. When job finishes execution it is moved to completed state. `IJobRestClient` has only two methods:

- `PostJobAsync` which sends request to start new job
- `GetJobAsync` to retrieve summary of a job
This extension method sends request to start new job and returns when job is running (it is waiting for the time when job is queued). It helps simplify client code which is interested only in jobs which are running at the moment.

Let's write some unit tests for it.

```
[Test]
public async Task ShouldReturnCorrectSummaryWhenJobIsRunningRightAfterPost()
{
    var jobRestClient = Substitute.For<IJobRestClient>();
    var jobRequest = new JobRequest();
    var token = CancellationToken.None;
    const string jobId = "testJobId";
    jobRestClient.PostJobAsync(jobRequest, token).Returns(jobId);
    var jobSummary = new JobSummary
    {
        IsRunning = true
    };
    jobRestClient.GetJobAsync(jobId, token).Returns(jobSummary);

    var result = await jobRestClient.RunJobAsync(jobRequest, token);

    result.Should().Be(jobSummary);
    await jobRestClient.Received(1).GetJobAsync(jobId, token);
}
```


        		<!-- public static IEnumerable<SourceInstanceDto> AsSourceInstanceDtos(this List<RelativityObject> objects)
		{
			foreach(var obj in objects ?? Enumerable.Empty<RelativityObject>())
			{
				yield return GetResponseObject(obj);
			}
		} -->

## Mighty villain