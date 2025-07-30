package com.example.todo.usecase;

import com.example.todo.domain.model.Todo;
import com.example.todo.domain.repository.TodoRepository;
import com.example.todo.dto.*;
import com.example.todo.exception.NotFoundException;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.stereotype.Service;

@Service
public class TodoServiceImpl implements TodoService {
    private final TodoRepository todoRepository;

    @Autowired
    public TodoServiceImpl(TodoRepository todoRepository) {
        this.todoRepository = todoRepository;
    }

    @Override
    public TodoDTO createTodo(TodoCreateDTO dto) {
        Todo todo = new Todo();
        todo.setTitle(dto.getTitle());
        todo.setDescription(dto.getDescription());
        todo = todoRepository.save(todo);
        return mapToDTO(todo);
    }

    @Override
    public Page<TodoDTO> getAllTodos(Pageable pageable) {
        return todoRepository.findAll(pageable).map(this::mapToDTO);
    }

    @Override
    public TodoDTO getTodoById(Long id) {
        Todo todo = todoRepository.findById(id).orElseThrow(() -> new NotFoundException("Todo not found"));
        return mapToDTO(todo);
    }

    @Override
    public TodoDTO updateTodo(Long id, TodoUpdateDTO dto) {
        Todo todo = todoRepository.findById(id).orElseThrow(() -> new NotFoundException("Todo not found"));
        todo.setTitle(dto.getTitle());
        todo.setDescription(dto.getDescription());
        todo = todoRepository.save(todo);
        return mapToDTO(todo);
    }

    @Override
    public void deleteTodo(Long id) {
        Todo todo = todoRepository.findById(id).orElseThrow(() -> new NotFoundException("Todo not found"));
        todoRepository.delete(todo);
    }

    private TodoDTO mapToDTO(Todo todo) {
        TodoDTO dto = new TodoDTO();
        dto.setId(todo.getId());
        dto.setTitle(todo.getTitle());
        dto.setDescription(todo.getDescription());
        return dto;
    }
}