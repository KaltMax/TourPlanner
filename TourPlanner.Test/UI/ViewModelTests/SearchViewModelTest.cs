using NSubstitute;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels;

namespace TourPlanner.Test.UI.ViewModelTests
{
    [TestFixture]
    public class SearchViewModelTests
    {
        private ITourDataService _mockTourDataService;
        private SearchViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            // Create substitute for dependency
            _mockTourDataService = Substitute.For<ITourDataService>();

            // Create view model
            _viewModel = new SearchViewModel(_mockTourDataService);
        }

        [Test]
        public void CanSearch_ShouldReturnTrue_WhenSearchQueryIsNotEmpty()
        {
            // Arrange
            _viewModel.SearchQuery = "Vienna";

            // Act
            bool canExecute = _viewModel.SearchCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.True);
        }

        [Test]
        public void CanSearch_ShouldReturnFalse_WhenSearchQueryIsEmpty()
        {
            // Arrange
            _viewModel.SearchQuery = "";

            // Act
            bool canExecute = _viewModel.SearchCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.False);
        }

        [Test]
        public void SearchCommand_ShouldCallSearchToursAndSetIsSearching()
        {
            // Arrange
            _viewModel.SearchQuery = "Vienna";

            // Act
            _viewModel.SearchCommand.Execute(null);

            // Assert
            _mockTourDataService.Received(1).SearchTours("Vienna");
            Assert.That(_viewModel.IsSearching, Is.True);
        }

        [Test]
        public void CanResetSearch_ShouldReturnTrue_WhenIsSearchingIsTrue()
        {
            // Arrange
            _viewModel.IsSearching = true;

            // Act
            bool canExecute = _viewModel.ResetSearchCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.True);
        }

        [Test]
        public void ResetSearchCommand_ShouldCallResetSearchAndClearState()
        {
            // Arrange
            _viewModel.SearchQuery = "Vienna";
            _viewModel.IsSearching = true;

            // Act
            _viewModel.ResetSearchCommand.Execute(null);

            // Assert
            _mockTourDataService.Received(1).ResetSearch();
            Assert.That(_viewModel.SearchQuery, Is.EqualTo(""));
            Assert.That(_viewModel.IsSearching, Is.False);
        }
    }
}