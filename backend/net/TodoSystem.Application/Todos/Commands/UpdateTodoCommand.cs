using MediatR;
using TodoSystem.Application.Dtos;
using TodoSystem.Domain.Entities;
using TodoSystem.Domain.Repositories;
using AutoMapper;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;

namespace TodoSystem.Application.Todos.Commands
{
    public class UpdateTodoCommand : IRequest<TodoDto>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
    }

    public class UpdateTodoCommandHandler : IRequestHandler<UpdateTodoCommand, TodoDto>
    {
        private readonly ITodoRepository _todoRepository;
        private readonly IMapper _mapper;

        public UpdateTodoCommandHandler(ITodoRepository todoRepository, IMapper mapper)
        {
            _todoRepository = todoRepository;
            _mapper = mapper;
        }

        public async Task<TodoDto> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
        {
            var todo = await _todoRepository.GetByIdAsync(request.Id);
            if (todo == null)
                throw new Exception("Todo not found");

            todo.Title = request.Title;
            await _todoRepository.UpdateAsync(todo);

            return _mapper.Map<TodoDto>(todo);
        }
    }

    public class UpdateTodoCommandValidator : AbstractValidator<UpdateTodoCommand>
    {
        public UpdateTodoCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
        }
    }
}