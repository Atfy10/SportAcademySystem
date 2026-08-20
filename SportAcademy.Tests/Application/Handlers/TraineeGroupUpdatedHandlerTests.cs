using Moq;
using SportAcademy.Application.EventHandlers;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Tests.Application.Handlers;

// Regression coverage for the S2 fix: TraineeGroupUpdated was fully wired (interface, hub
// implementation, frontend listener) but no backend event ever triggered it.
// UpdateTraineeGroupCommandHandler now publishes TraineeGroupUpdatedEvent after a successful
// update, and this handler is what turns that into a real-time broadcast.
public class TraineeGroupUpdatedHandlerTests
{
    [Fact]
    public async Task Handle_PublishesTraineeGroupUpdatedToRealtimeService()
    {
        var realtimeServiceMock = new Mock<IRealtimeService>();
        var handler = new TraineeGroupUpdatedHandler(realtimeServiceMock.Object);

        await handler.Handle(new TraineeGroupUpdatedEvent(7), CancellationToken.None);

        realtimeServiceMock.Verify(s => s.TraineeGroupUpdated(7), Times.Once);
    }
}
