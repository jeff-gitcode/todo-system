package com.example.todo.domain.repository;

import com.example.todo.domain.model.Todo;
import org.springframework.data.jpa.repository.JpaRepository;

public interface TodoRepository extends JpaRepository<Todo, Long> {
    // ...existing code...
}