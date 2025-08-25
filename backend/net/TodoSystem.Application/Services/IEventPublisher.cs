using System.Threading;
using System.Threading.Tasks;
using TodoSystem.Domain.Events;

namespace TodoSystem.Application.Services
{
    public interface IEventPublisher
    {
        Task PublishExternalTodoCreatedAsync(ExternalTodoCreatedEvent eventData, CancellationToken cancellationToken = default);
    }
}