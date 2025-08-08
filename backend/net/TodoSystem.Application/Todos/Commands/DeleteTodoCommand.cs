using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using TodoSystem.Domain.Repositories;

namespace TodoSystem.Application.Todos.Commands
{
    public class DeleteTodoCommand : IRequest
    {
        public Guid Id { get; set; }
    }

    public class DeleteTodoCommandHandler : IRequestHandler<DeleteTodoCommand>
    {
        private readonly ITodoRepository _todoRepository;

        public DeleteTodoCommandHandler(ITodoRepository todoRepository)
        {
            _todoRepository = todoRepository;
        }

        public async Task Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
        {
            await _todoRepository.DeleteAsync(request.Id);
        }
    }
}