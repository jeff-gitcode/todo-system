using AutoMapper;
using TodoSystem.Domain.Entities;

namespace TodoSystem.Application.Dtos
{
    public class TodoProfile : Profile
    {
        public TodoProfile()
        {
            CreateMap<Todo, TodoDto>();
            // Add other mappings as needed
        }
    }
}