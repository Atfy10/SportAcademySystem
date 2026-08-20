using Moq;
using SportAcademy.Application.EventHandlers;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Tests.Application.Handlers;

// Regression coverage for the S2 fix: SessionOccurrenceUpdated was fully wired (interface,
// hub implementation, frontend listener) but no backend event ever triggered it.
// UpdateSessionOccurrenceCommandHandler now publishes SessionOccurrenceUpdatedEvent after a
// successful update, and this handler is what turns that into a real-time broadcast.
public class SessionOccurrenceUpdatedHandlerTests
{
    [Fact]
    public async Task Handle_PublishesSessionOccurrenceUpdatedToRealtimeService()
    {
        var realtimeServiceMock = new Mock<IRealtimeService>();
        var handler = new SessionOccurrenceUpdatedHandler(realtimeServiceMock.Object);

        await handler.Handle(new SessionOccurrenceUpdatedEvent(42), CancellationToken.None);

        realtimeServiceMock.Verify(s => s.SessionOccurrenceUpdated(42), Times.Once);
    }
}
