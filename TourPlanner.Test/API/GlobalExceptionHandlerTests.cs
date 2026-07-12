using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TourPlanner.API.Middleware;
using TourPlanner.BLL.Exceptions;
using TourPlanner.Logging;

namespace TourPlanner.Test.API
{
    [TestFixture]
    public class GlobalExceptionHandlerTests
    {
        private IProblemDetailsService _problemDetailsService;
        private GlobalExceptionHandler _handler;

        [SetUp]
        public void Setup()
        {
            _problemDetailsService = Substitute.For<IProblemDetailsService>();
            _handler = new GlobalExceptionHandler(
                _problemDetailsService,
                Substitute.For<ILoggerWrapper<GlobalExceptionHandler>>());
        }

        // Invokes the handler and returns the status code set on the response together with
        // the ProblemDetails the handler handed to IProblemDetailsService.
        private async Task<(int StatusCode, ProblemDetails Problem)> InvokeAsync(Exception exception)
        {
            var context = new DefaultHttpContext();
            ProblemDetailsContext? captured = null;
            _problemDetailsService
                .TryWriteAsync(Arg.Do<ProblemDetailsContext>(c => captured = c))
                .Returns(true);

            var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

            Assert.That(handled, Is.True, "The handler should report the exception as handled.");
            Assert.That(captured, Is.Not.Null, "The handler should write a ProblemDetails response.");
            Assert.That(captured!.Exception, Is.SameAs(exception), "The original exception should be forwarded.");
            return (context.Response.StatusCode, captured.ProblemDetails);
        }

        [Test]
        public async Task GeocodingException_MapsTo400()
        {
            var (status, problem) = await InvokeAsync(new GeocodingException("boom", new Exception()));

            Assert.That(status, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(problem.Status, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(problem.Title, Is.EqualTo("Failed to geocode the provided locations."));
        }

        [Test]
        public async Task RoutingException_MapsTo502()
        {
            var (status, problem) = await InvokeAsync(new RoutingException("boom"));

            Assert.That(status, Is.EqualTo(StatusCodes.Status502BadGateway));
            Assert.That(problem.Status, Is.EqualTo(StatusCodes.Status502BadGateway));
            Assert.That(problem.Title, Is.EqualTo("Unable to retrieve routing information."));
        }

        [Test]
        public async Task TourNameAlreadyExistsException_MapsTo409_WithMessage()
        {
            var (status, problem) = await InvokeAsync(new TourNameAlreadyExistsException("Tour name already exists"));

            Assert.That(status, Is.EqualTo(StatusCodes.Status409Conflict));
            Assert.That(problem.Status, Is.EqualTo(StatusCodes.Status409Conflict));
            Assert.That(problem.Title, Is.EqualTo("Tour name already exists"));
        }

        [Test]
        public async Task TourServiceNotFoundException_MapsTo404_WithMessage()
        {
            var (status, problem) = await InvokeAsync(new TourServiceNotFoundException("Tour not found"));

            Assert.That(status, Is.EqualTo(StatusCodes.Status404NotFound));
            Assert.That(problem.Status, Is.EqualTo(StatusCodes.Status404NotFound));
            Assert.That(problem.Title, Is.EqualTo("Tour not found"));
        }

        [Test]
        public async Task TourLogServiceNotFoundException_MapsTo404_WithMessage()
        {
            var (status, problem) = await InvokeAsync(new TourLogServiceNotFoundException("Tour log not found"));

            Assert.That(status, Is.EqualTo(StatusCodes.Status404NotFound));
            Assert.That(problem.Status, Is.EqualTo(StatusCodes.Status404NotFound));
            Assert.That(problem.Title, Is.EqualTo("Tour log not found"));
        }

        [Test]
        public async Task TourServiceException_MapsTo500()
        {
            var (status, problem) = await InvokeAsync(new TourServiceException("Service error"));

            Assert.That(status, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(problem.Status, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(problem.Title, Is.EqualTo("An internal service error occurred."));
        }

        [Test]
        public async Task TourLogServiceException_MapsTo500()
        {
            var (status, problem) = await InvokeAsync(new TourLogServiceException("Service error"));

            Assert.That(status, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(problem.Status, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(problem.Title, Is.EqualTo("An internal error occurred."));
        }

        [Test]
        public async Task UnexpectedException_MapsTo500()
        {
            var (status, problem) = await InvokeAsync(new Exception("Unexpected error"));

            Assert.That(status, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(problem.Status, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(problem.Title, Is.EqualTo("An unexpected error occurred."));
        }
    }
}
