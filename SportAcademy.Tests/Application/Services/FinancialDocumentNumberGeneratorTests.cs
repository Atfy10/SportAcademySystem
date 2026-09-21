using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;

namespace SportAcademy.Tests.Application.Services;

// Regression coverage for the production failure
//   "Violation of PRIMARY KEY constraint 'PK_Payments' ... The duplicate key value is (PAY-2026-00002)"
// raised while creating a subscription. The per-tenant counter handed out a number that another
// row already held; because Payments.PaymentNumber / Invoices.InvoiceNumber are GLOBAL keys the
// insert failed. The generator must notice and move past it instead.
public class FinancialDocumentNumberGeneratorTests
{
    private readonly Mock<IDocumentNumberStore> _store = new(MockBehavior.Strict);
    private readonly FinancialDocumentNumberGenerator _generator;

    public FinancialDocumentNumberGeneratorTests()
    {
        _generator = new FinancialDocumentNumberGenerator(
            _store.Object, NullLogger<FinancialDocumentNumberGenerator>.Instance);
    }

    [Fact]
    public async Task GenerateAsync_NumberIsFree_ReturnsItWithoutTouchingTheCounter()
    {
        _store.Setup(s => s.NextFromTenantCounterAsync("PAY", It.IsAny<CancellationToken>()))
            .ReturnsAsync("PAY-2026-00007");
        _store.Setup(s => s.ExistsAsync("PAY", "PAY-2026-00007", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var number = await _generator.GenerateAsync("PAY");

        number.Should().Be("PAY-2026-00007");
        // Strict mock: any HighestUsed/Raise call would have thrown.
    }

    [Fact]
    public async Task GenerateAsync_NumberAlreadyTaken_JumpsCounterPastHighestUsedAndReturnsNext()
    {
        // The counter says 2, but PAY-2026-00002 exists (another tenant, or seed data) and the
        // highest number in use anywhere is 5.
        _store.SetupSequence(s => s.NextFromTenantCounterAsync("PAY", It.IsAny<CancellationToken>()))
            .ReturnsAsync("PAY-2026-00002")
            .ReturnsAsync("PAY-2026-00006");
        _store.Setup(s => s.ExistsAsync("PAY", "PAY-2026-00002", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _store.Setup(s => s.ExistsAsync("PAY", "PAY-2026-00006", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _store.Setup(s => s.HighestUsedAsync("PAY", 2026, It.IsAny<CancellationToken>())).ReturnsAsync(5);
        _store.Setup(s => s.RaiseTenantCounterAsync("PAY", 2026, 5, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var number = await _generator.GenerateAsync("PAY");

        number.Should().Be("PAY-2026-00006");
        _store.Verify(s => s.RaiseTenantCounterAsync("PAY", 2026, 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateAsync_TakesTheYearFromTheNumberItself_NotFromTheClock()
    {
        // e.g. a call that straddles New Year: the collision is in 2025's sequence, so 2025's
        // highest number is what must be looked up and raised.
        _store.SetupSequence(s => s.NextFromTenantCounterAsync("INV", It.IsAny<CancellationToken>()))
            .ReturnsAsync("INV-2025-00001")
            .ReturnsAsync("INV-2025-00010");
        _store.Setup(s => s.ExistsAsync("INV", "INV-2025-00001", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _store.Setup(s => s.ExistsAsync("INV", "INV-2025-00010", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _store.Setup(s => s.HighestUsedAsync("INV", 2025, It.IsAny<CancellationToken>())).ReturnsAsync(9);
        _store.Setup(s => s.RaiseTenantCounterAsync("INV", 2025, 9, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        (await _generator.GenerateAsync("INV")).Should().Be("INV-2025-00010");
    }

    [Fact]
    public async Task GenerateAsync_EveryAttemptCollides_GivesUpWithAClearErrorInsteadOfLooping()
    {
        _store.Setup(s => s.NextFromTenantCounterAsync("PAY", It.IsAny<CancellationToken>()))
            .ReturnsAsync("PAY-2026-00003");
        _store.Setup(s => s.ExistsAsync("PAY", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _store.Setup(s => s.HighestUsedAsync("PAY", 2026, It.IsAny<CancellationToken>())).ReturnsAsync(3);
        _store.Setup(s => s.RaiseTenantCounterAsync("PAY", 2026, 3, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var act = () => _generator.GenerateAsync("PAY");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*PAY*");
        _store.Verify(
            s => s.NextFromTenantCounterAsync("PAY", It.IsAny<CancellationToken>()),
            Times.Exactly(FinancialDocumentNumberGenerator.MaxAttempts));
    }

    [Fact]
    public async Task GenerateAsync_UnexpectedNumberFormat_FailsLoudlyRatherThanGuessingAYear()
    {
        _store.Setup(s => s.NextFromTenantCounterAsync("PAY", It.IsAny<CancellationToken>()))
            .ReturnsAsync("garbage");
        _store.Setup(s => s.ExistsAsync("PAY", "garbage", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _generator.GenerateAsync("PAY");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*garbage*");
    }
}
