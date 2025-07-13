using System.Runtime.CompilerServices;
using NSubstitute;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels;

namespace TourPlanner.Test.UI.ViewModelTests
{
    [TestFixture]
    public class AddNewTourViewModelTests
    {
        private TourDataService _realTourService;
        private IUiCoordinator _mockUiCoordinator;
        private IHttpService _mockHttpService;
        private ISelectedTourService _mockSelectedTourService;
        private IDialogService _mockDialogService;
        private IFileDialogService _mockFileDialogService;
        private AddNewTourViewModel _viewModel;
        private Tour? _lastCreatedTour;

        [SetUp]
        public void Setup()
        {
            _mockHttpService = Substitute.For<IHttpService>();
            _mockUiCoordinator = Substitute.For<IUiCoordinator>();
            _mockSelectedTourService = Substitute.For<ISelectedTourService>();
            _mockDialogService = Substitute.For<IDialogService>();
            _mockFileDialogService = Substitute.For<IFileDialogService>();

            // Simulate backend PostAsync returning a tour with a new ID
            _mockHttpService.PostAsync(Arg.Any<string>(), Arg.Any<Tour>())!
                .Returns(callInfo =>
                {
                    _lastCreatedTour = callInfo.ArgAt<Tour>(1);
                    _lastCreatedTour.Id = Guid.NewGuid(); // simulate ID assignment
                    return Task.FromResult(_lastCreatedTour);
                });

            // Simulate GetAsync returning a list with the created tour
            _mockHttpService.GetAsync<List<Tour>>(Arg.Any<string>())!
                .Returns(_ => Task.FromResult(
                    _lastCreatedTour != null
                        ? new List<Tour> { _lastCreatedTour }
                        : new List<Tour>()
                ));

            _realTourService = new TourDataService(_mockHttpService, _mockDialogService, _mockFileDialogService);
            _viewModel = new AddNewTourViewModel(_realTourService, _mockSelectedTourService, _mockUiCoordinator);
        }

        // Helper method to populate all required fields
        private void PopulateRequiredFields()
        {
            _viewModel.TourName = "Test Tour";
            _viewModel.Description = "A test description";
            _viewModel.From = "Vienna";
            _viewModel.To = "Salzburg";
            _viewModel.TransportType = "Car";
        }

        [Test]
        public void SaveCommand_ShouldBeDisabled_WhenViewModelInitialized()
        {
            Assert.IsFalse(_viewModel.SaveCommand.CanExecute(null));
        }

        [Test]
        public void SaveCommand_ShouldBeEnabled_WhenAllPropertiesAreSet()
        {
            PopulateRequiredFields();
            Assert.IsTrue(_viewModel.SaveCommand.CanExecute(null));
        }

        [Test]
        public async Task SaveCommand_ShouldCreate_And_AddTour_ToTourDataService()
        {
            var initialCount = _realTourService.Tours.Count;
            PopulateRequiredFields();

            await Task.Run(() => _viewModel.SaveCommand.Execute(null));

            Assert.That(_realTourService.Tours.Count, Is.EqualTo(initialCount + 1));
        }

        [Test]
        public async Task SaveCommand_ShouldCloseAddTourView_AfterSaving()
        {
            PopulateRequiredFields();
            await Task.Run(() => _viewModel.SaveCommand.Execute(null));

            _mockUiCoordinator.Received(1).CloseAddTourView();
        }

        [Test]
        public void CancelCommand_ShouldCall_CloseAddTourView()
        {
            _viewModel.CancelCommand.Execute(null);

            _mockUiCoordinator.Received(1).CloseAddTourView();
        }
    }
}