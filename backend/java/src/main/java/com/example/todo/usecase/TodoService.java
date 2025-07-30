package com.example.todo.usecase;

import com.example.todo.dto.*;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;

public interface TodoService {
    TodoDTO createTodo(TodoCreateDTO dto);
    Page<TodoDTO> getAllTodos(Pageable pageable);
    TodoDTO getTodoById(Long id);
    TodoDTO updateTodo(Long id, TodoUpdateDTO dto);
    void deleteTodo(Long id);
}
