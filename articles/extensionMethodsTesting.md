## Missing aspect

Probably every single one of us writes at least one extension method. They are extremely useful as they enable us to add functionality to existing types without creating derived types, recompiling existing types, and even modifying them. Even though they are static methods they can be called in the same way as if they were instance methods. Typical scenarios when we would like to use them are:

- adding functionality to collections
- adding functionalities to Domain Entities or Data Transfer Objects
- adding functionalities to predefined system types

When we read guidelines of extension methods we usually see much information:

- how to use them
- in which namespace they should be put
- how to be prepared for changing contracts of an extended type

However one aspect of extension methods is often omitted - testability. One thing is the testability extension method on the unit level. The second thing is the testability of the code which is using an extension method. In the following paragraphs I will explain to you what impact can extension method has on the testability of your code on a unit level. I have divided extension methods into three buckets: good citizens, neutral guys, and mighty villains. Let's meet these guys.

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
Neutral guys are just neutral in terms of testability. You can test both the extension method and code which is using it on the unit level. You can ask what is the difference then between a normal guy and a good citizen? The difference is that usually it takes more effort to test code which is using normal guys extension methods.
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

It was written to extend `IJobRestClient` which is used to start some jobs in a remote location. When a job start is requested, the job is queued. When resources are available to start a job, it is transferred to a running state. When a job finishes execution it is moved to a completed state. `IJobRestClient` has only two methods:

- `PostJobAsync` which sends a request to start a new job
- `GetJobAsync` to retrieve a summary of a job
This extension method sends a request to start a new job and returns when the job is running (it is waiting for the time when the job is queued). It helps simplify client code which is interested only in jobs that are running at the moment.

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
As you can see testing this extension method is not difficult. We are creating mock for the extended interface, create input data, execute the extension method, and assert on results. Nothing fancy here. Let's move to client code which is using this extension method.

```
public class ReportingJobRunner
{
    private readonly IJobRestClient _jobRestClient;
    private readonly IReportingSink _reportingSink;

    public ReportingJobRunner(IJobRestClient jobRestClient, IReportingSink reportingSink)
    {
        _jobRestClient = jobRestClient;
        _reportingSink = reportingSink;
    }

    public async Task RunJobAsync(JobRequest job, CancellationToken token)
    {
        var summary = await _jobRestClient.RunJobAsync(job, token);
        _reportingSink.Report(summary);
    }
}
```
This is a very simple decorator for `IJobRestClient` which apart from running a job with extension method reports its status to some sink (for example remote network location). Unit testing for such a simple decorator should be pretty easy, yes? It is not so easy as the extension method `RunJobAsync` cannot be mocked. The only solution for this is to mock `IJobRestClient` in a way that guarantees expected `RunJobAsync` behavior. Having that said let's write a unit test that checks if a proper summary is reported.

```
[Test]
public async Task ShouldReportJobSummaryToGivenSink()
{
    var jobRestClient = Substitute.For<IJobRestClient>();
    var reportingSink = Substitute.For<IReportingSink>();
    var uut = new ReportingJobRunner(jobRestClient, reportingSink);
    var jobRequest = new JobRequest();
    var token = CancellationToken.None;
    const string jobId = "testJobId";
    jobRestClient.PostJobAsync(jobRequest, token).Returns(jobId);
    var jobSummary = new JobSummary
    {
        IsRunning = true
    };
    jobRestClient.GetJobAsync(jobId, token).Returns(jobSummary);

    await uut.RunJobAsync(jobRequest, token);

    reportingSink.Received(1).Report(jobSummary);
}
```

Interestingly, the code inside the test is far more complicated than the code itself even though the tested method has two lines of code. The fact that it is not possible to mock the extension method is the reason for such a situation. To mock the behavior of `RunJobAsync` we need to know how it is implemented (we must take a look at its implementation). Then we must mock extended type `IJobRestClient` accordingly to have control over how our code behaves in the test. One thing to mention is that test code for testing `ReportingJobRunner` is very similar to test code for testing `RunJobAsync` extension method as we need to set up the same behavior for mocked extended type.

## Mighty villain

Now it is time to meet them. Mighty villains. Extension method which are hard or impossible to test and which makes client code very hard or impossible to test on a unit level. There is an example of the villain extension method presented below.

```
public static IStorageClient GetHttpStorageClient(this IHttpClientFactory httpClientFactory, Options options)
{
    var clientBootstrapper = new Bootstrapper(httpClientFactory);
    return clientBootstrapper.CreateHttpStorageClient(options);
}
```

This method extends `IHttpClientFactory` so that HttpStorageClient can be created. Even though this method has only two lines what makes it mighty villain is the first line of its body - the line in which a new operator is used. `Bootstrapper` is not injected but created inside the extension method and this leads to problems with testing this method and impossibility to test client code which is using this method. The only unit test for this method which we can write is presented below.

```
public void ShouldCreateHttpStorageClientWhenRequested()
{
    var httpClientFactory = Substitute.For<IHttpClientFactory>();
    var options = new Options
    {
        CorrelationIdProvider = () => Guid.NewGuid().ToString(),
        UseDefaultLogging = true
    };

    var result = httpClientFactory.GetHttpStorageClient(options);

    result.Should().BeOfType<HttpStorageClient>();
    result.Should().BeAssignableTo<IStorageClient>();
}
```

We can only assert the type of returned object. We cannot check if options were correctly passed or if an extended HTTP client factory was called at all during the process of httpStorageClient creation. Despite having 100% code coverage we do not know if our extension method behaves correctly.
Let's now move to the code of a class that is using this extension method.

```
public class HttpDataRetriever
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpDataRetriever(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Data GetData()
    {
        var options = new Options
        {
            UseDefaultLogging = false
        };
        var httpStorageClient = _httpClientFactory.GetHttpStorageClient(options);
        return httpStorageClient.ReadAll();
    }
}
```

Unit testing `GetHttpStorageClient` extension method was hard and the test had some limitations. But unit testing code which is using `GetHttpStorageClient` is impossible. We cannot mock the creation of `Bootstrapper` object as it is created directly in the extension method body. By writing `GetHttpStorageClient` is presented the way we impacted unit testing of every single client codebase which will be using it. This impact is huge and negative. `GetHttpStorageClient` is mighty villain indeed.

## Wrap up

You have seen some examples of extension methods, how they can be tested, and how they impact testability of code in which they are consumed. If you want to write extension methods which are always good citizens following rules are for you:

- keep your extension methods simple - they should not be complicated
- create extension methods to enrich types not to alter their behavior
- do not create a new object inside extension methods
- remember about code which will be using it - if you will be able to unit test it means you've done a great job

Happy extending and testing!
