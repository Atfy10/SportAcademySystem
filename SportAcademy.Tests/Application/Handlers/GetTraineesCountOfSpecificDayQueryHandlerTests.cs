using FluentAssertions;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.TraineeQueries.GetTraineesCountOfSpecificDay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Tests.Application.Handlers
{
public class GetTraineesCountOfSpecificDayQueryHandlerTests
    {
        private readonly Mock<ITraineeRepository> _traineeRepoMock = new();
        private readonly GetTraineesCountOfSpecificDayQueryHandler _handler;

        public GetTraineesCountOfSpecificDayQueryHandlerTests()
        {
            _handler = new GetTraineesCountOfSpecificDayQueryHandler(
                _traineeRepoMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsCountSuccessfully()
        {
            var date = new DateTime(2026, 02, 20);
            var query = new GetTraineesCountOfSpecificDayQuery(date);

            _traineeRepoMock
                .Setup(r => r.GetTraineesCountOfSpecificDayAsync(date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(7);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(7);
        }

        [Fact]
        public async Task Handle_PassesCorrectDateToRepository()
        {
            var date = new DateTime(2026, 02, 20);
            var query = new GetTraineesCountOfSpecificDayQuery(date);

            _traineeRepoMock
                .Setup(r => r.GetTraineesCountOfSpecificDayAsync(date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            await _handler.Handle(query, CancellationToken.None);

            _traineeRepoMock.Verify(
                r => r.GetTraineesCountOfSpecificDayAsync(
                    It.Is<DateTime>(d => d == date),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenNoTrainees_ReturnsZero()
        {
            var date = new DateTime(2026, 02, 20);
            var query = new GetTraineesCountOfSpecificDayQuery(date);

            _traineeRepoMock
                .Setup(r => r.GetTraineesCountOfSpecificDayAsync(date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(0);
        }
    }
}
